using System.ComponentModel;
using H.Core.CustomAttributes;
using H.Core.Enumerations;
using H.Core.Models.Animals;

namespace H.Core.Factories.Animals
{
    /// <summary>
    /// A class used to validate input as it relates to a <see cref="IManagementPeriodDto"/>. This class is used to valid input before any input is transferred to the <see cref="ManagementPeriod"/>
    /// </summary>
    public class ManagementPeriodDto : DtoBase, IManagementPeriodDto
    {
        #region Fields

        private DateTime _start;
        private DateTime _end;
        private int _numberOfDays;
        private int _numberOfAnimals = 20;
        private bool _isRecalculating;
        
        private double _energyRequiredForMilk;
        private double _energyRequiredForWool;
        private double _startWeight;
        private double _endWeight;
        private double _periodDailyGain;
        private double _milkProduction;
        private double _milkFatContent = 3.9;
        private double _milkProteinContentAsPercentage = 3.2;
        private double _woolProduction;
        private double _gainCoefficientA;
        private double _gainCoefficientB;
        private double _liveWeightChangeOfPregnantAnimal;
        private double _liveWeightOfYoungAtWeaningAge;
        private double _liveWeightOfYoungAtBirth;
        private ManureStateType _manureStateType = ManureStateType.NotSelected;
        private HousingType _housingType = HousingType.NotSelected;

        // Housing-related fields (from HousingDetails)
        private BeddingMaterialType _beddingMaterialType = BeddingMaterialType.None;
        private double _userDefinedBeddingRate;
        private double _activityCoefficientOfFeedingSituation;
        private double _baselineMaintenanceCoefficient;

        // Manure-related fields (from ManureDetails)
        private double _methaneConversionFactor;
        private double _volatilizationFraction;
        private double _n2ODirectEmissionFactor;
        private double _leachingFraction;
        private double _emissionFactorVolatilization;
        private double _emissionFactorLeaching;
        private double _ashContentOfManure = 8;
        private double _methaneProducingCapacityOfManure;
        private double _volatileSolidExcretion;
        private double _fractionOfNitrogenInManure;
        private double _fractionOfCarbonInManure;
        private double _fractionOfPhosphorusInManure;

        // Diet-related fields
        private DietAdditiveType _dietAdditiveType = DietAdditiveType.None;
        private DietType _selectedDietType = DietType.None;
        private double _crudeProtein;
        private double _forage;
        private double _totalDigestibleNutrient;
        private double _dailyDryMatterFeedIntakeOfFeed;

        #endregion

        #region Constructors

        public ManagementPeriodDto()
        {
            this.PropertyChanged += OnPropertyChanged;
        }

        #endregion

        #region Properties

        public DateTime Start
        {
            get => _start;
            set => SetProperty(ref _start, value);
        }

        public DateTime End
        {
            get => _end;
            set => SetProperty(ref _end, value);
        }

        public int NumberOfDays
        {
            get => _numberOfDays;
            set => SetProperty(ref _numberOfDays, value);
        }

        /// <summary>
        /// The total number of animals in this management period.
        /// </summary>
        public int NumberOfAnimals
        {
            get => _numberOfAnimals;
            set => SetProperty(ref _numberOfAnimals, value);
        }

        /// <summary>
        /// Energy required to produce 1 kg of milk.
        /// 
        /// MJ kg^-1
        /// </summary>
        [Units(MetricUnitsOfMeasurement.MegaJoulesPerKilogram)]
        public double EnergyRequiredForMilk
        {
            get => _energyRequiredForMilk;
            set => SetProperty(ref _energyRequiredForMilk, value);
        }

        /// <summary>
        /// MJ kg^-1
        /// </summary>
        [Units(MetricUnitsOfMeasurement.MegaJoulesPerKilogram)]
        public double EnergyRequiredForWool
        {
            get => _energyRequiredForWool;
            set => SetProperty(ref _energyRequiredForWool, value);
        }

        /// <summary>
        /// Start weight of animals (kg)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Kilograms)]
        public double StartWeight
        {
            get => _startWeight;
            set => SetProperty(ref _startWeight, value);
        }

        /// <summary>
        /// End weight of animals (kg)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Kilograms)]
        public double EndWeight
        {
            get => _endWeight;
            set => SetProperty(ref _endWeight, value);
        }

