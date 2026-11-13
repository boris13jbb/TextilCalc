using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TextilCalc.App.Models;
using TextilCalc.App.Services;
using TextilCalc.Core.Services;

namespace TextilCalc.App.ViewModels;

public partial class MetrajeViewModel : ObservableObject
{
    private readonly IAppPreferences _preferences;

    [ObservableProperty] public partial string WeightText { get; set; } = string.Empty;
    [ObservableProperty] public partial string GsmText { get; set; } = string.Empty;
    [ObservableProperty] public partial string WidthText { get; set; } = string.Empty;
    [ObservableProperty] public partial double? Metraje { get; set; }
    [ObservableProperty] public partial double? AreaM2 { get; set; }
    [ObservableProperty] public partial string? Error { get; set; }

    public MetrajeViewModel(IAppPreferences preferences)
    {
        _preferences = preferences;
        var saved = preferences.LoadMetraje();
        WeightText = saved.WeightText;
        GsmText = saved.GsmText;
        WidthText = saved.WidthText;
        Metraje = saved.Metraje;
        AreaM2 = saved.AreaM2;
    }

    public bool HasResult => Metraje.HasValue;
    public bool HasNoResult => !HasResult;
    public bool HasError => !string.IsNullOrWhiteSpace(Error);
    public string MetrajeDisplay => Metraje?.ToString("N2") ?? "--";
    public string AreaDisplay => AreaM2?.ToString("N2") ?? "--";

    partial void OnWeightTextChanged(string value) => Save();
    partial void OnGsmTextChanged(string value) => Save();
    partial void OnWidthTextChanged(string value) => Save();
    partial void OnErrorChanged(string? value) => OnPropertyChanged(nameof(HasError));

    [RelayCommand]
    private void Calculate()
    {
        if (!LocalizedNumberParser.TryParse(WeightText, out var weight) || weight <= 0)
        {
            Error = "Ingrese un peso válido (> 0).";
            return;
        }

        if (!LocalizedNumberParser.TryParse(GsmText, out var gsm) || gsm <= 0)
        {
            Error = "Ingrese una gramatura válida (> 0).";
            return;
        }

        if (!LocalizedNumberParser.TryParse(WidthText, out var width) || width <= 0)
        {
            Error = "Ingrese un ancho válido (> 0).";
            return;
        }

        var result = TextileCalculator.CalculateMetraje(weight, gsm, width);
        Metraje = result.Metraje;
        AreaM2 = result.AreaM2;
        Error = null;
        NotifyResults();
        Save();
    }

    [RelayCommand]
    private void Clear()
    {
        WeightText = string.Empty;
        GsmText = string.Empty;
        WidthText = string.Empty;
        Metraje = null;
        AreaM2 = null;
        Error = null;
        NotifyResults();
        _preferences.ClearMetraje();
    }

    [RelayCommand]
    private static Task GoBackAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private static Task OpenCalculatorAsync() => Shell.Current.GoToAsync(nameof(Views.CalculadoraPage));

    private void Save() =>
        _preferences.SaveMetraje(new MetrajeSavedState(WeightText, GsmText, WidthText, Metraje, AreaM2));

    private void NotifyResults()
    {
        OnPropertyChanged(nameof(HasResult));
        OnPropertyChanged(nameof(HasNoResult));
        OnPropertyChanged(nameof(MetrajeDisplay));
        OnPropertyChanged(nameof(AreaDisplay));
    }
}
