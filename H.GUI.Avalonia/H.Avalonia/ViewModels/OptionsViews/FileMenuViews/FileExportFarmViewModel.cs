using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using H.Core.Services.StorageService;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Regions;
using Avalonia.Controls.Notifications;
using H.Avalonia.Services;
using H.Avalonia.ViewModels.FarmCreationViews;

namespace H.Avalonia.ViewModels.OptionsViews.FileMenuViews
{
    /// <summary>
    /// ViewModel responsible for handling farm export functionality. 
    /// Allows users to select farms and export them to JSON format files.
    /// Inherits from FarmOpenExistingViewmodel to provide base farm management capabilities.
    /// </summary>
    public class FileExportFarmViewModel : FarmOpenExistingViewmodel
    {
        #region Fields
        /// <summary>
        /// Collection of farms selected by the user for export
        /// </summary>
        private IList<H.Core.Models.Farm> _selectedFarms = new List<H.Core.Models.Farm>();
        
        /// <summary>
        /// Flag indicating whether the export operation can be executed (i.e., farms are selected)
        /// </summary>
        private bool _canExportExecute = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the FileExportFarmViewModel class.
        /// Sets up the export command and inherits base functionality for farm management.
        /// </summary>
        /// <param name="regionManager">Service for managing UI regions in the application</param>
        /// <param name="storageService">Service for handling file storage operations</param>
        public FileExportFarmViewModel(IRegionManager regionManager, IStorageService storageService, INotificationManagerService notificationManager) : base(regionManager, storageService, notificationManager)
        {
            // Initialize the export command that will be bound to UI elements
            ExportFarms = new DelegateCommand<IStorageFile>(OnExport);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Asynchronously exports the selected farms to a JSON file.
        /// Uses JSON serialization with type name handling to preserve object types during serialization.
        /// Shows success/error notifications to inform the user of the operation status.
        /// </summary>
        /// <param name="file">The storage file where the farms will be exported</param>
        /// <returns>A task representing the asynchronous export operation</returns>
        public async Task ExportAsync(IStorageFile file)
        {
            try
            {
                // Perform the file writing operation on a background thread to avoid blocking the UI
                await Task.Run(async() =>
                {
                    // Open writing stream from the file
                    await using var stream = await file.OpenWriteAsync();
                    using var streamWriter = new StreamWriter(stream);

                    // Configure JSON serializer with type name handling to preserve object inheritance
                    JsonSerializer serializer = new()
                    {
                        // Serializer and de-serializer must both have this set to Auto
                        // This ensures proper handling of derived types and polymorphic objects
                        TypeNameHandling = TypeNameHandling.Auto,
                    };
                    
                    // Serialize the selected farms to JSON format
                    serializer.Serialize(streamWriter, SelectedFarms, typeof(H.Core.Models.Farm));
                });

                // Show success notification to the user
                NotificationManager?.ShowToast(H.Core.Properties.Resources.LabelSuccess, H.Core.Properties.Resources.LabelFarmExportSuccess, NotificationType.Success);
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
        /// Command handler for the export operation. Validates that a file is selected
        /// before proceeding with the export. Shows an error notification if no file is selected.
        /// </summary>
        /// <param name="file">The storage file selected by the user for export</param>
        private async void OnExport(IStorageFile file)
        {
            // Validate that a file has been selected
            if(file == null)
            {
                // Show error notification for missing file selection
                NotificationManager?.ShowToast(H.Core.Properties.Resources.ErrorError, H.Core.Properties.Resources.ErrorNoFileSelected, NotificationType.Error);
                return;
            }
            
            // Proceed with the export operation
            await this.ExportAsync(file);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Command that handles the farm export operation. 
        /// Bound to UI elements to trigger the export process when invoked.
        /// </summary>
        public DelegateCommand<IStorageFile> ExportFarms { get; }
        
        /// <summary>
        /// Gets or sets the collection of farms selected by the user for export.
        /// Automatically updates the CanExportExecute property based on whether farms are selected.
        /// </summary>
        public IList<H.Core.Models.Farm> SelectedFarms
        {
            get => _selectedFarms;
            set
            {
                SetProperty(ref _selectedFarms, value);
                
                // Update the export availability based on farm selection
                if(SelectedFarms.Count != 0)
                {
                    CanExportExecute = true;
                }
                else
                {
                    CanExportExecute = false;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the export operation can be executed.
        /// </summary>
        public bool CanExportExecute
        {
            get => _canExportExecute;
            set
            {
                SetProperty(ref _canExportExecute, value);
            }
        }
        #endregion
    }
}
