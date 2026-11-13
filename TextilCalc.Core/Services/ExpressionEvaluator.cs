using System.Globalization;

namespace TextilCalc.Core.Services;

/// <summary>
/// Evaluador seguro de expresiones matemáticas (sin eval dinámico).
/// Soporta aritmética, potencias, módulo, constantes, notación científica y funciones.
/// </summary>
public sealed class ExpressionEvaluator
{
    /// <summary>
    /// Precedencia: +− binarios &lt; ×÷ &lt; unarios/+− y ^ (misma prioridad, asociatividad derecha).
    /// Así -2^2 → -(2^2) = -4 y 2^-3 → 2^(-3) = 0.125.
    /// </summary>
    private static readonly Dictionary<string, (int Precedence, bool RightAssociative)> Operators = new(StringComparer.Ordinal)
    {
        ["+"] = (1, false),
        ["-"] = (1, false),
        ["*"] = (2, false),
        ["/"] = (2, false),
        ["mod"] = (2, false),
        ["u+"] = (3, true),
        ["u-"] = (3, true),
        ["^"] = (3, true)
    };

    private static readonly HashSet<string> Functions = new(StringComparer.OrdinalIgnoreCase)
    {
        "sin", "cos", "tan", "asin", "acos", "atan",
        "sinh", "cosh", "tanh",
        "sqrt", "cbrt", "abs", "floor", "ceil", "round",
        "log", "ln", "log2", "exp", "fact"
    };

