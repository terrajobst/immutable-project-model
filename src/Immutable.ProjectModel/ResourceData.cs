using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal readonly struct ResourceData : IEquatable<ResourceData>
    {
        private readonly ImmutableDictionary<ResourceField, object> _fields;

        public static ResourceData Create(ResourceId id)
        {
            Debug.Assert(!id.IsDefault);

            var fields = ImmutableDictionary.Create<ResourceField, object>()
                                            .Add(ResourceFields.Id, id);

            return new ResourceData(fields);
        }

        public bool IsDefault => _fields == null;

        private ResourceData(ImmutableDictionary<ResourceField, object> fields)
        {
            _fields = fields;
        }

        public ResourceId Id => GetValue(ResourceFields.Id);

        public string Name => GetValue(ResourceFields.Name);

        public IEnumerable<ResourceField> SetFields => ResourceFields.All.Where(HasValue);

        public bool HasValue(ResourceField field)
        {
            return _fields.ContainsKey(field);
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

            if (!_fields.TryGetValue(field, out var result))
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

            var fields = _fields.SetItem(field, value);
            return new ResourceData(fields);
        }

        public override bool Equals(object obj)
        {
            return obj is ResourceData other && Equals(other);
        }

        public bool Equals(ResourceData other)
        {
            return ReferenceEquals(_fields, other._fields);
        }

        public override int GetHashCode()
        {
            return _fields == null ? 0 : _fields.GetHashCode();
        }

        public static bool operator ==(ResourceData left, ResourceData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ResourceData left, ResourceData right)
        {
            return !left.Equals(right);
        }
    }
}
