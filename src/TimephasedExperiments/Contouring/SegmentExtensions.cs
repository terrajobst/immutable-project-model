using System;
using System.Collections.Generic;

namespace TimephasedExperiments.Contouring
{
    internal static class SegmentExtensions
    {
        public static int GetSegmentIndex<T>(this T contour, TimeSpan time)
            where T : IReadOnlyList<Segment>
        {
            var lo = 0;
            var hi = contour.Count - 1;

            while (lo <= hi)
            {
                var i = lo + ((hi - lo) >> 1);

                var segmentStart = contour.GetSegmentStart(i);
                var segmentEnd = contour.GetSegmentEnd(i);

                if (segmentStart <= time && time < segmentEnd)
                    return i;

                if (segmentEnd <= time)
                    lo = i + 1;
                else
                    hi = i - 1;
            }

            if (contour.Count > 0 && contour[contour.Count - 1].End == time)
                return contour.Count - 1;

            return ~lo;
        }

        public static TimeSpan GetSegmentStart<T>(this T contour, int segmentIndex)
            where T : IReadOnlyList<Segment>
        {
            if (segmentIndex == 0)
                return TimeSpan.Zero;

            return contour[segmentIndex - 1].End;
        }

        public static TimeSpan GetSegmentEnd<T>(this T contour, int segmentIndex)
            where T : IReadOnlyList<Segment>
        {
            return contour[segmentIndex].End;
        }

        public static TimeSpan GetSegmentDuration<T>(this T contour, int segmentIndex)
            where T : IReadOnlyList<Segment>
        {
            return contour.GetSegmentEnd(segmentIndex) - contour.GetSegmentStart(segmentIndex);
        }

        public static TimeSpan GetSegmentValue<T>(this T contour, int segmentIndex)
            where T : IReadOnlyList<Segment>
        {
            return contour[segmentIndex].Value;
        }
    }
}
