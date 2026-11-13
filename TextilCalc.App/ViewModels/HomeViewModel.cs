using CommunityToolkit.Mvvm.Input;
using TextilCalc.App.Services;

namespace TextilCalc.App.ViewModels;

public sealed partial class HomeViewModel
{
    private readonly IUserDialogService _dialogs;

    public HomeViewModel(IUserDialogService dialogs)
    {
        _dialogs = dialogs;
    }

    [RelayCommand]
    private static Task OpenGramaturaAsync() => Shell.Current.GoToAsync(nameof(Views.GramaturaPage));

    [RelayCommand]
    private static Task OpenMetrajeAsync() => Shell.Current.GoToAsync(nameof(Views.MetrajePage));

    [RelayCommand]
    private static Task OpenTelasAsync() => Shell.Current.GoToAsync(nameof(Views.TelasPage));

    [RelayCommand]
    private static Task OpenCalculatorAsync() => Shell.Current.GoToAsync(nameof(Views.CalculadoraPage));

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
}
