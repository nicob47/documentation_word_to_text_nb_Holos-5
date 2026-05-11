using H.Infrastructure;
using H.Avalonia.Infrastructure.Dialogs;
using H.Avalonia.Infrastructure.MapperServices;
using H.Avalonia.Services;
using H.Avalonia.Services.DietFormulator;
using H.Avalonia.ViewModels;
using H.Avalonia.ViewModels.ComponentViews;
using H.Avalonia.ViewModels.ComponentViews.Beef;
using H.Avalonia.ViewModels.ComponentViews.Dairy;
using H.Avalonia.ViewModels.ComponentViews.Infrastructure;
using H.Avalonia.ViewModels.ComponentViews.LandManagement;
using H.Avalonia.ViewModels.ComponentViews.LandManagement.Field;
using H.Avalonia.ViewModels.ComponentViews.LandManagement.Rotation;
using H.Avalonia.ViewModels.ComponentViews.OtherAnimals;
using H.Avalonia.ViewModels.ComponentViews.Poultry;
using H.Avalonia.ViewModels.ComponentViews.Sheep;
using H.Avalonia.ViewModels.ComponentViews.Swine;
using H.Avalonia.ViewModels.FarmCreationViews;
using H.Avalonia.ViewModels.OptionsViews;
using H.Avalonia.ViewModels.OptionsViews.FileMenuViews;
using H.Avalonia.ViewModels.Results;
using H.Avalonia.ViewModels.SupportingViews;
using H.Avalonia.ViewModels.SupportingViews.CountrySelection;
using H.Avalonia.ViewModels.SupportingViews.Disclaimer;
using H.Avalonia.ViewModels.SupportingViews.MeasurementProvince;
using H.Avalonia.ViewModels.SupportingViews.RegionSelection;
using H.Avalonia.ViewModels.SupportingViews.Start;
using H.Avalonia.Views;
using H.Avalonia.Views.ComponentViews;
using H.Avalonia.Views.ComponentViews.Beef;
using H.Avalonia.Views.ComponentViews.Dairy;
using H.Avalonia.Views.ComponentViews.Infrastructure;
using H.Avalonia.Views.ComponentViews.LandManagement;
using H.Avalonia.Views.ComponentViews.LandManagement.Field;
using H.Avalonia.Views.ComponentViews.OtherAnimals;
using H.Avalonia.Views.ComponentViews.Poultry;
using H.Avalonia.Views.ComponentViews.Sheep;
using H.Avalonia.Views.ComponentViews.Swine;
using H.Avalonia.Views.FarmCreationViews;
using H.Avalonia.Views.OptionsViews;
using H.Avalonia.Views.OptionsViews.FileMenuViews;
using H.Avalonia.Views.ResultViews;
using H.Avalonia.Views.SupportingViews;
using H.Avalonia.Views.SupportingViews.CountrySelection;
using H.Avalonia.Views.SupportingViews.Disclaimer;
using H.Avalonia.Views.SupportingViews.MeasurementProvince;
using H.Avalonia.Views.SupportingViews.RegionSelection;
using H.Avalonia.Views.SupportingViews.Start;
using H.Core;
using H.Core.Calculators.Carbon;
using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Factories;
using H.Core.Factories.Animals;
using H.Core.Factories.Climate;
using H.Core.Factories.Crops;
using H.Core.Factories.FarmFactory;
using H.Core.Factories.Fields;
using H.Core.Factories.Rotations;
using H.Core.Mappers;
using H.Core.Models.Animals;
using H.Core.Models.Climate;
using H.Core.Models.LandManagement.Fields;
using H.Core.Models.LandManagement.Rotation;
using H.Core.Providers;
using H.Core.Providers.Climate;
using H.Core.Providers.Energy;
using H.Core.Providers.Feed;
using H.Core.Services;
using H.Core.Services.Animals;
using H.Core.Services.Climate;
using H.Core.Services.Countries;
using H.Core.Services.DietService;
using H.Core.Services.Initialization;
using H.Core.Services.Animals.Dairy;
using H.Core.Services.LandManagement.Fields;
using H.Core.Services.Provinces;
using H.Core.Services.StorageService;
using H.Core.Services.CropColorService;
using H.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Prism.Ioc;
using ClimateResultsView = H.Avalonia.Views.ResultViews.ClimateResultsView;
using RotationComponentView = H.Avalonia.Views.ComponentViews.LandManagement.Rotation.RotationComponentView;
using SoilResultsView = H.Avalonia.Views.ResultViews.SoilResultsView;

