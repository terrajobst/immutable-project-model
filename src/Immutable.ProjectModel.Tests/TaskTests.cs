using System;
using System.Collections.Immutable;
using Immutable.ProjectModel.Tests.Helpers;

using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class TaskTests
    {
        [Fact]
        public void Task_IsScheduledProperly()
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
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertStart(new DateTime(2018, 2, 12, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero);
        }

        [Fact]
        public void Task_Duration_Set()
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
                                    .WithDuration(TimeSpan.FromDays(4)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(10))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(TimeSpan.FromDays(4))
                              .AssertStart(new DateTime(2018, 2, 12, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 15, 17, 0, 0));
        }

        [Fact]
        public void Task_Duration_Set_Zero()
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
                                    .WithDuration(TimeSpan.Zero).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(10))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(TimeSpan.Zero)
                              .AssertStart(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0));
        }

        [Fact]
        public void Task_Duration_Set_IncreasesLongestAssignment()
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
        public void Task_Duration_Set_HonorsAssignmentUnits()
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

        [Fact]
        public void Task_Work_Set_DoesNothing()
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
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertStart(new DateTime(2018, 2, 12, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .AssertWork(TimeSpan.FromHours(120));
        }

        [Fact]
        public void Task_Work_Set_Zero_DoesNothing()
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
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertStart(new DateTime(2018, 2, 12, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero);
        }

        [Fact]
        public void Task_Work_Set_IncreasesAssignmentWorkProportionally()
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
        public void Task_EarylyStart_EarylyFinish_LateStart_LateFinish()
        {
            var taskId3 = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 5))
                                 .AddNewTask()
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddNewTask()
                                    .WithDuration(TimeSpan.FromDays(5)).Project
                                 .AddNewTask(taskId3)
                                    .WithDuration(TimeSpan.FromDays(5)).Project
                                 .AddNewTask()
                                    .WithDuration(TimeSpan.FromDays(3))
                                    .AddPredecessorId(taskId3).Project
                                 .AddNewTask()
                                    .WithDuration(TimeSpan.FromDays(7)).Project;

            ProjectAssert.For(project)
                             .ForTask(0)
                                  .AssertDuration(TimeSpan.FromDays(10))
                                  .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                                  .AssertEarlyStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertEarlyFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                                  .AssertLateStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertLateFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                                  .Project
                             .ForTask(1)
                                  .AssertDuration(TimeSpan.FromDays(5))
                                  .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                                  .AssertEarlyStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertEarlyFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                                  .AssertLateStart(new DateTime(2018, 2, 12, 8, 0, 0))
                                  .AssertLateFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                                  .Project
                            .ForTask(2)
                                  .AssertDuration(TimeSpan.FromDays(5))
                                  .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                                  .AssertEarlyStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertEarlyFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                                  .AssertLateStart(new DateTime(2018, 2, 7, 8, 0, 0))
                                  .AssertLateFinish(new DateTime(2018, 2, 13, 17, 0, 0))
                                  .Project
                            .ForTask(3)
                                  .AssertDuration(TimeSpan.FromDays(3))
                                  .AssertStart(new DateTime(2018, 2, 12, 8, 0, 0))
                                  .AssertFinish(new DateTime(2018, 2, 14, 17, 0, 0))
                                  .AssertEarlyStart(new DateTime(2018, 2, 12, 8, 0, 0))
                                  .AssertEarlyFinish(new DateTime(2018, 2, 14, 17, 0, 0))
                                  .AssertLateStart(new DateTime(2018, 2, 14, 8, 0, 0))
                                  .AssertLateFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                                  .Project
                            .ForTask(4)
                                  .AssertDuration(TimeSpan.FromDays(7))
                                  .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertFinish(new DateTime(2018, 2, 13, 17, 0, 0))
                                  .AssertEarlyStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertEarlyFinish(new DateTime(2018, 2, 13, 17, 0, 0))
                                  .AssertLateStart(new DateTime(2018, 2, 8, 8, 0, 0))
                                  .AssertLateFinish(new DateTime(2018, 2, 16, 17, 0, 0));
        }

        [Fact]
        public void Task_Removal_UpdatesOrdinal()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var taskId3 = TaskId.Create();

            var project = Project.Create()
                                 .AddNewTask(taskId1)
                                    .Project
                                 .AddNewTask(taskId2)
                                    .Project
                                 .AddNewTask(taskId3)
                                    .Project
                                 .RemoveTask(taskId2);

            ProjectAssert.For(project)
                         .ForTask(taskId1)
                              .AssertOrdinal(0)
                              .Project
                         .ForTask(taskId3)
                              .AssertOrdinal(1);
        }

        [Fact]
        public void Task_Removal_UpdatesPredecessors()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .AddNewTask(taskId1)
                                    .Project
                                 .AddNewTask(taskId2)
                                    .AddPredecessorId(taskId1)
                                    .Project
                                 .RemoveTask(taskId1);

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertPredecessorIds(ImmutableArray<TaskId>.Empty);
        }

        [Fact]
        public void Task_Removal_RemovesAssignments()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .AddNewTask(taskId1)
                                    .Project
                                 .AddNewTask(taskId2)
                                    .AddPredecessorId(taskId1)
                                    .Project
                                 .AddNewResource(resourceId)
                                    .Project
                                 .AddNewAssignment(taskId1, resourceId)
                                    .Project
                                 .AddNewAssignment(taskId2, resourceId)
                                    .Project
                                 .RemoveTask(taskId1);

            ProjectAssert.For(project)
                         .HasNoTask(taskId1)
                         .HasTask(taskId2)
                         .HasResource(resourceId)
                         .HasNoAssignment(taskId1, resourceId)
                         .HasAssignment(taskId2, resourceId);
        }
    }
}
