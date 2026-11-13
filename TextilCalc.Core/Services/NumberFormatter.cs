using System.Globalization;

namespace TextilCalc.Core.Services;

/// <summary>
/// Formato visual de resultados: enteros limpios, decimales significativos o notación científica.
/// </summary>
public static class NumberFormatter
{
    private const double ScientificThreshold = 1e12;
    private const double ScientificSmallThreshold = 1e-9;

    public static string Format(double value)
    {
        if (!double.IsFinite(value))
        {
            return "Error";
        }

        if (value == 0)
        {
            return "0";
        }

        var abs = Math.Abs(value);
        if (abs >= ScientificThreshold || (abs > 0 && abs < ScientificSmallThreshold))
        {
            return value.ToString("0.##########E+0", CultureInfo.InvariantCulture);
        }

        if (Math.Abs(value - Math.Truncate(value)) < 1e-12)
        {
            return Math.Truncate(value).ToString("0", CultureInfo.InvariantCulture);
        }

        // Hasta 12 decimales significativos, sin relleno innecesario.
        var text = value.ToString("0.############", CultureInfo.InvariantCulture);
        if (text.Contains('.', StringComparison.Ordinal))
        {
            text = text.TrimEnd('0').TrimEnd('.');
        }

        return text;
    }
}
