﻿using System;

using Xunit;

namespace Immutable.ProjectModel.Tests.Helpers
{
    internal sealed class AssignmentAssert
    {
        private readonly Assignment _assignment;

        public AssignmentAssert(ProjectAssert project, Assignment assignment)
        {
            Project = project;
            _assignment = assignment;
        }

        public ProjectAssert Project { get; }

        public AssignmentAssert AssertField(AssignmentField field, object value)
        {
            var actualValue = _assignment.GetValue(field);
            var equals = Equals(value, actualValue);
            Assert.True(equals, $"Assignment {_assignment.Task.Name}/{_assignment.Resource.Name} {field.Name} should have been '{value}' but was '{actualValue}'");
            return this;
        }

        public AssignmentAssert AssertWork(TimeSpan value)
        {
            return AssertField(AssignmentFields.Work, value);
        }

        public AssignmentAssert AssertStart(DateTimeOffset value)
        {
            return AssertField(AssignmentFields.Start, value);
        }

        public AssignmentAssert AssertFinish(DateTimeOffset value)
        {
            return AssertField(AssignmentFields.Finish, value);
        }

        public AssignmentAssert AssertEarlyStart(DateTimeOffset value)
        {
            return AssertField(AssignmentFields.EarlyStart, value);
        }

        public AssignmentAssert AssertEarlyFinish(DateTimeOffset value)
        {
            return AssertField(AssignmentFields.EarlyFinish, value);
        }

        public AssignmentAssert AssertLateStart(DateTimeOffset value)
        {
            return AssertField(AssignmentFields.LateStart, value);
        }

        public AssignmentAssert AssertLateFinish(DateTimeOffset value)
        {
            return AssertField(AssignmentFields.LateFinish, value);
        }
    }
}
