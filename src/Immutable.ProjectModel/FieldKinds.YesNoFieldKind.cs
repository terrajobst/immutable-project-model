using System;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class YesNoFieldKind : FieldKind
        {
            public override Type Type => typeof(bool);

            public override string Format(Project project, object value)
            {
                var actualValue = (bool)value;
                return actualValue ? "Yes" : "No";
            }

            public override bool TryParse(Project project, string text, out object value)
            {
                var isYes = string.Equals(text, "Yes", StringComparison.OrdinalIgnoreCase);
                var isNo = string.Equals(text, "No", StringComparison.OrdinalIgnoreCase);

                value = isYes ? true : isNo ? (object) false : null;
                return isYes || isNo;
            }
        }
    }
}
