using TextilCalc.App.Models;

namespace TextilCalc.App.Services;

public interface IAppPreferences
{
    GramaturaSavedState LoadGramatura();
    void SaveGramatura(GramaturaSavedState state);
    void ClearGramatura();
    MetrajeSavedState LoadMetraje();
    void SaveMetraje(MetrajeSavedState state);
    void ClearMetraje();
    CalculadoraSavedState LoadCalculadora();
    void SaveCalculadora(CalculadoraSavedState state);
    bool HasSeenNavigationTutorial();
    void MarkNavigationTutorialAsSeen();
}
