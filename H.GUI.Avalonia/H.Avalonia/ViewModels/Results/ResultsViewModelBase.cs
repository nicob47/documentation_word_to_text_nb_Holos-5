using H.Avalonia.Services;
using H.Core.Services.StorageService;
using Prism.Regions;
using Prism.Commands;

namespace H.Avalonia.ViewModels.Results
{
    public class ResultsViewModelBase : ViewModelBase
    {
        #region Fields

        private bool _processing;

        #endregion

        #region Constructors

        protected ResultsViewModelBase() { }

        protected ResultsViewModelBase(IRegionManager regionManager) : base(regionManager)
        {
        }

        protected ResultsViewModelBase(IRegionManager regionManager, INotificationManagerService notificationManager) : base(regionManager, notificationManager)
        {
        }

        protected ResultsViewModelBase(IRegionManager regionManager, INotificationManagerService notificationManager, IStorageService storageService) : base(regionManager, notificationManager, storageService)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// A command that triggers when a user clicks the back button on the page.
        /// </summary>
        public DelegateCommand GoBackCommand { get; set; } = null!;

        /// <summary>
        /// A command that triggers when a user clicks the export to csv button on the page.
        /// </summary>
        public DelegateCommand<object> ExportToCsvCommand { get; set; } = null!;

        /// <summary>
        /// A bool that checks if data extraction is currently processing or not. Returns true if data is still processing, return false otherwise.
        /// </summary>
        public bool IsProcessingData
        {
            get => _processing;
            set => SetProperty(ref _processing, value);
        } 

        #endregion
    }
}
