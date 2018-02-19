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

        public TaskId Id => GetValue(TaskFields.Id);

        public int Ordinal => GetValue(TaskFields.Ordinal);

        public string Name => GetValue(TaskFields.Name);

        public TimeSpan Duration => GetValue(TaskFields.Duration);

        public TimeSpan Work => GetValue(TaskFields.Work);

        public DateTimeOffset Start => GetValue(TaskFields.Start);

        public DateTimeOffset Finish => GetValue(TaskFields.Finish);

        public ConstraintType ConstraintType => GetValue(TaskFields.ConstraintType);

        public DateTimeOffset? ConstraintDate => GetValue(TaskFields.ConstraintDate);

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

        public string Predecessors => GetValue(TaskFields.Predecessors);

        public ImmutableArray<TaskLink> PredecessorLinks => GetValue(TaskFields.PredecessorLinks);

        public IEnumerable<TaskId> PredecessorIds => PredecessorLinks.Select(l => l.PredecessorId);

        public IEnumerable<Task> PredecessorTasks => PredecessorLinks.Select(l => Project.GetTask(l.PredecessorId));

        public ImmutableArray<TaskLink> SuccessorLinks => GetValue(TaskFields.SuccessorLinks);

        public IEnumerable<TaskId> SuccessorIds => SuccessorLinks.Select(l => l.SuccessorId);

        public IEnumerable<Task> SuccessorTasks => SuccessorLinks.Select(l => Project.GetTask(l.SuccessorId));

        public IEnumerable<Assignment> Assignments => Project.Data.GetAssignments(Id)
                                                                  .Select(a => Project.GetAssignment(a));

        public IEnumerable<TaskField> SetFields => _data.SetFields;

        public bool HasValue(TaskField field)
        {
            return _data.HasValue(field);
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
            return _data.GetValue(field);
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

        public Task WithConstraintType(ConstraintType constraintType)
        {
            return SetValue(TaskFields.ConstraintType, constraintType);
        }

        public Task WithConstraintDate(DateTimeOffset? constraintDate)
        {
            return SetValue(TaskFields.ConstraintDate, constraintDate);
        }

        public Task WithIsMilestone(bool isMilestone)
        {
            return SetValue(TaskFields.IsMilestone, isMilestone);
        }

        public Task AddPredecessorLink(TaskId taskId)
        {
            return Project.AddTaskLink(taskId, Id).GetTask(Id);
        }

        public Task RemovePredecessorLink(TaskId taskId)
        {
            return _project.RemoveTaskLink(taskId, Id).GetTask(Id);
        }

        public Task AddSuccessorLink(TaskId taskId)
        {
            return Project.AddTaskLink(Id, taskId).GetTask(Id);
        }

        public Task RemoveSuccessorLink(TaskId taskId)
        {
            return _project.RemoveTaskLink(Id, taskId).GetTask(Id);
        }

        public Task WithResourceNames(string value)
        {
            return SetValue(TaskFields.ResourceNames, value);
        }

        public Task WithPredecessors(string value)
        {
            return SetValue(TaskFields.Predecessors, value);
        }
    }
}
