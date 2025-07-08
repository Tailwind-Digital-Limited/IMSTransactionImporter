using System;
#pragma warning disable AV1745

namespace Audacia.Spreadsheets.Extensions
{
    /// <summary>
    /// For more information on date systems in Excel, please read the following article.
    /// https://support.microsoft.com/en-za/help/214330/differences-between-the-1900-and-the-1904-date-system-in-excel
    /// </summary>
    public static class DateTimes
    {
        /* For when we get to implementing 1904 dates
        /// <summary>1904 Date System, Epoch is 1st January 1904.</summary>
        public static readonly DateTime EpochJan1904 = new DateTime(1904, 1, 1);
        */
        
        /// <summary>1900 Date System, Epoch is 30th December 1899.</summary>
#pragma warning disable AV1704
#pragma warning disable ACL1014
        //RS: pragma'd as a date
        public static readonly DateTime EpochJan1900 = new DateTime(1899, 12, 30);
#pragma warning restore ACL1014
#pragma warning restore AV1704

        /// <summary>
        /// Converts an OADate to a DateTime
        /// .Net Standard doesn't support OADate (used by OpenXml)
        /// Ref: http://stackoverflow.com/a/13922172/1336654
        /// </summary>
        /// <param name="source">Ticks since 30th December 1899</param>
        /// <returns>A DateTime</returns>
        public static DateTime FromOADatePrecise(this double source)
        {
            if (!(source >= 0))
            {
                // NaN or negative source not supported
                throw new ArgumentOutOfRangeException(nameof(source));
            }

            var ticks = Convert.ToInt64(source * TimeSpan.TicksPerDay);
            return EpochJan1900 + TimeSpan.FromTicks(ticks);
        }

        /// <summary>
        /// Converts a DateTime to an OADate
        /// .Net Standard doesn't support OADate (used by OpenXml)
        /// Ref: http://stackoverflow.com/a/13922172/1336654
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>An OADate (Ticks since 30th December 1899)</returns>
        public static double ToOADatePrecise(this DateTime dateTime)
        {
            if (dateTime < EpochJan1900)
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime));
            }

            return Convert.ToDouble((dateTime - EpochJan1900).Ticks) / TimeSpan.TicksPerDay;
        }
    }
}
