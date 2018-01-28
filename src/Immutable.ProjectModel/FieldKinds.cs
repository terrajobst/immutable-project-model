using System.Collections.Immutable;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        public static readonly FieldKind AssignmentId = new PassthruFieldKind(typeof(AssignmentId));
        public static readonly FieldKind ResourceId = new PassthruFieldKind(typeof(ResourceId));
        public static readonly FieldKind TaskId = new PassthruFieldKind(typeof(TaskId));
        public static readonly FieldKind TaskIdArray = new PassthruFieldKind(typeof(ImmutableArray<TaskId>));
        public static readonly FieldKind Int32 = new PassthruFieldKind(typeof(int));
        public static readonly FieldKind Boolean = new PassthruFieldKind(typeof(bool));
        public static readonly FieldKind Text = new TextFieldKind();
        public static readonly FieldKind DateTime = new DateTimeOffsetFieldKind();
        public static readonly FieldKind Work = new WorkFieldKind();
        public static readonly FieldKind Duration = new DurationFieldKind();
    }
}
