using System;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class TextFieldKind : FieldKind
        {
            public override Type Type => typeof(string);

            public override string Format(Project project, object value) => (string)value;

            public override bool TryParse(Project project, string text, out object value)
            {
                value = text;
                return true;
            }
        }
    }
}
