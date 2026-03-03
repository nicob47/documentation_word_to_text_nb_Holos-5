using H.Core.Enumerations;
using H.Core.Factories.Climate;
using H.Core.Models;
using H.Core.Models.Climate;
using H.Core.Providers.Climate;
using H.Core.Services.Animals;
using Microsoft.Extensions.Logging;

namespace H.Core.Services.Climate
{
    /// <summary>
    /// Orchestrates operations for DailyClimateData and DailyClimateDto objects.
    /// - Creates and transfers data between domain models and DTOs using <see cref="ITransferService{TModelBase, TDto}"/>.
    /// - Applies unit conversions via configured transfer services.
    /// - Provides climate data operations for UI-bound workflows.
    /// </summary>
    public class ClimateService : IClimateService, IClimateProvider
    {
        #region Fields

        private readonly IDailyClimateDataFactory _dailyClimateDataFactory;
        private readonly ITransferService<DailyClimateData, DailyClimateDto> _climateTransferService;
        private readonly ILogger _logger;
        private readonly IClimateProvider _climateProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ClimateService"/>.
        /// </summary>
        /// <param name="dailyClimateDataFactory">Factory used to create climate-related DTOs and data objects.</param>
        /// <param name="climateTransferService">
        /// Transfer service that maps between <see cref="DailyClimateData"/> (domain) and <see cref="DailyClimateDto"/> (DTO),
        /// including unit conversions for UI binding and persistence.
        /// </param>
        /// <param name="logger">Logger for diagnostics.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
        public ClimateService(
            IDailyClimateDataFactory dailyClimateDataFactory,
            ITransferService<DailyClimateData, DailyClimateDto> climateTransferService,
            ILogger logger,
            IClimateProvider climateProvider)
        {
            if (climateProvider != null)
            {
                _climateProvider = climateProvider; 
            }
            else
            {
                throw new ArgumentNullException(nameof(climateProvider));
            }

            if (climateTransferService != null)
            {
                _climateTransferService = climateTransferService;
            }
            else
            {
                throw new ArgumentNullException(nameof(climateTransferService));
            }

            if (dailyClimateDataFactory != null)
            {
                _dailyClimateDataFactory = dailyClimateDataFactory;
            }
            else
            {
                throw new ArgumentNullException(nameof(dailyClimateDataFactory));
            }

            if (logger != null)
            {
                _logger = logger;
            }
            else
            {
                throw new ArgumentNullException(nameof(logger));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts a <see cref="DailyClimateData"/> domain object to a <see cref="DailyClimateDto"/> for UI binding.
        /// </summary>
        /// <param name="dailyClimateData">Domain model instance.</param>
        /// <returns>Mapped DTO instance.</returns>
        public DailyClimateDto TransferDailyClimateDataToDto(DailyClimateData dailyClimateData)
        {
            _logger?.LogDebug("Transferring DailyClimateData to DTO for year: {Year}", dailyClimateData?.Year);
            
            return _climateTransferService.TransferDomainObjectToDto(dailyClimateData!);
        }

        /// <summary>
        /// Applies values from a <see cref="DailyClimateDto"/> to an existing <see cref="DailyClimateData"/> domain object.
        /// </summary>
        /// <param name="dailyClimateDto">Source DTO bound to the UI.</param>
        /// <param name="dailyClimateData">Target domain model to update.</param>
        /// <returns>The updated <see cref="DailyClimateData"/>.</returns>
        public DailyClimateData TransferClimateDtoToSystem(DailyClimateDto dailyClimateDto, DailyClimateData dailyClimateData)
        {
            _logger?.LogDebug("Transferring DailyClimateDto to system for year: {Year}", dailyClimateDto?.Year);
            
            return _climateTransferService.TransferDtoToDomainObject(dailyClimateDto!, dailyClimateData);
        }

        /// <summary>
        /// Creates a new domain object from a DTO for adding to the system.
        /// </summary>
        /// <param name="dailyClimateDto">DTO used to create the new domain object.</param>
        /// <returns>New domain object instance.</returns>
        public DailyClimateData? CreateDataFromDto(DailyClimateDto? dailyClimateDto)
        {
            _logger?.LogDebug("Creating new DailyClimateData from DTO for year: {Year}", dailyClimateDto?.Year);

            if (dailyClimateDto == null)
            {
                _logger?.LogWarning("Cannot create DailyClimateData from null DTO");
                return null;
            }

            var newData = _dailyClimateDataFactory.CreateData(dailyClimateDto);
            
            _logger?.LogInformation("Successfully created new DailyClimateData for year: {Year}", newData.Year);
            
            return newData;
        }

        #endregion

        #region Private Methods

        #endregion

        #region Implementation of IClimateProvider

        public void OutputDailyClimateData(Farm farm, string outputPath)
        {
            _climateProvider.OutputDailyClimateData(farm, outputPath);
        }

        public ClimateData? Get(double latitude, double longitude, TimeFrame climateNormalTimeFrame)
        {
            return _climateProvider.Get(latitude, longitude, climateNormalTimeFrame);
        }

        public double GetMeanTemperatureForDay(Farm farm, DateTime dateTime)
        {
            return _climateProvider.GetMeanTemperatureForDay(farm, dateTime);
        }

        public double GetAnnualEvapotranspiration(Farm farm, DateTime dateTime)
        {
            return _climateProvider.GetAnnualEvapotranspiration(farm, dateTime);
        }

        public double GetAnnualPrecipitation(Farm farm, DateTime dateTime)
        {
            return _climateProvider.GetAnnualPrecipitation(farm, dateTime);
        }

        public double GetGrowingSeasonPrecipitation(Farm farm, DateTime dateTime)
        {
            return _climateProvider.GetGrowingSeasonPrecipitation(farm, dateTime);
        }

        public double GetGrowingSeasonEvapotranspiration(Farm farm, DateTime dateTime)
        {
            return _climateProvider.GetGrowingSeasonEvapotranspiration(farm, dateTime);
        }

        public double GetAnnualPrecipitation(Farm farm, int year)
        {
            return _climateProvider.GetAnnualPrecipitation(farm, year);
        }

        public double GetAnnualEvapotranspiration(Farm farm, int year)
        {
            return _climateProvider.GetAnnualEvapotranspiration(farm, year);
        }

        public double GetGrowingSeasonPrecipitation(Farm farm, int year)
        {
            return _climateProvider.GetGrowingSeasonPrecipitation(farm, year);
        }

        public double GetGrowingSeasonEvapotranspiration(Farm farm, int year)
        {
            return _climateProvider.GetGrowingSeasonEvapotranspiration(farm, year);
        }

        public ClimateData? Get(double latitude, double longitude, TimeFrame climateNormalTimeFrame, Farm farm)
        {
            return _climateProvider.Get(latitude, longitude, climateNormalTimeFrame, farm);
        }

        public double GetTotalPET(int year, double latitude, double longitude)
        {
            return _climateProvider.GetTotalPET(year, latitude, longitude);
        }

        public double GetTotalPPT(int year, double latitude, double longitude)
        {
            return _climateProvider.GetTotalPPT(year, latitude, longitude);
        }

        public double GetMonthlyPPT(int year, int startingDay, int endingDay, double latitude, double longitude)
        {
            return _climateProvider.GetMonthlyPPT(year, startingDay, endingDay, latitude, longitude);
        }

        #endregion
    }
}