using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TextilCalc.App.Models;
using TextilCalc.App.Services;
using TextilCalc.Core.Services;

namespace TextilCalc.App.ViewModels;

public partial class CalculadoraViewModel : ObservableObject
{
    private readonly ExpressionEvaluator _evaluator;
    private readonly IAppPreferences _preferences;
    private int _parenthesesCount;

    [ObservableProperty] public partial string DisplayValue { get; set; } = "0";
    [ObservableProperty] public partial string PreviousExpression { get; set; } = string.Empty;
    [ObservableProperty] public partial double MemoryValue { get; set; }
    [ObservableProperty] public partial string AngleUnit { get; set; } = "DEG";
    [ObservableProperty] public partial bool IsExpanded { get; set; }

    public string ScientificModeLabel => IsExpanded ? "Ocultar funciones" : "Mostrar funciones";
    public bool HasMemory => Math.Abs(MemoryValue) > double.Epsilon;
    public string MemoryDisplay => $"M  {Format(MemoryValue)}";
    public bool IsError => DisplayValue == "Error";

    public CalculadoraViewModel(ExpressionEvaluator evaluator, IAppPreferences preferences)
    {
        _evaluator = evaluator;
        _preferences = preferences;
        var saved = preferences.LoadCalculadora();
        DisplayValue = saved.DisplayValue;
        PreviousExpression = saved.PreviousExpression;
        MemoryValue = saved.MemoryValue;
        AngleUnit = saved.AngleUnit;
        IsExpanded = saved.IsExpanded;
        _parenthesesCount = DisplayValue.Count(character => character == '(') -
                            DisplayValue.Count(character => character == ')');
    }

