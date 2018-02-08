using System;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class PassthruFieldKind : FieldKind
        {
            public PassthruFieldKind(Type type)
            {
                Type = type;
            }

            public override Type Type { get; }

            public override string Format(Project project, object value) => Convert.ToString(value);

            public override bool TryParse(Project project, string text, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}
