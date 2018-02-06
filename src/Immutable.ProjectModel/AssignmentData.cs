using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal sealed class AssignmentData
    {
        public static AssignmentData Create(AssignmentId id, TaskId taskId, ResourceId resourceId)
        {
            Debug.Assert(!id.IsDefault);
            Debug.Assert(!taskId.IsDefault);
            Debug.Assert(!resourceId.IsDefault);

            return new AssignmentData(id, taskId, resourceId, ImmutableDictionary<AssignmentField, object>.Empty);
        }

        private AssignmentData(AssignmentId id, TaskId taskId, ResourceId resourceId, ImmutableDictionary<AssignmentField, object> fields)
        {
            Id = id;
            TaskId = taskId;
            ResourceId = resourceId;
            Fields = fields;
        }

        public AssignmentId Id { get; }

        public TaskId TaskId { get; }

        public ResourceId ResourceId { get; }

        public TimeSpan Work => GetValue(AssignmentFields.Work);

        public double Units => GetValue(AssignmentFields.Units);

        public DateTimeOffset Start => GetValue(AssignmentFields.Start);

        public DateTimeOffset Finish => GetValue(AssignmentFields.Finish);

        private ImmutableDictionary<AssignmentField, object> Fields { get; }

        private AssignmentData WithFields(ImmutableDictionary<AssignmentField, object> fields)
        {
            if (fields == Fields)
                return this;

            return new AssignmentData(Id, TaskId, ResourceId, fields);
        }

        public IEnumerable<AssignmentField> SetFields => AssignmentFields.All.Where(HasValue);

        public bool HasValue(AssignmentField field)
        {
            return field.IsVirtual || Fields.ContainsKey(field);
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

            if (field.IsVirtual)
            {
                if (field == AssignmentFields.Id)
                    return Id;

                if (field == AssignmentFields.TaskId)
                    return TaskId;

                if (field == AssignmentFields.ResourceId)
                    return ResourceId;
            }

            if (!Fields.TryGetValue(field, out var result))
                return field.DefaultValue;

            return result;
        }

        public AssignmentData SetValue(AssignmentField field, object value)
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
