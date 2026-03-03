using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls.Notifications;
using H.Core.Services;
using H.Core.Services.StorageService;
using Prism.Commands;

namespace H.Avalonia.ViewModels.OptionsViews.FileMenuViews
{
    public class FileSaveOptionsViewModel : ViewModelBase
    {
        #region Fields

        private string _newFarmName = string.Empty;
        private IFarmResultsService_NEW _farmResultsService = null!;
        private bool _runValidationFlag;

        #endregion

        #region Constructors

        public FileSaveOptionsViewModel(IStorageService storageService, IFarmResultsService_NEW farmResultsService) : base(storageService)
        {
            if (farmResultsService != null)
            {
                _farmResultsService = farmResultsService;
            }
            else
            {
                throw new ArgumentNullException(nameof(farmResultsService));
            }

            this.SaveCommand = new DelegateCommand(OnSaveExecute, CanExecuteSave);
            this.SaveAsCommand = new DelegateCommand(OnSaveAsExecute);
            _runValidationFlag = true;
        }

        #endregion

        #region Properties

        public ICommand SaveCommand { get; set; }

        public ICommand SaveAsCommand { get; set; }

        public string NewFarmName
        {
            get => _newFarmName;
            set
            {
                if (SetProperty(ref _newFarmName, value) && _runValidationFlag)
                {
                    ValidateNewFarmName(nameof(NewFarmName), value);
                }
            }
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private void ValidateNewFarmName(string propertyName, string farmName)
        {
            base.RemoveError(propertyName);

            if (string.IsNullOrEmpty(farmName))
            {
                base.AddError(propertyName, H.Core.Properties.Resources.ErrorNameCannotBeEmpty);
            }
            else if (base.StorageService?.Storage.ApplicationData.Farms.Any(x => x.Name == farmName) == true)
            {
                base.AddError(propertyName, H.Core.Properties.Resources.ErrorFarmNameInUse);
            }
        }

        #endregion

        #region Event Handlers

        private async void OnSaveExecute()
        {
            if (base.StorageService != null)
            {
                await base.StorageService.Storage.SaveAsync();
            }
            NotificationManager?.ShowToast(H.Core.Properties.Resources.ToastTitleSaveSuccess, H.Core.Properties.Resources.ToastMessageSavedSuccessfully, NotificationType.Success);
        }

        private bool CanExecuteSave()
        {
            if (base.StorageService?.Storage.SaveTask != null && base.StorageService.Storage.SaveTask.Status.Equals(TaskStatus.Running)) return false;

            return true;
        }

        private void OnSaveAsExecute()
        {
            if (string.IsNullOrEmpty(this.NewFarmName) || this.HasErrors)
            {
                return;
            }
            if (base.ActiveFarm is null || base.StorageService == null) return;
            var replicatedFarm = _farmResultsService.ReplicateFarm(base.ActiveFarm);
            replicatedFarm.Name = this.NewFarmName;
            base.StorageService.Storage.ApplicationData.Farms.Add(replicatedFarm);
            _runValidationFlag = false;
            this.NewFarmName = string.Empty;
            _runValidationFlag = true;
            NotificationManager?.ShowToast(H.Core.Properties.Resources.ToastTitleSaveSuccess, H.Core.Properties.Resources.ToastMessageSavedSuccessfully, NotificationType.Success);
        }

        #endregion
    }
}
