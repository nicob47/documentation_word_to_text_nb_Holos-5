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
using H.Core.Services.StorageService;
using H.Infrastructure;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Regions;

namespace H.Avalonia.ViewModels.OptionsViews.FileMenuViews
{
    /// <summary>
    /// ViewModel for importing farm data from exported JSON files.
    /// Handles farm selection, validation, and importing multiple farms into the application.
    /// </summary>
    public class FileImportFarmViewModel : ViewModelBase
    {
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
                Trace.TraceError($"Error importing farms: {ex.Message}");
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
                return farms ?? Enumerable.Empty<H.Core.Models.Farm>();
            }
            catch (Exception e)
            {
                Trace.TraceError($"{e.Message}");
                if (e.InnerException != null)
                {
                    Trace.TraceError($"{e.InnerException.ToString()}");
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

        #endregion
    }
}
