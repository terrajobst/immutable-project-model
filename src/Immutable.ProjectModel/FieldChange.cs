namespace Immutable.ProjectModel
{
    public sealed class FieldChange<T>
        where T: FieldDefinition
    {
        public FieldChange(T field, object oldValue, object newValue)
        {
            Field = field;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T Field { get; }
        public object OldValue { get; }
        public object NewValue { get; }

        public override string ToString()
        {
            return $"{Field.Name} {OldValue} => {NewValue}";
        }
    }
}
