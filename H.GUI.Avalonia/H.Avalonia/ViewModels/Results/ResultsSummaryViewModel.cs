using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H.Avalonia.Services;
using H.Core.Services.StorageService;
using Prism.Commands;
using Prism.Regions;

namespace H.Avalonia.ViewModels.Results
{
    /// <summary>
    /// Placeholder view model for the results summary page
    /// </summary>
    public class ResultsSummaryViewModel : ResultsViewModelBase
    {
        #region Fields

        private string _farmName = "Sample Farm";
        private string _location = "Southern Alberta";
        private double _totalArea = 500.0;
        private string _primaryCrops = "Wheat, Barley, Canola";
        private string _livestockTypes = "Beef Cattle (200 head)";
        
        private double _totalGrainProduction = 2500.0;
        private double _totalForageProduction = 1200.0;
        private double _totalMilkProduction = 0.0;
        private double _totalBeefProduction = 85000.0;
        
        private double _totalGHGEmissions = 3250.5;
        private double _emissionsPerHectare = 6.5;
        private double _netCarbonBalance = -450.2;
        
        private double _entericFermentationPercent = 45.2;
        private double _manureManagementPercent = 25.8;
        private double _soilN2OPercent = 18.5;
        private double _energyUsePercent = 10.5;
        
        private double _totalCarbonSequestered = 650.0;
        private double _shelterbeltsSequestration = 180.0;
        private double _pasturelandSequestration = 320.0;
        private double _croppedLandSequestration = 150.0;
        
        private string _manureStorageType = "Solid Storage, Composted";
        private double _totalManureProduced = 2400.0;
        private double _manureN2OEmissions = 425.3;
        private double _manureCH4Emissions = 385.7;
        
        private string _keyFinding1 = "Enteric fermentation from cattle represents the largest emission source (45% of total emissions)";
        private string _keyFinding2 = "Carbon sequestration in shelterbelts and pastureland offsets 20% of total emissions";
        private string _keyFinding3 = "Improved manure management practices could reduce emissions by up to 15%";
        private string _recommendation1 = "Consider implementing rotational grazing to enhance soil carbon sequestration";
        private string _recommendation2 = "Evaluate feed additives to reduce enteric methane production";
        private string _recommendation3 = "Expand shelterbelt systems to increase carbon storage capacity";
        
        private const double MaxBarWidth = 400.0; // Maximum width for 100%

        #endregion

        #region Constructors

        public ResultsSummaryViewModel()
        {
        }

        public ResultsSummaryViewModel(IRegionManager regionManager, INotificationManagerService notificationManager, IStorageService storageService) 
            : base(regionManager, notificationManager, storageService)
        {
            InitializeCommands();
        }

        #endregion

        #region Properties

