using Microsoft.Extensions.Logging;

namespace SuntoryManagementSystem_App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registreer ViewModels en Pages voor Dependency Injection
            builder.Services.AddSingleton<ViewModels.MainViewModel>();
            builder.Services.AddSingleton<Pages.MainPage>();
            
            // Registreer nieuwe tab pages (namen gebaseerd op Models)
            builder.Services.AddSingleton<Pages.ProductPage>();
            builder.Services.AddSingleton<Pages.DeliveryPage>();
            builder.Services.AddSingleton<Pages.CustomerPage>();
            builder.Services.AddSingleton<Pages.SettingsPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
