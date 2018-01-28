using System;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class DateTimeOffsetFieldKind : FieldKind
        {
            public override Type Type => typeof(DateTimeOffset);

            public override string Format(object value)
            {
                var actualValue = (DateTimeOffset)value;
                return actualValue.ToString("MM/dd/yyyy hh:mm tt");
            }

            public override bool TryParse(string text, out object value)
            {
                var result = DateTimeOffset.TryParse(text, out var actualValue);
                value = actualValue;
                return result;
            }
        }
    }
}
