using System;
using System.Collections;
using System.Linq;

namespace Immutable.ProjectModel
{
    public abstract class FieldKind
    {
        private protected FieldKind()
        {
        }

        public abstract Type Type { get; }
        public virtual bool HasSuggestions => false;

        public abstract string Format(Project project, object value);
        public abstract bool TryParse(Project project, string text, out object value);
        public virtual IEnumerable GetSuggestions() => Enumerable.Empty<object>();
    }
}
