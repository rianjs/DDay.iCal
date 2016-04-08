using System;
using System.Diagnostics;
using System.Net.Security;
using NodaTime;

namespace DDay.iCal
{
    public class DateUtil
    {
        static private System.Globalization.Calendar _Calendar;

        static DateUtil()
        {
            _Calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        }

        public static IDateTime StartOfDay(IDateTime dt)
        {
            return dt.
                AddHours(-dt.Hour).
                AddMinutes(-dt.Minute).
                AddSeconds(-dt.Second);
        }

        public static IDateTime EndOfDay(IDateTime dt)
        {
            return StartOfDay(dt).AddDays(1).AddTicks(-1);
        }     

        public static DateTime SimpleDateTimeToMatch(IDateTime dt, IDateTime toMatch)
        {
            if (toMatch.IsUniversalTime && dt.IsUniversalTime)
                return dt.Value.ToDateTimeUtc();
            else if (toMatch.IsUniversalTime)
                return dt.Value.ToDateTimeUtc();
            else if (dt.IsUniversalTime)
                return dt.Value.LocalDateTime.ToDateTimeUnspecified();  //If something breaks on this, it may need to be DateTimeKind.Local
            else
                return dt.Value.ToDateTimeOffset().DateTime;
        }

        public static IDateTime MatchTimeZone(IDateTime dt1, IDateTime dt2)
        {
            if (dt1 == null || dt2 == null)
            {
                throw new ArgumentException("DateTimes may not be null");
            }

            // Associate the date/time with the first.
            //copy.AssociateWith(dt1);

            return string.Equals(dt1.TZID, dt2.TZID)    //Maybe associate with dt1?
                ? new iCalDateTime(dt2.Value)
                : new iCalDateTime(dt2.Value.WithZone(GetTimeZone(dt2.TZID)));
        }

        public static DateTime FirstDayOfWeek(DateTime dt, DayOfWeek firstDayOfWeek, out int offset)
        {
            offset = 0;
            while (dt.DayOfWeek != firstDayOfWeek)
            {
                dt = dt.AddDays(-1);
                offset++;
            }
            return dt;
        }

        public static DateTimeZone GetTimeZone(string tzId)
        {
            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId) ?? DateTimeZoneProviders.Bcl.GetZoneOrNull(tzId);
            if (zone != null)
            {
                return zone;
            }

            zone = DateTimeZoneProviders.Bcl.GetZoneOrNull(tzId);
            if (zone != null)
            {
                return zone;
            }

            tzId = tzId.Replace("-", "/");
            zone = DateTimeZoneProviders.Serialization.GetZoneOrNull(tzId);
            if (zone != null)
            {
                return zone;
            }
            throw new ArgumentException($"{zone} is not a recognized time zone");
        }

        public static ZonedDateTime GetMidnight(ZonedDateTime dt)
        {
            var localDate = dt.Date;
            var midnight = dt.Zone.AtStartOfDay(localDate);
            return midnight;
        }
    }
}
