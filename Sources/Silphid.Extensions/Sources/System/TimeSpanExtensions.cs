using System;

namespace Silphid.Extensions
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan Multiply(this TimeSpan timeSpan, double factor)
        {
            return TimeSpan.FromSeconds(timeSpan.TotalSeconds * factor);
        }

        public static TimeSpan TotalDaysFraction(this TimeSpan timeSpan)
        {
            return TimeSpan.FromDays(((float)timeSpan.TotalDays).Fractional());
        }
    }
}