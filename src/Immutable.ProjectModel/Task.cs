﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Immutable.ProjectModel
{
    public sealed class Task
    {
        internal Task(TaskData data, Project project)
        {
            Project = project;
            Data = data;
        }

        internal TaskData Data { get; }

        public Project Project { get; }

        public TaskId Id => GetValue(TaskFields.Id);

        public int Ordinal => GetValue(TaskFields.Ordinal);

        public string Name => GetValue(TaskFields.Name);

        public TimeSpan Duration => GetValue(TaskFields.Duration);

        public TimeSpan Work => GetValue(TaskFields.Work);

        public DateTimeOffset EarlyStart => GetValue(TaskFields.EarlyStart);

        public DateTimeOffset EarlyFinish => GetValue(TaskFields.EarlyFinish);

        public DateTimeOffset LateStart => GetValue(TaskFields.LateStart);

        public DateTimeOffset LateFinish => GetValue(TaskFields.LateFinish);

        public ImmutableArray<TaskId> PredecessorIds => GetValue(TaskFields.PredecessorIds);

        public IEnumerable<Task> Predecessors => PredecessorIds.Select(id => Project.GetTask(id));

        public IEnumerable<Assignment> Assignments => Project.Data.Assignments.Values
                                                                              .Where(a => a.TaskId == Id)
                                                                              .Select(a => Project.GetAssignment(a.Id));

        public bool IsCritical => GetValue(TaskFields.IsCritical);

        public IEnumerable<TaskField> SetFields => Data.SetFields;

        public bool HasValue(TaskField field)
        {
            return Data.HasValue(field);
        }

        public T GetValue<T>(TaskField<T> field)
        {
            return (T)GetValue((TaskField)field);
        }

        public Task SetValue<T>(TaskField<T> field, T value)
        {
            return SetValue((TaskField)field, value);
        }

        public object GetValue(TaskField field)
        {
            return Data.GetValue(field);
        }

        public Task SetValue(TaskField field, object value)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (field.IsReadOnly ||
                value == null && !field.Type.IsClass ||
                value != null && !field.Type.IsAssignableFrom(value.GetType()))
                throw new ArgumentException(nameof(value));

            var data = field.Setter(Project.Data, Data, value);
            return Project.UpdateProject(data).GetTask(Id);
        }

        public Task WithName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return SetValue(TaskFields.Name, name);
        }

        public Task WithDuration(TimeSpan duration)
        {
            return SetValue(TaskFields.Duration, duration);
        }

        public Task WithWork(TimeSpan work)
        {
            return SetValue(TaskFields.Work, work);
        }

        public Task WithPredecessorIds(ImmutableArray<TaskId> predecessorIds)
        {
            if (predecessorIds == PredecessorIds)
                return this;

            return SetValue(TaskFields.PredecessorIds, predecessorIds);
        }

        public Task AddPredecessorId(TaskId taskId)
        {
            if (PredecessorIds.Contains(taskId))
                return this;

            return WithPredecessorIds(PredecessorIds.Add(taskId));
        }

        public Task RemovePredecessorId(TaskId taskId)
        {
            return WithPredecessorIds(PredecessorIds.Remove(taskId));
        }
    }
}
