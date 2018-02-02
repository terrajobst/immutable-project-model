using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Immutable.ProjectModel
{
    public static class TaskFields
    {
        public static readonly TaskField<TaskId> Id = new TaskField<TaskId>("Task.Id", FieldKinds.TaskId, FieldFlags.ReadOnly | FieldFlags.Virtual);
        public static readonly TaskField<int> Ordinal = new TaskField<int>("Task.Ordinal", FieldKinds.Int32, FieldFlags.ReadOnly);
        public static readonly TaskField<string> Name = new TaskField<string>("Task.Name", FieldKinds.Text);
        public static readonly TaskField<TimeSpan> Duration = new TaskField<TimeSpan>("Task.Duration", FieldKinds.Duration, setter: Scheduler.SetTaskDuration);
        public static readonly TaskField<TimeSpan> Work = new TaskField<TimeSpan>("Task.Work", FieldKinds.Work, setter: Scheduler.SetTaskWork);
        public static readonly TaskField<DateTimeOffset> EarlyStart = new TaskField<DateTimeOffset>("Task.EarlyStart", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<DateTimeOffset> EarlyFinish = new TaskField<DateTimeOffset>("Task.EarlyFinish", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<DateTimeOffset> LateStart = new TaskField<DateTimeOffset>("Task.LateStart", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<DateTimeOffset> LateFinish = new TaskField<DateTimeOffset>("Task.LateFinish", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<ImmutableArray<TaskId>> PredecessorIds = new TaskField<ImmutableArray<TaskId>>("Task.PredecessorIds", FieldKinds.TaskIdArray, FieldFlags.ImpactsScheduling, ImmutableArray<TaskId>.Empty);
        public static readonly TaskField<bool> IsOnCriticalPath = new TaskField<bool>("Task.IsOnCriticalPath", FieldKinds.Boolean, FieldFlags.ReadOnly | FieldFlags.Virtual);

        public static readonly ImmutableArray<TaskField> All = typeof(TaskFields).GetFields(BindingFlags.Static | BindingFlags.Public)
                                                                                 .Select(f => f.GetValue(null))
                                                                                 .OfType<TaskField>()
                                                                                 .ToImmutableArray();
    }
}
