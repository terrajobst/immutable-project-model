using System;

using Immutable.ProjectModel.Tests.Helpers;

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

        [Fact]
        public void Assignment_RemovingAnotherAssignment_DecreasesWork()
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
                                 .AddNewAssignment(taskId, resourceId2).Project
                                 .RemoveAssignment(taskId, resourceId2);

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertWork(TimeSpan.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(TimeSpan.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0));
        }

        [Fact]
        public void Assignment_RemovingAllAssignments_SetsTasksWorkToZero()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddNewTask(taskId)
                                    .WithDuration(TimeSpan.FromDays(5)).Project
                                 .AddNewResource(resourceId).Project
                                 .RemoveAssignment(taskId, resourceId);

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertWork(TimeSpan.Zero)
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0));
        }

        [Fact]
        public void Assignment_SettingTaskWork_IncreasesProportionally()
        {
            var taskId = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 5))
                                 .AddNewTask(taskId).Project
                                 .AddNewResource(resourceId1).Project
                                 .AddNewResource(resourceId2).Project
                                 .AddNewAssignment(taskId, resourceId1)
                                    .WithWork(TimeSpan.FromHours(40)).Project
                                 .AddNewAssignment(taskId, resourceId2)
                                    .WithWork(TimeSpan.FromHours(80)).Project
                                 .GetTask(taskId)
                                    .WithWork(TimeSpan.FromHours(150)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(12.5))
                              .AssertWork(TimeSpan.FromHours(150))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 21, 12, 0, 0)).Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(TimeSpan.FromHours(50))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 13, 10, 0, 0)).Project
                         .ForAssignment(taskId, resourceId2)
                              .AssertWork(TimeSpan.FromHours(100))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 21, 12, 0, 0));
        }

        [Fact]
        public void Assignment_SettingTaskDuration_IncreasesLongest()
        {
            var taskId = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 5))
                                 .AddNewTask(taskId).Project
                                 .AddNewResource(resourceId1).Project
                                 .AddNewResource(resourceId2).Project
                                 .AddNewAssignment(taskId, resourceId1)
                                    .WithWork(TimeSpan.FromHours(40)).Project
                                 .AddNewAssignment(taskId, resourceId2)
                                    .WithWork(TimeSpan.FromHours(80)).Project
                                 .GetTask(taskId)
                                    .WithDuration(TimeSpan.FromDays(12)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(12))
                              .AssertWork(TimeSpan.FromHours(136))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 20, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(TimeSpan.FromHours(40))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId2)
                              .AssertWork(TimeSpan.FromHours(96))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 20, 17, 0, 0));
        }

        [Fact]
        public void Assignment_HonorsUnits()
        {
            var taskId = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 5))
                                 .AddNewTask(taskId).Project
                                 .AddNewResource(resourceId1).Project
                                 .AddNewResource(resourceId2).Project
                                 .AddNewAssignment(taskId, resourceId1)
                                    .WithWork(TimeSpan.FromHours(40))
                                    .WithUnits(.5).Project
                                 .AddNewAssignment(taskId, resourceId2)
                                    .WithWork(TimeSpan.FromHours(20)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(10))
                              .AssertWork(TimeSpan.FromHours(60))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(TimeSpan.FromHours(40))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .Project
                         .ForAssignment(taskId, resourceId2)
                              .AssertWork(TimeSpan.FromHours(20))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 7, 12, 0, 0));
        }

        [Fact]
        public void Assignment_SettingTaskDuration_HonorsUnits()
        {
            var taskId = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 5))
                                 .AddNewTask(taskId).Project
                                 .AddNewResource(resourceId1).Project
                                 .AddNewResource(resourceId2).Project
                                 .AddNewAssignment(taskId, resourceId1)
                                    .WithWork(TimeSpan.FromHours(40))
                                    .WithUnits(.5).Project
                                 .AddNewAssignment(taskId, resourceId2)
                                    .WithWork(TimeSpan.FromHours(80)).Project
                                 .GetTask(taskId)
                                    .WithDuration(TimeSpan.FromDays(12)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(12))
                              .AssertWork(TimeSpan.FromHours(144))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 20, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(TimeSpan.FromHours(48))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 20, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId2)
                              .AssertWork(TimeSpan.FromHours(96))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 20, 17, 0, 0));
        }
    }
}
