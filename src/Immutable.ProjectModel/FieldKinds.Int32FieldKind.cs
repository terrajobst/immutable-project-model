using System;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class Int32FieldKind : FieldKind
        {
            public override Type Type => typeof(int);

            public override string Format(Project project, object value)
            {
                var actualValue = (int)value;
                return actualValue.ToString();
            }

            public override bool TryParse(Project project, string text, out object value)
            {
                if (text != null)
                {
                    if (int.TryParse(text, out var actualValue))
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
