using System;
using System.Diagnostics;

using Immutable.ProjectModel;

namespace TimephasedExperiments.Contouring
{
    public sealed class Assignment
    {
        public Assignment(DateTimeOffset start, DateTimeOffset end, TimeSpan work, Calendar calendar)
        {
            Start = start;
            End = end;
            Work = work;
            Calendar = calendar;
            WorkContour = WorkContour.CreateFixedWork(work, 1.0);
        }

        public DateTimeOffset Start { get; }
        public DateTimeOffset End { get; private set; }
        public TimeSpan Work { get; private set; }
        public Calendar Calendar { get; }
        internal WorkContour WorkContour { get; private set; }

        public WorkContourKind ContourKind
        {
            get => WorkContour.Kind;
            set
            {
                WorkContour = WorkContour.CreateFixedWork(Work, 1.0, value);
                var resultingDuration = WorkContour.GetTotalDuration();
                End = Calendar.AddWork(Start, resultingDuration);
            }
        }

        public TimeSpan GetWork(DateTimeOffset from, DateTimeOffset to)
        {
            if (to < from)
                throw new ArgumentOutOfRangeException(nameof(to));

            // If it's outside, it's definately zero
            if (to <= Start || from >= End)
                return TimeSpan.Zero;

            if (from < Start)
                from = Start;

            if (to > End)
                to = End;

            var zeroBasedFrom = Calendar.GetWork(Start, from);
            var zeroBasedTo = Calendar.GetWork(Start, to);
            return WorkContour.GetWork(zeroBasedFrom, zeroBasedTo);
        }

        public void SetWork(TimeSpan value)
        {
            WorkContour = WorkContour.SetTotalWork(value, 1.0);
            Work = WorkContour.GetTotalWork();
            End = Calendar.AddWork(Start, WorkContour.GetTotalDuration());

            Debug.Assert(Work == value);
        }

        public void SetWork(DateTimeOffset from, DateTimeOffset to, TimeSpan value)
        {
            if (from < Start)
                throw new ArgumentOutOfRangeException(nameof(from));

            if (from >= to)
                throw new ArgumentOutOfRangeException(nameof(to));

            if (value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (Calendar.GetWork(from, to) == TimeSpan.Zero)
                throw new InvalidOperationException("Cannot assign work during non working hours.");

            var zeroBasedFrom = Calendar.GetWork(Start, from);
            var zeroBasedTo = Calendar.GetWork(Start, to);
            WorkContour = WorkContour.SetWork(zeroBasedFrom, zeroBasedTo, value);
            Work = WorkContour.GetTotalWork();
            End = Calendar.AddWork(Start, WorkContour.GetTotalDuration());
        }
    }
}
