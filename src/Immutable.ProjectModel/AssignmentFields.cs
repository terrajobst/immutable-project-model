using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Immutable.ProjectModel
{
    public static class AssignmentFields
    {
        public static readonly AssignmentField<AssignmentId> Id = new AssignmentField<AssignmentId>("Id", FieldKinds.AssignmentId, FieldFlags.ReadOnly | FieldFlags.Virtual);
        public static readonly AssignmentField<TaskId> TaskId = new AssignmentField<TaskId>("TaskId", FieldKinds.TaskId, FieldFlags.ReadOnly | FieldFlags.Virtual);
        public static readonly AssignmentField<ResourceId> ResourceId = new AssignmentField<ResourceId>("ResourceId", FieldKinds.ResourceId, FieldFlags.ReadOnly | FieldFlags.Virtual);
        public static readonly AssignmentField<TimeSpan> Work = new AssignmentField<TimeSpan>("Work", FieldKinds.Work);
        public static readonly AssignmentField<double> Units = new AssignmentField<double>("Units", FieldKinds.Percentage, defaultValue:1.0);
        public static readonly AssignmentField<DateTimeOffset> Start = new AssignmentField<DateTimeOffset>("Start", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly AssignmentField<DateTimeOffset> Finish = new AssignmentField<DateTimeOffset>("Finish", FieldKinds.DateTime, FieldFlags.ReadOnly);

        public static readonly ImmutableArray<AssignmentField> All = typeof(AssignmentFields).GetFields(BindingFlags.Static | BindingFlags.Public)
                                                                                             .Select(f => f.GetValue(null))
                                                                                             .OfType<AssignmentField>()
                                                                                             .ToImmutableArray();
    }
}
