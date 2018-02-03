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

        private ImmutableDictionary<TaskField, object> Fields { get; }

        private TaskData WithFields(ImmutableDictionary<TaskField, object> fields)
        {
            if (fields == Fields)
                return this;

            return new TaskData(Id, fields);
        }

        public string Name => GetValue(TaskFields.Name);

        public TimeSpan Duration => GetValue(TaskFields.Duration);

        public TimeSpan Work => GetValue(TaskFields.Work);

        public DateTimeOffset EarlyStart => GetValue(TaskFields.EarlyStart);

        public DateTimeOffset EarlyFinish => GetValue(TaskFields.EarlyFinish);

        public DateTimeOffset LateStart => GetValue(TaskFields.LateStart);

        public DateTimeOffset LateFinish => GetValue(TaskFields.LateFinish);

        public ImmutableArray<TaskId> PredecessorIds => GetValue(TaskFields.PredecessorIds);

        public IEnumerable<TaskField> SetFields => TaskFields.All.Where(HasValue);

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

                if (field == TaskFields.IsCritical)
                    return EarlyFinish == LateFinish;
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
            return WithFields(fields);
        }
    }
}
