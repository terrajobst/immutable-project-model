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
        public static readonly AssignmentField<TimeSpan> Work = new AssignmentField<TimeSpan>("Work", FieldKinds.Work, setter: Scheduler.SetAssignmentWork);
        public static readonly AssignmentField<double> Units = new AssignmentField<double>("Units", FieldKinds.Percentage, defaultValue:1.0, setter: Scheduler.SetAssignmentUnits);
        public static readonly AssignmentField<DateTimeOffset> Start = new AssignmentField<DateTimeOffset>("Start", FieldKinds.DateTime, FieldFlags.ReadOnly | FieldFlags.Virtual);
        public static readonly AssignmentField<DateTimeOffset> Finish = new AssignmentField<DateTimeOffset>("Finish", FieldKinds.DateTime, FieldFlags.ReadOnly | FieldFlags.Virtual);
        public static readonly AssignmentField<DateTimeOffset> EarlyStart = new AssignmentField<DateTimeOffset>("EarlyStart", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly AssignmentField<DateTimeOffset> EarlyFinish = new AssignmentField<DateTimeOffset>("EarlyFinish", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly AssignmentField<DateTimeOffset> LateStart = new AssignmentField<DateTimeOffset>("LateStart", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly AssignmentField<DateTimeOffset> LateFinish = new AssignmentField<DateTimeOffset>("LateFinish", FieldKinds.DateTime, FieldFlags.ReadOnly);

        public static readonly ImmutableArray<AssignmentField> All = typeof(AssignmentFields).GetFields(BindingFlags.Static | BindingFlags.Public)
                                                                                             .Select(f => f.GetValue(null))
                                                                                             .OfType<AssignmentField>()
                                                                                             .ToImmutableArray();
    }
}
