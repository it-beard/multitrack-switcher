using System.Globalization;

namespace PRProjMulticamCreator.Core.Extensions;

public static class StringExtensions
{
    public static double ToDouble(this string value)
    {
        var result = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue);
        return result ? doubleValue : 0;
    }

    public static int ToInt(this string value)
    {
        var result = int.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue);
        return result ? doubleValue : 0;
    }
}