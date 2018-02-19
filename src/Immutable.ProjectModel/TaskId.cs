﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public readonly struct TaskId : IEquatable<TaskId>
    {
        private readonly Guid _guid;

        private TaskId(Guid guid)
        {
            Debug.Assert(guid != Guid.Empty);

            _guid = guid;
        }

        public bool IsDefault => _guid == Guid.Empty;

        public static TaskId Create()
        {
            return Create(Guid.NewGuid());
        }

        public static TaskId Create(Guid guid)
        {
            return new TaskId(guid);
        }

        public TaskId CreateIfDefault()
        {
            return IsDefault ? Create() : this;
        }

        public override bool Equals(object obj)
        {
            return obj is TaskId other && Equals(other);
        }

        public bool Equals(TaskId other)
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

        public static bool operator ==(TaskId left, TaskId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TaskId left, TaskId right)
        {
            return !(left == right);
        }
    }
}
