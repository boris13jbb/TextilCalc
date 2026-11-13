using Microsoft.Extensions.DependencyInjection;

namespace TextilCalc.App;

public partial class App : Application
{
    private readonly AppShell _appShell;

    public App(IServiceProvider services)
    {
        InitializeComponent();

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
}