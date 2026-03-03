using System.Collections.ObjectModel;
using H.Avalonia.ViewModels.OptionsViews.DataTransferObjects;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Prism.Regions;

namespace H.Avalonia.ViewModels.OptionsViews
{
    public class DefaultManureCompositionViewModel : ViewModelBase
    {
        #region Fields

        private ObservableCollection<DefaultManureCompositionDTO> _defaultManureDTOs = null!;
        private string _nitrogenFractionHeader = string.Empty;
        private string _carbonFractionHeader = string.Empty;
        private string _phosphorusFractionHeader = string.Empty;
        private string _moistureContentHeader = string.Empty;

        #endregion

        #region Constructors

        public DefaultManureCompositionViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IStorageService storageService,
            ILogger logger) : base(regionManager, eventAggregator, storageService, logger)
        {
            this.DefaultManureDTOs = new ObservableCollection<DefaultManureCompositionDTO>();
            this.Initialize();
            base.IsInitialized = true;
        }
         
        #endregion

        #region Properties

        public ObservableCollection<DefaultManureCompositionDTO> DefaultManureDTOs
        {
            get => _defaultManureDTOs;
            set => SetProperty(ref _defaultManureDTOs, value);
        }

        public string NitrogenFractionHeader
        {
            get => _nitrogenFractionHeader;
            set => SetProperty(ref _nitrogenFractionHeader, value);
        }

        public string CarbonFractionHeader
        {
            get => _carbonFractionHeader;
            set => SetProperty(ref _carbonFractionHeader, value);
        }

        public string PhosphorusFractionHeader
        {
            get => _phosphorusFractionHeader;
            set => SetProperty(ref _phosphorusFractionHeader, value);
        }

        public string MoistureContentHeader
        {
            get => _moistureContentHeader;
            set => SetProperty(ref _moistureContentHeader, value);
        }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (base.ActiveFarm is null) return;
            foreach (var dataClassInstance in base.ActiveFarm.DefaultManureCompositionData)
            {
                var dto = new DefaultManureCompositionDTO(dataClassInstance);
                dto.SetSuppressValidationFlag(true);
                dto.MoistureContent = dataClassInstance.MoistureContent;
                dto.NitrogenFraction = dataClassInstance.NitrogenFraction;
                dto.CarbonFraction = dataClassInstance.CarbonFraction;
                dto.PhosphorusFraction = dataClassInstance.PhosphorusFraction;
                dto.CarbonToNitrogenRatio = dataClassInstance.CarbonToNitrogenRatio;
                dto.SetSuppressValidationFlag(false);
                this.DefaultManureDTOs.Add(dto);
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            SetStrings();

            if (!IsInitialized)
            {
                DefaultManureDTOs.Clear();
                this.Initialize();
                base.IsInitialized = true;
            }
        }

        #endregion

        #region Private Methods

        private void SetStrings()
        {
            if (StorageService == null) return;
            var displayUnits = StorageService.Storage.ApplicationData.DisplayUnitStrings;
            NitrogenFractionHeader = H.Core.Properties.Resources.LabelTotalNitrogen + " " + displayUnits.PercentageWetWeight;
            CarbonFractionHeader = H.Core.Properties.Resources.LabelTotalCarbon + " " + displayUnits.PercentageWetWeight;
            PhosphorusFractionHeader = H.Core.Properties.Resources.LabelTotalPhosphorus + " " + displayUnits.PercentageWetWeight;
            MoistureContentHeader = H.Core.Properties.Resources.LabelMoistureContent + " " + displayUnits.PercentageString;
        }

        #endregion

        #region Event Handlers

        #endregion
    }
}
