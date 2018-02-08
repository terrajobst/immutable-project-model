using System;
using System.Collections.Immutable;

namespace Immutable.ProjectModel
{
    public sealed class TimeConversion
    {
        public static TimeConversion Default = new TimeConversion(8, 40, 20, TimeUnit.Days, TimeUnit.Hours);

        private static ImmutableArray<(TimeUnit unit, string label)> _unitLabels = ImmutableArray.Create(
            (TimeUnit.Minutes, "m"),
            (TimeUnit.Minutes, "min"),
            (TimeUnit.Minutes, "minute"),
            (TimeUnit.Minutes, "minutes"),
            (TimeUnit.Hours, "h"),
            (TimeUnit.Hours, "hr"),
            (TimeUnit.Hours, "hrs"),
            (TimeUnit.Hours, "hour"),
            (TimeUnit.Hours, "hours"),
            (TimeUnit.Days, "d"),
            (TimeUnit.Days, "day"),
            (TimeUnit.Days, "days"),
            (TimeUnit.Weeks, "w"),
            (TimeUnit.Weeks, "wk"),
            (TimeUnit.Weeks, "wks"),
            (TimeUnit.Weeks, "week"),
            (TimeUnit.Weeks, "weeks"),
            (TimeUnit.Months, "mon"),
            (TimeUnit.Months, "mons"),
            (TimeUnit.Months, "month"),
            (TimeUnit.Months, "months")
        );

        public TimeConversion(double hoursPerDay,
                              double hoursPerWeek,
                              double daysPerMonth,
                              TimeUnit durationUnit,
                              TimeUnit workUnit)
        {
            HoursPerDay = hoursPerDay;
            HoursPerWeek = hoursPerWeek;
            DaysPerMonth = daysPerMonth;
            DurationUnit = durationUnit;
            WorkUnit = workUnit;
        }

        public double HoursPerDay { get; }
        public double HoursPerWeek { get; }
        public double DaysPerMonth { get; }
        public TimeUnit DurationUnit { get; }
        public TimeUnit WorkUnit { get; }

        public TimeConversion With(double hoursPerDay,
                                   double hoursPerWeek,
                                   double daysPerMonth,
                                   TimeUnit durationUnit,
                                   TimeUnit workUnit)
        {
            if (hoursPerDay == HoursPerDay &&
                hoursPerWeek == HoursPerWeek &&
                daysPerMonth == DaysPerMonth &&
                durationUnit == DurationUnit &&
                workUnit == WorkUnit)
                return this;

            return new TimeConversion(hoursPerDay,
                                      hoursPerWeek,
                                      daysPerMonth,
                                      durationUnit,
                                      workUnit);
        }

        public TimeConversion WithHoursPerDay(double hoursPerDay)
        {
            if (hoursPerDay <= 0)
                throw new ArgumentOutOfRangeException(nameof(hoursPerDay));

            return With(hoursPerDay, HoursPerWeek, DaysPerMonth, DurationUnit, WorkUnit);
        }

        public TimeConversion WithHoursPerWeek(double hoursPerWeek)
        {
            if (hoursPerWeek <= 0)
                throw new ArgumentOutOfRangeException(nameof(hoursPerWeek));

            return With(HoursPerDay, hoursPerWeek, DaysPerMonth, DurationUnit, WorkUnit);
        }

        public TimeConversion WithDaysPerMonth(double daysPerMonth)
        {
            if (daysPerMonth <= 0)
                throw new ArgumentOutOfRangeException(nameof(daysPerMonth));

            return With(HoursPerDay, HoursPerWeek, daysPerMonth, DurationUnit, WorkUnit);
        }

        public TimeConversion WithDurationUnit(TimeUnit durationUnit)
        {
            return With(HoursPerDay, HoursPerWeek, DaysPerMonth, durationUnit, WorkUnit);
        }

        public TimeConversion WithWorkUnit(TimeUnit workUnit)
        {
            return With(HoursPerDay, HoursPerWeek, DaysPerMonth, DurationUnit, workUnit);
        }

        public TimeSpan FromMinutes(double minutes)
        {
            return FromUnit(minutes, TimeUnit.Minutes);
        }

        public TimeSpan FromHours(double hours)
        {
            return FromUnit(hours, TimeUnit.Hours);
        }

