using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Immutable.ProjectModel
{
    public static class TaskFields
    {
        public static readonly TaskField<TaskId> Id = new TaskField<TaskId>("Id", FieldKinds.TaskId, FieldFlags.ReadOnly);
        public static readonly TaskField<int> Ordinal = new TaskField<int>("Ordinal", FieldKinds.Int32);
        public static readonly TaskField<string> Name = new TaskField<string>("Name", FieldKinds.Text);
        public static readonly TaskField<TimeSpan> Duration = new TaskField<TimeSpan>("Duration", FieldKinds.Duration);
        public static readonly TaskField<TimeSpan> Work = new TaskField<TimeSpan>("Work", FieldKinds.Work);
        public static readonly TaskField<DateTimeOffset> Start = new TaskField<DateTimeOffset>("Start", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<DateTimeOffset> Finish = new TaskField<DateTimeOffset>("Finish", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<DateTimeOffset> EarlyStart = new TaskField<DateTimeOffset>("EarlyStart", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<DateTimeOffset> EarlyFinish = new TaskField<DateTimeOffset>("EarlyFinish", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<DateTimeOffset> LateStart = new TaskField<DateTimeOffset>("LateStart", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<DateTimeOffset> LateFinish = new TaskField<DateTimeOffset>("LateFinish", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<TimeSpan> StartSlack = new TaskField<TimeSpan>("StartSlack", FieldKinds.Duration, FieldFlags.ReadOnly);
        public static readonly TaskField<TimeSpan> FinishSlack = new TaskField<TimeSpan>("FinishSlack", FieldKinds.Duration, FieldFlags.ReadOnly);
        public static readonly TaskField<TimeSpan> TotalSlack = new TaskField<TimeSpan>("TotalSlack", FieldKinds.Duration, FieldFlags.ReadOnly);
        public static readonly TaskField<TimeSpan> FreeSlack = new TaskField<TimeSpan>("FreeSlack", FieldKinds.Duration, FieldFlags.ReadOnly);
        public static readonly TaskField<ConstraintType> ConstraintType = new TaskField<ConstraintType>("ConstraintType", FieldKinds.ConstraintType);
        public static readonly TaskField<DateTimeOffset?> ConstraintDate = new TaskField<DateTimeOffset?>("ConstraintDate", FieldKinds.NullableDateTime);
        public static readonly TaskField<bool> IsCritical = new TaskField<bool>("IsCritical", FieldKinds.YesNo, FieldFlags.ReadOnly);
        public static readonly TaskField<bool> IsMilestone = new TaskField<bool>("IsMilestone", FieldKinds.YesNo, defaultValue: true);
        public static readonly TaskField<string> ResourceNames = new TaskField<string>("ResourceNames", FieldKinds.Text);
        public static readonly TaskField<string> Predecessors = new TaskField<string>("Predecessors", FieldKinds.Text);

        internal static readonly TaskField<ImmutableArray<TaskLink>> PredecessorLinks = new TaskField<ImmutableArray<TaskLink>>("PredecessorLinks", FieldKinds.TaskLinkArray, FieldFlags.ReadOnly, ImmutableArray<TaskLink>.Empty);
        internal static readonly TaskField<ImmutableArray<TaskLink>> SuccessorLinks = new TaskField<ImmutableArray<TaskLink>>("SuccessorLinks", FieldKinds.TaskLinkArray, FieldFlags.ReadOnly, ImmutableArray<TaskLink>.Empty);
        internal static readonly TaskField<ImmutableArray<AssignmentId>> Assignments = new TaskField<ImmutableArray<AssignmentId>>("Assignments", FieldKinds.AssignmentIdArray, FieldFlags.ReadOnly, ImmutableArray<AssignmentId>.Empty);

        public static readonly ImmutableArray<TaskField> All = typeof(TaskFields).GetFields(BindingFlags.Static | BindingFlags.Public)
                                                                                 .Select(f => f.GetValue(null))
                                                                                 .OfType<TaskField>()
                                                                                 .ToImmutableArray();

        public static readonly ImmutableArray<TaskField> Default = ImmutableArray.Create<TaskField>(
            Ordinal,
            Name,
            Duration,
            Work,
            Start,
            Finish,
            Predecessors,
            ResourceNames
        );
    }
}
