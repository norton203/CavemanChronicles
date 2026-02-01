using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Plugin.Maui.Audio;

namespace CavemanChronicles
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Register services
            builder.Services.AddSingleton(AudioManager.Current);
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddSingleton<AudioService>();
            builder.Services.AddSingleton<GameService>();
            builder.Services.AddSingleton<SaveService>();

            return builder.Build();
        }
    }
}