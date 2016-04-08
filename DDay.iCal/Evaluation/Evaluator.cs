using System;
using System.Collections.Generic;
using NodaTime;

namespace DDay.iCal
{
    public abstract class Evaluator : IEvaluator
    {
        #region Private Fields

        private System.Globalization.Calendar m_Calendar;
        private ZonedDateTime m_EvaluationStartBounds = DateTime.MaxValue;
        private ZonedDateTime m_EvaluationEndBounds = DateTime.MinValue;
        
        private ICalendarObject m_AssociatedObject;
        private ICalendarDataType m_AssociatedDataType;

        #endregion

        #region Protected Fields

        protected HashSet<IPeriod> m_Periods;

        #endregion

        #region Constructors

        public Evaluator()
        {
            Initialize();
        }

        public Evaluator(ICalendarObject associatedObject)
        {
            m_AssociatedObject = associatedObject;

            Initialize();
        }

        public Evaluator(ICalendarDataType dataType)
        {
            m_AssociatedDataType = dataType;

            Initialize();
        }

        void Initialize()
        {
            m_Calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            m_Periods = new HashSet<IPeriod>();
        }

        #endregion

        #region Protected Methods

        protected ZonedDateTime GetNextOccurrence(ZonedDateTime zonedDateTime, IRecurrencePattern pattern, int interval)
        {
            if (interval <= 0)
            {
                throw new ArgumentException($"Interval units must be greater than 0. You entered {interval}");
            }

            switch (pattern.Frequency)
            {
                case FrequencyType.Secondly:
                    return zonedDateTime.Plus(Duration.FromSeconds(interval));
                case FrequencyType.Minutely:
                    return zonedDateTime.Plus(Duration.FromMinutes(interval));
                case FrequencyType.Hourly:
                    return zonedDateTime.Plus(Duration.FromHours(interval));
                case FrequencyType.Daily:
                    return zonedDateTime.Plus(Duration.FromStandardDays(interval));
                case FrequencyType.Weekly:
                    return zonedDateTime.Plus(Duration.FromStandardWeeks(interval));
                case FrequencyType.Monthly:
                {
                    var nextDate = zonedDateTime.Date.PlusMonths(interval);
                    var exactTimeOfNextOccurrence = new LocalDateTime(nextDate.Year, nextDate.Month, nextDate.Day, zonedDateTime.TimeOfDay.Hour,
                        zonedDateTime.Minute, zonedDateTime.Second);
                    return new ZonedDateTime(exactTimeOfNextOccurrence, zonedDateTime.Zone, zonedDateTime.Offset);
                }
                case FrequencyType.Yearly:
                {
                    var nextDate = zonedDateTime.Date.PlusYears(interval);
                    var exactTimeOfNextOccurrence = new LocalDateTime(nextDate.Year, nextDate.Month, nextDate.Day, zonedDateTime.TimeOfDay.Hour,
                        zonedDateTime.Minute, zonedDateTime.Second);
                    return new ZonedDateTime(exactTimeOfNextOccurrence, zonedDateTime.Zone, zonedDateTime.Offset);
                }
                default:
                    throw new ArgumentException("FrequencyType.NONE cannot be evaluated. Please specify a FrequencyType before evaluating the recurrence.");
            }
        }

        #endregion

        #region IEvaluator Members

        public System.Globalization.Calendar Calendar
        {
            get { return m_Calendar; }
        }

        virtual public ZonedDateTime EvaluationStartBounds { get; set; }

        virtual public ZonedDateTime EvaluationEndBounds { get; set; }

        virtual public HashSet<IPeriod> Periods
        {
            get { return m_Periods; }
        }

        virtual public void Clear()
        {
            m_EvaluationStartBounds = new ZonedDateTime(Instant.MaxValue, m_EvaluationStartBounds.Zone);
            m_EvaluationEndBounds = new ZonedDateTime(Instant.MinValue, m_EvaluationEndBounds.Zone);
            m_Periods.Clear();
        }

        abstract public HashSet<IPeriod> Evaluate(
            IDateTime referenceDate,
            ZonedDateTime periodStart,
            ZonedDateTime periodEnd,
            bool includeReferenceDateInResults);

        #endregion
    }
}
