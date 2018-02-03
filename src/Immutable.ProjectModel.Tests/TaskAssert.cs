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
    }
}
