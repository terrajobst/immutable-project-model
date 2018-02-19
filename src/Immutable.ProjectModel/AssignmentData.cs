using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal readonly struct AssignmentData : IEquatable<AssignmentData>
    {
        private readonly ImmutableDictionary<AssignmentField, object> _fields;

        public static AssignmentData Create(AssignmentId id, TaskId taskId, ResourceId resourceId)
        {
            Debug.Assert(!id.IsDefault);
            Debug.Assert(!taskId.IsDefault);
            Debug.Assert(!resourceId.IsDefault);

            var fields = ImmutableDictionary.Create<AssignmentField, object>()
                                            .Add(AssignmentFields.Id, id)
                                            .Add(AssignmentFields.TaskId, taskId)
                                            .Add(AssignmentFields.ResourceId, resourceId);

            return new AssignmentData(fields);
        }

        private AssignmentData(ImmutableDictionary<AssignmentField, object> fields)
        {
            _fields = fields;
        }

        public bool IsDefault => _fields == null;

        public AssignmentId Id => GetValue(AssignmentFields.Id);

        public IEnumerable<AssignmentField> SetFields => AssignmentFields.All.Where(HasValue);

        public bool HasValue(AssignmentField field)
        {
            return _fields.ContainsKey(field);
        }

        public T GetValue<T>(AssignmentField<T> field)
        {
            return (T)GetValue((AssignmentField)field);
        }

        public AssignmentData SetValue<T>(AssignmentField<T> field, T value)
        {
            return SetValue((AssignmentField)field, value);
        }

        public object GetValue(AssignmentField field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (!_fields.TryGetValue(field, out var result))
                return field.DefaultValue;

            return result;
        }

        public AssignmentData SetValue(AssignmentField field, object value)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (!field.IsAssignableFrom(value))
                throw new ArgumentException(nameof(value));

            var existingValue = GetValue(field);
            if (Equals(value, existingValue))
                return this;

            var fields = _fields.SetItem(field, value);
            return new AssignmentData(fields);
        }

        public override bool Equals(object obj)
        {
            return obj is AssignmentData other && Equals(other);
        }

        public bool Equals(AssignmentData other)
        {
            return ReferenceEquals(_fields, other._fields);
        }

        public override int GetHashCode()
        {
            return _fields == null ? 0 : _fields.GetHashCode();
        }

        public static bool operator ==(AssignmentData left, AssignmentData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssignmentData left, AssignmentData right)
        {
            return !left.Equals(right);
        }
    }
}
