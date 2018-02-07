using System;

namespace Immutable.ProjectModel
{
    public abstract class TaskField : FieldDefinition
    {
        private protected TaskField(string name, FieldKind fieldType, FieldFlags flags, object defaultValue)
            : base(name, fieldType, flags | FieldFlags.Task, defaultValue)
        {
        }
    }
}
