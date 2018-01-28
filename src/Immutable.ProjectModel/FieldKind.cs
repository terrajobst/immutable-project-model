using System;

namespace Immutable.ProjectModel
{
    public abstract class FieldKind
    {
        private protected FieldKind()
        {
        }

        public abstract Type Type { get; }
        public abstract string Format(object value);
        public abstract bool TryParse(string text, out object value);
    }
}
