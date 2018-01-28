namespace Immutable.ProjectModel
{
    public abstract class ResourceField : FieldDefinition
    {
        private protected ResourceField(string name, FieldKind fieldType, FieldFlags flags, object defaultValue)
            : base(name, fieldType, flags | FieldFlags.Resource, defaultValue)
        {
        }
    }
}
