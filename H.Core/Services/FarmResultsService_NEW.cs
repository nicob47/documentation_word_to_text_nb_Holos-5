using H.Core.Mappers;
using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Models.LandManagement.Fields;
using H.Core.Providers;
using H.Core.Providers.Animals;
using H.Core.Providers.Climate;
using H.Core.Providers.Evapotranspiration;
using H.Core.Providers.Feed;
using H.Core.Providers.Precipitation;
using H.Core.Providers.Soil;
using H.Core.Providers.Temperature;

namespace H.Core.Services
{
    public class FarmResultsService_NEW : IFarmResultsService_NEW
    {
        #region Fields


        private readonly IFieldComponentHelper _fieldComponentHelper = new FieldComponentHelper();
        private readonly IAnimalComponentHelper _animalComponentHelper = new AnimalComponentHelper();

        #endregion

        #region Constructors

        public FarmResultsService_NEW()
        {
        }

        #endregion

        #region Public Methods

        // TODO: Add support for AD components
        public Farm ReplicateFarm(Farm farm)
        {
            var replicatedFarm = new Farm();

            PropertyMapper.CopyTo(farm, replicatedFarm);

            // PropertyMapper copies all matching properties including Guid and reference-type sub-objects.
            // Reset Guid so the replicated farm has its own unique identity.
            replicatedFarm.Guid = Guid.NewGuid();

            // Reset reference-type properties to new instances so they are not shared with the source farm.
            // PropertyMapper.CopyTo above copies these references, so we must create fresh objects before copying into them.
            replicatedFarm.Defaults = new Defaults();
            replicatedFarm.ClimateData = new ClimateData();
            replicatedFarm.GeographicData = new GeographicData();

            PropertyMapper.CopyTo(farm.Defaults, replicatedFarm.Defaults);
            PropertyMapper.CopyTo(farm.ClimateData, replicatedFarm.ClimateData);
            PropertyMapper.CopyTo(farm.GeographicData, replicatedFarm.GeographicData);

            // Reset DefaultSoilData so it is not shared with the source GeographicData (CopyTo copied the reference).
            replicatedFarm.GeographicData.DefaultSoilData = new SoilData();
            PropertyMapper.CopyTo(farm.GeographicData.DefaultSoilData, replicatedFarm.GeographicData.DefaultSoilData);

            replicatedFarm.Name = farm.Name;

            #region Animal Components

            foreach (var animalComponent in farm.AnimalComponents.Cast<AnimalComponentBase>())
            {
                var replicatedAnimalComponent = _animalComponentHelper.ReplicateAnimalComponent(animalComponent);

                replicatedFarm.Components.Add(replicatedAnimalComponent);
            }

            #endregion

            #region FieldSystemComponents

            foreach (var fieldSystemComponent in farm.FieldSystemComponents)
            {
                var replicatedFieldSystemComponent = _fieldComponentHelper.Replicate(fieldSystemComponent);

                replicatedFarm.Components.Add(replicatedFieldSystemComponent);
            }

            #endregion

            #region DailyClimateData

            foreach (var dailyClimateData in farm.ClimateData.DailyClimateData)
            {
                var replicatedDailyClimateData = new DailyClimateData();
                PropertyMapper.CopyTo(dailyClimateData, replicatedDailyClimateData);
                replicatedFarm.ClimateData.DailyClimateData.Add(dailyClimateData);
            }

            #endregion

            #region SoilData and CustomYieldData

            foreach (var soilData in farm.GeographicData.SoilDataForAllComponentsWithinPolygon)
            {
                var replicatedSoilData = new SoilData();
                PropertyMapper.CopyTo(soilData, replicatedSoilData);
                replicatedFarm.GeographicData.SoilDataForAllComponentsWithinPolygon.Add(replicatedSoilData);
            }

            foreach (var customYieldData in farm.GeographicData.CustomYieldData)
            {
                var replicatedCustomYieldData = new CustomUserYieldData();
                PropertyMapper.CopyTo(customYieldData, replicatedCustomYieldData);
                replicatedFarm.GeographicData.CustomYieldData.Add(replicatedCustomYieldData);
            }

            #endregion

            #region StageStates

            foreach (var fieldSystemDetailsStageState in farm.StageStates.OfType<FieldSystemDetailsStageState>().ToList())
            {
                var stageState = new FieldSystemDetailsStageState();
                replicatedFarm.StageStates.Add(stageState);

                foreach (var detailsScreenViewCropViewItem in fieldSystemDetailsStageState.DetailsScreenViewCropViewItems)
                {
                    var viewItem = new CropViewItem();

                    PropertyMapper.CopyTo(detailsScreenViewCropViewItem, viewItem);

                    stageState.DetailsScreenViewCropViewItems.Add(viewItem);
                }
            }

            #endregion

            return replicatedFarm;
        }

        #endregion
    }
}
