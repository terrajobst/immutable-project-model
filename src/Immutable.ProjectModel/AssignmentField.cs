namespace Immutable.ProjectModel
{
    public abstract class AssignmentField : FieldDefinition
    {
        private protected AssignmentField(string name, FieldKind fieldType, FieldFlags flags, object defaultValue)
            : base(name, fieldType, flags | FieldFlags.Assignment, defaultValue)
        {
        }
    }
}
