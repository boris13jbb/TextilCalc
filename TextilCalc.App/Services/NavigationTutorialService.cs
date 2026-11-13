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
            ("¡Bienvenido a TextilCalc!", "Desde el menú puede acceder a todas las herramientas textiles."),
            ("Calcular gramatura", "GSM = (Peso × 1000) / (Metraje × Ancho)."),
            ("Calcular metraje", "Metraje = (Peso × 1000) / (Gramatura × Ancho)."),
            ("Gestión de telas", "Puede administrar el catálogo y respaldarlo mediante archivos Excel."),
            ("Calculadora", "Incluye modos básico y científico, memoria y unidades angulares.")
        };

        foreach (var (title, message) in steps)
        {
            await dialogs.ShowAlertAsync(title, message, "Siguiente");
        }

        preferences.MarkNavigationTutorialAsSeen();
    }
}
