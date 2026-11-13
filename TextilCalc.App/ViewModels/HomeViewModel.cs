using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TextilCalc.App.Services;

namespace TextilCalc.App.ViewModels;

public sealed partial class HomeViewModel : ObservableObject
{
    private readonly IUserDialogService _dialogs;
    private readonly IAppPreferences _preferences;

    [ObservableProperty] public partial string ThemeLabel { get; set; } = "Tema: Sistema";

    public HomeViewModel(IUserDialogService dialogs, IAppPreferences preferences)
    {
        _dialogs = dialogs;
        _preferences = preferences;
        ThemeLabel = FormatThemeLabel(preferences.GetThemePreference());
    }

    [RelayCommand]
    private static Task OpenGramaturaAsync() => Shell.Current.GoToAsync(nameof(Views.GramaturaPage));

    [RelayCommand]
    private static Task OpenMetrajeAsync() => Shell.Current.GoToAsync(nameof(Views.MetrajePage));

    [RelayCommand]
    private static Task OpenCalculatorAsync() => Shell.Current.GoToAsync(nameof(Views.CalculadoraPage));

    [RelayCommand]
    private void CycleTheme()
    {
        var next = _preferences.GetThemePreference() switch
        {
            "System" => "Light",
            "Light" => "Dark",
            _ => "System"
        };

        _preferences.SetThemePreference(next);
        App.ApplyTheme(next);
        ThemeLabel = FormatThemeLabel(next);
    }

    [RelayCommand]
    private async Task ExitAsync()
    {
        var confirmed = await _dialogs.ConfirmAsync(
            "Salir de TextilCalc",
            "¿Deseas cerrar la aplicación?",
            "Salir",
            "Cancelar");
        if (!confirmed)
        {
            return;
        }

        var application = Application.Current;
        var window = application?.Windows.FirstOrDefault();
        if (application is not null && window is not null)
        {
            application.CloseWindow(window);
        }
    }

    private static string FormatThemeLabel(string preference) => preference switch
    {
        "Light" => "Tema: Claro",
        "Dark" => "Tema: Oscuro",
        _ => "Tema: Sistema"
    };
}
