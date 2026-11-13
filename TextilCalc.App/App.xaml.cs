using Microsoft.Extensions.DependencyInjection;
using TextilCalc.App.Services;

namespace TextilCalc.App;

public partial class App : Application
{
    private readonly AppShell _appShell;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        ApplyTheme(services.GetRequiredService<IAppPreferences>().GetThemePreference());

        // Las páginas usan recursos globales; deben resolverse después de cargar App.xaml.
        _appShell = services.GetRequiredService<AppShell>();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_appShell)
        {
            Title = "TextilCalc",
            MinimumWidth = 390,
            MinimumHeight = 650
        };
    }

    public static void ApplyTheme(string preference)
    {
        if (Current is null)
        {
            return;
        }

        Current.UserAppTheme = preference switch
        {
            "Light" => AppTheme.Light,
            "Dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }
}
