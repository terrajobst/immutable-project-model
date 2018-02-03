using System;

using Xunit;

namespace Immutable.ProjectModel.Tests
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

        public TaskAssert AssertWork(TimeSpan work)
        {
            return AssertField(TaskFields.Work, work);
        }

        public TaskAssert AssertDuration(TimeSpan duration)
        {
            return AssertField(TaskFields.Duration, duration);
        }

        public TaskAssert AssertEarlyStart(DateTimeOffset earlyStart)
        {
            return AssertField(TaskFields.EarlyStart, earlyStart);
        }

        public TaskAssert AssertEarlyFinish(DateTimeOffset earlyFinish)
        {
            return AssertField(TaskFields.EarlyFinish, earlyFinish);
        }

        public TaskAssert WithLatetart(DateTimeOffset lateStart)
        {
            return AssertField(TaskFields.LateStart, lateStart);
        }

        public TaskAssert WithLateFinish(DateTimeOffset lateFinish)
        {
            return AssertField(TaskFields.LateFinish, lateFinish);
        }
    }
}