        /// <summary>
        /// The daily gain of the animals (kg head-1 day-1)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Kilograms)]
        public double PeriodDailyGain
        {
            get => _periodDailyGain;
            set => SetProperty(ref _periodDailyGain, value);
        }

        /// <summary>
        /// Milk produced per day (kg head⁻¹ day⁻¹)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Kilograms)]
        public double MilkProduction
        {
            get => _milkProduction;
            set => SetProperty(ref _milkProduction, value);
        }

        /// <summary>
        /// Fat content of milk (%)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Percentage)]
        public double MilkFatContent
        {
            get => _milkFatContent;
            set => SetProperty(ref _milkFatContent, value);
        }

        /// <summary>
        /// Protein content of milk (%)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Percentage)]
        public double MilkProteinContentAsPercentage
        {
            get => _milkProteinContentAsPercentage;
            set => SetProperty(ref _milkProteinContentAsPercentage, value);
        }

        /// <summary>
        /// Wool produced per year (kg year^-1)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Kilograms)]
        public double WoolProduction
        {
            get => _woolProduction;
            set => SetProperty(ref _woolProduction, value);
        }

        /// <summary>
        /// MJ kg^-1
        /// </summary>
        [Units(MetricUnitsOfMeasurement.MegaJoulesPerKilogram)]
        public double GainCoefficientA
        {
            get => _gainCoefficientA;
            set => SetProperty(ref _gainCoefficientA, value);
        }

        /// <summary>
        /// MJ kg^-2
        /// </summary>
        [Units(MetricUnitsOfMeasurement.MegaJoulesPerKilogramSquared)]
        public double GainCoefficientB
        {
            get => _gainCoefficientB;
            set => SetProperty(ref _gainCoefficientB, value);
        }

        /// <summary>
        /// (kg head^-1)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Kilograms)]
        public double LiveWeightChangeOfPregnantAnimal
        {
            get => _liveWeightChangeOfPregnantAnimal;
            set => SetProperty(ref _liveWeightChangeOfPregnantAnimal, value);
        }

        /// <summary>
        /// (kg head^-1)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Kilograms)]
        public double LiveWeightOfYoungAtWeaningAge
        {
            get => _liveWeightOfYoungAtWeaningAge;
            set => SetProperty(ref _liveWeightOfYoungAtWeaningAge, value);
        }

        /// <summary>
        /// (kg head ^-1)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Kilograms)]
        public double LiveWeightOfYoungAtBirth
        {
            get => _liveWeightOfYoungAtBirth;
            set => SetProperty(ref _liveWeightOfYoungAtBirth, value);
        }

        /// <summary>
        /// The manure storage/handling system being used for this management period.
        /// </summary>
        public ManureStateType ManureStateType
        {
            get => _manureStateType;
            set => SetProperty(ref _manureStateType, value);
        }

        /// <summary>
        /// The type of housing facility used during this management period.
        /// </summary>
        public HousingType HousingType
        {
            get => _housingType;
            set => SetProperty(ref _housingType, value);
        }

        // ─── Housing-related properties (from HousingDetails) ───

        /// <summary>
        /// The type of bedding material used in the housing facility.
        /// </summary>
        public BeddingMaterialType BeddingMaterialType
        {
            get => _beddingMaterialType;
            set => SetProperty(ref _beddingMaterialType, value);
        }

        /// <summary>
        /// User-defined bedding application rate (kg head⁻¹ day⁻¹)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Kilograms)]
        public double UserDefinedBeddingRate
        {
            get => _userDefinedBeddingRate;
            set => SetProperty(ref _userDefinedBeddingRate, value);
        }

        /// <summary>
        /// Activity coefficient for the feeding situation (dimensionless)
        /// </summary>
        public double ActivityCoefficientOfFeedingSituation
        {
            get => _activityCoefficientOfFeedingSituation;
            set => SetProperty(ref _activityCoefficientOfFeedingSituation, value);
        }

        /// <summary>
        /// Baseline maintenance coefficient (MJ kg⁻¹ day⁻¹)
        /// </summary>
        public double BaselineMaintenanceCoefficient
        {
            get => _baselineMaintenanceCoefficient;
            set => SetProperty(ref _baselineMaintenanceCoefficient, value);
        }

