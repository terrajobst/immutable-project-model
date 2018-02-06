﻿using System;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Immutable.ProjectModel.Tests.Helpers
{
    internal sealed class TaskAssert
    {
        private readonly Task _task;

        public TaskAssert(ProjectAssert project, Task task)
        {
            Project = project;
            _task = task;
        }

        public ProjectAssert Project { get; }

        public TaskAssert AssertField(TaskField field, object value)
        {
            var actualValue = _task.GetValue(field);
            var equals = Equals(value, actualValue);
            Assert.True(equals, $"Task {_task.Ordinal + 1} {field.Name} should have been '{value}' but was '{actualValue}'");
            return this;
        }

        public TaskAssert AssertField<T>(TaskField<T> field, T value, Func<T, T, bool> checker)
        {
            var actualValue = _task.GetValue(field);
            var equals = checker(value, actualValue);
            Assert.True(equals, $"Task {_task.Ordinal + 1} {field.Name} should have been '{value}' but was '{actualValue}'");
            return this;
        }

        public TaskAssert AssertOrdinal(int value)
        {
            return AssertField(TaskFields.Ordinal, value);
        }

        public TaskAssert AssertWork(TimeSpan value)
        {
            return AssertField(TaskFields.Work, value);
        }

        public TaskAssert AssertDuration(TimeSpan value)
        {
            return AssertField(TaskFields.Duration, value);
        }

        public TaskAssert AssertStart(DateTimeOffset value)
        {
            return AssertField(TaskFields.Start, value);
        }

        public TaskAssert AssertFinish(DateTimeOffset value)
        {
            return AssertField(TaskFields.Finish, value);
        }

        public TaskAssert AssertEarlyStart(DateTimeOffset value)
        {
            return AssertField(TaskFields.EarlyStart, value);
        }

        public TaskAssert AssertEarlyFinish(DateTimeOffset value)
        {
            return AssertField(TaskFields.EarlyFinish, value);
        }

        public TaskAssert AssertLateStart(DateTimeOffset value)
        {
            return AssertField(TaskFields.LateStart, value);
        }

        public TaskAssert AssertLateFinish(DateTimeOffset value)
        {
            return AssertField(TaskFields.LateFinish, value);
        }

        public TaskAssert AssertStartSlack(TimeSpan value)
        {
            return AssertField(TaskFields.StartSlack, value);
        }

        public TaskAssert AssertFinishSlack(TimeSpan value)
        {
            return AssertField(TaskFields.FinishSlack, value);
        }

        public TaskAssert AssertTotalSlack(TimeSpan value)
        {
            return AssertField(TaskFields.TotalSlack, value);
        }

        public TaskAssert AssertFreeSlack(TimeSpan value)
        {
            return AssertField(TaskFields.FreeSlack, value);
        }

        public TaskAssert AssertIsCritical(bool value)
        {
            return AssertField(TaskFields.IsCritical, value);
        }

        public TaskAssert AssertPredecessorIds(ImmutableArray<TaskId> value)
        {
            return AssertField(TaskFields.PredecessorIds, value, (v, av) => v.SequenceEqual(av));
        }
    }
}
