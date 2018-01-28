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

            public override string Format(object value) => Convert.ToString(value);

            public override bool TryParse(string text, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}
