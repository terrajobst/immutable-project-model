using System;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public sealed class AssignmentField<T> : AssignmentField
    {
        internal AssignmentField(string name, FieldKind fieldType, FieldFlags flags = default, T defaultValue = default, Func<ProjectData, AssignmentData, T, ProjectData> setter = null)
            : base(name, fieldType, flags, defaultValue)
        {
            Setter = setter;
            Debug.Assert(typeof(T) == fieldType.Type);
        }

        public new T DefaultValue => (T)base.DefaultValue;

        internal new Func<ProjectData, AssignmentData, T, ProjectData> Setter { get; }

        internal override Func<ProjectData, AssignmentData, object, ProjectData> GetSetter()
        {
            if (Setter == null)
                return base.GetSetter();

            return (p, t, v) => Setter(p, t, (T)v);
        }
    }
}
