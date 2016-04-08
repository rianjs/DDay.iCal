﻿using System;
using System.Collections.Generic;
using NodaTime;

namespace DDay.iCal
{
    public class RecurringEvaluator :
        Evaluator
    {
        #region Private Fields

        private IRecurrable m_Recurrable;

        #endregion

        #region Protected Properties

        protected IRecurrable Recurrable
        {
            get { return m_Recurrable; }
            set { m_Recurrable = value; }
        }

        #endregion

        #region Constructors

        public RecurringEvaluator(IRecurrable obj)
        {
            Recurrable = obj;

            // We're not sure if the object is a calendar object
            // or a calendar data type, so we need to assign
            // the associated object manually
            if (obj is ICalendarObject)
                AssociatedObject = (ICalendarObject)obj;
            if (obj is ICalendarDataType)
            {
                var dt = (ICalendarDataType)obj;
                AssociatedObject = dt.AssociatedObject;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Evaulates the RRule component, and adds each specified Period to the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        /// <param name="includeReferenceDateInResults"></param>
        virtual protected void EvaluateRRule(IDateTime referenceDate, ZonedDateTime periodStart, ZonedDateTime periodEnd)
        {
            // Handle RRULEs
            if (Recurrable.RecurrenceRules != null &&
                Recurrable.RecurrenceRules.Count > 0)
            {
                foreach (var rrule in Recurrable.RecurrenceRules)
                {
                    var evaluator = rrule.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        var periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd);
                        foreach (var p in periods)
                        {
                            if (!Periods.Contains(p))
                                Periods.Add(p);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Evalates the RDate component, and adds each specified DateTime or Period to the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        virtual protected void EvaluateRDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            // Handle RDATEs
            if (Recurrable.RecurrenceDates != null)
            {
                foreach (var rdate in Recurrable.RecurrenceDates)
                {
                    var evaluator = rdate.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        var periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, false);
                        foreach (var p in periods)
                        {
                            if (!Periods.Contains(p))
                                Periods.Add(p);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Evaulates the ExRule component, and excludes each specified DateTime from the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        virtual protected void EvaluateExRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            // Handle EXRULEs
            if (Recurrable.ExceptionRules != null)
            {
                foreach (var exrule in Recurrable.ExceptionRules)
                {
                    var evaluator = exrule.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        var periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, false);
                        foreach (var p in periods)                        
                        {                            
                            if (this.Periods.Contains(p))
                                this.Periods.Remove(p);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Evalates the ExDate component, and excludes each specified DateTime or Period from the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        virtual protected void EvaluateExDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            // Handle EXDATEs
            if (Recurrable.ExceptionDates != null)
            {
                foreach (var exdate in Recurrable.ExceptionDates)
                {
                    var evaluator = exdate.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        var periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, false);
                        foreach (var p in periods)                        
                        {
                            // If no time was provided for the ExDate, then it excludes the entire day
                            if (!p.StartTime.HasTime || (p.EndTime != null && !p.EndTime.HasTime))
                                p.MatchesDateOnly = true;

                            while (Periods.Contains(p))
                                Periods.Remove(p);
                        }
                    }
                }
            }
        }

        #endregion

        #region Overrides

        public override HashSet<IPeriod> Evaluate(IDateTime referenceDate, ZonedDateTime periodStart, ZonedDateTime periodEnd)
        {
            // Evaluate extra time periods, without re-evaluating ones that were already evaluated
            if ((EvaluationStartBounds == DateTime.MaxValue && EvaluationEndBounds == DateTime.MinValue) ||
                (periodEnd.Equals(EvaluationStartBounds)) ||
                (periodStart.Equals(EvaluationEndBounds)))
            {
                EvaluateRRule(referenceDate, periodStart, periodEnd);
                EvaluateRDate(referenceDate, periodStart, periodEnd);
                EvaluateExRule(referenceDate, periodStart, periodEnd);
                EvaluateExDate(referenceDate, periodStart, periodEnd);
                if (EvaluationStartBounds == DateTime.MaxValue || EvaluationStartBounds > periodStart)
                    EvaluationStartBounds = periodStart;
                if (EvaluationEndBounds == DateTime.MinValue || EvaluationEndBounds < periodEnd)
                    EvaluationEndBounds = periodEnd;
            }
            else 
            {
                if (EvaluationStartBounds != DateTime.MaxValue && periodStart < EvaluationStartBounds)
                    Evaluate(referenceDate, periodStart, EvaluationStartBounds, includeReferenceDateInResults);
                if (EvaluationEndBounds != DateTime.MinValue && periodEnd > EvaluationEndBounds)
                    Evaluate(referenceDate, EvaluationEndBounds, periodEnd, includeReferenceDateInResults);
            }

            // Sort the list
            //m_Periods.Sort();

            return Periods;
        }

        #endregion
    }
}