        // ─── Manure-related properties (from ManureDetails) ───

        /// <summary>
        /// Methane conversion factor for the manure management system (dimensionless, 0-1)
        /// </summary>
        public double MethaneConversionFactor
        {
            get => _methaneConversionFactor;
            set => SetProperty(ref _methaneConversionFactor, value);
        }

        /// <summary>
        /// Fraction of nitrogen lost through volatilization (dimensionless, 0-1)
        /// </summary>
        public double VolatilizationFraction
        {
            get => _volatilizationFraction;
            set => SetProperty(ref _volatilizationFraction, value);
        }

        /// <summary>
        /// Direct N₂O emission factor for the manure management system (dimensionless)
        /// </summary>
        public double N2ODirectEmissionFactor
        {
            get => _n2ODirectEmissionFactor;
            set => SetProperty(ref _n2ODirectEmissionFactor, value);
        }

        /// <summary>
        /// Fraction of nitrogen lost through leaching (dimensionless, 0-1)
        /// </summary>
        public double LeachingFraction
        {
            get => _leachingFraction;
            set => SetProperty(ref _leachingFraction, value);
        }

        /// <summary>
        /// Emission factor for volatilization (kg N₂O-N per kg N)
        /// </summary>
        public double EmissionFactorVolatilization
        {
            get => _emissionFactorVolatilization;
            set => SetProperty(ref _emissionFactorVolatilization, value);
        }

        /// <summary>
        /// Emission factor for leaching (kg N₂O-N per kg N)
        /// </summary>
        public double EmissionFactorLeaching
        {
            get => _emissionFactorLeaching;
            set => SetProperty(ref _emissionFactorLeaching, value);
        }

        /// <summary>
        /// Ash content of manure (%)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Percentage)]
        public double AshContentOfManure
        {
            get => _ashContentOfManure;
            set => SetProperty(ref _ashContentOfManure, value);
        }

        /// <summary>
        /// Maximum methane producing capacity of manure (m³ CH₄ kg⁻¹ VS)
        /// </summary>
        public double MethaneProducingCapacityOfManure
        {
            get => _methaneProducingCapacityOfManure;
            set => SetProperty(ref _methaneProducingCapacityOfManure, value);
        }

        /// <summary>
        /// Volatile solid excretion rate (kg kg⁻¹)
        /// </summary>
        public double VolatileSolidExcretion
        {
            get => _volatileSolidExcretion;
            set => SetProperty(ref _volatileSolidExcretion, value);
        }

        /// <summary>
        /// Fraction of nitrogen in manure (%)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Percentage)]
        public double FractionOfNitrogenInManure
        {
            get => _fractionOfNitrogenInManure;
            set => SetProperty(ref _fractionOfNitrogenInManure, value);
        }

        /// <summary>
        /// Fraction of carbon in manure (%)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Percentage)]
        public double FractionOfCarbonInManure
        {
            get => _fractionOfCarbonInManure;
            set => SetProperty(ref _fractionOfCarbonInManure, value);
        }

        /// <summary>
        /// Fraction of phosphorus in manure (%)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Percentage)]
        public double FractionOfPhosphorusInManure
        {
            get => _fractionOfPhosphorusInManure;
            set => SetProperty(ref _fractionOfPhosphorusInManure, value);
        }

        // ─── Diet-related properties ───

        /// <summary>
        /// The type of diet additive used during this management period.
        /// </summary>
        public DietAdditiveType DietAdditiveType
        {
            get => _dietAdditiveType;
            set => SetProperty(ref _dietAdditiveType, value);
        }

        /// <summary>
        /// The selected predefined diet type for this management period.
        /// </summary>
        public DietType SelectedDietType
        {
            get => _selectedDietType;
            set => SetProperty(ref _selectedDietType, value);
        }

        /// <summary>
        /// Crude protein content of the selected diet (%)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Percentage)]
        public double CrudeProtein
        {
            get => _crudeProtein;
            set => SetProperty(ref _crudeProtein, value);
        }

        /// <summary>
        /// Forage content of the selected diet (%)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Percentage)]
        public double Forage
        {
            get => _forage;
            set => SetProperty(ref _forage, value);
        }

