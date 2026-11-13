using TextilCalc.Core.Models;

namespace TextilCalc.Core.Services;

public static class TextileCalculator
{
    public static GramaturaResult CalculateGramatura(double lengthMeters, double widthMeters, double weightKg)
    {
        EnsurePositive(lengthMeters, nameof(lengthMeters));
        EnsurePositive(widthMeters, nameof(widthMeters));
        EnsurePositive(weightKg, nameof(weightKg));

        var area = lengthMeters * widthMeters;
        var gsm = weightKg * 1000d / area;
        return new GramaturaResult(area, gsm, 1000d / gsm);
    }

    public static MetrajeResult CalculateMetraje(double weightKg, double gsm, double widthMeters)
    {
        EnsurePositive(weightKg, nameof(weightKg));
        EnsurePositive(gsm, nameof(gsm));
        EnsurePositive(widthMeters, nameof(widthMeters));

        var area = weightKg * 1000d / gsm;
        return new MetrajeResult(area, area / widthMeters);
    }

    private static void EnsurePositive(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "El valor debe ser finito y mayor que cero.");
        }
    }
}
