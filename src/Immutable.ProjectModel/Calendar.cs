﻿using System;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    public sealed class Calendar
    {
        public static Calendar Default = new Calendar(WorkingWeek.Default);
        public static Calendar TwentyFourSeven = new Calendar(WorkingWeek.TwentyFourSeven);

        private Calendar(WorkingWeek workingWeek)
        {
            WorkingWeek = workingWeek;
        }

        public WorkingWeek WorkingWeek { get; }

        public Calendar WithWorkingWeek(WorkingWeek workingWeek)
        {
            if (workingWeek == null)
                throw new ArgumentNullException(nameof(workingWeek));

            if (workingWeek.IsNonWorking)
                throw new ArgumentException("Cannot use a non-working week as the default week.", nameof(workingWeek));

            if (workingWeek == WorkingWeek)
                return this;

            return new Calendar(workingWeek);
        }

        public DateTimeOffset FindWorkStart(DateTimeOffset date)
        {
            return AddWork(date, TimeSpan.Zero);
        }

        public DateTimeOffset FindWorkEnd(DateTimeOffset date)
        {
            return SubtractWork(date, TimeSpan.Zero);
        }

        public DateTimeOffset AddWork(DateTimeOffset date, TimeSpan work)
        {
            if (work < TimeSpan.Zero)
                return SubtractWork(date, -work);

            var result = date;

            do
            {
                var workingTime = GetNextWorkingTime(ref result);
                if (workingTime > work)
                    workingTime = work;

                result += workingTime;
                work -= workingTime;
            }
            while (work > TimeSpan.Zero);

            return result;
        }

        private DateTimeOffset SubtractWork(DateTimeOffset date, TimeSpan work)
        {
            var result = date;

            do
            {
                var workingTime = GetPreviousWorkingTime(ref result);
                if (workingTime > work)
                    workingTime = work;

                result -= workingTime;
                work -= workingTime;
            }
            while (work > TimeSpan.Zero);

            return result;
        }

        private TimeSpan GetNextWorkingTime(ref DateTimeOffset date)
        {
            while (true)
            {
                var day = WorkingWeek[date.DayOfWeek];

                foreach (var time in day.WorkingTimes)
                {
                    var from = date.Date.Add(time.From);
                    var to = date.Date.Add(time.To);
                    if (date < to)
                    {
                        date = date < from ? from : date;
                        return to - date;
                    }
                }

                date = date.Date.AddDays(1);
            }
        }

        private TimeSpan GetPreviousWorkingTime(ref DateTimeOffset date)
        {
            var input = date;
            var atEndOfDay = false;

            while (true)
            {
                var day = WorkingWeek[date.DayOfWeek];

                foreach (var time in day.WorkingTimes.Reverse())
                {
                    var from = date.Date.Add(time.From);
                    var to = date.Date.Add(time.To);

                    if (atEndOfDay)
                        date = to;

                    if (date > from)
                    {
                        date = date > to ? to : date;
                        return date - from;
                    }
                }

                date = date.Date.AddDays(-1);
                atEndOfDay = true;
            }
        }

        public TimeSpan GetWork(DateTimeOffset start, DateTimeOffset end)
        {
            if (end < start)
                throw new ArgumentOutOfRangeException(nameof(end));

            start = FindWorkStart(start);
            end = FindWorkEnd(end);

            var result = TimeSpan.Zero;
            var date = start;

            while (date < end)
            {
                var workingTime = GetNextWorkingTime(ref date);
                date += workingTime;

                if (date > end)
                    workingTime -= date - end;

                result += workingTime;
            }

            return result;
        }
    }
}
