using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace TaskApp
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

            // ✅ ANDROID: enable edge-to-edge (removes top white space)
            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android => android.OnCreate((activity, bundle) =>
                {
                    activity.Window.SetDecorFitsSystemWindows(false);
                    activity.Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                }));
#endif
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
