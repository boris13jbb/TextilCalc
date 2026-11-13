using TextilCalc.App.Views;

namespace TextilCalc.App;

public partial class AppShell : Shell
{
    public AppShell(HomePage homePage)
    {
        InitializeComponent();
        HomeContent.Content = homePage;

        Routing.RegisterRoute(nameof(GramaturaPage), typeof(GramaturaPage));
        Routing.RegisterRoute(nameof(MetrajePage), typeof(MetrajePage));
        Routing.RegisterRoute(nameof(TelasPage), typeof(TelasPage));
        Routing.RegisterRoute(nameof(CalculadoraPage), typeof(CalculadoraPage));
    }
}
