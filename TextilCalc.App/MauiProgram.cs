using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TextilCalc.App.Services;
using TextilCalc.App.ViewModels;
using TextilCalc.App.Views;
using TextilCalc.Core.Services;

namespace TextilCalc.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<IAppPreferences, AppPreferences>();
        builder.Services.AddSingleton<IUserDialogService, UserDialogService>();
        builder.Services.AddSingleton<ICalculatorHistoryService, CalculatorHistoryService>();
        builder.Services.AddSingleton<ExpressionEvaluator>();
        builder.Services.AddSingleton<NavigationTutorialService>();

        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<GramaturaViewModel>();
        builder.Services.AddSingleton<MetrajeViewModel>();
        builder.Services.AddSingleton<CalculadoraViewModel>();

        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddTransient<GramaturaPage>();
        builder.Services.AddTransient<MetrajePage>();
        builder.Services.AddTransient<CalculadoraPage>();
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
