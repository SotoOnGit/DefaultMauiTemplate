using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;

namespace DefaultTemplateApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .ConfigureSyncfusionToolkit()
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit(options =>
                {
                    options.SetPopupOptionsDefaults(new DefaultPopupOptionsSettings
                    {
                        PageOverlayColor = Colors.Black.WithAlpha(0.8f),
                        Shadow = null,
                        Shape = null,
                    });
                })
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMauiMaps();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