namespace H.Avalonia.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Service responsible for registering all dependency injection container types
    /// </summary>
    public class ContainerRegistrationService
    {
        private readonly IContainerProvider _containerProvider;
        private readonly ILogger _logger;

        public ContainerRegistrationService(IContainerProvider containerProvider, ILogger logger)
        {
            _containerProvider = containerProvider;
            _logger = logger;
        }

        /// <summary>
        /// Register all types with the dependency injection container
        /// </summary>
        /// <param name="containerRegistry">The container registry to register types with</param>
        public void RegisterAllTypes(IContainerRegistry containerRegistry)
        {
            _logger.LogInformation("Starting dependency injection container registration process");
            
            RegisterStorage(containerRegistry);
            RegisterViews(containerRegistry);
            RegisterProviders(containerRegistry);
            RegisterServices(containerRegistry);
            RegisterFactories(containerRegistry);
            RegisterTables(containerRegistry);
            RegisterMappers(containerRegistry);
            RegisterCaching(containerRegistry);
            RegisterTransferServices(containerRegistry);
            RegisterDialogs(containerRegistry);
            
            _logger.LogInformation("Completed dependency injection container registration process");
        }

        #region Storage Registration

        /// <summary>
        /// Register storage-related services
        /// </summary>
        private void RegisterStorage(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering storage services");
            
            // V5 object
#pragma warning disable CS0618 // Storage is obsolete but still required during migration
            containerRegistry.RegisterSingleton<Storage>();
#pragma warning restore CS0618

            // V4 object
            containerRegistry.RegisterSingleton<IStorage, H.Core.Storage>();

            containerRegistry.RegisterSingleton<IStorageService, DefaultStorageService>();

            // Initialize storage
            var storage = _containerProvider.Resolve<IStorage>();
            storage.Load();
            var storageService = _containerProvider.Resolve<IStorageService>();
            var activeFarm = storageService.GetActiveFarm();
            
            _logger.LogInformation("Successfully registered storage services. Active farm: {FarmName}", activeFarm?.Name ?? "None");
        }

        #endregion

        #region View Registration

        /// <summary>
        /// Register views and view models for navigation
        /// </summary>
        private void RegisterViews(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering view and view model types for navigation");
            
            RegisterCoreViews(containerRegistry);
            RegisterOptionsViews(containerRegistry);
            RegisterResultsViews(containerRegistry);
            RegisterComponentViews(containerRegistry);
            RegisterAnimalComponentViews(containerRegistry);
            RegisterDietViews(containerRegistry);
            RegisterBlankView(containerRegistry);
            
            _logger.LogInformation("Successfully registered all view and view model types");
        }

        /// <summary>
        /// Register core application views
        /// </summary>
        private void RegisterCoreViews(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering core application views");
            
            containerRegistry.RegisterForNavigation<ToolbarView, ToolbarViewModel>();
            containerRegistry.RegisterForNavigation<SidebarView, SidebarViewModel>();
            containerRegistry.RegisterForNavigation<FooterView, FooterViewModel>();
            containerRegistry.RegisterForNavigation<ClimateDataView, ClimateDataViewModel>();
            containerRegistry.RegisterForNavigation<SoilDataView, SoilDataViewModel>();
            containerRegistry.RegisterForNavigation<AboutPageView, AboutPageViewModel>();
            containerRegistry.RegisterForNavigation<ClimateResultsView, ClimateResultsViewModel>();
            containerRegistry.RegisterForNavigation<SoilResultsView, SoilResultsViewModel>();
            containerRegistry.RegisterForNavigation<MyComponentsView, MyComponentsViewModel>();
            containerRegistry.RegisterForNavigation<ResultsSidebarView, ResultsSidebarViewModel>();
            containerRegistry.RegisterForNavigation<ChooseComponentsView, ChooseComponentsViewModel>();
            containerRegistry.RegisterForNavigation<FieldComponentView, FieldComponentViewModel>();
            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
            containerRegistry.RegisterForNavigation<DisclaimerView, DisclaimerViewModel>();
            containerRegistry.RegisterForNavigation<RegionSelectionView, RegionSelectionViewModel>();
            containerRegistry.RegisterForNavigation<MeasurementProvinceView, MeasurementProvinceViewModel>();
            containerRegistry.RegisterForNavigation<CountrySelectionView, CountrySelectionViewModel>();
            containerRegistry.RegisterForNavigation<FarmOptionsView, FarmOptionsViewModel>();
            containerRegistry.RegisterForNavigation<FarmCreationView, FarmCreationViewModel>();
            containerRegistry.RegisterForNavigation<FarmOpenExistingView, FarmOpenExistingViewModel>();
            containerRegistry.RegisterForNavigation<StartView, StartViewModel>();
            
            _logger.LogDebug("Completed registration of core application views");
        }

        /// <summary>
        /// Register options and settings views
        /// </summary>
        private void RegisterOptionsViews(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering options and settings views");
            
            containerRegistry.RegisterForNavigation<OptionsView, OptionsViewModel>();
            containerRegistry.RegisterForNavigation<SelectOptionView, SelectOptionViewModel>();
            containerRegistry.RegisterForNavigation<OptionFarmView, FarmSettingsViewModel>();
            containerRegistry.RegisterForNavigation<OptionUserSettingsView, UserSettingsViewModel>();
            containerRegistry.RegisterForNavigation<OptionSoilView, SoilSettingsViewModel>();
            containerRegistry.RegisterForNavigation<OptionSoilN2OBreakdownView, SoilN2OBreakdownSettingsViewModel>();
            containerRegistry.RegisterForNavigation<DefaultBeddingCompositionView, DefaultBeddingCompositionViewModel>();
            containerRegistry.RegisterForNavigation<DefaultManureCompositionView, DefaultManureCompositionViewModel>();
            containerRegistry.RegisterForNavigation<OptionPrecipitationView, PrecipitationSettingsViewModel>();
            containerRegistry.RegisterForNavigation<OptionEvapotranspirationView, EvapotranspirationSettingsViewModel>();
            containerRegistry.RegisterForNavigation<OptionTemperatureView, TemperatureSettingsViewModel>();
            containerRegistry.RegisterForNavigation<OptionBarnTemperatureView, BarnTemperatureSettingsViewModel>();

            // File menu views
            containerRegistry.RegisterForNavigation<FileNewFarmView, FileNewFarmViewModel>();
            containerRegistry.RegisterForNavigation<FileOpenFarmView, FileOpenFarmViewModel>();
            containerRegistry.RegisterForNavigation<FarmManagementView, FarmManagementViewModel>();
            containerRegistry.RegisterForNavigation<FileSaveOptionsView, FileSaveOptionsViewModel>();
            containerRegistry.RegisterForNavigation<FileExportFarmView, FileExportFarmViewModel>();
            containerRegistry.RegisterForNavigation<FileImportFarmView, FileImportFarmViewModel>();
            containerRegistry.RegisterForNavigation<FarmImportFileView, FarmImportFileViewModel>();
            containerRegistry.RegisterForNavigation<FileExportClimateView, FileExportClimateViewModel>();
            containerRegistry.RegisterForNavigation<FileExportManureView, FileExportManureViewModel>();
            containerRegistry.RegisterForNavigation<EnergySettingsView, EnergySettingsViewModel>();
            containerRegistry.RegisterForNavigation<CropDefaultsSettingsView, CropDefaultsSettingsViewModel>();
            
            _logger.LogDebug("Completed registration of options and settings views");
        }

        private void RegisterResultsViews(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering results views");

            
            containerRegistry.RegisterForNavigation<ResultsSummaryView, ResultsSummaryViewModel>();
            containerRegistry.RegisterForNavigation<MultiYearCarbonModellingView, MultiYearCarbonModellingViewModel>();
            containerRegistry.RegisterForNavigation<EstimatesOfProductionView, EstimatesOfProductionViewModel>();
            containerRegistry.RegisterForNavigation<FeedEstimateReportView, FeedEstimateReportViewModel>();
            containerRegistry.RegisterForNavigation<ManureManagementResultsView, ManureManagementResultsViewModel>();
            containerRegistry.RegisterForNavigation<EmissionPieChartView, EmissionPieChartViewModel>();
            containerRegistry.RegisterForNavigation<OverallEmissionsResultsView, OverallEmissionsResultsViewModel>();
            containerRegistry.RegisterForNavigation<ComponentEmissionsResultsView, ComponentEmissionsResultsViewModel>();
            containerRegistry.RegisterForNavigation<DetailedEmissionsReportResultsView, DetailedEmissionsReportResultsViewModel>();

            _logger.LogDebug("Completed registration of results views");
        }

        /// <summary>
        /// Register component views (land management and infrastructure)
        /// </summary>
        private void RegisterComponentViews(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering component views (land management and infrastructure)");
            
            containerRegistry.RegisterForNavigation<SheepComponentView, SheepComponentViewModel>();
            containerRegistry.RegisterForNavigation<RotationComponentView, RotationComponentViewModel>();
            containerRegistry.RegisterForNavigation<ShelterbeltComponentView, ShelterbeltComponentViewModel>();
            containerRegistry.RegisterForNavigation<AnaerobicDigestionComponentView, AnaerobicDigestionComponentViewModel>();
            
            _logger.LogDebug("Completed registration of component views");
        }

        /// <summary>
        /// Register animal component views (beef, dairy, sheep, swine, poultry, other animals)
        /// </summary>
        private void RegisterAnimalComponentViews(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering animal component views (beef, dairy, sheep, swine, poultry, other animals)");

            // Beef components
            containerRegistry.RegisterForNavigation<CowCalfComponentView, CowCalfComponentViewModel>();
            containerRegistry.RegisterForNavigation<BackgroundingComponentView, BackgroundingComponentViewModel>();
            containerRegistry.RegisterForNavigation<FinishingComponentView, FinishingComponentViewModel>();

            // Dairy components
            containerRegistry.RegisterForNavigation<DairyComponentView, DairyComponentViewModel>();

            // Sheep components
            containerRegistry.RegisterForNavigation<SheepFeedlotComponentView, SheepFeedlotComponentViewModel>();
            containerRegistry.RegisterForNavigation<RamsComponentView, RamsComponentViewModel>();
            containerRegistry.RegisterForNavigation<LambsAndEwesComponentView, LambsAndEwesComponentViewModel>();

            // Swine components
            containerRegistry.RegisterForNavigation<GrowerToFinishComponentView, GrowerToFinishComponentViewModel>();
            containerRegistry.RegisterForNavigation<FarrowToWeanComponentView, FarrowToWeanComponentViewModel>();
            containerRegistry.RegisterForNavigation<IsoWeanComponentView, IsoWeanComponentViewModel>();
            containerRegistry.RegisterForNavigation<FarrowToFinishComponentView, FarrowToFinishComponentViewModel>();

            // Poultry components
            containerRegistry.RegisterForNavigation<ChickenPulletsComponentView, ChickenPulletsComponentViewModel>();
            containerRegistry.RegisterForNavigation<ChickenMultiplierBreederComponentView, ChickenMultiplierBreederComponentViewModel>();
            containerRegistry.RegisterForNavigation<ChickenMeatProductionComponentView, ChickenMeatProductionComponentViewModel>();
            containerRegistry.RegisterForNavigation<TurkeyMultiplierBreederComponentView, TurkeyMultiplierBreederComponentViewModel>();
            containerRegistry.RegisterForNavigation<TurkeyMeatProductionComponentView, TurkeyMeatProductionComponentViewModel>();
            containerRegistry.RegisterForNavigation<ChickenEggProductionComponentView, ChickenEggProductionComponentViewModel>();
            containerRegistry.RegisterForNavigation<ChickenMultiplierHatcheryComponentView, ChickenMultiplierHatcheryComponentViewModel>();

            // Other animals
            containerRegistry.RegisterForNavigation<GoatsComponentView, GoatsComponentViewModel>();
            containerRegistry.RegisterForNavigation<DeerComponentView, DeerComponentViewModel>();
            containerRegistry.RegisterForNavigation<HorsesComponentView, HorsesComponentViewModel>();
            containerRegistry.RegisterForNavigation<MulesComponentView, MulesComponentViewModel>();
            containerRegistry.RegisterForNavigation<BisonComponentView, BisonComponentViewModel>();
            containerRegistry.RegisterForNavigation<LlamaComponentView, LlamaComponentViewModel>();
            
            _logger.LogDebug("Completed registration of animal component views");
        }

        /// <summary>
        /// Register diet-related views
        /// </summary>
        private void RegisterDietViews(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering diet-related views");
            
            containerRegistry.RegisterForNavigation<DietFormulatorView, DietFormulatorViewModel>();
            containerRegistry.RegisterForNavigation<FeedIngredientsView, FeedIngredientsViewModel>();
            
            _logger.LogDebug("Completed registration of diet-related views");
        }

        /// <summary>
        /// Register blank view
        /// </summary>
        private void RegisterBlankView(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering blank view");
            
            containerRegistry.RegisterForNavigation<BlankView, BlankViewModel>();
            
            _logger.LogDebug("Completed registration of blank view");
        }

        #endregion

        #region Providers Registration

        /// <summary>
        /// Register data providers
        /// </summary>
        private void RegisterProviders(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering data providers");
            
            containerRegistry.RegisterSingleton<GeographicDataProvider>();
            containerRegistry.RegisterSingleton<ExportHelpers>();
            containerRegistry.RegisterSingleton<ImportHelpers>();
            containerRegistry.RegisterSingleton<KmlHelpers>();

            containerRegistry.RegisterSingleton<ICountrySettings, CountrySettings>();
            containerRegistry.Register<ICountries, CountriesService>();
            containerRegistry.RegisterSingleton<IProvinces, ProvincesService>();
            containerRegistry.RegisterSingleton<IDietProvider, DietProvider>();
            containerRegistry.RegisterSingleton<IFeedIngredientProvider, FeedIngredientProvider>();
            containerRegistry.RegisterSingleton<IClimateProvider, ClimateProvider>();
            containerRegistry.RegisterSingleton<ISlcClimateProvider, SlcClimateDataProvider>();
            
            _logger.LogInformation("Successfully registered data providers");
        }

        #endregion

        #region Services Registration

        /// <summary>
        /// Register application services
        /// </summary>
        private void RegisterServices(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering application services");
            
            containerRegistry.RegisterSingleton<IFarmHelper, FarmHelper>();
            containerRegistry.RegisterSingleton<IComponentInitializationService, ComponentInitializationService>();
            containerRegistry.RegisterSingleton<IFieldComponentService, FieldComponentService>();
            containerRegistry.RegisterSingleton<IRotationComponentService, RotationComponentService>();
            containerRegistry.RegisterSingleton<IDairyComponentService, DairyComponentService>();
            containerRegistry.RegisterSingleton<IClimateService, ClimateService>();
            containerRegistry.RegisterSingleton<IFarmResultsService_NEW, FarmResultsService_NEW>();
            containerRegistry.RegisterSingleton<IDietService, DefaultDietService>();
            containerRegistry.RegisterSingleton<ICropInitializationService, CropInitializationService>();
            containerRegistry.RegisterSingleton<IAnimalComponentService, AnimalComponentService>();
            containerRegistry.RegisterSingleton<IManagementPeriodService, ManagementPeriodService>();
            containerRegistry.RegisterSingleton<IErrorHandlerService, ErrorHandlerService>();
            containerRegistry.RegisterSingleton<INotificationManagerService, NotificationManagerService>();
            containerRegistry.RegisterSingleton<IDietFormulatorWindowService, DietFormulatorWindowService>();
            containerRegistry.RegisterSingleton<IDefaultGeocoderService, NominatimGeocoderService>();
            containerRegistry.RegisterSingleton<ICropColorService, CropColorService>();

            // Carbon input calculators + orchestrating CarbonService (also registered in CoreModule
            // for non-GUI consumers; duplicated here for parity with the rest of the GUI service
            // wiring).
            containerRegistry.RegisterSingleton<IICBMCarbonInputCalculator, ICBMCarbonInputCalculator>();
            containerRegistry.RegisterSingleton<IIPCCTier2CarbonInputCalculator, IPCCTier2CarbonInputCalculator>();
            containerRegistry.RegisterSingleton<ICarbonService, CarbonService>();

            // Unit conversion
            containerRegistry.RegisterSingleton<IUnitsOfMeasurementCalculator, UnitsOfMeasurementCalculator>();
            
            _logger.LogInformation("Successfully registered application services");
        }

        #endregion

        #region Factories Registration

        /// <summary>
        /// Register factory services
        /// </summary>
        private void RegisterFactories(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering factory services");
            
            containerRegistry.RegisterSingleton<IDietFactory, DietFactory>();
            containerRegistry.RegisterSingleton<IFarmFactory, FarmFactory>();
            containerRegistry.RegisterSingleton<IManagementPeriodFactory, ManagementPeriodFactory>();
            containerRegistry.RegisterSingleton<IDailyClimateDataFactory, DailyClimateDataFactory>();

            containerRegistry.Register(typeof(IFactory<CropDto>), typeof(CropFactory));
            containerRegistry.Register(typeof(IFactory<FieldSystemComponentDto>), typeof(FieldFactory));
            containerRegistry.Register(typeof(IFactory<RotationComponentDto>), typeof(RotationComponentFactory));
            containerRegistry.Register(typeof(IFactory<AnimalComponentDto>), typeof(AnimalComponentFactory));
            containerRegistry.Register(typeof(IFactory<DailyClimateDto>), typeof(DailyClimateDataFactory));

            containerRegistry.Register(typeof(ICropFactory), typeof(CropFactory));
            containerRegistry.Register(typeof(IFieldFactory), typeof(FieldFactory));
            containerRegistry.RegisterSingleton<IAnimalComponentFactory, AnimalComponentFactory>();
            containerRegistry.RegisterSingleton<IAnimalGroupFactory, AnimalGroupFactory>();
            
            _logger.LogInformation("Successfully registered factory services");
        }

        #endregion

        #region Tables Registration

        /// <summary>
        /// Register table providers
        /// </summary>
        private void RegisterTables(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering table providers");
            
            containerRegistry.RegisterSingleton<ITable50FuelEnergyEstimatesProvider, Table50FuelEnergyEstimatesProvider>();
            
            _logger.LogInformation("Successfully registered table providers");
        }

        #endregion

        #region Mappers Registration

        /// <summary>
        /// Register mapper implementations
        /// </summary>
        private void RegisterMappers(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering mapper implementations");

            var mapperRegistrationService = new MapperRegistrationService();
            mapperRegistrationService.RegisterMappers(containerRegistry);

            _logger.LogInformation("Successfully registered mapper implementations");
        }

        #endregion

        #region Caching Registration

        /// <summary>
        /// Register caching services
        /// </summary>
        private void RegisterCaching(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering caching services");
            
            var options = new MemoryCacheOptions()
            {
                //SizeLimit = long.MaxValue,
            };

            containerRegistry.RegisterSingleton<IMemoryCache>(() => new MemoryCache(options));
            containerRegistry.RegisterSingleton<ICacheService, InMemoryCacheService>();
            
            _logger.LogInformation("Successfully registered caching services");
        }

        #endregion

        #region Transfer Services Registration

        /// <summary>
        /// Register transfer services for data conversion
        /// </summary>
        private void RegisterTransferServices(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering transfer services for data conversion");
            
            containerRegistry.Register(typeof(ITransferService<,>), typeof(TransferService<,>));

            RegisterDailyClimateTransferService(containerRegistry);
            RegisterCropTransferService(containerRegistry);
            RegisterFieldTransferService(containerRegistry);
            RegisterRotationTransferService(containerRegistry);
            RegisterAnimalTransferService(containerRegistry);
            
            _logger.LogInformation("Successfully registered transfer services");
        }

        /// <summary>
        /// Register TransferService for DailyClimateData and DailyClimateDto
        /// </summary>
        private void RegisterDailyClimateTransferService(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering DailyClimate transfer service");
            
            containerRegistry.Register<ITransferService<DailyClimateData, DailyClimateDto>>(() =>
            {
                var unitsCalculator = _containerProvider.Resolve<IUnitsOfMeasurementCalculator>();
                var dailyClimateDataFactory = _containerProvider.Resolve<IFactory<DailyClimateDto>>();
                var dtoToModelMapper = _containerProvider.Resolve<IModelMapper<DailyClimateDto, DailyClimateData>>(nameof(DailyClimateDtoToDailyClimateDataMapper));
                var modelToDtoMapper = _containerProvider.Resolve<IModelMapper<DailyClimateData, DailyClimateDto>>(nameof(DailyClimateDataToDailyClimateDtoMapper));

                return new TransferService<DailyClimateData, DailyClimateDto>(
                    unitsOfMeasurementCalculator: unitsCalculator,
                    dtoFactory: dailyClimateDataFactory,
                    dtoToModelMapper: dtoToModelMapper,
                    modelToDtoMapper: modelToDtoMapper
                );
            });
        }

        /// <summary>
        /// Register TransferService for CropViewItem and CropDto
        /// </summary>
        private void RegisterCropTransferService(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering Crop transfer service");
            
            containerRegistry.Register<ITransferService<CropViewItem, CropDto>>(() =>
            {
                var unitsCalculator = _containerProvider.Resolve<IUnitsOfMeasurementCalculator>();
                var cropDtoFactory = _containerProvider.Resolve<IFactory<CropDto>>();
                var dtoToModelMapper = _containerProvider.Resolve<IModelMapper<ICropDto, CropViewItem>>(nameof(CropDtoToCropViewItemMapper));
                var modelToDtoMapper = _containerProvider.Resolve<IModelMapper<CropViewItem, CropDto>>(nameof(CropViewItemToCropDtoMapper));

                return new TransferService<CropViewItem, CropDto>(
                    unitsOfMeasurementCalculator: unitsCalculator,
                    dtoFactory: cropDtoFactory,
                    dtoToModelMapper: dtoToModelMapper,
                    modelToDtoMapper: modelToDtoMapper
                );
            });
        }

        /// <summary>
        /// Register TransferService for FieldSystemComponent and FieldSystemComponentDto
        /// </summary>
        private void RegisterFieldTransferService(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering Field transfer service");
            
            containerRegistry.Register<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>(() =>
            {
                var unitsCalculator = _containerProvider.Resolve<IUnitsOfMeasurementCalculator>();
                var fieldDtoFactory = _containerProvider.Resolve<IFactory<FieldSystemComponentDto>>();
                var dtoToModelMapper = _containerProvider.Resolve<IModelMapper<FieldSystemComponentDto, FieldSystemComponent>>(nameof(FieldDtoToFieldComponentMapper));
                var modelToDtoMapper = _containerProvider.Resolve<IModelMapper<FieldSystemComponent, FieldSystemComponentDto>>(nameof(FieldComponentToDtoMapper));

                return new TransferService<FieldSystemComponent, FieldSystemComponentDto>(
                    unitsOfMeasurementCalculator: unitsCalculator,
                    dtoFactory: fieldDtoFactory,
                    dtoToModelMapper: dtoToModelMapper,
                    modelToDtoMapper: modelToDtoMapper
                );
            });
        }

        /// <summary>
        /// Register TransferService for RotationComponent and RotationComponentDto
        /// </summary>
        private void RegisterRotationTransferService(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering Rotation transfer service");
            
            containerRegistry.Register<ITransferService<RotationComponent, RotationComponentDto>>(() =>
            {
                var unitsCalculator = _containerProvider.Resolve<IUnitsOfMeasurementCalculator>();
                var rotationDtoFactory = _containerProvider.Resolve<IFactory<RotationComponentDto>>();
                var dtoToModelMapper = _containerProvider.Resolve<IModelMapper<RotationComponentDto, RotationComponent>>(nameof(RotationComponentDtoToRotationComponentMapper));
                var modelToDtoMapper = _containerProvider.Resolve<IModelMapper<RotationComponent, RotationComponentDto>>(nameof(RotationComponentToRotationComponentDtoMapper));

                return new TransferService<RotationComponent, RotationComponentDto>(
                    unitsOfMeasurementCalculator: unitsCalculator,
                    dtoFactory: rotationDtoFactory,
                    dtoToModelMapper: dtoToModelMapper,
                    modelToDtoMapper: modelToDtoMapper
                );
            });
        }

        /// <summary>
        /// Register TransferService for AnimalComponentBase and AnimalComponentDto
        /// </summary>
        private void RegisterAnimalTransferService(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering Animal transfer service");
            
            containerRegistry.Register<ITransferService<AnimalComponentBase, AnimalComponentDto>>(() =>
            {
                var unitsCalculator = _containerProvider.Resolve<IUnitsOfMeasurementCalculator>();
                var animalDtoFactory = _containerProvider.Resolve<IFactory<AnimalComponentDto>>();
                var dtoToModelMapper = _containerProvider.Resolve<IModelMapper<AnimalComponentDto, AnimalComponentBase>>(nameof(AnimalComponentDtoToAnimalComponentMapper));
                var modelToDtoMapper = _containerProvider.Resolve<IModelMapper<AnimalComponentBase, AnimalComponentDto>>(nameof(AnimalComponentBaseToAnimalComponentDtoMapper));

                return new TransferService<AnimalComponentBase, AnimalComponentDto>(
                    unitsOfMeasurementCalculator: unitsCalculator,
                    dtoFactory: animalDtoFactory,
                    dtoToModelMapper: dtoToModelMapper,
                    modelToDtoMapper: modelToDtoMapper
                );
            });
        }

        #endregion

        #region Dialogs Registration

        /// <summary>
        /// Register dialog services
        /// </summary>
        private void RegisterDialogs(IContainerRegistry containerRegistry)
        {
            _logger.LogDebug("Registering dialog services");
            
            containerRegistry.RegisterDialog<DeleteRowDialog, DeleteRowDialogViewModel>();
            
            _logger.LogInformation("Successfully registered dialog services");
        }

        #endregion
    }
}