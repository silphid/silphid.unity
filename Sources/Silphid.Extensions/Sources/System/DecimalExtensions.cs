using System.Globalization;

namespace Silphid.Extensions
{
    public static class DecimalExtensions
    {
        public static string ToInvariantString(this decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}