using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace TimephasedExperiments.Contouring
{
    public sealed class WorkContour
    {
        private static ImmutableArray<ImmutableArray<double>> _contourUnits = ImmutableArray.Create
        (
            // Flat
            ImmutableArray.Create(1.00),

            // Front Loaded
            ImmutableArray.Create(1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 0.75, 0.75, 0.75, 0.75, 0.50, 0.50, 0.50, 0.50, 0.25, 0.25, 0.15, 0.15, 0.10, 0.10),

            // Back Loaded
            ImmutableArray.Create(0.10, 0.10, 0.15, 0.15, 0.25, 0.25, 0.50, 0.50, 0.50, 0.50, 0.75, 0.75, 0.75, 0.75, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00),

            // Double Peak
            ImmutableArray.Create(0.25, 0.25, 0.50, 0.50, 1.00, 1.00, 0.50, 0.50, 0.25, 0.25, 0.25, 0.25, 0.50, 0.50, 1.00, 1.00, 0.50, 0.50, 0.25, 0.25),

            // Early Peak
            ImmutableArray.Create(0.25, 0.25, 0.50, 0.50, 1.00, 1.00, 1.00, 1.00, 0.75, 0.75, 0.50, 0.50, 0.50, 0.50, 0.25, 0.25, 0.15, 0.15, 0.10, 0.10),

            // Late Peak
            ImmutableArray.Create(0.10, 0.10, 0.15, 0.15, 0.25, 0.25, 0.50, 0.50, 0.50, 0.50, 0.75, 0.75, 1.00, 1.00, 1.00, 1.00, 0.50, 0.50, 0.25, 0.25),

            // Bell
            ImmutableArray.Create(0.10, 0.10, 0.20, 0.20, 0.40, 0.40, 0.80, 0.80, 1.00, 1.00, 1.00, 1.00, 0.80, 0.80, 0.40, 0.40, 0.20, 0.20, 0.10, 0.10),

            // Turtle
            ImmutableArray.Create(0.25, 0.25, 0.50, 0.50, 0.75, 0.75, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 0.75, 0.75, 0.50, 0.50, 0.25, 0.25)
        );

        public static WorkContour CreateFixedDuration(TimeSpan value, double units, WorkContourKind kind = WorkContourKind.Flat)
        {
            if (kind == WorkContourKind.Contoured)
                throw new ArgumentOutOfRangeException(nameof(kind));

            var duration = TimeSpan.FromHours(value.TotalHours / units);
            var index = (int)kind;
            var contourUnits = _contourUnits[index];
            var segmentDuration = TimeSpan.FromHours(duration.TotalHours / contourUnits.Length);
            var currentStart = TimeSpan.Zero;

            var builder = ImmutableArray.CreateBuilder<Segment>(contourUnits.Length);

            for (var i = 0; i < contourUnits.Length; i++)
            {
                var segmentUnits = contourUnits[i];
                var segmentStart = currentStart;
                var segmentEnd = segmentStart + segmentDuration;
                var segmentValue = TimeSpan.FromHours(value.TotalHours / contourUnits.Length * segmentUnits);
                var segment = new Segment(segmentEnd, segmentValue);
                builder.Add(segment);

                currentStart += segmentDuration;
            }

            var segments = builder.MoveToImmutable();

            return new WorkContour(segments, kind);
        }

        public static WorkContour CreateFixedWork(TimeSpan value, double units, WorkContourKind kind = WorkContourKind.Flat)
        {
            if (kind == WorkContourKind.Contoured)
                throw new ArgumentOutOfRangeException(nameof(kind));

            var duration = TimeSpan.FromHours(value.TotalHours / units);
            var index = (int)kind;
            var contourUnits = _contourUnits[index];
            var currentStart = TimeSpan.Zero;
            var unitsSum = contourUnits.Sum();

            var builder = ImmutableArray.CreateBuilder<Segment>(contourUnits.Length);

            for (var i = 0; i < contourUnits.Length; i++)
            {
                var segmentUnits = contourUnits[i];
                var segmentValue = TimeSpan.FromHours(value.TotalHours / unitsSum * segmentUnits);
                var segmentDuration = TimeSpan.FromHours(segmentValue.TotalHours / segmentUnits);

                var segmentStart = currentStart;
                var segmentEnd = segmentStart + segmentDuration;
                var segment = new Segment(segmentEnd, segmentValue);
                builder.Add(segment);

                currentStart += segmentDuration;
            }

            var segments = builder.MoveToImmutable();

            return new WorkContour(segments, kind);
        }

        private readonly ImmutableArray<Segment> _segments;
        private readonly WorkContourKind _kind;

        private WorkContour(ImmutableArray<Segment> segments, WorkContourKind kind, bool noTrimming = false)
        {
            _kind = kind;
            _segments = Collapse(segments, noTrimming);
        }

        public WorkContourKind Kind => _kind;

        public int SegmentCount => _segments.Length;

        public TimeSpan GetWork(TimeSpan from, TimeSpan to)
        {
            if (to < from)
                throw new ArgumentOutOfRangeException(nameof(to));

            if (from == to)
                return TimeSpan.Zero;

            var durationStart = from;
            var duration = to - from;
            var durationEnd = to;

            var result = TimeSpan.Zero;

            var segmentIndex = _segments.GetSegmentIndex(durationStart);
            Debug.Assert(segmentIndex >= 0);

            while (duration > TimeSpan.Zero)
            {
                var segmentEnd = _segments.GetSegmentEnd(segmentIndex);
                var segmentDuration = _segments.GetSegmentDuration(segmentIndex);
                var segmentValue = _segments.GetSegmentValue(segmentIndex);

                var rangeStart = durationStart;
                var rangeEnd = durationEnd < segmentEnd ? durationEnd : segmentEnd;
                var rangeDuration = rangeEnd - rangeStart;
                var rangeValue = TimeSpan.FromHours(segmentValue.TotalHours / segmentDuration.TotalHours * rangeDuration.TotalHours);

                result += rangeValue;

                durationStart = rangeEnd;
                duration -= rangeDuration;
                durationEnd = durationStart + duration;

                segmentIndex++;
            }

            return result;
        }

        public WorkContour SetWork(TimeSpan from, TimeSpan to, TimeSpan value)
        {
            if (to < from)
                throw new ArgumentOutOfRangeException(nameof(to));

            if (value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(value));

            var fromIndex = _segments.GetSegmentIndex(from);
            var toIndex = _segments.GetSegmentIndex(to);

            if (toIndex < 0)
            {
                var padding = to - GetTotalDuration();
                Debug.Assert(padding > TimeSpan.Zero);

                var paddedSegments = _segments.Add(new Segment(to, TimeSpan.Zero));
                var paddedContour = new WorkContour(paddedSegments, WorkContourKind.Contoured, noTrimming: true);
                return paddedContour.SetWork(from, to, value);
            }

            Debug.Assert(fromIndex >= 0 && toIndex >= fromIndex);

            // Get values for from and to segments

            var fromStart = _segments.GetSegmentStart(fromIndex);
            var fromDuration = _segments.GetSegmentDuration(fromIndex);
            var fromValue = _segments.GetSegmentValue(fromIndex);

            var toStart = _segments.GetSegmentStart(toIndex);
            var toDuration = _segments.GetSegmentDuration(toIndex);
            var toValue = _segments.GetSegmentValue(toIndex);

            // Compute head

            var headStart = fromStart;
            var headEnd = from;
            var headDuration = headEnd - headStart;

            Segment headSegment;

            if (headDuration > TimeSpan.Zero)
            {
                var headValue = TimeSpan.FromHours(fromValue.TotalHours / fromDuration.TotalHours * headDuration.TotalHours);
                headSegment = new Segment(headEnd, headValue);
            }
            else
            {
                headSegment = null;
            }

            // Compute tail

            var tailStart = to;
            var tailEnd = _segments.GetSegmentEnd(toIndex);
            var tailDuration = tailEnd - tailStart;

            Segment tailSegment;

            if (tailDuration > TimeSpan.Zero)
            {
                var tailValue = TimeSpan.FromHours(toValue.TotalHours / toDuration.TotalHours * tailDuration.TotalHours);
                tailSegment = new Segment(tailEnd, tailValue);
            }
            else
            {
                tailSegment = null;
            }

            // Compute center

            var centerEnd = to;
            var centerValue = value;
            var centerSegment = new Segment(centerEnd, centerValue);

            // Update segments
            //
            // First, we need the count:

            var segmentCount = fromIndex;
            if (headSegment != null)
                segmentCount++;

            segmentCount++;

            if (tailSegment != null)
                segmentCount++;

            segmentCount += _segments.Length - toIndex - 1;

            // Now we can create an appropriately sized builder and copy the data over.

            var builder = ImmutableArray.CreateBuilder<Segment>(segmentCount);

            for (var i = 0; i < fromIndex; i++)
                builder.Add(_segments[i]);

            if (headSegment != null)
                builder.Add(headSegment);

            builder.Add(centerSegment);

            if (tailSegment != null)
                builder.Add(tailSegment);

            for (var i = toIndex + 1; i < _segments.Length; i++)
                builder.Add(_segments[i]);

            var segments = builder.MoveToImmutable();
            return new WorkContour(segments, WorkContourKind.Contoured);
        }

        public int GetSegmentIndex(TimeSpan time)
        {
            return _segments.GetSegmentIndex(time);
        }

        public TimeSpan GetSegmentStart(int segmentIndex)
        {
            return _segments.GetSegmentStart(segmentIndex);
        }

        public TimeSpan GetSegmentEnd(int segmentIndex)
        {
            return _segments.GetSegmentEnd(segmentIndex);
        }

        public TimeSpan GetSegmentDuration(int segmentIndex)
        {
            return _segments.GetSegmentDuration(segmentIndex);
        }

        public TimeSpan GetSegmentValue(int segmentIndex)
        {
            return _segments.GetSegmentValue(segmentIndex);
        }

        public TimeSpan GetTotalDuration()
        {
            if (_segments.Length == 0)
                return TimeSpan.Zero;

            var lastIndex = _segments.Length - 1;
            return _segments[lastIndex].End;
        }

        public TimeSpan GetTotalWork()
        {
            var result = TimeSpan.Zero;
            foreach (var segment in _segments)
                result += segment.Value;
            return result;
        }

        public WorkContour SetTotalWork(TimeSpan value, double units)
        {
            if (_kind != WorkContourKind.Contoured)
                return CreateFixedWork(value, units, _kind);

            var delta = value - GetTotalWork();
            if (delta > TimeSpan.Zero)
            {
                var previousEnd = GetTotalDuration();
                var end = previousEnd + TimeSpan.FromHours(delta.TotalHours / units);
                var segments = _segments.Add(new Segment(end, delta));
                return new WorkContour(segments, Kind);
            }
            else
            {
                delta = -delta;

                var builder = _segments.ToBuilder();

                while (delta > TimeSpan.Zero)
                {
                    Debug.Assert(builder.Count > 0);

                    var lastIndex = builder.Count - 1;
                    var segmentWork = builder.GetSegmentValue(lastIndex);
                    if (segmentWork < delta)
                    {
                        delta -= segmentWork;
                        builder.RemoveAt(lastIndex);
                    }
                    else
                    {
                        var segmentStart = builder.GetSegmentStart(lastIndex);
                        var segmentDuration = builder.GetSegmentDuration(lastIndex);
                        var segmentUnits = segmentWork.TotalHours / segmentDuration.TotalHours;
                        var newSegmentWork = segmentWork - delta;
                        var newSegmentDuration = TimeSpan.FromHours(newSegmentWork.TotalHours * segmentUnits);
                        var newSegmentStart = segmentStart;
                        var newSegmentEnd = newSegmentStart + newSegmentDuration;
                        var newSegment = new Segment(newSegmentEnd, newSegmentWork);
                        builder[lastIndex] = newSegment;
                        delta = TimeSpan.Zero;
                    }
                }

                return new WorkContour(builder.ToImmutable(), _kind);
            }
        }

        private static ImmutableArray<Segment> Collapse(ImmutableArray<Segment> contour, bool noTrimming)
        {
            // The idea of this function is to collapse segments if their units are the
            // same. The goal is to minimize the number of segments.
            //
            // TODO: Do we need to do a sequential scan -- or -- can we isolate where
            //       the edit occurs and only merge around that? We need to be careful
            //       to handle cascading effects, though.
            //
            // TODO: Should we use an approximate comparison using an epsilon?

            var previousUnits = 0.0;
            var builder = (ImmutableArray<Segment>.Builder)null;

            for (var i = 0; i < contour.Length; i++)
            {
                var segmentDuration = contour.GetSegmentDuration(i);
                var segmentValue = contour.GetSegmentValue(i);
                var units = segmentValue.TotalHours / segmentDuration.TotalHours;

                if (i > 0 && previousUnits == units)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<Segment>(contour.Length);
                        for (var j = 0; j < i; j++)
                            builder.Add(contour[j]);
                    }

                    var lastIndex = builder.Count - 1;
                    var previousEnd = builder.GetSegmentEnd(lastIndex);
                    var previousValue = builder.GetSegmentValue(lastIndex);
                    builder[lastIndex] = new Segment(previousEnd + segmentDuration, previousValue + segmentValue);
                }
                else
                {
                    if (builder != null)
                        builder.Add(contour[i]);

                    previousUnits = units;
                }
            }

            // Trim end

            if (!noTrimming)
            {
                if (builder == null && contour.Length > 0 && contour[contour.Length - 1].Value == TimeSpan.Zero)
                {
                    builder = ImmutableArray.CreateBuilder<Segment>();
                    builder.AddRange(contour);
                }

                if (builder != null)
                {
                    while (builder.Count > 0 && builder[builder.Count - 1].Value == TimeSpan.Zero)
                        builder.RemoveAt(builder.Count - 1);
                }
            }

            if (builder == null)
                return contour;

            return builder.ToImmutableArray();
        }
    }
}
