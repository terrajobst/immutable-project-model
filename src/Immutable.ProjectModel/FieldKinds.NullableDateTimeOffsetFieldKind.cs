using System;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class NullableDateTimeOffsetFieldKind : FieldKind
        {
            public override Type Type => typeof(DateTimeOffset?);

            public override string Format(Project project, object value)
            {
                if (value == null)
                    return "NA";

                var actualValue = (DateTimeOffset)value;
                return actualValue.ToString("MM/dd/yyyy hh:mm tt");
            }

            public override bool TryParse(Project project, string text, out object value)
            {
                if (string.IsNullOrEmpty(text) || text == "NA")
                {
                    value = null;
                    return true;
                }

                var result = DateTimeOffset.TryParse(text, out var actualValue);
                value = actualValue;
                return result;
            }
        }
    }
}
