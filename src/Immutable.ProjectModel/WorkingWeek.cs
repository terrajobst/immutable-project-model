using System;

namespace Immutable.ProjectModel
{
    public sealed class WorkingWeek
    {
        public static WorkingWeek Default = Create(
            WorkingDay.CreateEightToFive(DayOfWeek.Monday),
            WorkingDay.CreateEightToFive(DayOfWeek.Tuesday),
            WorkingDay.CreateEightToFive(DayOfWeek.Wednesday),
            WorkingDay.CreateEightToFive(DayOfWeek.Thursday),
            WorkingDay.CreateEightToFive(DayOfWeek.Friday),
            WorkingDay.CreateNonWorking(DayOfWeek.Saturday),
            WorkingDay.CreateNonWorking(DayOfWeek.Sunday)
        );

        public static WorkingWeek NightShift = Create(
            WorkingDay.CreateNightShift(DayOfWeek.Monday),
            WorkingDay.CreateNightShift(DayOfWeek.Tuesday),
            WorkingDay.CreateNightShift(DayOfWeek.Wednesday),
            WorkingDay.CreateNightShift(DayOfWeek.Thursday),
            WorkingDay.CreateNightShift(DayOfWeek.Friday),
            WorkingDay.CreateNightShift(DayOfWeek.Saturday),
            WorkingDay.CreateNightShift(DayOfWeek.Sunday)
        );

        public static WorkingWeek TwentyFourSeven = Create(
            WorkingDay.CreateTwentyFour(DayOfWeek.Monday),
            WorkingDay.CreateTwentyFour(DayOfWeek.Tuesday),
            WorkingDay.CreateTwentyFour(DayOfWeek.Wednesday),
            WorkingDay.CreateTwentyFour(DayOfWeek.Thursday),
            WorkingDay.CreateTwentyFour(DayOfWeek.Friday),
            WorkingDay.CreateTwentyFour(DayOfWeek.Saturday),
            WorkingDay.CreateTwentyFour(DayOfWeek.Sunday)
        );

        public static WorkingWeek NonWorking = new WorkingWeek(
            WorkingDay.CreateNonWorking(DayOfWeek.Monday),
            WorkingDay.CreateNonWorking(DayOfWeek.Tuesday),
            WorkingDay.CreateNonWorking(DayOfWeek.Wednesday),
            WorkingDay.CreateNonWorking(DayOfWeek.Thursday),
            WorkingDay.CreateNonWorking(DayOfWeek.Friday),
            WorkingDay.CreateNonWorking(DayOfWeek.Saturday),
            WorkingDay.CreateNonWorking(DayOfWeek.Sunday)
        );

        public static WorkingWeek Create(WorkingDay monday,
                                         WorkingDay tuesday,
                                         WorkingDay wednesday,
                                         WorkingDay thursday,
                                         WorkingDay friday,
                                         WorkingDay saturday,
                                         WorkingDay sunday)
        {
            if (monday == null)
                throw new ArgumentNullException(nameof(monday));

            if (tuesday == null)
                throw new ArgumentNullException(nameof(tuesday));

            if (wednesday == null)
                throw new ArgumentNullException(nameof(wednesday));

            if (thursday == null)
                throw new ArgumentNullException(nameof(thursday));

            if (friday == null)
                throw new ArgumentNullException(nameof(friday));

            if (saturday == null)
                throw new ArgumentNullException(nameof(saturday));

            if (sunday == null)
                throw new ArgumentNullException(nameof(sunday));

            if (monday.DayOfWeek != DayOfWeek.Monday)
                throw new ArgumentOutOfRangeException(nameof(monday));

            if (tuesday.DayOfWeek != DayOfWeek.Tuesday)
                throw new ArgumentOutOfRangeException(nameof(tuesday));

            if (wednesday.DayOfWeek != DayOfWeek.Wednesday)
                throw new ArgumentOutOfRangeException(nameof(wednesday));

            if (thursday.DayOfWeek != DayOfWeek.Thursday)
                throw new ArgumentOutOfRangeException(nameof(thursday));

            if (friday.DayOfWeek != DayOfWeek.Friday)
                throw new ArgumentOutOfRangeException(nameof(friday));

            if (saturday.DayOfWeek != DayOfWeek.Saturday)
                throw new ArgumentOutOfRangeException(nameof(saturday));

            if (sunday.DayOfWeek != DayOfWeek.Sunday)
                throw new ArgumentOutOfRangeException(nameof(sunday));

            return new WorkingWeek(monday,
                                   tuesday,
                                   wednesday,
                                   thursday,
                                   friday,
                                   saturday,
                                   sunday);
        }

        private readonly WorkingDay _monday;
        private readonly WorkingDay _tuesday;
        private readonly WorkingDay _wednesday;
        private readonly WorkingDay _thursday;
        private readonly WorkingDay _friday;
        private readonly WorkingDay _saturday;
        private readonly WorkingDay _sunday;

        private WorkingWeek(WorkingDay monday,
                            WorkingDay tuesday,
                            WorkingDay wednesday,
                            WorkingDay thursday,
                            WorkingDay friday,
                            WorkingDay saturday,
                            WorkingDay sunday)
        {
            _monday = monday;
            _tuesday = tuesday;
            _wednesday = wednesday;
            _thursday = thursday;
            _friday = friday;
            _saturday = saturday;
            _sunday = sunday;
        }

        public bool IsNonWorking
        {
            get
            {
                return _monday.IsNonWorking &&
                       _tuesday.IsNonWorking &&
                       _wednesday.IsNonWorking &&
                       _thursday.IsNonWorking &&
                       _friday.IsNonWorking &&
                       _saturday.IsNonWorking &&
                       _sunday.IsNonWorking;
            }
        }

        public WorkingDay this[DayOfWeek dayOfWeek]
        {
            get
            {
                switch (dayOfWeek)
                {
                    case DayOfWeek.Monday:
                        return _monday;
                    case DayOfWeek.Tuesday:
                        return _tuesday;
                    case DayOfWeek.Wednesday:
                        return _wednesday;
                    case DayOfWeek.Thursday:
                        return _thursday;
                    case DayOfWeek.Friday:
                        return _friday;
                    case DayOfWeek.Saturday:
                        return _saturday;
                    case DayOfWeek.Sunday:
                    default:
                        return _sunday;
                }
            }
        }

        public WorkingWeek WithDay(WorkingDay day)
        {
            if (day == null)
                throw new ArgumentNullException(nameof(day));

            var monday = day.DayOfWeek == DayOfWeek.Monday ? day : this[DayOfWeek.Monday];
            var tuesday = day.DayOfWeek == DayOfWeek.Tuesday ? day : this[DayOfWeek.Tuesday];
            var wednesday = day.DayOfWeek == DayOfWeek.Wednesday ? day : this[DayOfWeek.Wednesday];
            var thursday = day.DayOfWeek == DayOfWeek.Thursday ? day : this[DayOfWeek.Thursday];
            var friday = day.DayOfWeek == DayOfWeek.Friday ? day : this[DayOfWeek.Friday];
            var saturday = day.DayOfWeek == DayOfWeek.Saturday ? day : this[DayOfWeek.Saturday];
            var sunday = day.DayOfWeek == DayOfWeek.Sunday ? day : this[DayOfWeek.Sunday];
            return new WorkingWeek(monday, tuesday, wednesday, thursday, friday, saturday, sunday);
        }
    }
}
