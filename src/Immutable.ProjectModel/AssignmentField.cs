using System;

namespace Immutable.ProjectModel
{
    public abstract class AssignmentField : FieldDefinition
    {
        private protected AssignmentField(string name, FieldKind fieldType, FieldFlags flags, object defaultValue)
            : base(name, fieldType, flags | FieldFlags.Assignment, defaultValue)
        {
        }

        internal virtual Func<ProjectData, AssignmentData, object, ProjectData> GetSetter()
            => (p, t, v) => p.UpdateAssignment(t.SetValue(this, v));

        internal Func<ProjectData, AssignmentData, object, ProjectData> Setter => GetSetter();
    }
}
