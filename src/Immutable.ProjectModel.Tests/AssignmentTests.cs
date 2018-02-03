using System;

using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class AssignmentTests
    {
        [Fact]
        public void Assignment_GetsWorkFromTaskDuration()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddNewTask(taskId)
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddNewResource(resourceId).Project
                                 .AddNewAssignment(taskId, resourceId).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(10))
                              .AssertWork(TimeSpan.FromHours(80))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId)
                              .AssertWork(TimeSpan.FromHours(80))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0));
        }

        [Fact]
        public void Assignment_GetsWorkFromTaskWork()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddNewTask(taskId)
                                    .WithDuration(TimeSpan.FromDays(10))
                                    .WithWork(TimeSpan.FromHours(40)).Project
                                 .AddNewResource(resourceId).Project
                                 .AddNewAssignment(taskId, resourceId).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertWork(TimeSpan.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId)
                              .AssertWork(TimeSpan.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0));
        }

        [Fact]
        public void Assignment_AddingAnotherAssignment_IncreasesWork()
        {
            var taskId = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddNewTask(taskId)
                                    .WithDuration(TimeSpan.FromDays(5)).Project
                                 .AddNewResource(resourceId1).Project
                                 .AddNewResource(resourceId2).Project
                                 .AddNewAssignment(taskId, resourceId1).Project
                                 .AddNewAssignment(taskId, resourceId2).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertWork(TimeSpan.FromHours(80))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(TimeSpan.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId2)
                              .AssertWork(TimeSpan.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0));
        }
    }
}
