using System;

namespace Immutable.ProjectModel
{
    public abstract class TaskField : FieldDefinition
    {
        private protected TaskField(string name, FieldKind fieldType, FieldFlags flags, object defaultValue)
            : base(name, fieldType, flags | FieldFlags.Task, defaultValue)
        {
        }

        internal virtual Func<ProjectData, TaskData, object, ProjectData> GetSetter()
            => (p, t, v) => p.UpdateTask(t.SetValue(this, v));

        internal Func<ProjectData, TaskData, object, ProjectData> Setter => GetSetter();
    }
}
