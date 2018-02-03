using System;

using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class TaskTests
    {
        [Fact]
        public void Task_WithoutAssignments_IsScheduledProperly()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddNewTask(taskId1)
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddNewTask(taskId2)
                                    .WithDuration(TimeSpan.FromDays(5))
                                    .AddPredecessorId(taskId1).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(10))
                              .AssertEarlyStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertEarlyFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertEarlyStart(new DateTime(2018, 2, 12, 8, 0, 0))
                              .AssertEarlyFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero);
        }

        [Fact]
        public void Task_WithoutAssignments_SetWork_DoesNothing()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddNewTask(taskId1)
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddNewTask(taskId2)
                                    .WithDuration(TimeSpan.FromDays(5))
                                    .AddPredecessorId(taskId1).Project
                                 .GetTask(taskId2)
                                    .WithWork(TimeSpan.FromHours(120)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(10))
                              .AssertEarlyStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertEarlyFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertEarlyStart(new DateTime(2018, 2, 12, 8, 0, 0))
                              .AssertEarlyFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .AssertWork(TimeSpan.FromHours(120));
        }

        [Fact]
        public void Task_WithoutAssignments_ClearingWork_DoesNothing()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddNewTask(taskId1)
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddNewTask(taskId2)
                                    .WithDuration(TimeSpan.FromDays(5))
                                    .AddPredecessorId(taskId1).Project
                                 .GetTask(taskId2)
                                    .WithWork(TimeSpan.Zero).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(10))
                              .AssertEarlyStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertEarlyFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertEarlyStart(new DateTime(2018, 2, 12, 8, 0, 0))
                              .AssertEarlyFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero);
        }
    }
}
