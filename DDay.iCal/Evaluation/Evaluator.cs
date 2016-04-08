using System;
using System.Collections.Generic;
using NodaTime;

namespace DDay.iCal
{
    public abstract class Evaluator : IEvaluator
    {
        #region Private Fields

        private System.Globalization.Calendar m_Calendar;
        private DateTime m_EvaluationStartBounds = DateTime.MaxValue;
        private DateTime m_EvaluationEndBounds = DateTime.MinValue;
        
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

        protected IDateTime ConvertToIDateTime(DateTime dt, IDateTime referenceDate)
        {
            IDateTime newDt = new iCalDateTime(dt, referenceDate.TZID);
            //newDt.AssociateWith(referenceDate);
            return newDt;
        }

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

        virtual public DateTime EvaluationStartBounds
        {
            get { return m_EvaluationStartBounds; }
            set { m_EvaluationStartBounds = value; }
        }

        virtual public DateTime EvaluationEndBounds
        {
            get { return m_EvaluationEndBounds; }
            set { m_EvaluationEndBounds = value; }
        }

        virtual public ICalendarObject AssociatedObject
        {
            get
            {
                if (m_AssociatedObject != null)
                    return m_AssociatedObject;
                else if (m_AssociatedDataType != null)
                    return m_AssociatedDataType.AssociatedObject;
                else
                    return null;
            }
            protected set { m_AssociatedObject = value; }
        }

        virtual public HashSet<IPeriod> Periods
        {
            get { return m_Periods; }
        }

        virtual public void Clear()
        {
            m_EvaluationStartBounds = DateTime.MaxValue;
            m_EvaluationEndBounds = DateTime.MinValue;
            m_Periods.Clear();
        }

        abstract public HashSet<IPeriod> Evaluate(
            IDateTime referenceDate,
            DateTime periodStart,
            DateTime periodEnd,
            bool includeReferenceDateInResults);

        #endregion
    }
}
