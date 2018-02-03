using System;

namespace Immutable.ProjectModel
{
    public sealed class WorkingTime
    {
        public static WorkingTime Create(TimeSpan from, TimeSpan to)
        {
            return new WorkingTime(from, to);
        }

        private WorkingTime(TimeSpan from, TimeSpan to)
        {
            From = from;
            To = to;
        }

        public TimeSpan From { get; }

        public TimeSpan To { get; }

        public TimeSpan Length => To - From;

        public WorkingTime With(TimeSpan from, TimeSpan to)
        {
            if (from == From && to == To)
                return this;

            return new WorkingTime(from, to);
        }

        public WorkingTime WithFrom(TimeSpan from) => With(from, To);

        public WorkingTime WithTo(TimeSpan to) => With(From, to);

        public override string ToString()
        {
            return $"{From.Hours:D2}:{From.Minutes:D2} - {To.Hours:D2}:{To.Minutes:D2}";
        }
    }
}