        public TimeSpan FromDays(double days)
        {
            return FromUnit(days, TimeUnit.Days);
        }

        public TimeSpan FromWeeks(double weeks)
        {
            return FromUnit(weeks, TimeUnit.Weeks);
        }

        public TimeSpan FromMonths(double months)
        {
            return FromUnit(months, TimeUnit.Months);
        }

        public TimeSpan FromUnit(double value, TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.Minutes:
                    return TimeSpan.FromMinutes(value);
                case TimeUnit.Hours:
                    return TimeSpan.FromHours(value);
                case TimeUnit.Days:
                    return TimeSpan.FromHours(value * HoursPerDay);
                case TimeUnit.Weeks:
                    return TimeSpan.FromHours(value * HoursPerWeek);
                case TimeUnit.Months:
                default:
                    return TimeSpan.FromHours(value * DaysPerMonth * HoursPerDay);
            }
        }

        public double ToMinutes(TimeSpan timeSpan)
        {
            return ToUnit(timeSpan, TimeUnit.Minutes);
        }

        public double ToHours(TimeSpan timeSpan)
        {
            return ToUnit(timeSpan, TimeUnit.Hours);
        }

        public double ToDays(TimeSpan timeSpan)
        {
            return ToUnit(timeSpan, TimeUnit.Days);
        }

        public double ToWeeks(TimeSpan timeSpan)
        {
            return ToUnit(timeSpan, TimeUnit.Weeks);
        }

        public double ToMonths(TimeSpan timeSpan)
        {
            return ToUnit(timeSpan, TimeUnit.Months);
        }

        public double ToUnit(TimeSpan timeSpan, TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.Minutes:
                    return timeSpan.TotalMinutes;
                case TimeUnit.Hours:
                    return timeSpan.TotalHours;
                case TimeUnit.Days:
                    return timeSpan.TotalHours / HoursPerDay;
                case TimeUnit.Weeks:
                    return timeSpan.TotalHours / HoursPerWeek;
                case TimeUnit.Months:
                default:
                    return timeSpan.TotalHours / HoursPerDay / DaysPerMonth;
            }
        }

        public bool TryParseWork(string text, out TimeSpan result)
        {
            return TryParse(text, WorkUnit, out result);
        }

        public TimeSpan ParseWork(string text)
        {
            if (TryParseWork(text, out var result))
                return result;

            throw new FormatException($"'{text}' isn't a valid value for work");
        }

        public string FormatWork(TimeSpan work)
        {
            return Format(work, WorkUnit);
        }

        public bool TryParseDuration(string text, out TimeSpan result)
        {
            return TryParse(text, DurationUnit, out result);
        }

        public TimeSpan ParseDuration(string text)
        {
            if (TryParseDuration(text, out var result))
                return result;

            throw new FormatException($"'{text}' isn't a valid value for duration");
        }

        public string FormatDuration(TimeSpan work)
        {
            return Format(work, DurationUnit);
        }

        private bool TryParse(string text, TimeUnit defaultUnit, out TimeSpan result)
        {
            if (text != null)
            {
                var unit = ParseUnit(ref text, defaultUnit);

                if (double.TryParse(text, out var value))
                {
                    result = FromUnit(value, unit);
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static TimeUnit ParseUnit(ref string text, TimeUnit defaultUnit)
        {
            foreach (var (unit, label) in _unitLabels)
            {
                if (text.EndsWith(label, StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Substring(0, text.Length - label.Length).Trim();
                    return unit;
                }
            }

            return defaultUnit;
        }

        private string Format(TimeSpan span, TimeUnit unit)
        {
            string GetUnitText(TimeUnit desiredUnit, bool useSingular)
            {
                switch (desiredUnit)
                {
                    case TimeUnit.Minutes:
                        return useSingular ? "min" : "mins";
                    case TimeUnit.Hours:
                        return useSingular ? "hr" : "hrs";
                    case TimeUnit.Days:
                        return useSingular ? "day" : "days";
                    case TimeUnit.Weeks:
                        return useSingular ? "wk" : "wks";
                    case TimeUnit.Months:
                    default:
                        return useSingular ? "mon" : "mons";
                }
            }

            var value = ToUnit(span, unit);
            var unitText = GetUnitText(unit, value == 1.0);
            return $"{value} {unitText}";
        }
    }
}
