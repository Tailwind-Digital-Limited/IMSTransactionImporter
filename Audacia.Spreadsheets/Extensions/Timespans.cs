using System;

namespace Audacia.Spreadsheets.Extensions
{
#pragma warning disable AV1745
    public static class Timespans
#pragma warning restore AV1745
    {
        public static double ToOADatePrecise(this TimeSpan time)
        {
            return Convert.ToDouble(time.Ticks) / TimeSpan.TicksPerDay;
        }
    }
}