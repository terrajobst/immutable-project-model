using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public struct AssignmentId : IEquatable<AssignmentId>
    {
        private readonly Guid _guid;

        private AssignmentId(Guid guid)
        {
            Debug.Assert(guid != Guid.Empty);

            _guid = guid;
        }

        public bool IsDefault => _guid == Guid.Empty;

        public static AssignmentId Create()
        {
            return Create(Guid.NewGuid());
        }

        public static AssignmentId Create(Guid guid)
        {
            return new AssignmentId(guid);
        }

        public AssignmentId CreateIfDefault()
        {
            return IsDefault ? Create() : this;
        }

        public override bool Equals(object obj)
        {
            return obj is AssignmentId other && Equals(other);
        }

        public bool Equals(AssignmentId other)
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

        public static bool operator ==(AssignmentId left, AssignmentId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssignmentId left, AssignmentId right)
        {
            return !(left == right);
        }
    }
}
