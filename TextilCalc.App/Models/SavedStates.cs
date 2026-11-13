namespace TextilCalc.App.Models;

public sealed record GramaturaSavedState(
    string LengthText,
    string WidthText,
    string WeightText,
    double? AreaM2,
    double? Gsm,
    double? Rendimiento);

public sealed record MetrajeSavedState(
    string WeightText,
    string GsmText,
    string WidthText,
    double? Metraje,
    double? AreaM2);

public sealed record CalculadoraSavedState(
    string DisplayValue,
    string PreviousExpression,
    double MemoryValue,
    string AngleUnit,
    bool IsExpanded);
