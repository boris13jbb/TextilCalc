using TextilCalc.Core.Services;

namespace TextilCalc.Core.Tests;

public sealed class TextileCalculatorTests
{
    [Theory]
    [InlineData("1,82", 1.82)]
    [InlineData("1.82", 1.82)]
    [InlineData("-3,5", -3.5)]
    public void LocalizedParser_AcceptsCommaAndPoint(string input, double expected)
    {
        Assert.True(LocalizedNumberParser.TryParse(input, out var result));
        Assert.Equal(expected, result, 10);
    }

    [Fact]
    public void CalculateGramatura_MatchesDocumentedCase()
    {
        var result = TextileCalculator.CalculateGramatura(50, 1.6, 80);

        Assert.Equal(80, result.AreaM2, 10);
        Assert.Equal(1000, result.Gsm, 10);
        Assert.Equal(1, result.Rendimiento, 10);
    }

    [Fact]
    public void CalculateMetraje_UsesWeightGsmAndWidth()
    {
        var result = TextileCalculator.CalculateMetraje(80, 200, 1.6);

        Assert.Equal(400, result.AreaM2, 10);
        Assert.Equal(250, result.Metraje, 10);
    }

    [Theory]
    [InlineData(0, 1.6, 80)]
    [InlineData(50, 0, 80)]
    [InlineData(50, 1.6, 0)]
    public void CalculateGramatura_RejectsNonPositiveValues(double length, double width, double weight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TextileCalculator.CalculateGramatura(length, width, weight));
    }

    [Theory]
    [InlineData(0, 200, 1.6)]
    [InlineData(80, 0, 1.6)]
    [InlineData(80, 200, 0)]
    public void CalculateMetraje_RejectsNonPositiveValues(double weight, double gsm, double width)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TextileCalculator.CalculateMetraje(weight, gsm, width));
    }
}

public sealed class ExpressionEvaluatorTests
{
    private readonly ExpressionEvaluator _evaluator = new();

    [Theory]
    [InlineData("2+3*4", 14)]
    [InlineData("(2+3)*4", 20)]
    [InlineData("2^3^2", 512)]
    [InlineData("-5+2", -3)]
    [InlineData("10mod3", 1)]
    [InlineData("10÷2", 5)]
    [InlineData("3×4", 12)]
    [InlineData("1.5e2", 150)]
    [InlineData("2.5E-1", 0.25)]
    public void ExpressionEvaluator_RespectsPrecedenceAndNotation(string expression, double expected)
    {
        Assert.Equal(expected, _evaluator.Evaluate(expression), 10);
    }

    /// <summary>
    /// Unarios: -(…), -función, 2×(-3), 2^(-3), (-2)^2 y -2^2 (potencia sobre negación).
    /// </summary>
    [Theory]
    [InlineData("-(2+3)", -5)]
    [InlineData("2×(-3)", -6)]
    [InlineData("2*(-3)", -6)]
    [InlineData("2^(-3)", 0.125)]
    [InlineData("2^-3", 0.125)]
    [InlineData("(-2)^2", 4)]
    [InlineData("-2^2", -4)]
    [InlineData("--2", 2)]
    [InlineData("-2*3", -6)]
    [InlineData("2*-3", -6)]
    public void ExpressionEvaluator_UnaryOperators(string expression, double expected)
    {
        Assert.Equal(expected, _evaluator.Evaluate(expression), 10);
    }

    [Fact]
    public void ExpressionEvaluator_UnaryNegatesSinInDegrees()
    {
        _evaluator.AngleMode = AngleUnit.Degrees;
        Assert.Equal(-0.5, _evaluator.Evaluate("-sin(30)"), 10);
        Assert.Equal(-0.5, _evaluator.Evaluate("sin(-30)"), 10);
    }

    [Theory]
    [InlineData("pi", Math.PI)]
    [InlineData("e", Math.E)]
    [InlineData("π", Math.PI)]
    public void ExpressionEvaluator_ResolvesConstants(string expression, double expected)
    {
        Assert.Equal(expected, _evaluator.Evaluate(expression), 12);
    }

    [Fact]
    public void ExpressionEvaluator_SinCosInDegrees()
    {
        _evaluator.AngleMode = AngleUnit.Degrees;
        Assert.Equal(0.5, _evaluator.Evaluate("sin(30)"), 10);
        Assert.Equal(0.5, _evaluator.Evaluate("cos(60)"), 10);
    }

    [Fact]
    public void ExpressionEvaluator_Tan90Degrees_ThrowsClearError()
    {
        _evaluator.AngleMode = AngleUnit.Degrees;
        var ex = Assert.Throws<ArithmeticException>(() => _evaluator.ApplyFunction("tan", 90));
        Assert.Contains("no está definida", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExpressionEvaluator_DivideByZero_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => _evaluator.Evaluate("5/0"));
    }

    [Fact]
    public void ExpressionEvaluator_SqrtNegative_Throws()
    {
        var ex = Assert.Throws<ArithmeticException>(() => _evaluator.ApplyFunction("sqrt", -9));
        Assert.Contains("negativo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExpressionEvaluator_LogZero_Throws()
    {
        var ex = Assert.Throws<ArithmeticException>(() => _evaluator.ApplyFunction("log", 0));
        Assert.Contains("mayor que cero", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("sqrt", 9, 3)]
    [InlineData("sqrt", 81, 9)]
    [InlineData("cbrt", 27, 3)]
    [InlineData("abs", -4, 4)]
    [InlineData("floor", 3.7, 3)]
    [InlineData("ceil", 3.2, 4)]
    [InlineData("round", 3.5, 4)]
    [InlineData("log", 100, 2)]
    [InlineData("ln", Math.E, 1)]
    [InlineData("log2", 8, 3)]
    [InlineData("fact", 5, 120)]
    public void ExpressionEvaluator_UnaryFunctions(string function, double input, double expected)
    {
        Assert.Equal(expected, _evaluator.ApplyFunction(function, input), 10);
    }

    [Fact]
    public void ExpressionEvaluator_Sqrt81_ViaExpression()
    {
        Assert.Equal(9, _evaluator.Evaluate("sqrt(81)"), 10);
    }

    [Fact]
    public void ExpressionEvaluator_NestedFunctions()
    {
        _evaluator.AngleMode = AngleUnit.Degrees;
        Assert.Equal(1, _evaluator.Evaluate("sin(90)"), 10);
        Assert.Equal(2, _evaluator.Evaluate("log(100)"), 10);
        Assert.Equal(8, _evaluator.Evaluate("2^3"), 10);
    }
}

public sealed class NumberFormatterTests
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(12, "12")]
    [InlineData(1.25, "1.25")]
    public void NumberFormatter_FormatsCommonValues(double value, string expected)
    {
        Assert.Equal(expected, NumberFormatter.Format(value));
    }

    [Fact]
    public void NumberFormatter_UsesScientificForHugeValues()
    {
        var formatted = NumberFormatter.Format(1.23456789e15);
        Assert.Contains("E", formatted, StringComparison.OrdinalIgnoreCase);
    }
}
