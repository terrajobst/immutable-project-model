using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public sealed class AssignmentField<T> : AssignmentField
    {
        internal AssignmentField(string name, FieldKind fieldType, FieldFlags flags = default, T defaultValue = default)
            : base(name, fieldType, flags, defaultValue)
        {
            Debug.Assert(typeof(T) == fieldType.Type);
        }

        public new T DefaultValue => (T)base.DefaultValue;
    }
}
