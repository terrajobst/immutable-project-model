using System;
using System.Collections.Generic;

namespace Immutable.ProjectModel
{
    public sealed class Assignment
    {
        internal Assignment(AssignmentData data, Project project)
        {
            Project = project;
            Data = data;
        }

        internal AssignmentData Data { get; }

        public Project Project { get; }

        public AssignmentId Id => Data.Id;

        public Task Task => Project.GetTask(Data.TaskId);

        public Resource Resource => Project.GetResource(Data.ResourceId);

        public TimeSpan Work => GetValue(AssignmentFields.Work);

        public IEnumerable<AssignmentField> SetFields => Data.SetFields;

        public bool HasValue(AssignmentField field)
        {
            return Data.HasValue(field);
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
            return Data.GetValue(field);
        }

        public Assignment SetValue(AssignmentField field, object value)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (field.IsReadOnly ||
                value == null && !field.Type.IsClass ||
                value != null && !field.Type.IsAssignableFrom(value.GetType()))
                throw new ArgumentException(nameof(value));

            var data = field.Setter(Project.Data, Data, value);
            return Project.UpdateProject(data).GetAssignment(Id);
        }

        public Assignment WithWork(TimeSpan work)
        {
            return SetValue(AssignmentFields.Work, work);
        }
    }
}
