using System;

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
        public FieldKind Kind { get; }
        public Type Type => Kind.Type;
        internal FieldFlags Flags { get; }
        public object DefaultValue { get; }
        public bool IsTask => (Flags & FieldFlags.Task) == FieldFlags.Task;
        public bool IsResource => (Flags & FieldFlags.Resource) == FieldFlags.Resource;
        public bool IsAssignment => (Flags & FieldFlags.Assignment) == FieldFlags.Assignment;
        internal bool IsVirtual => (Flags & FieldFlags.Virtual) == FieldFlags.Virtual;
        public bool IsReadOnly => (Flags & FieldFlags.ReadOnly) == FieldFlags.ReadOnly;
        public bool ImpactsScheduling => (Flags & FieldFlags.ImpactsScheduling) == FieldFlags.ImpactsScheduling;
        public override string ToString() => Name;
    }
}
