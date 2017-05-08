namespace Silphid.Extensions
{
    public static class IntegerExtensions
    {
        public static string ToZeroPaddedString(this int value, int digits)
        {
            var format = "{0:D" + digits + "}";
            return string.Format(format, value);
        }
    }
}