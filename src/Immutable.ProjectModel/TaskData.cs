using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal sealed class TaskData
    {
        public static TaskData Create(TaskId id)
        {
            Debug.Assert(!id.IsDefault);

            return new TaskData(id, ImmutableDictionary<TaskField, object>.Empty);
        }

        private TaskData(TaskId id, ImmutableDictionary<TaskField, object> fields)
        {
            Id = id;
            Fields = fields;
        }

        public TaskId Id { get; }

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

        public ImmutableArray<TaskId> PredecessorIds => GetValue(TaskFields.PredecessorIds);

        public IEnumerable<TaskField> SetFields => TaskFields.All.Where(HasValue);

        private ImmutableDictionary<TaskField, object> Fields { get; }

        private TaskData WithFields(ImmutableDictionary<TaskField, object> fields)
        {
            if (fields == Fields)
                return this;

            return new TaskData(Id, fields);
        }

        public bool HasValue(TaskField field)
        {
            return field.IsVirtual || Fields.ContainsKey(field);
        }

        public T GetValue<T>(TaskField<T> field)
        {
            return (T)GetValue((TaskField)field);
        }

        public TaskData SetValue<T>(TaskField<T> field, T value)
        {
            return SetValue((TaskField)field, value);
        }

        public object GetValue(TaskField field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (field.IsVirtual)
            {
                if (field == TaskFields.Id)
                    return Id;
            }

            if (!Fields.TryGetValue(field, out var result))
                return field.DefaultValue;

            return result;
        }

        public TaskData SetValue(TaskField field, object value)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (value == null && !field.Type.IsClass ||
                value != null && !field.Type.IsAssignableFrom(value.GetType()))
                throw new ArgumentException(nameof(value));

            var existingValue = GetValue(field);
            if (Equals(value, existingValue))
                return this;

            var fields = Fields.SetItem(field, value);

            if (field == TaskFields.Duration)
            {
                var oldValue = (TimeSpan)existingValue;
                var newValue = (TimeSpan)value;

                if (oldValue == TimeSpan.Zero && newValue != TimeSpan.Zero)
                    fields = fields.SetItem(TaskFields.IsMilestone, false);
                else if (oldValue != TimeSpan.Zero && newValue == TimeSpan.Zero)
                    fields = fields.SetItem(TaskFields.IsMilestone, true);
            }

            return WithFields(fields);
        }
    }
}
