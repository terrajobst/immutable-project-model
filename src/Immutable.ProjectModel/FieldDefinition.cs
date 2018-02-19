using System;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public abstract class FieldDefinition
    {
        private protected FieldDefinition(string name, FieldKind fieldType, FieldFlags flags, object defaultValue)
        {
            Name = name;
            Kind = fieldType;
            Flags = flags;
            DefaultValue = defaultValue;
        }

        public string Name { get; }

        public string QualifiedName
        {
            get
            {
                if (IsTask) return "Task." + Name;
                if (IsResource) return "Resource." + Name;
                Debug.Assert(IsAssignment);
                return "Assignment." + Name;
            }
        }

        public FieldKind Kind { get; }

        public Type Type => Kind.Type;

        internal FieldFlags Flags { get; }

        public object DefaultValue { get; }

        public bool IsTask => (Flags & FieldFlags.Task) == FieldFlags.Task;

        public bool IsResource => (Flags & FieldFlags.Resource) == FieldFlags.Resource;

        public bool IsAssignment => (Flags & FieldFlags.Assignment) == FieldFlags.Assignment;

        public bool IsReadOnly => (Flags & FieldFlags.ReadOnly) == FieldFlags.ReadOnly;

        public bool IsAssignableFrom(object value)
        {
            if (value == null)
            {
                return Type.IsClass ||
                       Type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            return Type.IsAssignableFrom(value.GetType());
        }

        public override string ToString() => Name;
    }
}
