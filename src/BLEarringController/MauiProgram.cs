﻿using BLEarringController.Services;
using BLEarringController.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.BLE;

namespace BLEarringController
{
    public static class MauiProgram
    {
        #region Methods

        #region Private Static

        /// <summary>
        /// Manually register services from libraries that do not natively support
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection">
        /// Microsoft.Extensions.DependencyInjection</see>.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to register the services with.
        /// </param>
        private static void RegisterLibraryServices(in IServiceCollection services)
        {
            services
                // Register services from the Plugin.BLE library.
                .AddSingleton(CrossBluetoothLE.Current)          // IBluetoothLE implementation.
                .AddSingleton(CrossBluetoothLE.Current.Adapter); // IAdapter implementation.
        }

        /// <summary>
        /// Register the app's services with the dependency injection container.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to register the services with.
        /// </param>
        private static void RegisterServices(in IServiceCollection services)
        {
            // Use Scrutor to scan the assembly and register services with the dependency injection
            // container.
            services.Scan(scan => scan
                // Use the ISingletonService interface to get the assembly to scan.
                .FromAssemblyOf<ISingletonService>()

                // ------------------------------ Singleton Services ------------------------------

                // Singleton services should all implement ISingletonService, so filter out all
                // classes that implement ISingletonService from the list of all public,
                // none-abstract classes within the assembly.
                .AddClasses(classes => classes.AssignableTo<ISingletonService>())
                    // Register each matching type as all of its implemented interfaces, which all
                    // return an instance of the main type. This allows consumers to reference the
                    // services by an interface they implement or by the class name.
                    .AsSelfWithInterfaces()
                    // Register the classes with a singleton lifetime so a single instance will
                    // exist for the lifetime of the app.
                    .WithSingletonLifetime()

                // ------------------------------ Transient Services ------------------------------

                // Transient services should all implement ITransientService, so filter out all
                // classes that implement ITransientService from the list of all public,
                // none-abstract classes within the assembly.
                .AddClasses(classes => classes.AssignableTo<ITransientService>())
                    // Register each matching type as all of its implemented interfaces, which all
                    // return an instance of the main type. This allows consumers to reference the
                    // services by an interface they implement or by the class name.
                    .AsSelfWithInterfaces()
                    // Register the classes with a transient lifetime, so new instances are
                    // returned each time they are requested.
                    .WithTransientLifetime());
        }

        /// <summary>
        /// Register all Views and ViewModels within the app with the dependency injection
        /// container.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to register the services with.
        /// </param>
        private static void RegisterViewsAndViewModels(in IServiceCollection services)
        {
            // Use Scrutor to scan the assembly and register Views and ViewModels with the
            // dependency injection container.
            services.Scan(scan => scan
                // Use the IViewModel interface to get the assembly to scan. For now assume all
                // Views will be in the same assembly as the ViewModels.
                .FromAssemblyOf<IViewModel>()

                // ------------------------------------ Views -------------------------------------

                // All Views will derive from ContentPage, so filter out all classes that derive
                // from ContentPage from the list of all public, none-abstract classes within the
                // assembly.
                .AddClasses(classes => classes.AssignableTo<ContentPage>())
                    // Register each matching type as itself, as they will only be referred to by
                    // their class name to distinguish the different views.
                    .AsSelf()
                    // Register the views with a transient lifetime, as they should be
                    // re-instantiated each time they are referenced.
                    .WithTransientLifetime()

                // ---------------------------------- ViewModels ----------------------------------

                // All ViewModels will implement IViewModel, so filter out all classes that
                // implement IViewModel from the list of all public, none-abstract classes within
                // the assembly.
                .AddClasses(classes => classes.AssignableTo<IViewModel>())
                    // Register each matching type as itself, as they will only be referred to by
                    // name since each View will request a specific ViewModel.
                    .AsSelf()
                    // Register the ViewModels with a transient lifetime so they are
                    // re-instantiated for each reference by a View, ensuring a consistent state
                    // when each View is created.
                    .WithTransientLifetime());
        }

        #endregion

        #region Public Static

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // ------------------------ Dependency Injection Registrations ------------------------

            // Register all services within the app.
            RegisterServices(builder.Services);

            // Register services from libraries.
            RegisterLibraryServices(builder.Services);

            // Register Views and ViewModels within the app.
            RegisterViewsAndViewModels(builder.Services);

            // ------------------------------------------------------------------------------------

            return builder.Build();
        }

        #endregion

        #endregion
    }
}
