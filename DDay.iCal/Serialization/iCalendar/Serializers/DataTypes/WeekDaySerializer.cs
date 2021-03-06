﻿using System;
using System.Text.RegularExpressions;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class WeekDaySerializer :
        EncodableDataTypeSerializer
    {
        public override Type TargetType
        {
            get { return typeof(WeekDay); }
        }

        public override string SerializeToString(object obj)
        {
            var ds = obj as IWeekDay;
            if (ds != null)
            {
                var value = string.Empty;
                if (ds.Offset != int.MinValue)
                    value += ds.Offset;
                value += Enum.GetName(typeof(DayOfWeek), ds.DayOfWeek).ToUpper().Substring(0, 2);

                return Encode(ds, value);
            }
            return null;
        }

        internal static readonly Regex _dayOfWeek = new Regex(@"(\+|-)?(\d{1,2})?(\w{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            // Create the day specifier and associate it with a calendar object
            var ds = CreateAndAssociate() as IWeekDay;

            // Decode the value, if necessary
            value = Decode(ds, value);

            var match = _dayOfWeek.Match(value);
            if (match.Success)
            {
                if (match.Groups[2].Success)
                {
                    ds.Offset = Convert.ToInt32(match.Groups[2].Value);
                    if (match.Groups[1].Success && match.Groups[1].Value.Contains("-"))
                        ds.Offset *= -1;
                }
                ds.DayOfWeek = RecurrencePatternSerializer.GetDayOfWeek(match.Groups[3].Value);
                return ds;
            }

            return null;
        }
    }
}
