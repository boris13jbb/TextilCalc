using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TextilCalc.App.Models;
using TextilCalc.App.Services;
using TextilCalc.Core.Models;
using TextilCalc.Core.Services;

namespace TextilCalc.App.ViewModels;

public partial class CalculadoraViewModel : ObservableObject
{
    private readonly ExpressionEvaluator _evaluator;
    private readonly IAppPreferences _preferences;
    private readonly ICalculatorHistoryService _history;
    private readonly IUserDialogService _dialogs;

    private int _parenthesesCount;
    private bool _justEvaluated;
    private bool _overwriteOnNextDigit;
    private double? _lastResult;

    [ObservableProperty] public partial string DisplayValue { get; set; } = "0";
    [ObservableProperty] public partial string PreviousExpression { get; set; } = string.Empty;
    [ObservableProperty] public partial string ErrorMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial double MemoryValue { get; set; }
    [ObservableProperty] public partial string AngleUnitLabel { get; set; } = "DEG";
    [ObservableProperty] public partial bool IsExpanded { get; set; }
    [ObservableProperty] public partial bool IsHistoryVisible { get; set; }

    public CalculadoraViewModel(
        ExpressionEvaluator evaluator,
        IAppPreferences preferences,
        ICalculatorHistoryService history,
        IUserDialogService dialogs)
    {
        _evaluator = evaluator;
        _preferences = preferences;
        _history = history;
        _dialogs = dialogs;

        var saved = preferences.LoadCalculadora();
        DisplayValue = string.IsNullOrWhiteSpace(saved.DisplayValue) ? "0" : saved.DisplayValue;
        PreviousExpression = saved.PreviousExpression;
        MemoryValue = saved.MemoryValue;
        AngleUnitLabel = string.IsNullOrWhiteSpace(saved.AngleUnit) ? "DEG" : saved.AngleUnit;
        IsExpanded = saved.IsExpanded;
        ApplyAngleMode();
        _parenthesesCount = CountOpenParentheses(DisplayValue);
        if (TryParseDisplay(out var current))
        {
            _lastResult = current;
        }
    }

    public ObservableCollection<CalculatorHistoryEntry> History => _history.Entries;
    public bool HasHistory => History.Count > 0;
    public string ScientificModeLabel => IsExpanded ? "Ocultar científicas" : "Mostrar científicas";
    public bool HasMemory => Math.Abs(MemoryValue) > double.Epsilon;
    public string MemoryDisplay => $"M  {NumberFormatter.Format(MemoryValue)}";
    public bool IsError => !string.IsNullOrWhiteSpace(ErrorMessage) ||
                           DisplayValue.StartsWith("Error", StringComparison.OrdinalIgnoreCase);
    public string DisplayTextColorHint => IsError ? "error" : "normal";

    partial void OnDisplayValueChanged(string value)
    {
        OnPropertyChanged(nameof(IsError));
        Save();
    }

