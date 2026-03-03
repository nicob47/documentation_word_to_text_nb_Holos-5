using H.Core.Enumerations;
using System.Collections.ObjectModel;
using H.Core.Services.StorageService;
using Prism.Regions;
using H.Avalonia.ViewModels.OptionsViews.DataTransferObjects;

namespace H.Avalonia.ViewModels.OptionsViews
{
    public class FarmSettingsViewModel : ViewModelBase
    {
        #region Fields

        private ObservableCollection<MeasurementSystemType> _measurementSystemTypes = null!;
        private MeasurementSystemType _selectedMeasurementType;
        private FarmSettingsDTO _data = null!;

        #endregion

        #region Constructors
        public FarmSettingsViewModel() { }
        public FarmSettingsViewModel(IStorageService storageService) : base(storageService)
        {
            _measurementSystemTypes = new ObservableCollection<MeasurementSystemType>() { MeasurementSystemType.Metric, MeasurementSystemType.Imperial };
            this.Initialize();
            base.IsInitialized = true;
        }

        #endregion

        #region Properties

        public FarmSettingsDTO Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        public ObservableCollection<MeasurementSystemType> MeasurementSystemTypes
        {
            get => _measurementSystemTypes; 
        }

        public MeasurementSystemType SelectedMeasurementSystem
        {
            get => _selectedMeasurementType; 
            set
            {
                if (SetProperty(ref _selectedMeasurementType, value))
                {
                    if (base.IsInitialized && MeasurementSystemTypes.Contains(value) && base.ActiveFarm is not null && base.StorageService != null)
                    {
                        base.ActiveFarm.MeasurementSystemType = value;
                        base.ActiveFarm.MeasurementSystemSelected = true;
                        base.StorageService.Storage.ApplicationData.DisplayUnitStrings.SetStrings(base.ActiveFarm.MeasurementSystemType);
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            this.Data = new FarmSettingsDTO(StorageService!);
            this.SelectedMeasurementSystem = StorageService!.GetActiveFarm()?.MeasurementSystemType ?? MeasurementSystemType.Metric;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!IsInitialized)
            {
                this.Initialize();
                base.IsInitialized = true;
            }
        }

        #endregion

        #region Event Handlers

        #endregion
    }
}
