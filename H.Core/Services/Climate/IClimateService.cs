using H.Core.Models.Climate;
using H.Core.Providers.Climate;

namespace H.Core.Services.Climate
{
    /// <summary>
    /// Orchestrates operations for DailyClimateData and DailyClimateDto objects.
    /// - Creates and transfers data between domain models and DTOs.
    /// - Applies unit conversions via configured transfer services.
    /// - Provides climate data operations for UI-bound workflows.
    /// </summary>
    public interface IClimateService : IClimateProvider
    {
        /// <summary>
        /// Converts a <see cref="DailyClimateData"/> domain object to a <see cref="DailyClimateDto"/> for UI binding.
        /// </summary>
        /// <param name="dailyClimateData">Domain model instance.</param>
        /// <returns>Mapped DTO instance.</returns>
        DailyClimateDto TransferDailyClimateDataToDto(DailyClimateData dailyClimateData);

        /// <summary>
        /// Applies values from a <see cref="DailyClimateDto"/> to an existing <see cref="DailyClimateData"/> domain object.
        /// </summary>
        /// <param name="dailyClimateDto">Source DTO bound to the UI.</param>
        /// <param name="dailyClimateData">Target domain model to update.</param>
        /// <returns>The updated <see cref="DailyClimateData"/>.</returns>
        DailyClimateData TransferClimateDtoToSystem(DailyClimateDto dailyClimateDto, DailyClimateData dailyClimateData);

        /// <summary>
        /// Creates a new domain object from a DTO for adding to the system.
        /// </summary>
        /// <param name="dailyClimateDto">DTO used to create the new domain object.</param>
        /// <returns>New domain object instance.</returns>
        DailyClimateData? CreateDataFromDto(DailyClimateDto? dailyClimateDto);
    }
}