    partial void OnPreviousExpressionChanged(string value) => Save();
    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(IsError));
    partial void OnMemoryValueChanged(double value)
    {
        OnPropertyChanged(nameof(HasMemory));
        OnPropertyChanged(nameof(MemoryDisplay));
        Save();
    }

    partial void OnAngleUnitLabelChanged(string value)
    {
        ApplyAngleMode();
        Save();
    }

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

        ClearError();

        if (value.Length == 1 && char.IsDigit(value[0]))
        {
            AppendDigit(value);
            return;
        }

        switch (value)
        {
            case ".":
            case ",":
                AppendDecimal();
                break;
            case "+":
            case "-":
            case "×":
            case "÷":
            case "*":
            case "/":
            case "^":
            case "mod":
                AppendOperator(NormalizeOperator(value));
                break;
            case "(":
                AppendOpenParenthesis();
                break;
            case ")":
                AppendCloseParenthesis();
                break;
            case "%":
                ApplyPercent();
                break;
            case "C":
            case "AC":
            case "Escape":
                AllClear();
                break;
            case "⌫":
            case "Backspace":
                Backspace();
                break;
            case "=":
            case "Enter":
                Evaluate();
                break;
        }
    }

    [RelayCommand]
    private void ApplyFunction(string function)
    {
        ClearError();

        if (function == "ANGLE")
        {
            AngleUnitLabel = AngleUnitLabel switch
            {
                "DEG" => "RAD",
                "RAD" => "GRAD",
                _ => "DEG"
            };
            return;
        }

        if (function == "TOGGLE")
        {
            IsExpanded = !IsExpanded;
            return;
        }

        if (function == "HISTORY")
        {
            IsHistoryVisible = !IsHistoryVisible;
            OnPropertyChanged(nameof(HasHistory));
            return;
        }

        if (function is "PI" or "E")
        {
            var constant = function == "PI" ? Math.PI : Math.E;
            if (_justEvaluated || _overwriteOnNextDigit || DisplayValue is "0" || IsError)
            {
                DisplayValue = NumberFormatter.Format(constant);
            }
            else if (EndsWithOperator(DisplayValue) || DisplayValue.EndsWith('('))
            {
                DisplayValue += NumberFormatter.Format(constant);
            }
            else
            {
                DisplayValue = NumberFormatter.Format(constant);
            }

            _justEvaluated = false;
            _overwriteOnNextDigit = false;
            _lastResult = constant;
            return;
        }

        if (!TryResolveCurrentValue(out var value))
        {
            SetError("No hay un valor válido para aplicar la función.");
            return;
        }

        try
        {
            var result = function switch
            {
                "SIGN" => -value,
                "PERCENT" => ApplyPercentOnValue(value),
                "RECIPROCAL" => value == 0
                    ? throw new DivideByZeroException("No se puede calcular 1/0.")
                    : 1d / value,
                "SQRT" => _evaluator.ApplyFunction("sqrt", value),
                "CBRT" => _evaluator.ApplyFunction("cbrt", value),
                "SQUARE" => Math.Pow(value, 2),
                "CUBE" => Math.Pow(value, 3),
                "FACTORIAL" => _evaluator.ApplyFunction("fact", value),
                "LOG" => _evaluator.ApplyFunction("log", value),
                "LN" => _evaluator.ApplyFunction("ln", value),
                "LOG2" => _evaluator.ApplyFunction("log2", value),
                "EXP" or "EPOWER" => Math.Exp(value),
                "TENPOWER" => Math.Pow(10, value),
                "TWOPOWER" => Math.Pow(2, value),
                "SIN" => _evaluator.ApplyFunction("sin", value),
                "COS" => _evaluator.ApplyFunction("cos", value),
                "TAN" => _evaluator.ApplyFunction("tan", value),
                "ASIN" => _evaluator.ApplyFunction("asin", value),
                "ACOS" => _evaluator.ApplyFunction("acos", value),
                "ATAN" => _evaluator.ApplyFunction("atan", value),
                "SINH" => _evaluator.ApplyFunction("sinh", value),
                "COSH" => _evaluator.ApplyFunction("cosh", value),
                "TANH" => _evaluator.ApplyFunction("tanh", value),
                "ABS" => _evaluator.ApplyFunction("abs", value),
                "FLOOR" => _evaluator.ApplyFunction("floor", value),
                "CEIL" => _evaluator.ApplyFunction("ceil", value),
                "ROUND" => _evaluator.ApplyFunction("round", value),
                _ => throw new ArithmeticException("Función no válida para el valor actual.")
            };

            PreviousExpression = $"{function}({NumberFormatter.Format(value)})";
            DisplayValue = NumberFormatter.Format(result);
            _lastResult = result;
            _justEvaluated = true;
            _overwriteOnNextDigit = true;
            _parenthesesCount = 0;
            _history.Add(PreviousExpression, DisplayValue);
            OnPropertyChanged(nameof(HasHistory));
        }
        catch (Exception ex)
        {
            SetError(MapException(ex));
        }
    }

    [RelayCommand]
    private void Memory(string operation)
    {
        ClearError();
        switch (operation)
        {
            case "MC":
                MemoryValue = 0;
                break;
            case "MR":
                DisplayValue = NumberFormatter.Format(MemoryValue);
                _lastResult = MemoryValue;
                _justEvaluated = true;
                _overwriteOnNextDigit = true;
                break;
            case "M+" when TryResolveCurrentValue(out var addend):
                MemoryValue += addend;
                break;
            case "M-" when TryResolveCurrentValue(out var subtrahend):
                MemoryValue -= subtrahend;
                break;
        }
    }

    [RelayCommand]
    private void ReuseHistory(CalculatorHistoryEntry? entry)
    {
        if (entry is null)
        {
            return;
        }

        ClearError();
        DisplayValue = entry.Result;
        PreviousExpression = entry.Expression;
        if (double.TryParse(entry.Result, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            _lastResult = value;
        }

        _justEvaluated = true;
        _overwriteOnNextDigit = true;
        _parenthesesCount = 0;
        IsHistoryVisible = false;
    }

    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        if (!HasHistory)
        {
            return;
        }

        var confirmed = await _dialogs.ConfirmAsync(
            "Borrar historial",
            "¿Desea eliminar las últimas operaciones guardadas?",
            "Borrar",
            "Cancelar");
        if (!confirmed)
        {
            return;
        }

        _history.Clear();
        OnPropertyChanged(nameof(HasHistory));
        IsHistoryVisible = false;
    }

    [RelayCommand]
    private void HandleKey(string key) => Input(key);

    [RelayCommand]
    private static Task GoBackAsync() => Shell.Current.GoToAsync("..");

    private void AppendDigit(string digit)
    {
        if (IsError || _overwriteOnNextDigit || DisplayValue is "0")
        {
            DisplayValue = digit;
            _overwriteOnNextDigit = false;
            _justEvaluated = false;
            _parenthesesCount = 0;
            return;
        }

        DisplayValue += digit;
    }

    private void AppendDecimal()
    {
        if (IsError || _overwriteOnNextDigit)
        {
            DisplayValue = "0.";
            _overwriteOnNextDigit = false;
            _justEvaluated = false;
            return;
        }

        var lastNumber = GetLastNumberToken(DisplayValue);
        if (!lastNumber.Contains('.', StringComparison.Ordinal))
        {
            DisplayValue += EndsWithOperator(DisplayValue) || DisplayValue.EndsWith('(') ? "0." : ".";
        }
    }

    private void AppendOperator(string op)
    {
        if (IsError)
        {
            if (op == "-" && _lastResult is null)
            {
                DisplayValue = "-";
                _overwriteOnNextDigit = false;
                _justEvaluated = false;
            }
            else if (_lastResult is double last)
            {
                DisplayValue = NumberFormatter.Format(last) + op;
                _overwriteOnNextDigit = false;
                _justEvaluated = false;
            }

            return;
        }

        if (_justEvaluated && _lastResult is double result)
        {
            DisplayValue = NumberFormatter.Format(result) + op;
            _justEvaluated = false;
            _overwriteOnNextDigit = false;
            return;
        }

        if (DisplayValue is "0" or "")
        {
            if (op == "-")
            {
                DisplayValue = "-";
            }

            return;
        }

        DisplayValue = EndsWithOperator(DisplayValue)
            ? DisplayValue[..^OperatorLength(DisplayValue)] + op
            : DisplayValue + op;
        _overwriteOnNextDigit = false;
        _justEvaluated = false;
    }

    private void AppendOpenParenthesis()
    {
        if (IsError || _overwriteOnNextDigit || DisplayValue is "0")
        {
            DisplayValue = "(";
            _parenthesesCount = 1;
            _overwriteOnNextDigit = false;
            _justEvaluated = false;
            return;
        }

        if (EndsWithOperator(DisplayValue) || DisplayValue.EndsWith('('))
        {
            DisplayValue += "(";
        }
        else
        {
            DisplayValue += "×(";
        }

        _parenthesesCount++;
    }

    private void AppendCloseParenthesis()
    {
        if (_parenthesesCount <= 0 || EndsWithOperator(DisplayValue) || DisplayValue.EndsWith('(') || IsError)
        {
            return;
        }

        DisplayValue += ")";
        _parenthesesCount--;
    }

    private void ApplyPercent()
    {
        if (!TryResolveCurrentValue(out var current))
        {
            SetError("No hay un valor válido para calcular el porcentaje.");
            return;
        }

        try
        {
            var result = ApplyPercentOnValue(current);
            PreviousExpression = $"{DisplayValue}%";
            DisplayValue = NumberFormatter.Format(result);
            _lastResult = result;
            _justEvaluated = true;
            _overwriteOnNextDigit = true;
            _parenthesesCount = 0;
            _history.Add(PreviousExpression, DisplayValue);
            OnPropertyChanged(nameof(HasHistory));
        }
        catch (Exception ex)
        {
            SetError(MapException(ex));
        }
    }

    /// <summary>
    /// Porcentaje contextual: 200×10% → 20; 200+10% → 220; valor suelto → /100.
    /// </summary>
    private double ApplyPercentOnValue(double current)
    {
        if (_justEvaluated || IsSimpleNumber(DisplayValue))
        {
            return current / 100d;
        }

        if (!TrySplitBinary(DisplayValue, out var left, out var op, out var right))
        {
            return current / 100d;
        }

        var percent = right / 100d;
        return op switch
        {
            "+" => left + (left * percent),
            "-" => left - (left * percent),
            "×" or "*" => left * percent,
            "÷" or "/" => percent == 0
                ? throw new DivideByZeroException("No se puede dividir entre cero.")
                : left / percent,
            _ => current / 100d
        };
    }

    private void Evaluate()
    {
        if (IsError)
        {
            return;
        }

        if (_parenthesesCount != 0)
        {
            SetError("Los paréntesis no están balanceados.");
            return;
        }

        if (DisplayValue is "0" || EndsWithOperator(DisplayValue) || DisplayValue.EndsWith('('))
        {
            return;
        }

        try
        {
            var expression = DisplayValue;
            var result = _evaluator.Evaluate(ToEvaluatorExpression(expression));
            PreviousExpression = expression;
            DisplayValue = NumberFormatter.Format(result);
            _lastResult = result;
            _justEvaluated = true;
            _overwriteOnNextDigit = true;
            _parenthesesCount = 0;
            _history.Add(expression, DisplayValue);
            OnPropertyChanged(nameof(HasHistory));
        }
        catch (Exception ex)
        {
            SetError(MapException(ex));
        }
    }

    private void AllClear()
    {
        DisplayValue = "0";
        PreviousExpression = string.Empty;
        ErrorMessage = string.Empty;
        _parenthesesCount = 0;
        _justEvaluated = false;
        _overwriteOnNextDigit = false;
        _lastResult = null;
    }

    private void Backspace()
    {
        if (IsError || _overwriteOnNextDigit)
        {
            AllClear();
            return;
        }

        if (DisplayValue.Length <= 1)
        {
            DisplayValue = "0";
            _parenthesesCount = 0;
            return;
        }

        if (DisplayValue.EndsWith("mod", StringComparison.Ordinal))
        {
            DisplayValue = DisplayValue[..^3];
            return;
        }

        if (DisplayValue.EndsWith('('))
        {
            _parenthesesCount = Math.Max(0, _parenthesesCount - 1);
        }
        else if (DisplayValue.EndsWith(')'))
        {
            _parenthesesCount++;
        }

        DisplayValue = DisplayValue[..^1];
        if (string.IsNullOrEmpty(DisplayValue))
        {
            DisplayValue = "0";
        }
    }

    private bool TryResolveCurrentValue(out double value)
    {
        if (TryParseDisplay(out value))
        {
            return true;
        }

        if (_justEvaluated && _lastResult is double last)
        {
            value = last;
            return true;
        }

        try
        {
            if (!EndsWithOperator(DisplayValue) && _parenthesesCount == 0 && DisplayValue is not "0")
            {
                value = _evaluator.Evaluate(ToEvaluatorExpression(DisplayValue));
                return true;
            }
        }
        catch
        {
            // Se reporta en el llamador.
        }

        value = default;
        return false;
    }

    private bool TryParseDisplay(out double value) =>
        double.TryParse(DisplayValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
        double.IsFinite(value);

    private void ApplyAngleMode()
    {
        _evaluator.AngleMode = AngleUnitLabel switch
        {
            "RAD" => AngleUnit.Radians,
            "GRAD" => AngleUnit.Gradians,
            _ => AngleUnit.Degrees
        };
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        DisplayValue = "Error";
        _overwriteOnNextDigit = true;
        _justEvaluated = false;
    }

    private void ClearError()
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ErrorMessage = string.Empty;
        }
    }

    private void Save() =>
        _preferences.SaveCalculadora(
            new CalculadoraSavedState(DisplayValue, PreviousExpression, MemoryValue, AngleUnitLabel, IsExpanded));

    private static string NormalizeOperator(string value) => value switch
    {
        "*" => "×",
        "/" => "÷",
        _ => value
    };

    private static string ToEvaluatorExpression(string expression) =>
        expression.Replace("×", "*", StringComparison.Ordinal).Replace("÷", "/", StringComparison.Ordinal);

    private static bool EndsWithOperator(string expression) =>
        expression.EndsWith("mod", StringComparison.Ordinal) ||
        expression.EndsWith('+') ||
        expression.EndsWith('-') ||
        expression.EndsWith('×') ||
        expression.EndsWith('÷') ||
        expression.EndsWith('*') ||
        expression.EndsWith('/') ||
        expression.EndsWith('^');

    private static int OperatorLength(string expression) =>
        expression.EndsWith("mod", StringComparison.Ordinal) ? 3 : 1;

    private static int CountOpenParentheses(string expression) =>
        expression.Count(c => c == '(') - expression.Count(c => c == ')');

    private static bool IsSimpleNumber(string expression) =>
        double.TryParse(expression, NumberStyles.Float, CultureInfo.InvariantCulture, out _);

    private static string GetLastNumberToken(string expression)
    {
        var parts = expression.Split(['+', '-', '×', '÷', '*', '/', '^', '(', ')'], StringSplitOptions.None);
        return parts.LastOrDefault() ?? string.Empty;
    }

    private static bool TrySplitBinary(string expression, out double left, out string op, out double right)
    {
        left = 0;
        right = 0;
        op = string.Empty;

        for (var i = expression.Length - 2; i >= 1; i--)
        {
            var candidate = expression[i] switch
            {
                '×' or '*' or '÷' or '/' or '+' or '-' => expression[i].ToString(),
                _ => null
            };
            if (candidate is null)
            {
                continue;
            }

            var leftText = expression[..i];
            var rightText = expression[(i + 1)..];
            if (string.IsNullOrWhiteSpace(leftText) ||
                string.IsNullOrWhiteSpace(rightText) ||
                EndsWithOperator(leftText) ||
                leftText.EndsWith('('))
            {
                continue;
            }

            if (double.TryParse(leftText, NumberStyles.Float, CultureInfo.InvariantCulture, out left) &&
                double.TryParse(rightText, NumberStyles.Float, CultureInfo.InvariantCulture, out right))
            {
                op = candidate;
                return true;
            }
        }

        return false;
    }

    private static string MapException(Exception ex) => ex switch
    {
        DivideByZeroException => string.IsNullOrWhiteSpace(ex.Message)
            ? "No se puede dividir entre cero."
            : ex.Message,
        ArithmeticException => string.IsNullOrWhiteSpace(ex.Message)
            ? "Operación matemática no válida."
            : ex.Message,
        FormatException => string.IsNullOrWhiteSpace(ex.Message)
            ? "La expresión no es válida."
            : ex.Message,
        _ => "No se pudo completar la operación."
    };
}
