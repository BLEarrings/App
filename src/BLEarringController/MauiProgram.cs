using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace BLEarringController
{
    public static class MauiProgram
    {
        #region Methods

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

                // ----------------------------- Views and ViewModels -----------------------------

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

            return builder.Build();
        }

        #endregion

        #endregion
    }
}