        // Farm Profile
        public string FarmName
        {
            get => _farmName;
            set => SetProperty(ref _farmName, value);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public double TotalArea
        {
            get => _totalArea;
            set => SetProperty(ref _totalArea, value);
        }

        public string PrimaryCrops
        {
            get => _primaryCrops;
            set => SetProperty(ref _primaryCrops, value);
        }

        public string LivestockTypes
        {
            get => _livestockTypes;
            set => SetProperty(ref _livestockTypes, value);
        }

        // Annual Production Summary
        public double TotalGrainProduction
        {
            get => _totalGrainProduction;
            set => SetProperty(ref _totalGrainProduction, value);
        }

        public double TotalForageProduction
        {
            get => _totalForageProduction;
            set => SetProperty(ref _totalForageProduction, value);
        }

        public double TotalMilkProduction
        {
            get => _totalMilkProduction;
            set => SetProperty(ref _totalMilkProduction, value);
        }

        public double TotalBeefProduction
        {
            get => _totalBeefProduction;
            set => SetProperty(ref _totalBeefProduction, value);
        }

        // Total GHG Emissions
        public double TotalGHGEmissions
        {
            get => _totalGHGEmissions;
            set => SetProperty(ref _totalGHGEmissions, value);
        }

        public double EmissionsPerHectare
        {
            get => _emissionsPerHectare;
            set => SetProperty(ref _emissionsPerHectare, value);
        }

        public double NetCarbonBalance
        {
            get => _netCarbonBalance;
            set => SetProperty(ref _netCarbonBalance, value);
        }

        // Emissions Breakdown - Percentages
        public double EntericFermentationPercent
        {
            get => _entericFermentationPercent;
            set
            {
                if (SetProperty(ref _entericFermentationPercent, value))
                {
                    RaisePropertyChanged(nameof(EntericFermentationWidth));
                }
            }
        }

        public double ManureManagementPercent
        {
            get => _manureManagementPercent;
            set
            {
                if (SetProperty(ref _manureManagementPercent, value))
                {
                    RaisePropertyChanged(nameof(ManureManagementWidth));
                }
            }
        }

        public double SoilN2OPercent
        {
            get => _soilN2OPercent;
            set
            {
                if (SetProperty(ref _soilN2OPercent, value))
                {
                    RaisePropertyChanged(nameof(SoilN2OWidth));
                }
            }
        }

        public double EnergyUsePercent
        {
            get => _energyUsePercent;
            set
            {
                if (SetProperty(ref _energyUsePercent, value))
                {
                    RaisePropertyChanged(nameof(EnergyUseWidth));
                }
            }
        }

        // Emissions Breakdown - Calculated Widths
        public double EntericFermentationWidth => (EntericFermentationPercent / 100.0) * MaxBarWidth;
        
        public double ManureManagementWidth => (ManureManagementPercent / 100.0) * MaxBarWidth;
        
        public double SoilN2OWidth => (SoilN2OPercent / 100.0) * MaxBarWidth;
        
        public double EnergyUseWidth => (EnergyUsePercent / 100.0) * MaxBarWidth;

        // Carbon Sequestration
        public double TotalCarbonSequestered
        {
            get => _totalCarbonSequestered;
            set => SetProperty(ref _totalCarbonSequestered, value);
        }

        public double ShelterbeltsSequestration
        {
            get => _shelterbeltsSequestration;
            set => SetProperty(ref _shelterbeltsSequestration, value);
        }

        public double PasturelandSequestration
        {
            get => _pasturelandSequestration;
            set => SetProperty(ref _pasturelandSequestration, value);
        }

        public double CroppedLandSequestration
        {
            get => _croppedLandSequestration;
            set => SetProperty(ref _croppedLandSequestration, value);
        }

        // Manure Management
        public string ManureStorageType
        {
            get => _manureStorageType;
            set => SetProperty(ref _manureStorageType, value);
        }

        public double TotalManureProduced
        {
            get => _totalManureProduced;
            set => SetProperty(ref _totalManureProduced, value);
        }

        public double ManureN2OEmissions
        {
            get => _manureN2OEmissions;
            set => SetProperty(ref _manureN2OEmissions, value);
        }

        public double ManureCH4Emissions
        {
            get => _manureCH4Emissions;
            set => SetProperty(ref _manureCH4Emissions, value);
        }

        // Key Findings and Recommendations
        public string KeyFinding1
        {
            get => _keyFinding1;
            set => SetProperty(ref _keyFinding1, value);
        }

        public string KeyFinding2
        {
            get => _keyFinding2;
            set => SetProperty(ref _keyFinding2, value);
        }

        public string KeyFinding3
        {
            get => _keyFinding3;
            set => SetProperty(ref _keyFinding3, value);
        }

        public string Recommendation1
        {
            get => _recommendation1;
            set => SetProperty(ref _recommendation1, value);
        }

        public string Recommendation2
        {
            get => _recommendation2;
            set => SetProperty(ref _recommendation2, value);
        }

        public string Recommendation3
        {
            get => _recommendation3;
            set => SetProperty(ref _recommendation3, value);
        }

        #endregion

        #region Methods

        private void InitializeCommands()
        {
            GoBackCommand = new DelegateCommand(OnGoBack);
        }

        private void OnGoBack()
        {
            // Navigation handled by parent view
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            
            // Load actual farm data here from StorageService or navigation parameters
            LoadFarmData();
        }

        private void LoadFarmData()
        {
            // TODO: Load real data from the active farm
            // This is placeholder data for demonstration
        }

        #endregion
    }
}
