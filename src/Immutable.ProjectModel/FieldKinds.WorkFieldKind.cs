using System;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class WorkFieldKind : FieldKind
        {
            public override Type Type => typeof(TimeSpan);

            public override string Format(object value)
            {
                var actualValue = (TimeSpan)value;
                var hours = Math.Round(actualValue.TotalHours, 2, MidpointRounding.AwayFromZero);
                return $"{hours} hrs";
            }

            public override bool TryParse(string text, out object value)
            {
                if (text != null)
                {
                    if (text.EndsWith("hrs"))
                        text = text.Substring(0, text.Length - 3);

                    text = text.Trim();

                    if (double.TryParse(text, out var actualValue))
                    {
                        value = TimeSpan.FromHours(actualValue);
                        return true;
                    }
                }

                value = null;
                return false;
            }
        }
    }
}
