using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.Animals.Beef;
using H.Core.Models.Animals.Sheep;
using H.Core.Models.LandManagement.Fields;
using H.Core.Providers;
using H.Core.Providers.Climate;
using H.Core.Providers.Soil;
using H.Core.Services;
using Moq;

#nullable disable

namespace H.Core.Test.Services
{
    [TestClass]
    public class FarmResultsService_NEWTests
    {
        private ClimateProvider _climateProvider = null!;
        private Mock<ISlcClimateProvider> _mockSlcClimateProvider = null!;
        private ISlcClimateProvider _slcClimateProviderMock = null!;
        private FarmResultsService_NEW _farmResultsService = null!;


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _mockSlcClimateProvider = new Mock<ISlcClimateProvider>();
            _slcClimateProviderMock = _mockSlcClimateProvider.Object;
            _climateProvider = new ClimateProvider(_slcClimateProviderMock);
            _farmResultsService = new FarmResultsService_NEW();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        // Test taken from original FarmResultService's tests (FarmResultsServiceTest.cs)

        [TestMethod]
        public void TestReplicateFarm()
        {
            #region Init Farm
            var geographicDataProvider = new GeographicDataProvider();
            geographicDataProvider.Initialize();

            var farmToReplicate = new Farm()
            {
                Name = "Farm 1",
                StageStates = new List<StageStateBase>()
                {
                    new FieldSystemDetailsStageState()
                    {
                        DetailsScreenViewCropViewItems = new ObservableCollection<CropViewItem>()
                        {
                            new CropViewItem()
                            {
                                Year = 1985,
                                CropType = CropType.Barley,
                                CropEconomicData =
                                {
                                    ExpectedMarketPrice = 0.88,
                                }
                            }
                        }
                    }
                },
                Longitude = -112,
                Latitude = 49,
                ClimateData = _climateProvider.Get(49, -112, TimeFrame.NineteenNinetyToTwoThousand),
                GeographicData = geographicDataProvider.GetGeographicalData(793011),
                PolygonId = 793011,
            };
            farmToReplicate.GeographicData.CustomYieldData = new List<CustomUserYieldData> { new CustomUserYieldData() };
            farmToReplicate.Components.Add(new SheepComponent());
            farmToReplicate.Components.Add(new FieldSystemComponent()
            {
                CropViewItems = new ObservableCollection<CropViewItem>()
                {
                    new CropViewItem()
                    {
                        Year = 1985,
                        CropType = CropType.Barley,
                        CropEconomicData =
                        {
                            ExpectedMarketPrice = 0.88,
                        }
                    }
                }
            });
            #endregion

            var result = _farmResultsService.ReplicateFarm(farmToReplicate);

            //GUID
            Assert.AreNotEqual(result.Guid, farmToReplicate.Guid);

            //Defaults
            farmToReplicate.Defaults.CarbonConcentration = 55;
            Assert.AreNotEqual(result.Defaults.CarbonConcentration, 55, "Assert that a copy of the defaults are made (they don't reference same object");

            //Stage States
            Assert.AreEqual(result.StageStates.OfType<FieldSystemDetailsStageState>().Single().DetailsScreenViewCropViewItems.Count, 1, "Assert that stage state was copied");

            //climate data
            Assert.AreEqual(result.ClimateData.TemperatureData.GetMeanAnnualTemperature(), farmToReplicate.ClimateData.TemperatureData.GetMeanAnnualTemperature());
            farmToReplicate.ClimateData = _climateProvider.Get(50, -105, TimeFrame.NineteenNinetyToTwoThousand);
            Assert.AreNotEqual(result.ClimateData.TemperatureData.GetMeanAnnualTemperature(), farmToReplicate.ClimateData.TemperatureData.GetMeanAnnualTemperature(), "Assert that climate data was copied");

            //DailyClimateData
            Assert.AreNotEqual(result.ClimateData.DailyClimateData[0].MeanDailyAirTemperature, farmToReplicate.ClimateData.DailyClimateData[0].MeanDailyAirTemperature);

            //latitude & Longitude
            Assert.AreEqual(result.Latitude, farmToReplicate.Latitude);
            farmToReplicate.Latitude = 11;
            farmToReplicate.Longitude = 11;
            Assert.AreNotEqual(result.Latitude, farmToReplicate.Latitude);
            Assert.AreNotEqual(result.Longitude, farmToReplicate.Longitude);

            //DefaultSoilData
            Assert.AreEqual(result.GeographicData.DefaultSoilData.BulkDensity, farmToReplicate.GeographicData.DefaultSoilData.BulkDensity);
            farmToReplicate.GeographicData.DefaultSoilData.BulkDensity = 22;
            Assert.AreNotEqual(result.GeographicData.DefaultSoilData.BulkDensity, farmToReplicate.GeographicData.DefaultSoilData.BulkDensity);

            //CustomUserYieldData
            Assert.AreEqual(result.GeographicData.CustomYieldData[0].Yield, farmToReplicate.GeographicData.CustomYieldData[0].Yield);
            farmToReplicate.GeographicData.CustomYieldData[0].Yield = 2323;
            Assert.AreNotEqual(result.GeographicData.CustomYieldData[0].Yield, farmToReplicate.GeographicData.CustomYieldData[0].Yield);

            //Components
            Assert.AreEqual(result.Components.Count, farmToReplicate.Components.Count);
            Assert.AreEqual(result.Components[0].ComponentCategory, farmToReplicate.Components[0].ComponentCategory);
            farmToReplicate.Components.Add(new CowCalfComponent());
            Assert.AreNotEqual(result.Components.Count, farmToReplicate.Components.Count);

            //economics
            //check the economics copied over correctly
            Assert.AreEqual(
                farmToReplicate.FieldSystemComponents.ElementAt(0).CropViewItems[0].CropEconomicData
                    .ExpectedMarketPrice,
                result.FieldSystemComponents.ElementAt(0).CropViewItems[0].CropEconomicData.ExpectedMarketPrice);
            result.FieldSystemComponents.ElementAt(0).CropViewItems[0].CropEconomicData.ExpectedMarketPrice = 99;
            //true copy won't affect the original
            Assert.AreNotEqual(
                farmToReplicate.FieldSystemComponents.ElementAt(0).CropViewItems[0].CropEconomicData
                    .ExpectedMarketPrice,
                result.FieldSystemComponents.ElementAt(0).CropViewItems[0].CropEconomicData.ExpectedMarketPrice);
        }
    }
}
