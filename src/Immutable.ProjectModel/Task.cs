using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Immutable.ProjectModel
{
    public sealed class Task
    {
        private readonly Project _project;
        private readonly TaskData _data;

        internal Task(Project project, TaskData data)
        {
            _project = project;
            _data = data;
        }

        public Project Project => _project;

        internal TaskData Data => _data;

        public TaskId Id => GetValue(TaskFields.Id);

        public int Ordinal => GetValue(TaskFields.Ordinal);

        public string Name => GetValue(TaskFields.Name);

        public TimeSpan Duration => GetValue(TaskFields.Duration);

        public TimeSpan Work => GetValue(TaskFields.Work);

        public DateTimeOffset Start => GetValue(TaskFields.Start);

        public DateTimeOffset Finish => GetValue(TaskFields.Finish);

        public DateTimeOffset EarlyStart => GetValue(TaskFields.EarlyStart);

        public DateTimeOffset EarlyFinish => GetValue(TaskFields.EarlyFinish);

        public DateTimeOffset LateStart => GetValue(TaskFields.LateStart);

        public DateTimeOffset LateFinish => GetValue(TaskFields.LateFinish);

        public TimeSpan StartSlack => GetValue(TaskFields.StartSlack);

        public TimeSpan FinishSlack => GetValue(TaskFields.FinishSlack);

        public TimeSpan TotalSlack => GetValue(TaskFields.TotalSlack);

        public TimeSpan FreeSlack => GetValue(TaskFields.FreeSlack);

        public bool IsCritical => GetValue(TaskFields.IsCritical);

        public bool IsMilestone => GetValue(TaskFields.IsMilestone);

        public string ResourceNames => GetValue(TaskFields.ResourceNames);

        public ImmutableArray<TaskId> PredecessorIds => GetValue(TaskFields.PredecessorIds);

        public IEnumerable<Task> PredecessorTasks => PredecessorIds.Select(id => Project.GetTask(id));

        public IEnumerable<Assignment> Assignments => Project.Data.Assignments.Values
                                                                              .Where(a => a.TaskId == Id)
                                                                              .Select(a => Project.GetAssignment(a.Id));

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

            return Project.SetTaskField(this, field, value);
        }

        public Task WithOrdinal(int ordinal)
        {
            return SetValue(TaskFields.Ordinal, ordinal);
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

        public Task WithIsMilestone(bool isMilestone)
        {
            return SetValue(TaskFields.IsMilestone, isMilestone);
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

        public Task WithResourceNames(string value)
        {
            return SetValue(TaskFields.ResourceNames, value);
        }
    }
}
