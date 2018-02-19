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

            var fields = ImmutableDictionary.Create<AssignmentField, object>()
                                            .Add(AssignmentFields.Id, id)
                                            .Add(AssignmentFields.TaskId, taskId)
                                            .Add(AssignmentFields.ResourceId, resourceId);

            return new AssignmentData(fields);
        }

        private AssignmentData(ImmutableDictionary<AssignmentField, object> fields)
        {
            Fields = fields;
        }

        public AssignmentId Id => GetValue(AssignmentFields.Id);

        public TaskId TaskId => GetValue(AssignmentFields.TaskId);

        public ResourceId ResourceId => GetValue(AssignmentFields.ResourceId);

        public TimeSpan Work => GetValue(AssignmentFields.Work);

        public double Units => GetValue(AssignmentFields.Units);

        public DateTimeOffset Start => GetValue(AssignmentFields.Start);

        public DateTimeOffset Finish => GetValue(AssignmentFields.Finish);

        public string TaskName => GetValue(AssignmentFields.TaskName);

        public string ResourceName => GetValue(AssignmentFields.ResourceName);

        private ImmutableDictionary<AssignmentField, object> Fields { get; }

        private AssignmentData WithFields(ImmutableDictionary<AssignmentField, object> fields)
        {
            if (fields == Fields)
                return this;

            return new AssignmentData(fields);
        }

        public IEnumerable<AssignmentField> SetFields => AssignmentFields.All.Where(HasValue);

        public bool HasValue(AssignmentField field)
        {
            return Fields.ContainsKey(field);
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
