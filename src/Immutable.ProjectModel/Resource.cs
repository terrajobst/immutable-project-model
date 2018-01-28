using System;
using System.Collections.Generic;
using System.Linq;

namespace Immutable.ProjectModel
{
    public sealed class Resource
    {
        internal Resource(ResourceData data, Project project)
        {
            Data = data;
            Project = project;
        }

        internal ResourceData Data { get; }

        public Project Project { get; }

        public ResourceId Id => GetValue(ResourceFields.Id);

        public string Name => GetValue(ResourceFields.Name);

        public IEnumerable<Assignment> Assignments => Project.Data.Assignments.Values
                                                                              .Where(a => a.ResourceId == Id)
                                                                              .Select(a => Project.GetAssignment(a.Id));

        public IEnumerable<ResourceField> SetFields => Data.SetFields;

        public bool HasValue(ResourceField field)
        {
            return Data.HasValue(field);
        }

        public T GetValue<T>(ResourceField<T> field)
        {
            return (T)GetValue((ResourceField)field);
        }

        public Resource SetValue<T>(ResourceField<T> field, T value)
        {
            return SetValue((ResourceField)field, value);
        }

        public object GetValue(ResourceField field)
        {
            return Data.GetValue(field);
        }

        public Resource SetValue(ResourceField field, object value)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (field.IsReadOnly ||
                value == null && !field.Type.IsClass ||
                value != null && !field.Type.IsAssignableFrom(value.GetType()))
                throw new ArgumentException(nameof(value));

            var data = Data.SetValue(field, value);
            return Project.UpdateResource(data);
        }

        public Resource WithName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return SetValue(ResourceFields.Name, name);
        }
    }
}
