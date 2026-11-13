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
    [InlineData("2+3*4", 14)]
    [InlineData("(2+3)*4", 20)]
    [InlineData("2^3^2", 512)]
    [InlineData("-5+2", -3)]
    [InlineData("10mod3", 1)]
    public void ExpressionEvaluator_RespectsPrecedence(string expression, double expected)
    {
        var evaluator = new ExpressionEvaluator();

        Assert.Equal(expected, evaluator.Evaluate(expression), 10);
    }
}
