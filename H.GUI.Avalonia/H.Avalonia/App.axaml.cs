using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DryIoc;
using H.Avalonia.Infrastructure.DependencyInjection;
using H.Avalonia.Views;
using H.Avalonia.Views.FarmCreationViews;
using H.Avalonia.Views.SupportingViews.Disclaimer;
using H.Avalonia.Views.SupportingViews.MeasurementProvince;
using H.Core;
using H.Core.Enumerations;
using H.Core.Providers;
using H.Core.Services;
using H.Infrastructure;
using H.Localization;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using System;
using KmlHelpers = H.Core.KmlHelpers;

namespace H.Avalonia
{
    public partial class App : PrismApplication
    {
        /// <summary>
        /// Initializes the application by loading XAML resources and calling base initialization.
        /// This is the first method called during application startup.
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        /// <summary>
        /// Completes framework initialization by setting up the main window and exit handler.
        /// Called after Initialize() when the application framework is ready.
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Resolve through Prism so ViewModelLocator can run
                desktop.MainWindow = (Window)CreateShell();
                desktop.Exit += OnExit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// Handles application exit by ensuring storage data is saved before shutdown.
        /// </summary>
        /// <param name="sender">The application lifetime object</param>
        /// <param name="e">Exit event arguments</param>
        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            var storage = Container.Resolve<IStorage>();
            if (storage != null)
            {
                storage.Save();
            }
        }

        /// <summary>
        /// Registers all dependency injection services and views with comprehensive error handling.
        /// Sets up logging first, then delegates to ContainerRegistrationService for organized registration.
        /// </summary>
        /// <param name="containerRegistry">The container registry to register types with</param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Set up logging first as it's needed for error handling
            SetUpLogging(containerRegistry);

