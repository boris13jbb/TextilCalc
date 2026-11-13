namespace TextilCalc.App.Services;

public sealed class NavigationTutorialService(IAppPreferences preferences, IUserDialogService dialogs)
{
    private bool _checkedThisSession;

    public async Task ShowIfNeededAsync()
    {
        if (_checkedThisSession || preferences.HasSeenNavigationTutorial())
        {
            return;
        }

        _checkedThisSession = true;
        var steps = new[]
        {
            ("¡Bienvenido a TextilCalc!", "Desde el inicio puede acceder a gramatura, metraje y la calculadora científica."),
            ("Calcular gramatura", "GSM = (Peso × 1000) / (Metraje × Ancho). Ingrese los valores manualmente."),
            ("Calcular metraje", "Metraje = (Peso × 1000) / (Gramatura × Ancho). Ingrese peso, GSM y ancho."),
            ("Calculadora", "Incluye modo científico, memoria, historial y unidades DEG/RAD/GRAD.")
        };

        foreach (var (title, message) in steps)
        {
            await dialogs.ShowAlertAsync(title, message, "Siguiente");
        }

        preferences.MarkNavigationTutorialAsSeen();
    }
}
