using Microsoft.Extensions.Logging;
using ProjectCompScience.Services;

namespace ProjectCompScience
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            LocalDataService.GetLocalDataService().Init();
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
