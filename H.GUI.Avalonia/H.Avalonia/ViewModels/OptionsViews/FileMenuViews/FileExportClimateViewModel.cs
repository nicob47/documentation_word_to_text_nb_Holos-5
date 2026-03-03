using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using H.Avalonia.Services;
using H.Core.Providers;
using H.Core.Providers.Climate;
using H.Core.Services.StorageService;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static H.Avalonia.Views.OptionsViews.FileMenuViews.FileExportClimateView;

namespace H.Avalonia.ViewModels.OptionsViews.FileMenuViews
{
    /// <summary>
    /// ViewModel responsible for handling climate data export functionality.
    /// Exports both daily and monthly climate data for a specific farm to CSV format files.
    /// Inherits from FileExportFarmViewModel to leverage base export functionality and notification management.
    /// </summary>
    public class FileExportClimateViewModel : FileExportFarmViewModel
    {

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the FileExportClimateViewModel class.
        /// Sets up the climate export command and inherits base export functionality.
        /// </summary>
        /// <param name="regionManager">Service for managing UI regions in the application</param>
        /// <param name="storageService">Service for handling file storage operations</param>
        public FileExportClimateViewModel(IRegionManager regionManager, IStorageService storageService, INotificationManagerService notificationManager) : base(regionManager, storageService, notificationManager)
        {
            // Initialize the climate export command that will be bound to UI elements
            this.ExportClimate = new DelegateCommand<object>(OnExport);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Command that handles the climate data export operation.
        /// Accepts an ExportClimateData object containing the farm and file information.
        /// Bound to UI elements to trigger the climate export process when invoked.
        /// </summary>
        public DelegateCommand<object> ExportClimate { get; }
        
        /// <summary>
        /// Asynchronously exports climate data for a specific farm to CSV files.
        /// Creates both daily and monthly climate data files using the ClimateProvider.
        /// The monthly file is automatically generated with "_monthly" suffix in the filename.
        /// Shows success/error notifications to inform the user of the operation status.
        /// </summary>
        /// <param name="farm">The farm object containing location and climate data information</param>
        /// <param name="file">The storage file where the daily climate data will be exported</param>
        /// <returns>A task representing the asynchronous export operation</returns>
        public async Task ExportAsync(H.Core.Models.Farm farm, IStorageFile file)
        {
            try
            {
                const string Extension = ".csv";
                
                // Initialize climate provider with SLC (Soil Landscapes of Canada) data provider
                // This provides access to climate data based on farm location
                ClimateProvider climateProvider = new ClimateProvider(new SlcClimateDataProvider());
  
                // Perform the climate data export operation on a background thread to avoid blocking the UI
                await Task.Run(() =>
                {
                    // Export daily climate data to the specified file
                    climateProvider.OutputDailyClimateData(farm, file.Path.LocalPath);
                    
                    // Generate monthly climate data filename by inserting "_monthly" before the file extension
                    var indexOfExtension = file.Path.LocalPath.IndexOf(Extension, StringComparison.OrdinalIgnoreCase);
                    var monthlyFilename = file.Path.LocalPath.Insert(indexOfExtension, "_monthly");
                    
                    // Export monthly climate data to the generated filename
                    climateProvider.OutputMonthlyClimateData(farm, monthlyFilename);
                });

                // Show success notification to the user with the exported file name
                NotificationManager?.ShowToast(H.Core.Properties.Resources.LabelSuccess, String.Format(H.Core.Properties.Resources.ExportClimateDataSuccess, file.Name), NotificationType.Success);
            }
            catch (Exception ex)
            {
                // Log the error for debugging purposes
                Debug.WriteLine($"Error exporting farms: {ex.Message}");
                
                // Show error notification to the user with the exception message
                NotificationManager?.ShowToast(H.Core.Properties.Resources.ErrorError, ex.Message, NotificationType.Error);
            }
        }

        /// <summary>
        /// Command handler for the climate export operation. Validates that the required 
        /// ExportClimateData object is provided with valid farm and file information.
        /// Shows an error notification if the data is invalid or missing.
        /// </summary>
        /// <param name="obj">Expected to be an ExportClimateData object containing farm and file information</param>
        private async void OnExport(object obj)
        {
            // Validate that the parameter is the expected ExportClimateData type
            if (obj is ExportClimateData data)
            {
                // Extract farm and file from the data object and validate both are present
                if(data.Farm is H.Core.Models.Farm farm && data.File is IStorageFile file)
                {
                    // Proceed with the climate export operation
                    await this.ExportAsync(farm, file);
                }
            }
            else
            {
                // Show error notification for invalid or missing export data
                NotificationManager?.ShowToast(H.Core.Properties.Resources.ErrorError, H.Core.Properties.Resources.ErrorNoDataForExport, NotificationType.Error);
            }
        }
        #endregion
    }
}
