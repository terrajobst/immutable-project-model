using System;

namespace Immutable.ProjectModel
{
    public abstract class FieldKind
    {
        private protected FieldKind()
        {
        }

        public abstract Type Type { get; }
        public abstract string Format(Project project, object value);
        public abstract bool TryParse(Project project, string text, out object value);
    }
}
