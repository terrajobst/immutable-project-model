﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Immutable.ProjectModel
{
    public static class TaskFields
    {
        public static readonly TaskField<TaskId> Id = new TaskField<TaskId>("Id", FieldKinds.TaskId, FieldFlags.ReadOnly | FieldFlags.Virtual);
        public static readonly TaskField<int> Ordinal = new TaskField<int>("Ordinal", FieldKinds.Int32, FieldFlags.ReadOnly);
        public static readonly TaskField<string> Name = new TaskField<string>("Name", FieldKinds.Text);
        public static readonly TaskField<TimeSpan> Duration = new TaskField<TimeSpan>("Duration", FieldKinds.Duration, setter: Scheduler.SetTaskDuration);
        public static readonly TaskField<TimeSpan> Work = new TaskField<TimeSpan>("Work", FieldKinds.Work, setter: Scheduler.SetTaskWork);
        public static readonly TaskField<DateTimeOffset> EarlyStart = new TaskField<DateTimeOffset>("EarlyStart", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<DateTimeOffset> EarlyFinish = new TaskField<DateTimeOffset>("EarlyFinish", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<DateTimeOffset> LateStart = new TaskField<DateTimeOffset>("LateStart", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<DateTimeOffset> LateFinish = new TaskField<DateTimeOffset>("LateFinish", FieldKinds.DateTime, FieldFlags.ReadOnly);
        public static readonly TaskField<ImmutableArray<TaskId>> PredecessorIds = new TaskField<ImmutableArray<TaskId>>("PredecessorIds", FieldKinds.TaskIdArray, FieldFlags.ImpactsScheduling, ImmutableArray<TaskId>.Empty);
        public static readonly TaskField<bool> IsOnCriticalPath = new TaskField<bool>("IsOnCriticalPath", FieldKinds.YesNo, FieldFlags.ReadOnly | FieldFlags.Virtual);

        public static readonly ImmutableArray<TaskField> All = typeof(TaskFields).GetFields(BindingFlags.Static | BindingFlags.Public)
                                                                                 .Select(f => f.GetValue(null))
                                                                                 .OfType<TaskField>()
                                                                                 .ToImmutableArray();
    }
}
