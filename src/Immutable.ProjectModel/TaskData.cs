using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal readonly struct TaskData : IEquatable<TaskData>
    {
        private readonly ImmutableDictionary<TaskField, object> _fields;

        public static TaskData Create(TaskId id)
        {
            Debug.Assert(!id.IsDefault);

            var fields = ImmutableDictionary.Create<TaskField, object>()
                                            .Add(TaskFields.Id, id);

            return new TaskData(fields);
        }

        private TaskData(ImmutableDictionary<TaskField, object> fields)
        {
            _fields = fields;
        }

        public bool IsDefault => _fields == null;

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

        public ImmutableArray<TaskId> PredecessorIds => GetValue(TaskFields.PredecessorIds);

        public IEnumerable<TaskField> SetFields => TaskFields.All.Where(HasValue);

        public bool HasValue(TaskField field)
        {
            return _fields.ContainsKey(field);
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

            if (!_fields.TryGetValue(field, out var result))
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

            var fields = _fields.SetItem(field, value);

            if (field == TaskFields.Duration)
            {
                var oldValue = (TimeSpan)existingValue;
                var newValue = (TimeSpan)value;

                if (oldValue == TimeSpan.Zero && newValue != TimeSpan.Zero)
                    fields = fields.SetItem(TaskFields.IsMilestone, false);
                else if (oldValue != TimeSpan.Zero && newValue == TimeSpan.Zero)
                    fields = fields.SetItem(TaskFields.IsMilestone, true);
            }

            return new TaskData(fields);
        }

        public override bool Equals(object obj)
        {
            return obj is TaskData other && Equals(other);
        }

        public bool Equals(TaskData other)
        {
            return ReferenceEquals(_fields, other._fields);
        }

        public override int GetHashCode()
        {
            return _fields == null ? 0 : _fields.GetHashCode();
        }

        public static bool operator ==(TaskData left, TaskData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TaskData left, TaskData right)
        {
            return !left.Equals(right);
        }
    }
}
