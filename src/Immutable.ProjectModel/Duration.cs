using System;
using System.Collections.Generic;

namespace Immutable.ProjectModel
{
    public struct Duration : IEquatable<Duration>
    {
        private readonly TimeSpan _span;
        private readonly sbyte _unit;

        public Duration(TimeSpan span, TimeUnit unit, bool isEstimated)
        {
            _span = span;
            _unit = (sbyte)unit;
            if (isEstimated)
                _unit = (sbyte)-_unit;
        }

        public TimeSpan Span => _span;

        public TimeUnit Unit => (TimeUnit)Math.Abs(_unit);

        public bool IsEstimated => _unit < 0;

        public override bool Equals(object obj)
        {
            var other = obj as Duration?;
            return other != null && Equals(other.Value);
        }

        public bool Equals(Duration other)
        {
            return _span.Equals(other._span) &&
                   _unit == other._unit;
        }

        public override int GetHashCode()
        {
            var hashCode = -2128944354;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<TimeSpan>.Default.GetHashCode(_span);
            hashCode = hashCode * -1521134295 + _unit.GetHashCode();
            return hashCode;
        }

        public Duration WithSpan(TimeSpan span)
        {
            return new Duration(span, Unit, IsEstimated);
        }

        public Duration WithUnit(TimeUnit unit)
        {
            return new Duration(Span, unit, IsEstimated);
        }

        public Duration WithIsEstimated(bool isEstimated)
        {
            return new Duration(Span, Unit, isEstimated);
        }

        public static bool TryParse(string text, out Duration result)
        {
            return TryParse(text, TimeConversion.Default, out result);
        }

        public static bool TryParse(string text, TimeConversion conversion, out Duration result)
        {
            if (conversion.TryParseDuration(text, out var span, out var units, out var isEstimated))
            {
                result = new Duration(span, units, isEstimated);
                return true;
            }

            result = default;
            return false;
        }

        public static Duration Parse(string text)
        {
            return Parse(text, TimeConversion.Default);
        }

        public static Duration Parse(string text, TimeConversion conversion)
        {
            if (TryParse(text, conversion, out var result))
                return result;

            throw new FormatException($"'{text}' isn't a valid value for duration");
        }

        public string ToString(TimeConversion conversion)
        {
            return conversion.FormatDuration(Span, Unit, IsEstimated);
        }

        public override string ToString()
        {
            return ToString(TimeConversion.Default);
        }

        public static bool operator ==(Duration duration1, Duration duration2)
        {
            return duration1.Equals(duration2);
        }

        public static bool operator !=(Duration duration1, Duration duration2)
        {
            return !(duration1 == duration2);
        }
    }
}
