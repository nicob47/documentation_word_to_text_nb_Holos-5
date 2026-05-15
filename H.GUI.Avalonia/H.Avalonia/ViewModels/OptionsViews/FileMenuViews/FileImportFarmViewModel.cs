using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using H.Avalonia.Services;
using H.Core.Enumerations;
using H.Core.Services.StorageService;
using H.Infrastructure;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Regions;
using NLog;

namespace H.Avalonia.ViewModels.OptionsViews.FileMenuViews
{
    /// <summary>
    /// ViewModel for the "Import Farm" flow — lets the user pick one or more v4-shape
    /// <c>.json</c> export files (or directories of them) and load the contained farms into
    /// the current v5 application state.
    ///
    /// <para><b>Pipeline:</b></para>
    /// <list type="number">
    ///   <item>User selects a file / directory in the view.</item>
    ///   <item><see cref="GetFarmsFromExportFileAsync"/> reads the JSON off the UI thread via <see cref="Task.Run"/>, deserializes it through Newtonsoft.Json with <c>TypeNameHandling.Auto</c> (matching the v4 serializer), and lands a <see cref="ObservableCollection{Farm}"/> the user can pick from.</item>
    ///   <item><see cref="NormalizeProvinceOnImport"/> runs on every deserialized farm to reset any non-Canadian <c>Province</c> values (Guard B — protects against v4-era Ireland-mode imports producing NaN-filled charts downstream).</item>
    ///   <item>On confirm, <see cref="OnImport"/> calls <see cref="IStorageService.AddFarm"/> for each selected farm.</item>
    /// </list>
    ///
    /// <para><b>v4 schema migration:</b></para>
    /// Schema fixes (renamed properties, added defaults, etc.) are <i>not</i> applied here —
    /// they go through <see cref="H.Core.Migrations.JsonMigrationPipeline.MigrateFarmExport"/>
    /// at a lower layer. This class is just the GUI binding surface plus the province guard.
    /// </summary>
    public class FileImportFarmViewModel : ViewModelBase
    {
        // NLog logger. Routes through the same NLog pipeline as ILogger so every
        // log line in the app uses the unified format from NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Fields
        private bool _showGrid;
        private bool _isFarmImported;
        private IList<H.Core.Models.Farm> _selectedFarms = new List<H.Core.Models.Farm>();
        private bool _canImport = false;
        private const string exportedFileExtension = ".json";
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the FileImportFarmViewModel.
        /// Sets up the import command and initializes collections and properties.
        /// </summary>
        /// <param name="regionManager">Manager for handling navigation regions</param>
        /// <param name="storageService">Service for farm data storage operations</param>
        public FileImportFarmViewModel(IRegionManager regionManager, IStorageService storageService, INotificationManagerService notificationManager) : base(regionManager, storageService, notificationManager)
        {
            ImportFarms = new DelegateCommand(OnImport);
            this.Farms = new ObservableCollection<H.Core.Models.Farm>();
            this.ShowGrid = false;
            this.IsFarmImported = false;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Command to execute the farm import operation for selected farms.
        /// </summary>
        public DelegateCommand ImportFarms { get; }

        /// <summary>
        /// Collection of farms loaded from export files that are available for import.
        /// </summary>
        public ObservableCollection<H.Core.Models.Farm> Farms { get; set; }

        /// <summary>
        /// Gets or sets whether the farm selection grid should be displayed to the user.
        /// </summary>
        public bool ShowGrid
        {
            get => _showGrid;
            set => SetProperty(ref _showGrid, value);
        }

        /// <summary>
        /// Gets or sets whether farms have been successfully imported.
        /// Used to provide feedback to the user about the import status.
        /// </summary>
        public bool IsFarmImported
        {
            get => _isFarmImported;
            set
            {
                SetProperty(ref _isFarmImported, value);
            }
        }

        /// <summary>
        /// Gets or sets the list of farms selected by the user for import.
        /// Automatically updates the CanImport property based on selection count.
        /// </summary>
        public IList<H.Core.Models.Farm> SelectedFarms
        {
            get => _selectedFarms;
            set
            {
                SetProperty(ref _selectedFarms, value);
                if(SelectedFarms.Count != 0)
                {
                    CanImport = true;
                }
                else
                {
                    CanImport = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the import operation can be performed.
        /// Depends on having at least one farm selected.
        /// </summary>
        public bool CanImport
        {
            get => _canImport;
            set => SetProperty(ref _canImport, value);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the farm import operation when the ImportFarms command is executed.
        /// Adds each selected farm to the storage service and displays success/error notifications.
        /// </summary>
        private void OnImport()
        {
            try
            {
                if (this.StorageService != null)
                {
                    foreach (var farm in SelectedFarms)
                    {
                        this.StorageService.AddFarm(farm);
                    }
                }
                NotificationManager?.ShowToast(H.Core.Properties.Resources.LabelSuccess, H.Core.Properties.Resources.LabelFarmImportSuccess, NotificationType.Success);
                IsFarmImported = true;
            }
            catch (Exception ex)
            {
                _log.Error($"Error importing farms: {ex.Message}");
                NotificationManager?.ShowToast(H.Core.Properties.Resources.ErrorError, ex.Message, NotificationType.Error);
            }
        }

        /// <summary>
        /// Asynchronously reads and deserializes farm data from a JSON export file.
        /// Uses JSON.NET with TypeNameHandling.Auto to properly deserialize complex farm objects.
        /// </summary>
        /// <param name="filePath">Path to the JSON file containing exported farm data</param>
        /// <returns>Collection of farms from the file, or empty collection if file is invalid or errors occur</returns>
        public async Task<IEnumerable<H.Core.Models.Farm>> GetFarmsFromExportFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Enumerable.Empty<H.Core.Models.Farm>();
            }

            try
            {
                var farms = await Task.Run(() =>
                {
                    // Open reading stream from the file.
                    using var streamReader = new StreamReader(filePath);
                    using JsonReader jsonReader = new JsonTextReader(streamReader);

                    JsonSerializer serializer = new()
                    {
                        // Serializer and de-serializer must both have this set to Auto
                        TypeNameHandling = TypeNameHandling.Auto,
                    };
                    return serializer.Deserialize<List<H.Core.Models.Farm>>(jsonReader);
                });

                // Guard B: when a v4 .json is imported into v5, the file may carry a
                // non-Canadian province (e.g. one of the 26 Irish counties from v4's Ireland
                // mode). Holos v5 only supports the 13 Canadian provinces/territories, so
                // remap any non-Canadian Farm.Province (and the soil-polygon province) back
                // to Province.SelectProvince. This forces the user to pick a Canadian
                // province in the soil settings before they can run the analysis — better
                // than silently producing a NaN-filled chart downstream. See
                // CanadianProvinces.IsCanadian / FarmAnalysisService Guard A for the
                // runtime check that catches anything we miss here.
                var normalized = farms ?? Enumerable.Empty<H.Core.Models.Farm>();
                foreach (var farm in normalized)
                {
                    NormalizeProvinceOnImport(farm, filePath);
                }
                return normalized;
            }
            catch (Exception e)
            {
                _log.Error($"{e.Message}");
                if (e.InnerException != null)
                {
                    _log.Error($"{e.InnerException.ToString()}");
                }
                return Enumerable.Empty<H.Core.Models.Farm>();
            }
        }

        /// <summary>
        /// Recursively scans a directory for JSON export files and extracts all farm data.
        /// Searches for all .json files in the specified directory and subdirectories,
        /// then processes each file to extract farm information.
        /// </summary>
        /// <param name="path">Root directory path to scan for export files</param>
        /// <returns>Collection of all farms found in export files within the directory tree</returns>
        public async Task<IEnumerable<H.Core.Models.Farm>> GetExportedFarmsFromDirectoryRecursivelyAsync(string path)
        {
            var result = new List<H.Core.Models.Farm>();

            var stringCollection = new StringCollection();
            var files = FileSystemHelper.ListAllFiles(stringCollection, path, $"*{exportedFileExtension}", isRecursiveScan: true);
            if (files == null)
            {
                return result;
            }

            var farmNumber = 1;
            var totalFarms = files.Count;
            foreach (var filePath in files)
            {
                if(filePath != null && File.Exists(filePath))
                {
                    var farmsFromFile = await GetFarmsFromExportFileAsync(filePath);
                    result.AddRange(farmsFromFile);
                    farmNumber++;
                }
                
            }

            return result;
        }

        /// <summary>
        /// Inspect <paramref name="farm"/> after deserialization and reset
        /// <see cref="H.Core.Models.Farm.Province"/> (plus the soil polygon's province if
        /// available) to <see cref="Province.SelectProvince"/> when the imported value
        /// isn't one of the supported Canadian provinces or territories. The user is then
        /// forced through the soil-data picker before the analysis pipeline will accept
        /// the farm. Logs each remap so we can grep for affected imports.
        /// </summary>
        private static void NormalizeProvinceOnImport(H.Core.Models.Farm farm, string filePath)
        {
            if (farm is null)
            {
                return;
            }

            if (!CanadianProvinces.IsCanadian(farm.Province))
            {
                _log.Warn(
                    $"{nameof(FileImportFarmViewModel)}.{nameof(NormalizeProvinceOnImport)} " +
                    $"farm='{farm.Name}' file='{filePath}' had non-Canadian Farm.Province='{farm.Province}'; " +
                    $"resetting to {nameof(Province.SelectProvince)}. User must re-select a Canadian province before analysis.");
                farm.Province = Province.SelectProvince;
            }

            var defaultSoil = farm.GeographicData?.DefaultSoilData;
            if (defaultSoil is not null && !CanadianProvinces.IsCanadian(defaultSoil.Province))
            {
                _log.Warn(
                    $"{nameof(FileImportFarmViewModel)}.{nameof(NormalizeProvinceOnImport)} " +
                    $"farm='{farm.Name}' file='{filePath}' had non-Canadian DefaultSoilData.Province='{defaultSoil.Province}'; " +
                    $"resetting to {nameof(Province.SelectProvince)}.");
                defaultSoil.Province = Province.SelectProvince;
            }
        }

        #endregion
    }
}
