using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using H.CLI.UserInput;
using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Providers;
using H.Core.Providers.Climate;
using H.Localization.Resources.Strings;

namespace H.CLI.FileAndDirectoryAccessors
{
    public class SettingsHandler
    {
        #region Fields

        private readonly DirectoryHandler _directoryHandler = new DirectoryHandler();
        private readonly UnitsOfMeasurementCalculator _unitsOfMeasurementCalculator = new UnitsOfMeasurementCalculator();
        private readonly SlcClimateDataProvider _slcClimateDataProvider = new SlcClimateDataProvider();
        private readonly IClimateProvider _climateProvider = new ClimateProvider(new SlcClimateDataProvider());

        public List<int> PolygonIDList { get; set; } = new List<int>();

        #endregion

        #region Public Methods

        public void InitializePolygonIDList(GeographicDataProvider geographicDataProvider)
        {
            PolygonIDList = geographicDataProvider.GetPolygonIdList().ToList();
        }

        /// <summary>
        /// Takes in a dictionary that corresponds to the key for the settings file and the value as a string as well as a reference to
        /// the applicationData being passed by referenced from the main program. I.E. any changes to applicationData in this function
        /// will be reflected in the main program. For every farm, we will set the defaults based on the User's settings file.
        /// </summary> 
        public void ApplySettingsFromUserFile(ref ApplicationData applicationData, ref Farm farm, Dictionary<string, string> userSettings)
        {
            var userDefaults = new Defaults();
            userDefaults.CarbonConcentration = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.KilogramPerKilogram, double.Parse(userSettings[AppStrings.Settings_CarbonConcentration]), false);

