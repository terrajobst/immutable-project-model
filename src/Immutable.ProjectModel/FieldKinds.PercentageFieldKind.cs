using System;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class PercentageFieldKind : FieldKind
        {
            public override Type Type => typeof(double);

            public override string Format(Project project, object value)
            {
                var actualValue = (double)value * 100;
                return $"{actualValue}%";
            }

            public override bool TryParse(Project project, string text, out object value)
            {
                if (text != null)
                {
                    if (text.EndsWith("%"))
                        text = text.Substring(0, text.Length - 1);

                    text = text.Trim();

                    if (double.TryParse(text, out var actualValue))
                    {
                        value = actualValue / 100.0;
                        return true;
                    }
                }

                value = null;
                return false;
            }
        }
    }
}
