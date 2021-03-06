﻿using System;
using System.Collections.Generic;

namespace Immutable.ProjectModel
{
    public sealed class Assignment
    {
        private readonly Project _project;
        private readonly AssignmentData _data;

        internal Assignment(Project project, AssignmentData data)
        {
            _project = project;
            _data = data;
        }

        public Project Project => _project;

        public AssignmentId Id => GetValue(AssignmentFields.Id);

        public TaskId TaskId => GetValue(AssignmentFields.TaskId);

        public string TaskName => GetValue(AssignmentFields.TaskName);

        public Task Task => Project.GetTask(TaskId);

        public ResourceId ResourceId => GetValue(AssignmentFields.ResourceId);

        public string ResourceName => GetValue(AssignmentFields.ResourceName);

        public Resource Resource => Project.GetResource(ResourceId);

        public TimeSpan Work => GetValue(AssignmentFields.Work);

        public double Units => GetValue(AssignmentFields.Units);

        public DateTimeOffset Start => GetValue(AssignmentFields.Start);

        public DateTimeOffset Finish => GetValue(AssignmentFields.Finish);

        public IEnumerable<AssignmentField> SetFields => _data.SetFields;

        public bool HasValue(AssignmentField field)
        {
            return _data.HasValue(field);
        }

        public T GetValue<T>(AssignmentField<T> field)
        {
            return (T)GetValue((AssignmentField)field);
        }

        public Assignment SetValue<T>(AssignmentField<T> field, T value)
        {
            return SetValue((AssignmentField)field, value);
        }

        public object GetValue(AssignmentField field)
        {
            return _data.GetValue(field);
        }

        public Assignment SetValue(AssignmentField field, object value)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (field.IsReadOnly ||
                value == null && !field.Type.IsClass ||
                value != null && !field.Type.IsAssignableFrom(value.GetType()))
                throw new ArgumentException(nameof(value));

            return Project.SetAssignmentField(this, field, value);
        }

        public Assignment WithWork(TimeSpan work)
        {
            return SetValue(AssignmentFields.Work, work);
        }

        public Assignment WithUnits(double units)
        {
            return SetValue(AssignmentFields.Units, units);
        }
    }
}
