using System;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class DurationFieldKind : FieldKind
        {
            public override Type Type => typeof(TimeSpan);

            public override string Format(Project project, object value)
            {
                var actualValue = (TimeSpan)value;
                return project.TimeConversion.FormatDuration(actualValue);
            }

            public override bool TryParse(Project project, string text, out object value)
            {
                if (text != null)
                {
                    if (project.TimeConversion.TryParseDuration(text, out var actualValue))
                    {
                        value = actualValue;
                        return true;
                    }
                }

                value = null;
                return false;
            }
        }
    }
}
