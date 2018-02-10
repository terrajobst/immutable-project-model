using System;

namespace TimephasedExperiments.Contouring
{
    internal sealed class Segment
    {
        public Segment(TimeSpan end, TimeSpan value)
        {
            End = end;
            Value = value;
        }

        public TimeSpan End { get; }
        public TimeSpan Value { get; }

        public override string ToString()
        {
            return $"{End} --- {Value}";
        }
    }
}