            try
            {
                var logger = Container.Resolve<ILogger>();
                
                // Create and use the registration service to handle all type registrations
                var registrationService = new ContainerRegistrationService(Container, logger);
                registrationService.RegisterAllTypes(containerRegistry);

                logger.LogInformation("All container types registered successfully");
            }
            catch (Exception ex)
            {
                var logger = Container.Resolve<ILogger>();
                logger.LogError(ex, "Failed to register container types: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates and returns the main application shell (window).
        /// Resolves the MainWindow through the DI container to ensure proper initialization.
        /// </summary>
        /// <returns>The main application window as an AvaloniaObject</returns>
        protected override AvaloniaObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        /// <summary>
        /// Performs post-initialization setup including language configuration and view region registration.
        /// Called after all services are registered and the container is ready for use.
        ///
        /// <para><b>Language restoration:</b> <see cref="SetLanguage"/> is called first
        /// to restore the user's previously chosen language from <c>app.config</c> before
        /// any views are created. This ensures the Disclaimer screen (and all subsequent views)
        /// display in the correct language from the moment they appear.</para>
        /// </summary>
        protected override void OnInitialized()
        {
            SetLanguage();

            // Register views to the Region it will appear in. Don't register them in the ViewModel.
            var regionManager = Container.Resolve<IRegionManager>();

            regionManager.RegisterViewWithRegion(UiRegions.ToolbarRegion, typeof(ToolbarView));
                        
            //regionManager.RegisterViewWithRegion(UiRegions.SidebarRegion, typeof(SidebarView));
            regionManager.RegisterViewWithRegion(UiRegions.FooterRegion, typeof(FooterView));
            regionManager.RegisterViewWithRegion(UiRegions.ContentRegion, typeof(DisclaimerView));
            regionManager.RegisterViewWithRegion(UiRegions.ContentRegion, typeof(MeasurementProvinceView));
            regionManager.RegisterViewWithRegion(UiRegions.ContentRegion, typeof(FarmOptionsView));
            regionManager.RegisterViewWithRegion(UiRegions.ContentRegion, typeof(FarmCreationView));
            regionManager.RegisterViewWithRegion(UiRegions.ContentRegion, typeof(FarmOpenExistingView));

            var geographicProvider = Container.Resolve<GeographicDataProvider>();
            geographicProvider.Initialize(); 
            Container.Resolve<KmlHelpers>();
        }

        /// <summary>
        /// Restores the user's language preference on application startup.
        ///
        /// <para><b>How the persisted language is read:</b></para>
        /// <list type="number">
        ///   <item>
        ///     <see cref="ICountrySettings"/> is registered as a singleton.
        ///     Its constructor calls <see cref="H.Core.Helpers.ConfigurationFileHelper.GetLanguage"/>
        ///     to read the <c>"Language"</c> appSetting from <c>app.config</c>.
        ///   </item>
        ///   <item>
        ///     If the persisted value is <c>"french"</c>, the <see cref="Languages.French"/>
        ///     branch runs, calling <see cref="LanguageManager.SetLanguage"/> with <c>"fr"</c>.
        ///   </item>
        ///   <item>
        ///     <c>LanguageManager.SetLanguage</c> sets thread cultures and
        ///     <see cref="LocalizationService.Instance.CurrentCulture"/>, triggering a
        ///     <see cref="LocalizationService.Refresh"/> that propagates to every
        ///     <see cref="LocalizedString"/> → the UI starts in French.
        ///   </item>
        /// </list>
        ///
        /// <para><b>Legacy resource classes:</b> <c>H.Avalonia.Resources.Culture</c> and
        /// <c>H.Core.Properties.Resources.Culture</c> are also set so that any ViewModels
        /// still using the older <c>{x:Static}</c> pattern display correctly.</para>
        /// </summary>
        private void SetLanguage()
        {
            var settings = Container.Resolve<ICountrySettings>();
            var language = settings.Language;

            if (language == Languages.French)
            {
                // Update thread cultures and LocalizationService so that all
                // LocalizedString.Value lookups return French strings at startup.
                LanguageManager.SetLanguage("fr");

                // Keep explicit culture assignments on legacy resource classes that may still
                // be referenced by ViewModels not yet migrated to AppStrings.
                H.Avalonia.Resources.Culture = InfrastructureConstants.FrenchCultureInfo;
                H.Core.Properties.Resources.Culture = InfrastructureConstants.FrenchCultureInfo;
            }
        }

        /// <summary>
        /// Configures application logging using NLog provider with comprehensive error tracking.
        /// Sets up logger factory, registers logger instance, and configures DryIoc container logging.
        /// </summary>
        /// <param name="containerRegistry">The container registry to register the logger with</param>
        private void SetUpLogging(IContainerRegistry containerRegistry)
        {
            // Create a LoggerFactory and add NLog as the logging provider
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders(); // Clear any default providers
                builder.SetMinimumLevel(LogLevel.Trace); // Set your desired minimum log level
                builder.AddNLog(); // Add NLog as the logging provider
            });

            var logger = loggerFactory.CreateLogger<App>();

            // Register the ILogger instance as a singleton in the Prism container
            containerRegistry.RegisterInstance(typeof(ILogger), logger);
            
            // Configure DryIoc logging for resolution errors
            ConfigureDryIocLogging(containerRegistry, logger);
        }

        /// <summary>
        /// Configures DryIoc container with enhanced error reporting and diagnostic features.
        /// Enables stack trace capture and disposable object tracking for better debugging.
        /// </summary>
        /// <param name="containerRegistry">The container registry to configure</param>
        /// <param name="logger">Logger instance for recording configuration status</param>
        private void ConfigureDryIocLogging(IContainerRegistry containerRegistry, ILogger logger)
        {
            try
            {
                // Access the underlying DryIoc container
                if (containerRegistry is DryIocContainerExtension dryIocExtension)
                {
                    var container = dryIocExtension.Instance;
                    
                    // Configure DryIoc with enhanced error reporting
                    var newContainer = container.With(rules => rules
                        .WithCaptureContainerDisposeStackTrace()
                        .WithTrackingDisposableTransients()
                        .WithDefaultReuse(Reuse.Transient));
                    
                    logger.LogInformation("DryIoc container logging configured successfully");
                }
                else
                {
                    logger.LogWarning("Unable to configure DryIoc logging - container is not a DryIocContainerExtension");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to configure DryIoc logging: {ErrorMessage}", ex.Message);
            }
        }
    }
}