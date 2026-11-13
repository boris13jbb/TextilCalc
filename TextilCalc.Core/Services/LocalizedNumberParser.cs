using System.Globalization;

namespace TextilCalc.Core.Services;

public static class LocalizedNumberParser
{
    public static bool TryParse(string? text, out double value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = text.Trim().Replace(',', '.');
        return double.TryParse(
            normalized,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value) && double.IsFinite(value);
    }
}