    partial void OnDisplayValueChanged(string value)
    {
        OnPropertyChanged(nameof(IsError));
        Save();
    }
    partial void OnPreviousExpressionChanged(string value) => Save();
    partial void OnMemoryValueChanged(double value)
    {
        OnPropertyChanged(nameof(HasMemory));
        OnPropertyChanged(nameof(MemoryDisplay));
        Save();
    }
    partial void OnAngleUnitChanged(string value) => Save();
    partial void OnIsExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(ScientificModeLabel));
        Save();
    }

    [RelayCommand]
    private void Input(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (value.Length == 1 && char.IsDigit(value[0]))
        {
            DisplayValue = DisplayValue is "0" or "Error" ? value : DisplayValue + value;
            return;
        }

        switch (value)
        {
            case ".":
                AppendDecimal();
                break;
            case "+":
            case "-":
            case "×":
            case "÷":
            case "^":
            case "mod":
                AppendOperator(value);
                break;
            case "(":
                DisplayValue = DisplayValue is "0" or "Error"
                    ? "("
                    : EndsWithOperator(DisplayValue) || DisplayValue.EndsWith('(')
                        ? DisplayValue + "("
                        : DisplayValue + "×(";
                _parenthesesCount++;
                break;
            case ")" when _parenthesesCount > 0 && !EndsWithOperator(DisplayValue):
                DisplayValue += ")";
                _parenthesesCount--;
                break;
            case "C":
                DisplayValue = "0";
                PreviousExpression = string.Empty;
                _parenthesesCount = 0;
                break;
            case "⌫":
                Backspace();
                break;
            case "=":
                Evaluate();
                break;
        }
    }

    [RelayCommand]
    private void ApplyFunction(string function)
    {
        if (function == "ANGLE")
        {
            AngleUnit = AngleUnit switch { "DEG" => "RAD", "RAD" => "GRAD", _ => "DEG" };
            return;
        }

        if (function == "TOGGLE")
        {
            IsExpanded = !IsExpanded;
            return;
        }

        if (function == "PI")
        {
            DisplayValue = Format(Math.PI);
            return;
        }

        if (function == "E")
        {
            DisplayValue = Format(Math.E);
            return;
        }

        if (function == "RND")
        {
            DisplayValue = Format(Random.Shared.NextDouble());
            return;
        }

        if (!TryCurrentValue(out var value))
        {
            return;
        }

        try
        {
            var result = function switch
            {
                "SIGN" => -value,
                "PERCENT" => value / 100d,
                "RECIPROCAL" when value != 0 => 1d / value,
                "SQRT" when value >= 0 => Math.Sqrt(value),
                "CBRT" => Math.Cbrt(value),
                "SQUARE" => Math.Pow(value, 2),
                "CUBE" => Math.Pow(value, 3),
                "FACTORIAL" => Factorial(value),
                "LOG" when value > 0 => Math.Log10(value),
                "LN" when value > 0 => Math.Log(value),
                "LOG2" when value > 0 => Math.Log2(value),
                "EXP" => Math.Exp(value),
                "TENPOWER" => Math.Pow(10, value),
                "TWOPOWER" => Math.Pow(2, value),
                "EPOWER" => Math.Exp(value),
                "SIN" => Math.Sin(ToRadians(value)),
                "COS" => Math.Cos(ToRadians(value)),
                "TAN" => Math.Tan(ToRadians(value)),
                "ASIN" when value is >= -1 and <= 1 => FromRadians(Math.Asin(value)),
                "ACOS" when value is >= -1 and <= 1 => FromRadians(Math.Acos(value)),
                "ATAN" => FromRadians(Math.Atan(value)),
                "SINH" => Math.Sinh(value),
                "COSH" => Math.Cosh(value),
                "TANH" => Math.Tanh(value),
                "ABS" => Math.Abs(value),
                "FLOOR" => Math.Floor(value),
                "CEIL" => Math.Ceiling(value),
                "ROUND" => Math.Round(value),
                _ => throw new ArithmeticException("Función no válida para el valor actual.")
            };
            DisplayValue = Format(result);
        }
        catch (Exception) when (function is not "TOGGLE")
        {
            DisplayValue = "Error";
        }
    }

    [RelayCommand]
    private void Memory(string operation)
    {
        switch (operation)
        {
            case "MC":
                MemoryValue = 0;
                break;
            case "MR":
                DisplayValue = Format(MemoryValue);
                break;
            case "M+" when TryCurrentValue(out var addend):
                MemoryValue += addend;
                break;
            case "M-" when TryCurrentValue(out var subtrahend):
                MemoryValue -= subtrahend;
                break;
        }
    }

    [RelayCommand]
    private static Task GoBackAsync() => Shell.Current.GoToAsync("..");

    private void Evaluate()
    {
        if (_parenthesesCount != 0 || DisplayValue is "0" or "Error" || EndsWithOperator(DisplayValue))
        {
            return;
        }

        try
        {
            var expression = DisplayValue;
            DisplayValue = Format(_evaluator.Evaluate(expression));
            PreviousExpression = expression;
        }
        catch
        {
            DisplayValue = "Error";
        }
    }

    private void AppendOperator(string value)
    {
        if (DisplayValue is "0" or "Error")
        {
            if (value == "-")
            {
                DisplayValue = "-";
            }
            return;
        }

        DisplayValue = EndsWithOperator(DisplayValue)
            ? DisplayValue[..^OperatorLength(DisplayValue)] + value
            : DisplayValue + value;
    }

    private void AppendDecimal()
    {
        var lastNumber = DisplayValue.Split(['+', '-', '×', '÷', '^', '(', ')']).LastOrDefault() ?? string.Empty;
        if (!lastNumber.Contains('.'))
        {
            DisplayValue += ".";
        }
    }

    private void Backspace()
    {
        if (DisplayValue.Length <= 1 || DisplayValue == "Error")
        {
            DisplayValue = "0";
            _parenthesesCount = 0;
            return;
        }

        if (DisplayValue.EndsWith('('))
        {
            _parenthesesCount--;
        }
        else if (DisplayValue.EndsWith(')'))
        {
            _parenthesesCount++;
        }

        DisplayValue = DisplayValue[..^1];
    }

    private bool TryCurrentValue(out double value) =>
        double.TryParse(DisplayValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
        double.IsFinite(value);

    private double ToRadians(double value) => AngleUnit switch
    {
        "RAD" => value,
        "GRAD" => value * Math.PI / 200d,
        _ => value * Math.PI / 180d
    };

    private double FromRadians(double value) => AngleUnit switch
    {
        "RAD" => value,
        "GRAD" => value * 200d / Math.PI,
        _ => value * 180d / Math.PI
    };

    private static double Factorial(double value)
    {
        if (value < 0 || value > 20 || value != Math.Truncate(value))
        {
            throw new ArithmeticException();
        }

        var result = 1d;
        for (var number = 2; number <= (int)value; number++)
        {
            result *= number;
        }
        return result;
    }

    private static string Format(double value)
    {
        if (!double.IsFinite(value))
        {
            return "Error";
        }

        return value == Math.Truncate(value)
            ? value.ToString("0", CultureInfo.InvariantCulture)
            : value.ToString("0.########", CultureInfo.InvariantCulture);
    }

    private static bool EndsWithOperator(string expression) =>
        expression.EndsWith("mod", StringComparison.Ordinal) ||
        expression.EndsWith('+') ||
        expression.EndsWith('-') ||
        expression.EndsWith('×') ||
        expression.EndsWith('÷') ||
        expression.EndsWith('^');

    private static int OperatorLength(string expression) =>
        expression.EndsWith("mod", StringComparison.Ordinal) ? 3 : 1;

    private void Save() =>
        _preferences.SaveCalculadora(
            new CalculadoraSavedState(DisplayValue, PreviousExpression, MemoryValue, AngleUnit, IsExpanded));
}
