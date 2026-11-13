using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using TextilCalc.App.Views;

namespace TextilCalc.App;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    WindowSoftInputMode = SoftInput.AdjustResize,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override bool DispatchKeyEvent(KeyEvent? e)
    {
        if (e is not null &&
            e.Action == KeyEventActions.Down &&
            Shell.Current?.CurrentPage is CalculadoraPage calculadora &&
            calculadora.TryHandleAndroidKey(e.KeyCode, e))
        {
            return true;
        }

        return base.DispatchKeyEvent(e);
    }
}
