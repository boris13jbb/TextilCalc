using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TextilCalc.App.Models;
using TextilCalc.App.Services;
using TextilCalc.Core.Services;

namespace TextilCalc.App.ViewModels;

public partial class GramaturaViewModel : ObservableObject
{
    private readonly IAppPreferences _preferences;
    private bool _isInitializing = true;

    [ObservableProperty] public partial string LengthText { get; set; } = string.Empty;
    [ObservableProperty] public partial string WidthText { get; set; } = string.Empty;
    [ObservableProperty] public partial string WeightText { get; set; } = string.Empty;
    [ObservableProperty] public partial double? AreaM2 { get; set; }
    [ObservableProperty] public partial double? Gsm { get; set; }
    [ObservableProperty] public partial double? Rendimiento { get; set; }
    [ObservableProperty] public partial string? Error { get; set; }

    public GramaturaViewModel(IAppPreferences preferences)
    {
        _preferences = preferences;
        var saved = preferences.LoadGramatura();
        LengthText = saved.LengthText;
        WidthText = saved.WidthText;
        WeightText = saved.WeightText;
        AreaM2 = saved.AreaM2;
        Gsm = saved.Gsm;
        Rendimiento = saved.Rendimiento;
        _isInitializing = false;
    }

    public bool HasResult => Gsm.HasValue;
    public bool HasNoResult => !HasResult;
    public bool HasError => !string.IsNullOrWhiteSpace(Error);
    /// <summary>Presentación de GSM con exactamente 2 decimales. Área/rendimiento se calculan internamente pero no se muestran.</summary>
    public string GsmDisplay => Gsm?.ToString("N2") ?? "--";

    partial void OnLengthTextChanged(string value) => InvalidateAndSave();
    partial void OnWidthTextChanged(string value) => InvalidateAndSave();
    partial void OnWeightTextChanged(string value) => InvalidateAndSave();
    partial void OnErrorChanged(string? value) => OnPropertyChanged(nameof(HasError));

    [RelayCommand]
    private void Calculate()
    {
        if (!LocalizedNumberParser.TryParse(LengthText, out var length) || length <= 0)
        {
            ClearComputedResults();
            Error = "Ingrese un metraje válido (> 0).";
            return;
        }

        if (!LocalizedNumberParser.TryParse(WidthText, out var width) || width <= 0)
        {
            ClearComputedResults();
            Error = "Ingrese un ancho válido (> 0).";
            return;
        }

        if (!LocalizedNumberParser.TryParse(WeightText, out var weight) || weight <= 0)
        {
            ClearComputedResults();
            Error = "Ingrese un peso válido (> 0).";
            return;
        }

        var result = TextileCalculator.CalculateGramatura(length, width, weight);
        AreaM2 = result.AreaM2;
        Gsm = result.Gsm;
        Rendimiento = result.Rendimiento;
        Error = null;
        NotifyResults();
        Save();
    }

    [RelayCommand]
    private void Clear()
    {
        LengthText = string.Empty;
        WidthText = string.Empty;
        WeightText = string.Empty;
        AreaM2 = null;
        Gsm = null;
        Rendimiento = null;
        Error = null;
        NotifyResults();
        _preferences.ClearGramatura();
    }

    [RelayCommand]
    private static Task GoBackAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private static Task OpenCalculatorAsync() => Shell.Current.GoToAsync(nameof(Views.CalculadoraPage));

    private void Save() =>
        _preferences.SaveGramatura(
            new GramaturaSavedState(LengthText, WidthText, WeightText, AreaM2, Gsm, Rendimiento));

    /// <summary>
    /// Al editar entradas se invalidan resultados previos para no mostrar valores obsoletos.
    /// </summary>
    private void InvalidateAndSave()
    {
        if (_isInitializing)
        {
            return;
        }

        ClearComputedResults();
        Error = null;
        Save();
    }

    private void ClearComputedResults()
    {
        if (AreaM2 is null && Gsm is null && Rendimiento is null)
        {
            return;
        }

        AreaM2 = null;
        Gsm = null;
        Rendimiento = null;
        NotifyResults();
    }

    private void NotifyResults()
    {
        OnPropertyChanged(nameof(HasResult));
        OnPropertyChanged(nameof(HasNoResult));
        OnPropertyChanged(nameof(GsmDisplay));
    }
}