            // Climate parameter
            userDefaults.EmergenceDay = int.Parse(userSettings[AppStrings.Settings_EmergenceDay]);
            userDefaults.RipeningDay = int.Parse(userSettings[AppStrings.Settings_RipeningDay]);
            userDefaults.Variance = double.Parse(userSettings[AppStrings.Settings_Variance]);
            userDefaults.Alfa = double.Parse(userSettings[AppStrings.Settings_Alfa]);
            userDefaults.DecompositionMinimumTemperature = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.DegreesCelsius, double.Parse(userSettings[AppStrings.Settings_DecompositionMinimumTemperature]), false);
            userDefaults.DecompositionMaximumTemperature = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.DegreesCelsius, double.Parse(userSettings[AppStrings.Settings_DecompositionMaximumTemperature]), false);
            userDefaults.MoistureResponseFunctionAtSaturation = double.Parse(userSettings[AppStrings.Settings_MoistureResponseFunctionAtSaturation]);
            userDefaults.MoistureResponseFunctionAtWiltingPoint = double.Parse(userSettings[AppStrings.Settings_MoistureResponseFunctionAtWiltingPoint]);

            //Annuals
            userDefaults.PercentageOfProductReturnedToSoilForAnnuals = double.Parse(userSettings[AppStrings.Settings_PercentageOfProductReturnedToSoilForAnnuals]);
            userDefaults.PercentageOfStrawReturnedToSoilForAnnuals = double.Parse(userSettings[AppStrings.Settings_PercentageOfStrawReturnedToSoilForAnnuals]);
            userDefaults.PercentageOfRootsReturnedToSoilForAnnuals = double.Parse(userSettings[AppStrings.Settings_PercentageOfRootsReturnedToSoilForAnnuals]);

            //Silage Crops
            userDefaults.PercentageOfProductYieldReturnedToSoilForSilageCrops = double.Parse(userSettings[AppStrings.Settings_PercentageOfProductYieldReturnedToSoilForSilageCrops]);
            userDefaults.PercentageOfRootsReturnedToSoilForSilageCrops = double.Parse(userSettings[AppStrings.Settings_PercentageOfRootsReturnedToSoilForSilageCrops]);

            //Cover Crops
            userDefaults.PercentageOfProductYieldReturnedToSoilForCoverCrops = double.Parse(userSettings[AppStrings.Settings_PercentageOfProductYieldReturnedToSoilForCoverCrops]);
            userDefaults.PercentageOfProductYieldReturnedToSoilForCoverCropsForage = double.Parse(userSettings[AppStrings.Settings_PercentageOfProductYieldReturnedToSoilForCoverCropsForage]);
            userDefaults.PercentageOfProductYieldReturnedToSoilForCoverCropsProduce = double.Parse(userSettings[AppStrings.Settings_PercentageOfProductYieldReturnedToSoilForCoverCropsProduce]);
            userDefaults.PercentageOfStrawReturnedToSoilForCoverCrops = double.Parse(userSettings[AppStrings.Settings_PercentageOfStrawReturnedToSoilForCoverCrops]);
            userDefaults.PercetageOfRootsReturnedToSoilForCoverCrops = double.Parse(userSettings[AppStrings.Settings_PercentageOfRootsReturnedToSoilForCoverCrops]);

            //Root Crops
            userDefaults.PercentageOfProductReturnedToSoilForRootCrops = double.Parse(userSettings[AppStrings.Settings_PercentageOfProductReturnedToSoilForRootCrops]);
            userDefaults.PercentageOfStrawReturnedToSoilForRootCrops = double.Parse(userSettings[AppStrings.Settings_PercentageOfStrawReturnedToSoilForRootCrops]);

            //Perennial Crops
            userDefaults.PercentageOfProductReturnedToSoilForPerennials = double.Parse(userSettings[AppStrings.Settings_PercentageOfProductReturnedToSoilForPerennials]);
            userDefaults.PercentageOfRootsReturnedToSoilForPerennials = double.Parse(userSettings[AppStrings.Settings_PercentageOfRootsReturnedToSoilForPerennials]);

            //Rangeland
            //PercentageOfProductReturnedToSoilForRangelandDueToGrazingLoss = double.Parse(userSettings["Percentage Of Product Returned To Soil For Rangeland Due To Grazing Loss"]));

            userDefaults.PercentageOfProductReturnedToSoilForRangelandDueToHarvestLoss = double.Parse(userSettings[AppStrings.Settings_PercentageOfProductReturnedToSoilForRangelandDueToHarvestLoss]);
            userDefaults.PercentageOfRootsReturnedToSoilForRangeland = double.Parse(userSettings[AppStrings.Settings_PercentageOfRootsReturnedToSoilForRangeland]);

            //Fodder Corn
            userDefaults.PercentageOfProductReturnedToSoilForFodderCorn = double.Parse(userSettings[AppStrings.Settings_PercentageOfProductReturnedToSoilForFodderCorn]);
            userDefaults.PercentageOfRootsReturnedToSoilForFodderCorn = double.Parse(userSettings[AppStrings.Settings_PercentageOfRootsReturnedToSoilForFodderCorn]);

            userDefaults.CarbonModellingStrategy = (CarbonModellingStrategies)Enum.Parse(typeof(CarbonModellingStrategies), userSettings[AppStrings.Settings_CarbonModellingStrategy], true);
            userDefaults.DefaultRunInPeriod = int.Parse(userSettings[AppStrings.Settings_RunInPeriodYears]);

            // ICBM
            userDefaults.HumificationCoefficientAboveGround = double.Parse(userSettings[AppStrings.Settings_HumificationCoefficientAboveGround]);
            userDefaults.HumificationCoefficientBelowGround = double.Parse(userSettings[AppStrings.Settings_HumificationCoefficientBelowGround]);
            userDefaults.HumificationCoefficientManure = double.Parse(userSettings[AppStrings.Settings_HumificationCoefficientManure]);
            userDefaults.DecompositionRateConstantYoungPool = double.Parse(userSettings[AppStrings.Settings_DecompositionRateConstantYoungPool]);
            userDefaults.DecompositionRateConstantOldPool = double.Parse(userSettings[AppStrings.Settings_DecompositionRateConstantOldPool]);
            userDefaults.OldPoolCarbonN = double.Parse(userSettings[AppStrings.Settings_OldPoolCarbonN]);
            userDefaults.NORatio = double.Parse(userSettings[AppStrings.Settings_NORatio]);
            userDefaults.EmissionFactorForLeachingAndRunoff = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.KilogramsN2ONPerKilogramN, double.Parse(userSettings[AppStrings.Settings_EmissionFactorForLeachingAndRunOff]), false);
            userDefaults.EmissionFactorForVolatilization = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.KilogramsN2ONPerKilogramN, double.Parse(userSettings[AppStrings.Settings_EmissionFactorForVolatilization]), false);
            userDefaults.FractionOfNLostByVolatilization = double.Parse(userSettings[AppStrings.Settings_FractionOfNLostByVolatilization]);
            userDefaults.MicrobeDeath = double.Parse(userSettings[AppStrings.Settings_MicrobeDeath]);
            userDefaults.Denitrification = double.Parse(userSettings[AppStrings.Settings_Denitrification]);

            if (userSettings.ContainsKey(AppStrings.Settings_UseClimateParameterInsteadOfManagementFactor))
            {
                farm.Defaults.UseClimateParameterInsteadOfManagementFactor = bool.Parse(userSettings[AppStrings.Settings_UseClimateParameterInsteadOfManagementFactor]);
            }

            if (userSettings.ContainsKey(AppStrings.Settings_EnableCarbonModelling))
            {
                farm.EnableCarbonModelling = bool.Parse(userSettings[AppStrings.Settings_EnableCarbonModelling]);
            }

            if (userSettings.ContainsKey(AppStrings.Settings_YieldAssignmentMethod))
            {
                farm.YieldAssignmentMethod = (YieldAssignmentMethod)Enum.Parse(typeof(YieldAssignmentMethod), userSettings[AppStrings.Settings_YieldAssignmentMethod], true);
            }


            farm.Defaults = userDefaults;

            // This setting might not exist in old settings files
            if (userSettings.ContainsKey(AppStrings.Settings_ClimateFilename))
            {
                farm.ClimateDataFileName = userSettings[AppStrings.Settings_ClimateFilename];
            }

            if (userSettings.ContainsKey(AppStrings.Settings_PolygonNumber))
            {
                farm.PolygonId = int.Parse(userSettings[AppStrings.Settings_PolygonNumber]);
            }

            if (userSettings.ContainsKey(AppStrings.Settings_Latitude) && userSettings.ContainsKey(AppStrings.Settings_Longitude))
            {
                farm.Longitude = double.Parse(userSettings[AppStrings.Settings_Longitude]);
                farm.Latitude = double.Parse(userSettings[AppStrings.Settings_Latitude]);
            }

            this.ApplyClimateData(userSettings, farm);

            var userGeographicData = new GeographicData()
            {
                DefaultSoilData =
                {
                    Province = (Province)Enum.Parse(typeof(Province), userSettings[AppStrings.Settings_Province], true),
                   YearOfObservation = int.Parse(userSettings[AppStrings.Settings_YearOfObservation]),
                   EcodistrictId = int.Parse(userSettings[AppStrings.Settings_EcodistrictID]),
                   SoilGreatGroup =  (SoilGreatGroupType)Enum.Parse(typeof(SoilGreatGroupType), userSettings[AppStrings.Settings_SoilGreatGroup], true),
                   BulkDensity = double.Parse(userSettings[AppStrings.Settings_BulkDensity]),
                   SoilTexture = (SoilTexture)Enum.Parse(typeof(SoilTexture), userSettings[AppStrings.Settings_SoilTexture], true),
                   SoilPh = double.Parse(userSettings[AppStrings.Settings_SoilPh]),
                   TopLayerThickness = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters, double.Parse(userSettings[AppStrings.Settings_TopLayerThickness]), false),
                   ProportionOfSandInSoil = double.Parse(userSettings[AppStrings.Settings_ProportionOfSandInSoil]),
                   ProportionOfClayInSoil = double.Parse(userSettings[AppStrings.Settings_ProportionOfClayInSoil]),
                   ProportionOfSoilOrganicCarbon = double.Parse(userSettings[AppStrings.Settings_ProportionOfSoilOrganicCarbon]),
                },
            };

            farm.GeographicData = userGeographicData;

            if (userSettings.ContainsKey(AppStrings.Settings_SoilFunctionalCategory))
            {
                farm.GeographicData.DefaultSoilData.SoilFunctionalCategory = (SoilFunctionalCategory)Enum.Parse(typeof(SoilFunctionalCategory), userSettings[AppStrings.Settings_SoilFunctionalCategory], true);
            }
        }

        public void GetUserSettingsMenuChoice(string farmDirectoryPath, GeographicDataProvider geographicDataProvider)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            var farmName = Path.GetFileName(farmDirectoryPath);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(AppStrings.Prompt_UserForPolygonSelection, farmName);
            Console.WriteLine(Environment.NewLine);

            int userChosenMenuNumber;
            string userMenuChoice;

            do
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(String.Format(AppStrings.Prompt_SelectPolygonOptions + Environment.NewLine +
                                  AppStrings.Error_ExitAndRunHolosForFarmLocation + Environment.NewLine +
                                  AppStrings.Label_CreateSettingsFileBasedOnPolygon + Environment.NewLine +
                                  AppStrings.Label_CreateDefaultLethbridgeSettingsFile + Environment.NewLine +
                                  AppStrings.Status_SettingsFileWillBeCreatedHere + Environment.NewLine +
                                  AppStrings.Prompt_EnterChoice, farmDirectoryPath));
                userMenuChoice = Console.ReadLine();
                int.TryParse(userMenuChoice, out userChosenMenuNumber);
            } while (userChosenMenuNumber < 0 || userChosenMenuNumber > 3 || !int.TryParse(userMenuChoice, out userChosenMenuNumber));

            this.ExecuteUserChoice(userChosenMenuNumber, geographicDataProvider, farmDirectoryPath);
        }

        public void ExecuteUserChoice(int userChosenMenuNumber, GeographicDataProvider geographicDataProvider, string farmDirectoryPath)
        {
            if (userChosenMenuNumber == 1)
            {
                //exit the program
                Environment.Exit(1);
            }

            ///////////Get Polygon ID From User///////////////////////////
            if (userChosenMenuNumber == 2)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(AppStrings.Prompt_ToEnterPolygonID);
                var polygonIDString = Console.ReadLine();
                var polygonID = int.Parse(polygonIDString);

                if (PolygonIDList.Contains(polygonID))
                {
                    var geographicData = geographicDataProvider.GetGeographicalData(polygonID);
                    var farm = new Farm() { PolygonId = polygonID, GeographicData = geographicData };
                    _directoryHandler.GenerateGlobalSettingsFile(farmDirectoryPath, farm);
                }

                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(String.Format(AppStrings.Error_NotAValidPolygonID, polygonIDString));
                    throw new Exception("Not A Valid Polygon ID");
                }

                Console.ResetColor();

            }

            ///////////Create Default Lethbridge Settings File///////////////////////////
            if (userChosenMenuNumber == 3)
            {
                Console.WriteLine(String.Format(AppStrings.Status_CreatingDefaultLethbridgeGeographicData, farmDirectoryPath));

                var lethbridgePolygonID = 793006;
                var lethbridgeGeographicData = geographicDataProvider.GetGeographicalData(lethbridgePolygonID);
                var climateData = _slcClimateDataProvider.GetClimateData(lethbridgePolygonID, TimeFrame.NineteenNinetyToTwoThousandSeventeen);

                _directoryHandler.GenerateGlobalSettingsFile(farmDirectoryPath, new Farm() { PolygonId = lethbridgePolygonID, GeographicData = lethbridgeGeographicData, ClimateData = climateData });
            }
        }

        public void ApplyClimateData(Dictionary<string, string> userSettings, Farm farm)
        {
            if (userSettings.ContainsKey(AppStrings.Settings_ClimateDataAcquisition))
            {
                farm.ClimateAcquisition = farm.ClimateAcquisitionStringToEnum(userSettings[AppStrings.Settings_ClimateDataAcquisition]);
            }

            switch (farm.ClimateAcquisition)
            {
                case Farm.ChosenClimateAcquisition.Custom:
                {
                    farm.ClimateData = new ClimateData()
                    {
                        PrecipitationData =
                        {
                            January = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_JanuaryPrecipitation]), false),
                            February = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_FebruaryPrecipitation]), false),
                            March = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_MarchPrecipitation]), false),
                            April = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_AprilPrecipitation]), false),
                            May = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_MayPrecipitation]), false),
                            June = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_JunePrecipitation]), false),
                            July = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_JulyPrecipitation]), false),
                            August = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_AugustPrecipitation]), false),
                            September = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_SeptemberPrecipitation]),
                                false),
                            October = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_OctoberPrecipitation]), false),
                            November = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_NovemberPrecipitation]), false),
                            December = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem, MetricUnitsOfMeasurement.Millimeters,
                                double.Parse(userSettings[AppStrings.Settings_DecemberPrecipitation]), false),
                        },

                        EvapotranspirationData =
                        {
                            January = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_JanuaryPotentialEvapotranspiration]),
                                false),
                            February = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_FebruaryPotentialEvapotranspiration]),
                                false),
                            March = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_MarchPotentialEvapotranspiration]),
                                false),
                            April = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_AprilPotentialEvapotranspiration]),
                                false),
                            May = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_MayPotentialEvapotranspiration]), false),
                            June = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_JunePotentialEvapotranspiration]),
                                false),
                            July = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_JulyPotentialEvapotranspiration]),
                                false),
                            August = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_AugustPotentialEvapotranspiration]),
                                false),
                            September = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_SeptemberPotentialEvapotranspiration]),
                                false),
                            October = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_OctoberPotentialEvapotranspiration]),
                                false),
                            November = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_NovemberPotentialEvapotranspiration]),
                                false),
                            December = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.MillimetersPerYear,
                                double.Parse(
                                    userSettings[AppStrings.Settings_DecemberPotentialEvapotranspiration]),
                                false),
                        },

                        TemperatureData =
                        {
                            January = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_JanuaryMeanTemperature]),
                                false),
                            February = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_FebruaryMeanTemperature]),
                                false),
                            March = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_MarchMeanTemperature]), false),
                            April = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_AprilMeanTemperature]), false),
                            May = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_MayMeanTemperature]), false),
                            June = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_JuneMeanTemperature]), false),
                            July = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_JulyMeanTemperature]), false),
                            August = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_AugustMeanTemperature]), false),
                            September = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_SeptemberMeanTemperature]),
                                false),
                            October = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_OctoberMeanTemperature]),
                                false),
                            November = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_NovemberMeanTemperature]),
                                false),
                            December = _unitsOfMeasurementCalculator.GetUnitsOfMeasurementValue(
                                CLIUnitsOfMeasurementConstants.measurementSystem,
                                MetricUnitsOfMeasurement.DegreesCelsius,
                                double.Parse(userSettings[AppStrings.Settings_DecemberMeanTemperature]),
                                false),
                        },
                    };
                        break;
                }

                default:
                    farm.ClimateData = _climateProvider.Get(farm.Latitude, farm.Longitude, farm.Defaults.TimeFrame);
                    break;
            }
        }

        #endregion
    }
}

