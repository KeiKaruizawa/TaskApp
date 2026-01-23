using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;

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

            // FORCE Light Mode for the entire app (this affects dialogs too)
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;

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

            // Force light status bar
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