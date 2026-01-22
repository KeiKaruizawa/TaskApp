using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace TaskApp
{
    [Activity(Theme = "@style/MainTheme",
              MainLauncher = true,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // IMPORTANT: Hide the ActionBar (removes black bar with "TaskApp")
            if (ActionBar != null)
            {
                ActionBar.Hide();
            }

            // Support bar (Toolbar) - also hide it
            if (SupportActionBar != null)
            {
                SupportActionBar.Hide();
            }

            // Force light mode for the entire app
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)
                    (SystemUiFlags.LightStatusBar |
                     SystemUiFlags.LightNavigationBar);
            }

            // Make status bar white
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window?.SetStatusBarColor(Android.Graphics.Color.White);
            }
        }
    }
}