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
                                 .AddTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(10)).Project
                                 .AddTask(taskId2)
                                    .WithDuration(ProjectTime.FromDays(5))
                                    .AddPredecessorLink(taskId1).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(10))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(ProjectTime.FromDays(5))
                              .AssertStart(new DateTime(2018, 2, 12, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero);
        }

        [Fact]
        public void Task_Ordinal_IsUpated_WhenTaskIsRemoved()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var taskId3 = TaskId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .Project
                                 .AddTask(taskId3)
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
        public void Task_Duration_Set()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(10)).Project
                                 .AddTask(taskId2)
                                    .WithDuration(ProjectTime.FromDays(5))
                                    .AddPredecessorLink(taskId1).Project
                                 .GetTask(taskId2)
                                    .WithDuration(ProjectTime.FromDays(4)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(10))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(ProjectTime.FromDays(4))
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
                                 .AddTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(10)).Project
                                 .AddTask(taskId2)
                                    .WithDuration(ProjectTime.FromDays(5))
                                    .AddPredecessorLink(taskId1).Project
                                 .GetTask(taskId2)
                                    .WithDuration(TimeSpan.Zero).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(10))
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
                                 .AddTask(taskId).Project
                                 .AddResource(resourceId1).Project
                                 .AddResource(resourceId2).Project
                                 .AddAssignment(taskId, resourceId1)
                                    .WithWork(ProjectTime.FromHours(40)).Project
                                 .AddAssignment(taskId, resourceId2)
                                    .WithWork(ProjectTime.FromHours(80)).Project
                                 .GetTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(12)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(12))
                              .AssertWork(ProjectTime.FromHours(136))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 20, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(ProjectTime.FromHours(40))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId2)
                              .AssertWork(ProjectTime.FromHours(96))
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
                                 .AddTask(taskId).Project
                                 .AddResource(resourceId1).Project
                                 .AddResource(resourceId2).Project
                                 .AddAssignment(taskId, resourceId1)
                                    .WithWork(ProjectTime.FromHours(40))
                                    .WithUnits(.5).Project
                                 .AddAssignment(taskId, resourceId2)
                                    .WithWork(ProjectTime.FromHours(80)).Project
                                 .GetTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(12)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(12))
                              .AssertWork(ProjectTime.FromHours(144))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 20, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(ProjectTime.FromHours(48))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 20, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId2)
                              .AssertWork(ProjectTime.FromHours(96))
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
                                 .AddTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(10)).Project
                                 .AddTask(taskId2)
                                    .WithDuration(ProjectTime.FromDays(5))
                                    .AddPredecessorLink(taskId1).Project
                                 .GetTask(taskId2)
                                    .WithWork(ProjectTime.FromHours(120)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(10))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(ProjectTime.FromDays(5))
                              .AssertStart(new DateTime(2018, 2, 12, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .AssertWork(ProjectTime.FromHours(120));
        }

        [Fact]
        public void Task_Work_Set_Zero_DoesNothing()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(10)).Project
                                 .AddTask(taskId2)
                                    .WithDuration(ProjectTime.FromDays(5))
                                    .AddPredecessorLink(taskId1).Project
                                 .GetTask(taskId2)
                                    .WithWork(TimeSpan.Zero).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(10))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(ProjectTime.FromDays(5))
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
                                 .AddTask(taskId).Project
                                 .AddResource(resourceId1).Project
                                 .AddResource(resourceId2).Project
                                 .AddAssignment(taskId, resourceId1)
                                    .WithWork(ProjectTime.FromHours(40)).Project
                                 .AddAssignment(taskId, resourceId2)
                                    .WithWork(ProjectTime.FromHours(80)).Project
                                 .GetTask(taskId)
                                    .WithWork(ProjectTime.FromHours(150)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(12.5))
                              .AssertWork(ProjectTime.FromHours(150))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 21, 12, 0, 0)).Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(ProjectTime.FromHours(50))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 13, 10, 0, 0)).Project
                         .ForAssignment(taskId, resourceId2)
                              .AssertWork(ProjectTime.FromHours(100))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 21, 12, 0, 0));
        }

        [Fact]
        public void Task_Work_Increased_WhenAssignmentsAreAdded()
        {
            var taskId = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(5)).Project
                                 .AddResource(resourceId1).Project
                                 .AddResource(resourceId2).Project
                                 .AddAssignment(taskId, resourceId1).Project
                                 .AddAssignment(taskId, resourceId2).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(5))
                              .AssertWork(ProjectTime.FromHours(80))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(ProjectTime.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId2)
                              .AssertWork(ProjectTime.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0));
        }

        [Fact]
        public void Task_Work_Decreased_WhenAssignmentsAreRemoved()
        {
            var taskId = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(5)).Project
                                 .AddResource(resourceId1).Project
                                 .AddResource(resourceId2).Project
                                 .AddAssignment(taskId, resourceId1).Project
                                 .AddAssignment(taskId, resourceId2).Project
                                 .RemoveAssignment(taskId, resourceId2);

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(5))
                              .AssertWork(ProjectTime.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(ProjectTime.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0));
        }

        [Fact]
        public void Task_Work_Cleared_WhenAllAssignmentsAreRemoved()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(5)).Project
                                 .AddResource(resourceId).Project
                                 .AddAssignment(taskId, resourceId).Project
                                 .RemoveAssignment(taskId, resourceId);

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(5))
                              .AssertWork(TimeSpan.Zero)
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0));
        }

        [Fact]
        public void Task_Work_Decreased_WhenResourcesAreRemoved()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(5)).Project
                                 .AddResource(resourceId).Project
                                 .AddAssignment(taskId, resourceId).Project
                                 .RemoveResource(resourceId);

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(5))
                              .AssertWork(TimeSpan.Zero)
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0));
        }

        [Fact]
        public void Task_EarylyStart_EarylyFinish_LateStart_LateFinish()
        {
            var taskId3 = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 5))
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(10)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(5)).Project
                                 .AddTask(taskId3)
                                    .WithDuration(ProjectTime.FromDays(5)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId3).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(7)).Project;

            ProjectAssert.For(project)
                             .ForTask(0)
                                  .AssertDuration(ProjectTime.FromDays(10))
                                  .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                                  .AssertEarlyStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertEarlyFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                                  .AssertLateStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertLateFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                                  .Project
                             .ForTask(1)
                                  .AssertDuration(ProjectTime.FromDays(5))
                                  .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                                  .AssertEarlyStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertEarlyFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                                  .AssertLateStart(new DateTime(2018, 2, 12, 8, 0, 0))
                                  .AssertLateFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                                  .Project
                            .ForTask(2)
                                  .AssertDuration(ProjectTime.FromDays(5))
                                  .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                                  .AssertEarlyStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertEarlyFinish(new DateTime(2018, 2, 9, 17, 0, 0))
                                  .AssertLateStart(new DateTime(2018, 2, 7, 8, 0, 0))
                                  .AssertLateFinish(new DateTime(2018, 2, 13, 17, 0, 0))
                                  .Project
                            .ForTask(3)
                                  .AssertDuration(ProjectTime.FromDays(3))
                                  .AssertStart(new DateTime(2018, 2, 12, 8, 0, 0))
                                  .AssertFinish(new DateTime(2018, 2, 14, 17, 0, 0))
                                  .AssertEarlyStart(new DateTime(2018, 2, 12, 8, 0, 0))
                                  .AssertEarlyFinish(new DateTime(2018, 2, 14, 17, 0, 0))
                                  .AssertLateStart(new DateTime(2018, 2, 14, 8, 0, 0))
                                  .AssertLateFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                                  .Project
                            .ForTask(4)
                                  .AssertDuration(ProjectTime.FromDays(7))
                                  .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertFinish(new DateTime(2018, 2, 13, 17, 0, 0))
                                  .AssertEarlyStart(new DateTime(2018, 2, 5, 8, 0, 0))
                                  .AssertEarlyFinish(new DateTime(2018, 2, 13, 17, 0, 0))
                                  .AssertLateStart(new DateTime(2018, 2, 8, 8, 0, 0))
                                  .AssertLateFinish(new DateTime(2018, 2, 16, 17, 0, 0));
        }

        [Fact]
        public void Task_Slack_IsCritical()
        {
            var taskIdA = TaskId.Create();
            var taskIdB = TaskId.Create();
            var taskIdC = TaskId.Create();
            var taskIdD = TaskId.Create();
            var taskIdE = TaskId.Create();
            var taskIdF = TaskId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 5))
                                 .AddTask(taskIdA)
                                    .WithDuration(ProjectTime.FromDays(5)).Project
                                 .AddTask(taskIdB)
                                    .WithDuration(ProjectTime.FromDays(4))
                                    .AddPredecessorLink(taskIdA).Project
                                 .AddTask(taskIdC)
                                    .WithDuration(ProjectTime.FromDays(5))
                                    .AddPredecessorLink(taskIdA).Project
                                 .AddTask(taskIdD)
                                    .WithDuration(ProjectTime.FromDays(6))
                                    .AddPredecessorLink(taskIdB).Project
                                 .AddTask(taskIdE)
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskIdC).Project
                                 .AddTask(taskIdF)
                                    .WithDuration(ProjectTime.FromDays(4))
                                    .AddPredecessorLink(taskIdD)
                                    .AddPredecessorLink(taskIdE).Project;

            ProjectAssert.For(project)
                         .ForTask(taskIdA)
                              .AssertStartSlack(TimeSpan.Zero)
                              .AssertFinishSlack(TimeSpan.Zero)
                              .AssertTotalSlack(TimeSpan.Zero)
                              .AssertFreeSlack(TimeSpan.Zero)
                              .AssertIsCritical(true)
                              .Project
                         .ForTask(taskIdB)
                              .AssertStartSlack(TimeSpan.Zero)
                              .AssertFinishSlack(TimeSpan.Zero)
                              .AssertTotalSlack(TimeSpan.Zero)
                              .AssertFreeSlack(TimeSpan.Zero)
                              .AssertIsCritical(true)
                              .Project
                         .ForTask(taskIdC)
                              .AssertStartSlack(ProjectTime.FromDays(2))
                              .AssertFinishSlack(ProjectTime.FromDays(2))
                              .AssertTotalSlack(ProjectTime.FromDays(2))
                              .AssertFreeSlack(TimeSpan.Zero)
                              .AssertIsCritical(false)
                              .Project
                         .ForTask(taskIdD)
                              .AssertStartSlack(TimeSpan.Zero)
                              .AssertFinishSlack(TimeSpan.Zero)
                              .AssertTotalSlack(TimeSpan.Zero)
                              .AssertFreeSlack(TimeSpan.Zero)
                              .AssertIsCritical(true)
                              .Project
                         .ForTask(taskIdE)
                              .AssertStartSlack(ProjectTime.FromDays(2))
                              .AssertFinishSlack(ProjectTime.FromDays(2))
                              .AssertTotalSlack(ProjectTime.FromDays(2))
                              .AssertFreeSlack(ProjectTime.FromDays(2))
                              .AssertIsCritical(false)
                              .Project
                         .ForTask(taskIdF)
                              .AssertStartSlack(TimeSpan.Zero)
                              .AssertFinishSlack(TimeSpan.Zero)
                              .AssertTotalSlack(TimeSpan.Zero)
                              .AssertFreeSlack(TimeSpan.Zero)
                              .AssertIsCritical(true);
        }

        [Fact]
        public void Task_ResourceNames_IsOrderedByName()
        {
            var taskId = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId)
                                    .Project
                                 .AddResource(resourceId1)
                                    .WithName("ZZZ")
                                    .Project
                                 .AddResource(resourceId2)
                                    .WithName("AAA")
                                    .Project
                                 .AddAssignment(taskId, resourceId1)
                                    .Project
                                 .AddAssignment(taskId, resourceId2)
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId)
                              .AssertResourceNames("AAA, ZZZ");
        }

        [Fact]
        public void Task_ResourceNames_IsUpdated_WhenResourceIsRenamed()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId)
                                    .Project
                                 .AddResource(resourceId)
                                    .WithName("Resource")
                                    .Project
                                 .AddAssignment(taskId, resourceId)
                                    .Project
                                 .GetResource(resourceId)
                                    .WithName("New Name")
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId)
                              .AssertResourceNames("New Name");
        }

        [Fact]
        public void Task_ResourceNames_IsUpdated_WhenResourceIsRemoved()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId)
                                    .Project
                                 .AddResource(resourceId)
                                    .WithName("Resource")
                                    .Project
                                 .AddAssignment(taskId, resourceId)
                                    .Project
                                 .RemoveResource(resourceId);

            ProjectAssert.For(project)
                         .ForTask(taskId)
                              .AssertResourceNames(string.Empty);
        }

        [Fact]
        public void Task_ResourceNames_IsUpdated_WhenAssignmentUnitsChange()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId)
                                    .Project
                                 .AddResource(resourceId)
                                    .WithName("Resource")
                                    .Project
                                 .AddAssignment(taskId, resourceId)
                                    .Project
                                 .GetAssignment(taskId, resourceId)
                                    .WithUnits(.5)
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId)
                              .AssertResourceNames("Resource [50%]");
        }

        [Fact]
        public void Task_ResourceNames_IsUpdated_WhenAssignmentIsRemoved()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId)
                                    .Project
                                 .AddResource(resourceId)
                                    .WithName("Resource")
                                    .Project
                                 .AddAssignment(taskId, resourceId)
                                    .Project
                                 .RemoveAssignment(taskId, resourceId);

            ProjectAssert.For(project)
                         .ForTask(taskId)
                              .AssertResourceNames(string.Empty);
        }

        [Fact]
        public void Task_ResourceNames_Set()
        {
            var taskId = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId)
                                    .Project
                                 .AddResource(resourceId1)
                                    .WithName("Res1")
                                    .Project
                                 .AddResource(resourceId2)
                                    .WithName("Res2")
                                    .Project
                                 .AddAssignment(taskId, resourceId1)
                                    .Project
                                 .GetTask(taskId)
                                    .WithResourceNames("Res1 [10%], Res2, Res3")
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId)
                              .AssertResourceNames("Res1 [10%], Res2, Res3")
                              .Project
                         .HasResource(r => r.Name == "Res1")
                         .HasResource(r => r.Name == "Res2")
                         .HasResource(r => r.Name == "Res3")
                         .ForAssignment(taskId, resourceId1)
                              .AssertUnits(.10)
                              .Project
                         .HasAssignment(taskId, resourceId1)
                         .HasAssignment(taskId, resourceId2);
        }

        [Fact]
        public void Task_Links_DetectsCycle_WhenDirect()
        {
            var taskId = TaskId.Create();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                Project.Create()
                       .AddTask(taskId)
                           .AddPredecessorLink(taskId)
            );

            Assert.Equal("Cannot add a link from task 0 to task 0 as this would cause a cycle.", exception.Message);
        }

        [Fact]
        public void Task_Links_DetectsCycle_WhenIndirect()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var taskId3 = TaskId.Create();
            var taskId4 = TaskId.Create();
            var taskId5 = TaskId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                     .Project
                                 .AddTask(taskId2)
                                     .AddPredecessorLink(taskId1)
                                     .Project
                                 .AddTask(taskId3)
                                     .AddPredecessorLink(taskId2)
                                     .Project
                                 .AddTask(taskId4)
                                     .AddPredecessorLink(taskId3)
                                     .Project
                                 .AddTask(taskId5)
                                     .AddPredecessorLink(taskId4)
                                     .Project;

            var exception = Assert.Throws<InvalidOperationException>(() =>
                project.GetTask(taskId1).AddPredecessorLink(taskId5)
            );

            Assert.Equal("Cannot add a link from task 4 to task 0 as this would cause a cycle.", exception.Message);
        }

        [Fact]
        public void Task_Links_IsUpdated_WhenLinkIsAdded()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .Project
                                 .AddTaskLink(taskId1, taskId2);

            var link = project.GetTaskLink(taskId1, taskId2);
            Assert.NotNull(link);

            var expectedLinks = ImmutableArray.Create(link);

            ProjectAssert.For(project)
                         .ForTask(taskId1)
                            .AssertSuccessorLinks(expectedLinks)
                            .Project
                         .ForTask(taskId2)
                            .AssertPredecessorLinks(expectedLinks);
        }

        [Fact]
        public void Task_Links_IsUpdated_WhenLinkIsRemoved()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .AddPredecessorLink(taskId1)
                                    .Project
                                 .RemoveTaskLink(taskId1, taskId2);

            var expectedLinks = ImmutableArray<TaskLink>.Empty;

            ProjectAssert.For(project)
                         .ForTask(taskId1)
                            .AssertSuccessorLinks(expectedLinks)
                            .Project
                         .ForTask(taskId2)
                            .AssertPredecessorLinks(expectedLinks);
        }

        [Fact]
        public void Task_Links_IsUpdated_WhenTasksIsRemoved()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .AddPredecessorLink(taskId1)
                                    .Project
                                 .RemoveTask(taskId1);

            ProjectAssert.For(project)
                         .ForTask(taskId2)
                              .AssertPredecessorLinks(ImmutableArray<TaskLink>.Empty);
        }

        [Fact]
        public void Task_Predecessors_IsOrderedByOrdinal()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var taskId3 = TaskId.Create();
            var taskId4 = TaskId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .Project
                                 .AddTask(taskId3)
                                    .Project
                                 .AddTask(taskId4)
                                    .AddPredecessorLink(taskId2)
                                    .AddPredecessorLink(taskId1)
                                    .AddPredecessorLink(taskId3)
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId4)
                              .AssertPredecessors("0,1,2");
        }

        [Fact]
        public void Task_Predecessors_Set()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var taskId3 = TaskId.Create();
            var taskId4 = TaskId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .Project
                                 .AddTask(taskId3)
                                    .Project
                                 .AddTask(taskId4)
                                    .AddPredecessorLink(taskId1)
                                    .AddPredecessorLink(taskId2)
                                    .Project
                                 .GetTask(taskId3)
                                    .WithPredecessors("0,1")
                                    .Project
                                 .GetTask(taskId4)
                                    .WithPredecessors("1,2")
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId3)
                              .AssertPredecessors("0,1")
                              .Project
                         .ForTask(taskId4)
                              .AssertPredecessors("1,2");
        }

        [Fact]
        public void Task_Predecessors_DetectsCycle()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .Project
                                 .GetTask(taskId2)
                                    .WithPredecessors("0")
                                    .Project;

            var exception = Assert.Throws<InvalidOperationException>(() =>
                project.GetTask(taskId1).WithPredecessors("1")
            );

            Assert.Equal("Cannot add a link from task 1 to task 0 as this would cause a cycle.", exception.Message);
        }

        [Fact]
        public void Task_Predecessors_IsUpdated_WhenOrdinalIsUpdated()
        {
            var taskId0 = TaskId.Create();
            var taskId3 = TaskId.Create();

            var project = Project.Create()
                                 .AddTask(taskId0)
                                    .Project
                                 .AddTask()
                                    .Project
                                 .AddTask()
                                    .Project
                                 .AddTask(taskId3)
                                    .WithPredecessors("0,1")
                                    .Project
                                 .GetTask(taskId0)
                                    .WithOrdinal(3)
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId3)
                              .AssertPredecessors("0,3");
        }

        [Fact]
        public void Task_Predecessors_IsUpdated_WhenPredecessorIsRemoved()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var taskId3 = TaskId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .AddPredecessorLink(taskId1)
                                    .Project
                                 .AddTask(taskId3)
                                    .AddPredecessorLink(taskId1)
                                    .AddPredecessorLink(taskId2)
                                    .Project
                                 .GetTask(taskId2)
                                    .RemovePredecessorLink(taskId1)
                                    .Project
                                 .GetTask(taskId3)
                                    .RemovePredecessorLink(taskId2)
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId2)
                              .AssertPredecessors(string.Empty)
                              .Project
                         .ForTask(taskId3)
                              .AssertPredecessors("0");
        }

        [Fact]
        public void Task_IsMilestone_IsUpdated_WhenDurationChanges_FromNonZero_ToZero()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(1))
                                    .Project
                                 .AddTask(taskId2)
                                    .WithDuration(ProjectTime.FromDays(2))
                                    .Project
                                 .AddResource(resourceId)
                                    .Project
                                 .AddAssignment(taskId2, resourceId)
                                    .Project
                                 .GetTask(taskId1)
                                    .WithDuration(TimeSpan.Zero)
                                    .Project
                                 .GetAssignment(taskId2, resourceId)
                                    .WithWork(TimeSpan.Zero)
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId1)
                              .AssertIsMilestone(true)
                              .Project
                         .ForTask(taskId2)
                              .AssertIsMilestone(true);
        }

        [Fact]
        public void Task_IsMilestone_IsUpdated_WhenDurationChanges_FromZero_ToNonZero()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .Project
                                 .AddResource(resourceId)
                                    .Project
                                 .AddAssignment(taskId2, resourceId)
                                    .Project
                                 .GetTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(1))
                                    .Project
                                 .GetAssignment(taskId2, resourceId)
                                    .WithWork(ProjectTime.FromHours(8))
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId1)
                              .AssertIsMilestone(false)
                              .Project
                         .ForTask(taskId2)
                              .AssertIsMilestone(false);
        }

        [Fact]
        public void Task_IsMilestone_IsNotUpdated_WhenDurationChanges_FromNonZero_ToNonZero()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(2))
                                    .WithIsMilestone(true)
                                    .Project
                                 .AddTask(taskId2)
                                    .WithDuration(ProjectTime.FromDays(2))
                                    .WithIsMilestone(true)
                                    .Project
                                 .AddResource(resourceId)
                                    .Project
                                 .AddAssignment(taskId2, resourceId)
                                    .Project
                                 .GetTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(1))
                                    .Project
                                 .GetAssignment(taskId2, resourceId)
                                    .WithWork(ProjectTime.FromHours(8))
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId1)
                              .AssertIsMilestone(true)
                              .Project
                         .ForTask(taskId2)
                              .AssertIsMilestone(true);
        }

        [Fact]
        public void Task_IsMilestone_IsNotUpdated_WhenDurationChanges_FromZero_ToZero()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId)
                                    .WithWork(ProjectTime.FromHours(0))
                                    .WithIsMilestone(false)
                                    .Project
                                 .AddResource(resourceId)
                                    .Project
                                 .AddAssignment(taskId, resourceId)
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(taskId)
                              .AssertIsMilestone(false);
        }
    }
}