    private static readonly Dictionary<string, double> Constants = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pi"] = Math.PI,
        ["π"] = Math.PI,
        ["e"] = Math.E
    };

    public AngleUnit AngleMode { get; set; } = AngleUnit.Degrees;

    public double Evaluate(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        var normalized = Normalize(expression);
        var result = EvaluatePostfix(ToPostfix(Tokenize(normalized)));

        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            throw new ArithmeticException("El resultado no es un número finito.");
        }

        return result;
    }

    /// <summary>
    /// Evalúa una función unaria sobre un valor ya resuelto (botones científicos).
    /// </summary>
    public double ApplyFunction(string function, double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(function);
        return EvaluateFunction(function.Trim().ToLowerInvariant(), value);
    }

    private static string Normalize(string expression) =>
        expression
            .Replace("×", "*", StringComparison.Ordinal)
            .Replace("÷", "/", StringComparison.Ordinal)
            .Replace("−", "-", StringComparison.Ordinal)
            .Replace("π", "pi", StringComparison.Ordinal)
            .Trim();

    private static IReadOnlyList<string> Tokenize(string expression)
    {
        var tokens = new List<string>();
        var index = 0;

        while (index < expression.Length)
        {
            var current = expression[index];
            if (char.IsWhiteSpace(current))
            {
                index++;
                continue;
            }

            // Unario: inicio, tras '(', operador (incl. otro unario) o nombre de función.
            // No se fusiona con el literal para respetar -2^2 = -(2^2).
            var isUnarySign = (current is '+' or '-') &&
                              (tokens.Count == 0 ||
                               tokens[^1] == "(" ||
                               Operators.ContainsKey(tokens[^1]) ||
                               Functions.Contains(tokens[^1]));

            if (isUnarySign)
            {
                tokens.Add(current == '-' ? "u-" : "u+");
                index++;
                continue;
            }

            if (char.IsDigit(current) || current == '.')
            {
                var start = index++;

                while (index < expression.Length &&
                       (char.IsDigit(expression[index]) || expression[index] == '.'))
                {
                    index++;
                }

                // Notación científica: 1.5e-10 / 2E+3
                if (index < expression.Length && (expression[index] is 'e' or 'E'))
                {
                    var expStart = index;
                    index++;
                    if (index < expression.Length && expression[index] is '+' or '-')
                    {
                        index++;
                    }

                    var expDigits = 0;
                    while (index < expression.Length && char.IsDigit(expression[index]))
                    {
                        index++;
                        expDigits++;
                    }

                    if (expDigits == 0)
                    {
                        index = expStart;
                    }
                }

                var number = expression[start..index];
                if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    throw new FormatException($"Número inválido: {number}");
                }

                tokens.Add(number);
                continue;
            }

            if (index + 3 <= expression.Length &&
                expression.AsSpan(index, 3).Equals("mod", StringComparison.OrdinalIgnoreCase) &&
                (index + 3 >= expression.Length || !char.IsLetter(expression[index + 3])))
            {
                tokens.Add("mod");
                index += 3;
                continue;
            }

            if (char.IsLetter(current) || current == 'π')
            {
                var start = index++;
                while (index < expression.Length && (char.IsLetter(expression[index]) || expression[index] == '₂'))
                {
                    index++;
                }

                var identifier = expression[start..index];
                var canonical = identifier.Equals("log₂", StringComparison.OrdinalIgnoreCase) ||
                                identifier.Equals("log2", StringComparison.OrdinalIgnoreCase)
                    ? "log2"
                    : identifier.ToLowerInvariant();

                if (Constants.ContainsKey(canonical) || Constants.ContainsKey(identifier))
                {
                    tokens.Add(Constants.ContainsKey(canonical) ? canonical : identifier);
                    continue;
                }

                if (Functions.Contains(canonical))
                {
                    tokens.Add(canonical);
                    continue;
                }

                throw new FormatException($"Identificador no admitido: {identifier}");
            }

            var token = current.ToString();
            if (Operators.ContainsKey(token) || token is "(" or ")")
            {
                tokens.Add(token);
                index++;
                continue;
            }

            throw new FormatException($"Carácter no admitido: {current}");
        }

        return tokens;
    }

    private static IReadOnlyList<string> ToPostfix(IReadOnlyList<string> tokens)
    {
        var output = new List<string>();
        var stack = new Stack<string>();

        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];

            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _) ||
                Constants.ContainsKey(token))
            {
                output.Add(token);
                continue;
            }

            if (Functions.Contains(token))
            {
                stack.Push(token);
                continue;
            }

            if (Operators.TryGetValue(token, out var currentOperator))
            {
                while (stack.TryPeek(out var top) &&
                       Operators.TryGetValue(top, out var topOperator) &&
                       (topOperator.Precedence > currentOperator.Precedence ||
                        (topOperator.Precedence == currentOperator.Precedence && !currentOperator.RightAssociative)))
                {
                    output.Add(stack.Pop());
                }

                stack.Push(token);
                continue;
            }

            if (token == "(")
            {
                stack.Push(token);
                continue;
            }

            if (token == ")")
            {
                while (stack.TryPeek(out var top) && top != "(")
                {
                    output.Add(stack.Pop());
                }

                if (!stack.TryPop(out var opening) || opening != "(")
                {
                    throw new FormatException("Los paréntesis no están balanceados.");
                }

                if (stack.TryPeek(out var maybeFunction) && Functions.Contains(maybeFunction))
                {
                    output.Add(stack.Pop());
                }
            }
        }

        while (stack.Count > 0)
        {
            var token = stack.Pop();
            if (token is "(" or ")")
            {
                throw new FormatException("Los paréntesis no están balanceados.");
            }

            output.Add(token);
        }

        return output;
    }

    private double EvaluatePostfix(IReadOnlyList<string> tokens)
    {
        var values = new Stack<double>();

        foreach (var token in tokens)
        {
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                values.Push(number);
                continue;
            }

            if (Constants.TryGetValue(token, out var constant))
            {
                values.Push(constant);
                continue;
            }

            if (Functions.Contains(token))
            {
                if (values.Count < 1)
                {
                    throw new FormatException($"La función {token} requiere un argumento.");
                }

                values.Push(EvaluateFunction(token, values.Pop()));
                continue;
            }

            if (token is "u+" or "u-")
            {
                if (values.Count < 1)
                {
                    throw new FormatException("La expresión está incompleta.");
                }

                var operand = values.Pop();
                values.Push(token == "u-" ? -operand : operand);
                continue;
            }

            if (values.Count < 2)
            {
                throw new FormatException("La expresión está incompleta.");
            }

            var right = values.Pop();
            var left = values.Pop();
            var result = token switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                "/" when right != 0 => left / right,
                "mod" when right != 0 => left % right,
                "^" when Math.Abs(right) <= 1000 => Math.Pow(left, right),
                "/" or "mod" => throw new DivideByZeroException("No se puede dividir entre cero."),
                "^" => throw new ArithmeticException("El exponente está fuera del rango admitido."),
                _ => throw new FormatException($"Operador no admitido: {token}")
            };

            if (double.IsNaN(result) || double.IsInfinity(result))
            {
                throw new ArithmeticException("El resultado no es un número finito.");
            }

            values.Push(result);
        }

        return values.Count == 1
            ? values.Pop()
            : throw new FormatException("La expresión no es válida.");
    }

    private double EvaluateFunction(string function, double value)
    {
        try
        {
            var result = function switch
            {
                "sin" => Math.Sin(ToRadians(value)),
                "cos" => Math.Cos(ToRadians(value)),
                "tan" => EvaluateTan(value),
                "asin" when value is >= -1 and <= 1 => FromRadians(Math.Asin(value)),
                "acos" when value is >= -1 and <= 1 => FromRadians(Math.Acos(value)),
                "atan" => FromRadians(Math.Atan(value)),
                "sinh" => Math.Sinh(value),
                "cosh" => Math.Cosh(value),
                "tanh" => Math.Tanh(value),
                "sqrt" when value >= 0 => Math.Sqrt(value),
                "cbrt" => Math.Cbrt(value),
                "abs" => Math.Abs(value),
                "floor" => Math.Floor(value),
                "ceil" => Math.Ceiling(value),
                "round" => Math.Round(value, MidpointRounding.AwayFromZero),
                "log" when value > 0 => Math.Log10(value),
                "ln" when value > 0 => Math.Log(value),
                "log2" when value > 0 => Math.Log2(value),
                "exp" => Math.Exp(value),
                "fact" => Factorial(value),
                "asin" or "acos" => throw new ArithmeticException("El argumento de la inversa trigonométrica debe estar entre -1 y 1."),
                "sqrt" => throw new ArithmeticException("No se puede calcular la raíz cuadrada de un número negativo."),
                "log" or "ln" or "log2" => throw new ArithmeticException("El argumento del logaritmo debe ser mayor que cero."),
                _ => throw new FormatException($"Función no admitida: {function}")
            };

            if (double.IsNaN(result) || double.IsInfinity(result))
            {
                throw new ArithmeticException("El resultado no es un número finito.");
            }

            return result;
        }
        catch (OverflowException)
        {
            throw new ArithmeticException("El resultado está fuera de rango.");
        }
    }

    private double EvaluateTan(double value)
    {
        var radians = ToRadians(value);
        var cos = Math.Cos(radians);
        if (Math.Abs(cos) < 1e-14)
        {
            var unit = AngleMode switch
            {
                AngleUnit.Radians => "rad",
                AngleUnit.Gradians => "grad",
                _ => "°"
            };
            throw new ArithmeticException($"tan({FormatAngle(value)}{unit}) no está definida.");
        }

        var result = Math.Tan(radians);
        if (double.IsNaN(result) || double.IsInfinity(result) || Math.Abs(result) > 1e16)
        {
            var unit = AngleMode switch
            {
                AngleUnit.Radians => "rad",
                AngleUnit.Gradians => "grad",
                _ => "°"
            };
            throw new ArithmeticException($"tan({FormatAngle(value)}{unit}) no está definida.");
        }

        return result;
    }

    private double ToRadians(double value) => AngleMode switch
    {
        AngleUnit.Radians => value,
        AngleUnit.Gradians => value * Math.PI / 200d,
        _ => value * Math.PI / 180d
    };

    private double FromRadians(double value) => AngleMode switch
    {
        AngleUnit.Radians => value,
        AngleUnit.Gradians => value * 200d / Math.PI,
        _ => value * 180d / Math.PI
    };

    private static double Factorial(double value)
    {
        if (value < 0 || value != Math.Truncate(value))
        {
            throw new ArithmeticException("El factorial solo admite enteros no negativos.");
        }

        if (value > 170)
        {
            throw new ArithmeticException("El factorial supera el rango numérico admitido.");
        }

        var result = 1d;
        for (var number = 2; number <= (int)value; number++)
        {
            result *= number;
        }

        return result;
    }

    private static string FormatAngle(double value) =>
        value.ToString("0.##########", CultureInfo.InvariantCulture);
}
