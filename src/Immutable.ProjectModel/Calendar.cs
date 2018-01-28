using System;

namespace Immutable.ProjectModel
{
    public sealed class Calendar
    {
        public static Calendar Default = new Calendar(WorkingWeek.Default);

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
    }
}
