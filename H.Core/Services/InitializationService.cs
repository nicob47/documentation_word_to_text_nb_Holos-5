using H.Core.Models;
using H.Core.Providers.Climate;

namespace H.Core.Services
{
    public class InitializationService : IInitializationService
    {
        #region Fields

        private readonly IIndoorTemperatureProvider _indoorTemperatureProvider;

        #endregion

        #region Constructors

        public InitializationService()
        {
            _indoorTemperatureProvider = new Table_63_Indoor_Temperature_Provider();
        }

        #endregion

        #region Public Methods

        public void CheckInitialization(Farm farm)
        {
            if (farm is null)
            {
                return;
            }

            if (farm.DefaultSoilData is null)
            {
                return;
            }

            var soilData = farm.DefaultSoilData;

            if (farm.ClimateData is null)
            {
                return;
            }

            var climateData = farm.ClimateData;

            var barnTemperature = climateData.BarnTemperatureData;
            if (barnTemperature is null || barnTemperature.IsInitialized == false)
            {
                barnTemperature = _indoorTemperatureProvider.GetIndoorTemperature(soilData.Province);
                barnTemperature.IsInitialized = true;
            }
        } 

        #endregion
    }
}