        /// <summary>
        /// Total digestible nutrient content of the selected diet (%)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Percentage)]
        public double TotalDigestibleNutrient
        {
            get => _totalDigestibleNutrient;
            set => SetProperty(ref _totalDigestibleNutrient, value);
        }

        /// <summary>
        /// Daily dry matter feed intake (kg head⁻¹ day⁻¹)
        /// </summary>
        [Units(MetricUnitsOfMeasurement.Kilograms)]
        public double DailyDryMatterFeedIntakeOfFeed
        {
            get => _dailyDryMatterFeedIntakeOfFeed;
            set => SetProperty(ref _dailyDryMatterFeedIntakeOfFeed, value);
        }

        #endregion

        #region Private Methods

        private void ValidatePeriodName()
        {
            if (string.IsNullOrEmpty(Name))
            {
                AddError(nameof(Name), H.Core.Properties.Resources.ErrorNameCannotBeEmpty);
            }
            else
            {
                RemoveError(nameof(Name));
            }
        }

        private void ValidateStart()
        {
            if ((Start >= End && End != default) || Start == default)
            {
                AddError(nameof(Start), H.Core.Properties.Resources.ErrorStartDate);
            }
            else
            {
                RemoveError(nameof(Start));
            }
        }

        private void ValidateEnd()
        {
            if ((End <= Start && Start != default) || End == default)
            {
                AddError(nameof(End), H.Core.Properties.Resources.ErrorEndDate);
            }
            else
            {
                RemoveError(nameof(End));
            }
        }

        private void ValidateNumberOfDays()
        {
            if (NumberOfDays <= 0)
            {
                AddError(nameof(NumberOfDays), H.Core.Properties.Resources.ErrorNumberOfDaysMustBeGreaterThanZero);
            }
            else
            {
                RemoveError(nameof(NumberOfDays));
            }
        }

        private void ValidateMilkFatContent()
        {
            var key = nameof(MilkFatContent);
            
            if (MilkFatContent < 0)
            {
                AddError(key, "Milk fat content cannot be negative");
            }
            else if (MilkFatContent > 10)
            {
                AddError(key, "Milk fat content cannot exceed 10%");
            }
            else
            {
                RemoveError(key);
            }
        }

        private void ValidateMilkProteinContent()
        {
            var key = nameof(MilkProteinContentAsPercentage);
            
            if (MilkProteinContentAsPercentage < 0)
            {
                AddError(key, "Milk protein content cannot be negative");
            }
            else if (MilkProteinContentAsPercentage > 10)
            {
                AddError(key, "Milk protein content cannot exceed 10%");
            }
            else
            {
                RemoveError(key);
            }
        }

        private void RecalculateNumberOfDays()
        {
            if (_isRecalculating) return;

            try
            {
                _isRecalculating = true;
                if (Start != default && End != default && End > Start)
                {
                    NumberOfDays = (End - Start).Days;
                }
            }
            finally
            {
                _isRecalculating = false;
            }
        }

        private void RecalculateEndDate()
        {
            if (_isRecalculating) return;

            try
            {
                _isRecalculating = true;
                if (Start != default && NumberOfDays > 0)
                {
                    End = Start.AddDays(NumberOfDays);
                }
            }
            finally
            {
                _isRecalculating = false;
            }
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != null)
            {
                if (e.PropertyName.Equals(nameof(Name)))
                {
                    this.ValidatePeriodName();
                }
                else if (e.PropertyName.Equals(nameof(NumberOfDays)))
                {
                    this.ValidateNumberOfDays();
                    this.RecalculateEndDate();
                }
                else if (e.PropertyName.Equals(nameof(Start)))
                {
                    this.ValidateStart();
                    this.ValidateEnd();
                    this.RecalculateNumberOfDays();
                }
                else if (e.PropertyName.Equals(nameof(End)))
                {
                    this.ValidateEnd();
                    this.ValidateStart();
                    this.RecalculateNumberOfDays();
                }
                else if (e.PropertyName.Equals(nameof(MilkFatContent)))
                {
                    this.ValidateMilkFatContent();
                }
                else if (e.PropertyName.Equals(nameof(MilkProteinContentAsPercentage)))
                {
                    this.ValidateMilkProteinContent();
                }
            }
        }

        #endregion
    }
}
