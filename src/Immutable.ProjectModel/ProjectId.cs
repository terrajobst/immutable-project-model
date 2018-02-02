using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public struct ProjectId : IEquatable<ProjectId>
    {
        private readonly Guid _guid;

        private ProjectId(Guid guid)
        {
            Debug.Assert(guid != Guid.Empty);

            _guid = guid;
        }

        public bool IsDefault => _guid == Guid.Empty;

        public static ProjectId Create()
        {
            return Create(Guid.NewGuid());
        }

        public static ProjectId Create(Guid guid)
        {
            return new ProjectId(guid);
        }

        public ProjectId CreateIfDefault()
        {
            return IsDefault ? Create() : this;
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectId other && Equals(other);
        }

        public bool Equals(ProjectId other)
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

        public static bool operator ==(ProjectId left, ProjectId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ProjectId left, ProjectId right)
        {
            return !(left == right);
        }
    }

}
