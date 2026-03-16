using H.Core.Factories.Animals;
using H.Core.Factories.Animals.Dairy;
using H.Core.Factories.Crops;
using H.Core.Factories.Fields;
using H.Core.Factories.Rotations;
using H.Core.Mappers;
using H.Core.Models.Animals;
using H.Core.Models.Animals.Dairy;
using H.Core.Models.Climate;
using H.Core.Models.LandManagement.Fields;
using H.Core.Models.LandManagement.Rotation;
using H.Core.Providers.Climate;
using H.Core.Providers.Feed;
using Prism.Ioc;

namespace H.Avalonia.Infrastructure.MapperServices;

/// <summary>
/// Service responsible for configuring and registering IModelMapper implementations with the dependency injection container.
/// </summary>
public class MapperRegistrationService
{
    /// <summary>
    /// Configures and registers all IModelMapper implementations with the container.
    /// </summary>
    /// <param name="containerRegistry">The Prism container registry for dependency injection.</param>
    public void RegisterMappers(IContainerRegistry containerRegistry)
    {
        // Crop mappers
        RegisterCropMappers(containerRegistry);

        // Field mappers
        RegisterFieldMappers(containerRegistry);

        // Rotation mappers
        RegisterRotationMappers(containerRegistry);

        // Feed ingredient mappers
        RegisterFeedIngredientMappers(containerRegistry);

        // Animal component mappers
        RegisterAnimalComponentMappers(containerRegistry);

        // Dairy component mappers
        RegisterDairyComponentMappers(containerRegistry);

        // Management period mappers
        RegisterManagementPeriodMappers(containerRegistry);

        // Climate mappers
        RegisterClimateMappers(containerRegistry);
    }

    private void RegisterCropMappers(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance<IModelMapper<CropDto, CropDto>>(
            new CropDtoToCropDtoMapper(), nameof(CropDtoToCropDtoMapper));

        containerRegistry.RegisterInstance<IModelMapper<ICropDto, CropViewItem>>(
            new CropDtoToCropViewItemMapper(), nameof(CropDtoToCropViewItemMapper));

        containerRegistry.RegisterInstance<IModelMapper<CropViewItem, CropDto>>(
            new CropViewItemToCropDtoMapper(), nameof(CropViewItemToCropDtoMapper));
    }

    private void RegisterFieldMappers(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance<IModelMapper<FieldSystemComponent, FieldSystemComponentDto>>(
            new FieldComponentToDtoMapper(), nameof(FieldComponentToDtoMapper));

        containerRegistry.RegisterInstance<IModelMapper<FieldSystemComponentDto, FieldSystemComponent>>(
            new FieldDtoToFieldComponentMapper(), nameof(FieldDtoToFieldComponentMapper));

        containerRegistry.RegisterInstance<IModelMapper<FieldSystemComponentDto, FieldSystemComponentDto>>(
            new FieldDtoToFieldDtoMapper(), nameof(FieldDtoToFieldDtoMapper));
    }

    private void RegisterRotationMappers(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance<IModelMapper<RotationComponent, RotationComponentDto>>(
            new RotationComponentToRotationComponentDtoMapper(), nameof(RotationComponentToRotationComponentDtoMapper));

        containerRegistry.RegisterInstance<IModelMapper<RotationComponentDto, RotationComponent>>(
            new RotationComponentDtoToRotationComponentMapper(), nameof(RotationComponentDtoToRotationComponentMapper));
    }

    private void RegisterFeedIngredientMappers(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance<IModelMapper<FeedIngredient, FeedIngredient>>(
            new FeedIngredientToFeedIngredientMapper(), nameof(FeedIngredientToFeedIngredientMapper));
    }

    private void RegisterAnimalComponentMappers(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance<IModelMapper<AnimalComponentDto, AnimalComponentBase>>(
            new AnimalComponentDtoToAnimalComponentMapper(), nameof(AnimalComponentDtoToAnimalComponentMapper));

        containerRegistry.RegisterInstance<IModelMapper<AnimalComponentDto, AnimalComponentDto>>(
            new AnimalComponentDtoToAnimalComponentDtoMapper(), nameof(AnimalComponentDtoToAnimalComponentDtoMapper));

        containerRegistry.RegisterInstance<IModelMapper<AnimalComponentBase, AnimalComponentDto>>(
            new AnimalComponentBaseToAnimalComponentDtoMapper(), nameof(AnimalComponentBaseToAnimalComponentDtoMapper));

        containerRegistry.RegisterInstance<IModelMapper<AnimalGroupDto, AnimalGroup>>(
            new AnimalGroupDtoToAnimalGroupMapper(), nameof(AnimalGroupDtoToAnimalGroupMapper));

        containerRegistry.RegisterInstance<IModelMapper<AnimalGroupDto, AnimalGroupDto>>(
            new AnimalGroupDtoToAnimalGroupDtoMapper(), nameof(AnimalGroupDtoToAnimalGroupDtoMapper));

        containerRegistry.RegisterInstance<IModelMapper<AnimalGroup, AnimalGroupDto>>(
            new AnimalGroupToAnimalGroupDtoMapper(), nameof(AnimalGroupToAnimalGroupDtoMapper));
    }

    private void RegisterDairyComponentMappers(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance<IModelMapper<DairyComponent, DairyComponentDto>>(
            new DairyComponentToDtoMapper(), nameof(DairyComponentToDtoMapper));

        containerRegistry.RegisterInstance<IModelMapper<DairyComponentDto, DairyComponent>>(
            new DairyComponentDtoToComponentMapper(), nameof(DairyComponentDtoToComponentMapper));
    }

    private void RegisterManagementPeriodMappers(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance<IModelMapper<ManagementPeriodDto, ManagementPeriodDto>>(
            new ManagementPeriodDtoToManagementPeriodDtoMapper(), nameof(ManagementPeriodDtoToManagementPeriodDtoMapper));

        containerRegistry.RegisterInstance<IModelMapper<ManagementPeriod, ManagementPeriodDto>>(
            new ManagementPeriodToManagementPeriodDtoMapper(), nameof(ManagementPeriodToManagementPeriodDtoMapper));

        containerRegistry.RegisterInstance<IModelMapper<ManagementPeriodDto, ManagementPeriod>>(
            new ManagementPeriodDtoToManagementPeriodMapper(), nameof(ManagementPeriodDtoToManagementPeriodMapper));
    }

    private void RegisterClimateMappers(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance<IModelMapper<DailyClimateData, DailyClimateDto>>(
            new DailyClimateDataToDailyClimateDtoMapper(), nameof(DailyClimateDataToDailyClimateDtoMapper));

        containerRegistry.RegisterInstance<IModelMapper<DailyClimateDto, DailyClimateData>>(
            new DailyClimateDtoToDailyClimateDataMapper(), nameof(DailyClimateDtoToDailyClimateDataMapper));

        containerRegistry.RegisterInstance<IModelMapper<DailyClimateDto, DailyClimateDto>>(
            new DailyClimateDtoToDailyClimateDtoMapper(), nameof(DailyClimateDtoToDailyClimateDtoMapper));
    }
}
