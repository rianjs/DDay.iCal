using System;
using System.Runtime.Serialization;
using DDay.Collections;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents an RFC 5545 VTIMEZONE component.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class iCalTimeZone : CalendarComponent, ITimeZone
    {
        private readonly TimeZoneEvaluator _mEvaluator;
        ICalendarObjectList<ITimeZoneInfo> m_TimeZoneInfos;

        #region Constructors

        public iCalTimeZone()
        {
            Name = Components.TIMEZONE;
            _mEvaluator = new TimeZoneEvaluator(this);
            Initialize();
        }

        private void Initialize()
        {
            m_TimeZoneInfos = new CalendarObjectListProxy<ITimeZoneInfo>(Children);
            Children.ItemAdded += Children_ItemAdded;
            Children.ItemRemoved += Children_ItemRemoved;
            SetService(_mEvaluator);
        }        

        #endregion

        #region Event Handlers

        void Children_ItemRemoved(object sender, ObjectEventArgs<ICalendarObject, int> e)
        {
            _mEvaluator.Clear();
        }

        void Children_ItemAdded(object sender, ObjectEventArgs<ICalendarObject, int> e)
        {
            _mEvaluator.Clear();
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        #endregion

        #region ITimeZone Members

        virtual public string TZID
        {
            get { return Properties.Get<string>("TZID"); }
            set { Properties.Set("TZID", value); }
        }

        virtual public IDateTime LastModified
        {
            get { return Properties.Get<IDateTime>("LAST-MODIFIED"); }
            set { Properties.Set("LAST-MODIFIED", value); }
        }
        #endregion
    }
}
