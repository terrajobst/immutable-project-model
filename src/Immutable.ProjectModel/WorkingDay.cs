﻿using System;
using System.Collections.Immutable;
using System.Text;

namespace Immutable.ProjectModel
{
    public sealed class WorkingDay
    {
        public static WorkingDay Create(DayOfWeek dayOfWeek, ImmutableArray<WorkingTime> workingTimes)
        {
            return new WorkingDay(dayOfWeek, workingTimes);
        }

        public static WorkingDay CreateNonWorking(DayOfWeek dayOfWeek)
        {
            return Create(dayOfWeek, ImmutableArray<WorkingTime>.Empty);
        }

        public static WorkingDay CreateEightToFive(DayOfWeek dayOfWeek)
        {
            var eigthToFive = ImmutableArray.Create(
                WorkingTime.Create(TimeSpan.FromHours(8), TimeSpan.FromHours(12)),
                WorkingTime.Create(TimeSpan.FromHours(13), TimeSpan.FromHours(17))
            );

            return Create(dayOfWeek, eigthToFive);
        }

        public static WorkingDay CreateNightShift(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return CreateNonWorking(DayOfWeek.Sunday);
                case DayOfWeek.Monday:
                    return Create(dayOfWeek, ImmutableArray.Create(
                        WorkingTime.Create(TimeSpan.FromHours(23), TimeSpan.FromHours(24))
                    ));
                default:
                    return Create(dayOfWeek, ImmutableArray.Create(
                        WorkingTime.Create(TimeSpan.FromHours(0), TimeSpan.FromHours(3)),
                        WorkingTime.Create(TimeSpan.FromHours(4), TimeSpan.FromHours(8)),
                        WorkingTime.Create(TimeSpan.FromHours(23), TimeSpan.FromHours(24))
                    ));
                case DayOfWeek.Saturday:
                    return Create(dayOfWeek, ImmutableArray.Create(
                        WorkingTime.Create(TimeSpan.FromHours(0), TimeSpan.FromHours(3)),
                        WorkingTime.Create(TimeSpan.FromHours(4), TimeSpan.FromHours(8))
                    ));
            }
        }

        public static WorkingDay CreateTwentyFour(DayOfWeek dayOfWeek)
        {
            var twentyFour = ImmutableArray.Create(
                WorkingTime.Create(TimeSpan.FromHours(0), TimeSpan.FromHours(24))
            );

            return Create(dayOfWeek, twentyFour);
        }

        private WorkingDay(DayOfWeek dayOfWeek, ImmutableArray<WorkingTime> workingTimes)
        {
            DayOfWeek = dayOfWeek;
            WorkingTimes = workingTimes;
        }

        public DayOfWeek DayOfWeek { get; }

        public bool IsNonWorking => WorkingTimes.IsDefaultOrEmpty;

        public ImmutableArray<WorkingTime> WorkingTimes { get; }

        public override string ToString()
        {
            return String.Join(", ", WorkingTimes);
        }
    }
}
