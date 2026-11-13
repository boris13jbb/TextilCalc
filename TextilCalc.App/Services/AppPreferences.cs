using System.Text.Json;
using TextilCalc.App.Models;

namespace TextilCalc.App.Services;

public sealed class AppPreferences : IAppPreferences
{
    private const string GramaturaKey = "gramatura_state";
    private const string MetrajeKey = "metraje_state";
    private const string CalculadoraKey = "calculadora_state";
    private const string NavigationTutorialKey = "tutorial_navigation_shown";

    public GramaturaSavedState LoadGramatura() =>
        Load(GramaturaKey, new GramaturaSavedState(string.Empty, string.Empty, string.Empty, null, null, null));

    public void SaveGramatura(GramaturaSavedState state) => Save(GramaturaKey, state);

    public void ClearGramatura() => Preferences.Default.Remove(GramaturaKey);

    public MetrajeSavedState LoadMetraje() =>
        Load(MetrajeKey, new MetrajeSavedState(string.Empty, string.Empty, string.Empty, null, null));

    public void SaveMetraje(MetrajeSavedState state) => Save(MetrajeKey, state);

    public void ClearMetraje() => Preferences.Default.Remove(MetrajeKey);

    public CalculadoraSavedState LoadCalculadora() =>
        Load(CalculadoraKey, new CalculadoraSavedState("0", string.Empty, 0, "DEG", false));

    public void SaveCalculadora(CalculadoraSavedState state) => Save(CalculadoraKey, state);

    public bool HasSeenNavigationTutorial() =>
        Preferences.Default.Get(NavigationTutorialKey, false);

    public void MarkNavigationTutorialAsSeen() =>
        Preferences.Default.Set(NavigationTutorialKey, true);

    private static T Load<T>(string key, T fallback)
    {
        var json = Preferences.Default.Get(key, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            return fallback;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json) ?? fallback;
        }
        catch (JsonException)
        {
            Preferences.Default.Remove(key);
            return fallback;
        }
    }

    private static void Save<T>(string key, T value) =>
        Preferences.Default.Set(key, JsonSerializer.Serialize(value));
}
