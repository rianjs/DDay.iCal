using System;
using NodaTime;
using NodaTime.TimeZones;

namespace DDay.iCal
{
    /// <summary>
    /// The iCalendar equivalent of the .NET <see cref="DateTime"/> class.
    /// <remarks>
    /// In addition to the features of the <see cref="DateTime"/> class, the <see cref="iCalDateTime"/>
    /// class handles time zone differences, and integrates seamlessly into the iCalendar framework.
    /// </remarks>
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public sealed class iCalDateTime :
        EncodableDataType,
        IDateTime
    {
        #region Static Public Properties

        static public iCalDateTime Now
        {
            get
            {
                return new iCalDateTime(DateTime.Now);
            }
        }

        static public iCalDateTime Today
        {
            get
            {
                return new iCalDateTime(DateTime.Today);
            }            
        }

        #endregion

        #region Private Fields

        private TimeZoneObservance? _TimeZoneObservance;

        #endregion

        #region Constructors

        public iCalDateTime(DateTime dateTime, string tzId)
        {
            var instant = new Instant(dateTime.Ticks);
            var zone = DateUtil.GetTimeZone(tzId);
            var noda = new ZonedDateTime(instant, zone);
            _value = noda;
        }

        public iCalDateTime(DateTime dateTime, string tzId, IICalendar associatedCalendar) : this(dateTime, tzId)
        {
            AssociatedObject = associatedCalendar;
        }

        public iCalDateTime(ZonedDateTime zonedDateTime, IDateTime associatedObject)
        {
            _value = zonedDateTime;
            AssociatedObject = associatedObject;
        }

        public iCalDateTime(ZonedDateTime zonedDateTime) : this(zonedDateTime, null) {}

        public iCalDateTime(DateTime value) : this(value, null) {}

        public iCalDateTime(int year, int month, int day, int hour, int minute, int second, string tzId, IICalendar associatedCalendar)
        {
            var dt = DateTime.MinValue;

            // NOTE: determine if a date/time value exceeds the representable date/time values in .NET.
            // If so, let's automatically adjust the date/time to compensate.
            // FIXME: should we have a parsing setting that will throw an exception
            // instead of automatically adjusting the date/time value to the
            // closest representable date/time?

            try
            {
                if (year > 9999)
                    dt = DateTime.MaxValue;
                else if (year > 0)
                    dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
            }
            catch { }

            var instant = new Instant(dt.Ticks);
            var zone = DateUtil.GetTimeZone(tzId);
            var noda = new ZonedDateTime(instant, zone);
            _value = noda;
            AssociatedObject = associatedCalendar;
        }

        public iCalDateTime(int year, int month, int day, int hour, int minute, int second, string tzid)
            : this(year, month, day, hour, minute, second, tzid, null) {}

        public iCalDateTime(int year, int month, int day, int hour, int minute, int second) : this(year, month, day, hour, minute, second, string.Empty, null) {}

        public iCalDateTime(int year, int month, int day)
            : this(year, month, day, 0, 0, 0, string.Empty, null) { }

        public iCalDateTime(int year, int month, int day, string tzid)
            : this(year, month, day, 0, 0, 0, tzid) { }

        
        
        #endregion

        #region Overrides

        public override ICalendarObject AssociatedObject
        {
            get
            {
                return base.AssociatedObject;
            }
            set
            {
                if (!Equals(AssociatedObject, value))
                {
                    base.AssociatedObject = value;
                }
            }
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            var dt = obj as IDateTime;
            if (dt != null)
            {
                _Value = dt.Value;
                _IsUniversalTime = dt.IsUniversalTime;                
                _HasDate = dt.HasDate;
                _HasTime = dt.HasTime;
                
                AssociateWith(dt);
            }
        }

        public override bool Equals(object obj) => Value.Equals(obj);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => ToString(null, null);

        #endregion

        #region Operators

        public static bool operator <(iCalDateTime left, IDateTime right) => right != null && left.Value < right.Value;

        public static bool operator >(iCalDateTime left, IDateTime right) => right != null && left.Value > right.Value;

        public static bool operator <=(iCalDateTime left, IDateTime right) => right != null && left.Value <= right.Value;

        public static bool operator >=(iCalDateTime left, IDateTime right) => right != null && left.Value >= right.Value;

        public static bool operator ==(iCalDateTime left, IDateTime right) => (right != null) && left.Value.Equals(right.Value);

        public static bool operator !=(iCalDateTime left, IDateTime right) => (right != null) && !left.Value.Equals(right.Value);

        public static TimeSpan operator -(iCalDateTime left, IDateTime right)
        {
            if (right == null)
            {
                throw new ArgumentException("Operand must be non-null");
            }
            var leftTicks = left.Value.ToInstant().Ticks;
            var rightTicks = right.Value.ToInstant().Ticks;
            return TimeSpan.FromTicks(leftTicks - rightTicks);
        }

        public static IDateTime operator -(iCalDateTime left, TimeSpan right)
        {
            var newValue = left.Value.Minus(Duration.FromTimeSpan(right));
            return new iCalDateTime(newValue);
        }

        public static IDateTime operator +(iCalDateTime left, TimeSpan right)
        {
            var newValue = left.Value.Plus(Duration.FromTimeSpan(right));
            return new iCalDateTime(newValue);
        }

        public static implicit operator iCalDateTime(DateTime left) => new iCalDateTime(left);

        #endregion

        #region IDateTime Members

        /// <summary>
        /// Converts the date/time to this computer's local date/time.
        /// </summary>
        public ZonedDateTime Local
        {
            get
            {
                var zone = NodaTime.TimeZones.BclDateTimeZone.ForSystemDefault();
                var local = new ZonedDateTime(Value.LocalDateTime, zone, Value.Offset);
                return local;
            }
        }

        /// <summary>
        /// Converts the date/time to UTC (Coordinated Universal Time)
        /// </summary>
        public ZonedDateTime UTC => Value.WithZone(DateTimeZone.Utc);

        public bool IsUniversalTime => Value.Zone.Equals(DateTimeZone.Utc);

        private readonly ZonedDateTime _value;
        public ZonedDateTime Value => _value;

        public bool HasDate => true;

        public bool HasTime => Value.Second == 0 && Value.Minute == 0 && Value.Hour == 0;

        public string TZID => Value.Zone.Id;

        public int Year => Value.Year;

        public int Month => Value.Month;

        public int Day => Value.Day;

        public int Hour => Value.Hour;

        public int Minute => Value.Minute;

        public int Second => Value.Second;

        public int Millisecond => Value.Millisecond;

        public long Ticks => Value.ToDateTimeUtc().Ticks;

        public DayOfWeek DayOfWeek => Value.ToDateTimeOffset().DayOfWeek;   //Microsoft's DayOfWeek enum doesn't conform to ISO-8601. NodaTime does. Ugh.

        public LocalDate Date => Value.LocalDateTime.Date;

        public IDateTime Add(TimeSpan ts)
        {
            return this + ts;
        }

        public IDateTime Subtract(TimeSpan ts)
        {
            return this - ts;
        }

        public TimeSpan Subtract(IDateTime dt)
        {
            return this - dt;
        }

        private static IDateTime Add(iCalDateTime thisDateTime, TimeSpan span)
        {
            var newDt = thisDateTime.Value.Plus(Duration.FromTimeSpan(span));
            var newiCalDateTime = new iCalDateTime(newDt);
            return newiCalDateTime;
        }

        public IDateTime AddYears(int years)
        {
            var now = DateTime.Now;
            var span = now.AddYears(years) - now;
            return Add(this, span);
        }

        public IDateTime AddDays(int days) => Add(this, TimeSpan.FromDays(days));

        public IDateTime AddHours(int hours) => Add(this, TimeSpan.FromHours(hours));

        public IDateTime AddMinutes(int minutes) => Add(this, TimeSpan.FromMinutes(minutes));

        public IDateTime AddSeconds(int seconds) => Add(this, TimeSpan.FromSeconds(seconds));

        public IDateTime AddMilliseconds(int milliseconds) => Add(this, TimeSpan.FromMilliseconds(milliseconds));

        public IDateTime AddTicks(long ticks) => Add(this, TimeSpan.FromTicks(ticks));

        public bool LessThan(IDateTime dt) => this < dt;

        public bool GreaterThan(IDateTime dt) => this > dt;

        public bool LessThanOrEqual(IDateTime dt) => this <= dt;

        public bool GreaterThanOrEqual(IDateTime dt) => this >= dt;

        //public void AssociateWith(IDateTime dt)
        //{
        //    if (AssociatedObject == null && dt.AssociatedObject != null)
        //        AssociatedObject = dt.AssociatedObject;
        //    else if (AssociatedObject != null && dt.AssociatedObject == null)
        //        dt.AssociatedObject = AssociatedObject;

        //    // If these share the same TZID, then let's see if we
        //    // can share the time zone observance also!
        //    if (TZID != null && string.Equals(TZID, dt.TZID))
        //    {
        //        if (TimeZoneObservance != null && dt.TimeZoneObservance == null)
        //        {
        //            IDateTime normalizedDt = new iCalDateTime(TimeZoneObservance.Value.TimeZoneInfo.OffsetTo.ToUTC(dt.Value));
        //            if (TimeZoneObservance.Value.Contains(normalizedDt))
        //                dt.TimeZoneObservance = TimeZoneObservance;
        //        }
        //        else if (dt.TimeZoneObservance != null && TimeZoneObservance == null)
        //        {
        //            IDateTime normalizedDt = new iCalDateTime(dt.TimeZoneObservance.Value.TimeZoneInfo.OffsetTo.ToUTC(Value));
        //            if (dt.TimeZoneObservance.Value.Contains(normalizedDt))
        //                TimeZoneObservance = dt.TimeZoneObservance;
        //        }
        //    }
        //}

        #endregion

        #region IComparable Members

        public int CompareTo(IDateTime dt)
        {
            if (Equals(dt))
                return 0;
            else if (this < dt)
                return -1;
            else if (this > dt)
                return 1;
            throw new Exception("An error occurred while comparing two IDateTime values.");
        }

        #endregion

        #region IFormattable Members

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var tz = " " + TZID;

            if (format != null)
            {
                return Value.ToString(format, formatProvider) + tz;
            }
            else if (HasTime && HasDate)
            {
                return Value + tz;
            }
            else if (HasTime)
            {
                return Value.TimeOfDay + tz;
            }
            else
            {
                return Value.Date + tz;}
        }

        #endregion
    }
}
