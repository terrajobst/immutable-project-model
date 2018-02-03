using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal sealed class ResourceData
    {
        public static ResourceData Create(ResourceId id)
        {
            Debug.Assert(!id.IsDefault);

            return new ResourceData(id, ImmutableDictionary<ResourceField, object>.Empty);
        }

        private ResourceData(ResourceId id, ImmutableDictionary<ResourceField, object> fields)
        {
            Id = id;
            Fields = fields;
        }

        public ResourceId Id { get; }

        public IEnumerable<ResourceField> SetFields => ResourceFields.All.Where(HasValue);

        private ImmutableDictionary<ResourceField, object> Fields { get; }

        private ResourceData WithFields(ImmutableDictionary<ResourceField, object> fields)
        {
            if (fields == Fields)
                return this;

            return new ResourceData(Id, fields);
        }

        public bool HasValue(ResourceField field)
        {
            return field.IsVirtual || Fields.ContainsKey(field);
        }

        public T GetValue<T>(ResourceField<T> field)
        {
            return (T)GetValue((ResourceField)field);
        }

        public ResourceData SetValue<T>(ResourceField<T> field, T value)
        {
            return SetValue((ResourceField)field, value);
        }

        public object GetValue(ResourceField field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (field.IsVirtual)
            {
                if (field == ResourceFields.Id)
                    return Id;
            }

            if (!Fields.TryGetValue(field, out var result))
                return field.DefaultValue;

            return result;
        }

        public ResourceData SetValue(ResourceField field, object value)
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
