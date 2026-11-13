using System.Globalization;

namespace TextilCalc.Core.Services;

public sealed class ExpressionEvaluator
{
    private static readonly Dictionary<string, (int Precedence, bool RightAssociative)> Operators = new()
    {
        ["+"] = (1, false),
        ["-"] = (1, false),
        ["*"] = (2, false),
        ["/"] = (2, false),
        ["mod"] = (2, false),
        ["^"] = (3, true)
    };

    public double Evaluate(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        var normalized = expression.Replace("×", "*").Replace("÷", "/");
        var result = EvaluatePostfix(ToPostfix(Tokenize(normalized)));

        if (!double.IsFinite(result))
        {
            throw new ArithmeticException("El resultado no es un número finito.");
        }

        return result;
    }

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

            var unarySign = (current is '+' or '-') &&
                            (tokens.Count == 0 || tokens[^1] == "(" || Operators.ContainsKey(tokens[^1]));
            if (char.IsDigit(current) || current == '.' || unarySign)
            {
                var start = index++;
                while (index < expression.Length &&
                       (char.IsDigit(expression[index]) || expression[index] == '.'))
                {
                    index++;
                }

                var number = expression[start..index];
                if (number is "+" or "-" ||
                    !double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    throw new FormatException($"Número inválido: {number}");
                }

                tokens.Add(number);
                continue;
            }

            if (index + 3 <= expression.Length &&
                expression.AsSpan(index, 3).Equals("mod", StringComparison.OrdinalIgnoreCase))
            {
                tokens.Add("mod");
                index += 3;
                continue;
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

        foreach (var token in tokens)
        {
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                output.Add(token);
                continue;
            }

            if (Operators.TryGetValue(token, out var currentOperator))
            {
                while (stack.TryPeek(out var top) && Operators.TryGetValue(top, out var topOperator) &&
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

    private static double EvaluatePostfix(IReadOnlyList<string> tokens)
    {
        var values = new Stack<double>();

        foreach (var token in tokens)
        {
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                values.Push(number);
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
                "/" or "mod" => throw new DivideByZeroException(),
                "^" => throw new ArithmeticException("El exponente está fuera del rango admitido."),
                _ => throw new FormatException($"Operador no admitido: {token}")
            };
            values.Push(result);
        }

        return values.Count == 1
            ? values.Pop()
            : throw new FormatException("La expresión no es válida.");
    }
}
