using System;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public sealed class TaskField<T> : TaskField
    {
        internal TaskField(string name, FieldKind fieldType, FieldFlags flags = default, T defaultValue = default, Func<ProjectData, TaskData, T, ProjectData> setter = null)
            : base(name, fieldType, flags, defaultValue)
        {
            Setter = setter;
            Debug.Assert(typeof(T) == fieldType.Type);
        }

        public new T DefaultValue => (T)base.DefaultValue;

        internal new Func<ProjectData, TaskData, T, ProjectData> Setter { get; }

        internal override Func<ProjectData, TaskData, object, ProjectData> GetSetter()
        {
            if (Setter == null)
                return base.GetSetter();

            return (p, t, v) => Setter(p, t, (T)v);
        }
    }
}
