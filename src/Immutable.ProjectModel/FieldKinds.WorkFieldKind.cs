using System;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class WorkFieldKind : FieldKind
        {
            public override Type Type => typeof(TimeSpan);

            public override string Format(Project project, object value)
            {
                var actualValue = (TimeSpan)value;
                return project.TimeConversion.FormatWork(actualValue);
            }

            public override bool TryParse(Project project, string text, out object value)
            {
                if (text != null)
                {
                    if (project.TimeConversion.TryParseWork(text, out var work))
                    {
                        value = work;
                        return true;
                    }
                }

                value = null;
                return false;
            }
        }
    }
}
