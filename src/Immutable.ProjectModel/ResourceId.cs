using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public struct ResourceId : IEquatable<ResourceId>
    {
        private readonly Guid _guid;

        private ResourceId(Guid guid)
        {
            Debug.Assert(guid != Guid.Empty);

            _guid = guid;
        }

        public bool IsDefault => _guid == Guid.Empty;

        public static ResourceId Create()
        {
            return Create(Guid.NewGuid());
        }

        public static ResourceId Create(Guid guid)
        {
            return new ResourceId(guid);
        }

        public ResourceId CreateIfDefault()
        {
            return IsDefault ? Create() : this;
        }

        public override bool Equals(object obj)
        {
            return obj is ResourceId other && Equals(other);
        }

        public bool Equals(ResourceId other)
        {
            return _guid.Equals(other._guid);
        }

        public override int GetHashCode()
        {
            var hashCode = 1937295223;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(_guid);
            return hashCode;
        }

        public override string ToString()
        {
            return _guid.ToString();
        }

        public static bool operator ==(ResourceId left, ResourceId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ResourceId left, ResourceId right)
        {
            return !(left == right);
        }
    }
}
