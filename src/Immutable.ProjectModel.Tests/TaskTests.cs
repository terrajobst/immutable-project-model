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
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddTask(taskId2)
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
                                 .AddTask(taskId1)
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddTask(taskId2)
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
                                 .AddTask(taskId1)
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddTask(taskId2)
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
                                 .AddTask(taskId).Project
                                 .AddResource(resourceId1).Project
                                 .AddResource(resourceId2).Project
                                 .AddAssignment(taskId, resourceId1)
                                    .WithWork(TimeSpan.FromHours(40)).Project
                                 .AddAssignment(taskId, resourceId2)
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
                                 .AddTask(taskId).Project
                                 .AddResource(resourceId1).Project
                                 .AddResource(resourceId2).Project
                                 .AddAssignment(taskId, resourceId1)
                                    .WithWork(TimeSpan.FromHours(40))
                                    .WithUnits(.5).Project
                                 .AddAssignment(taskId, resourceId2)
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
                                 .AddTask(taskId1)
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddTask(taskId2)
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
                                 .AddTask(taskId1)
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddTask(taskId2)
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
                                 .AddTask(taskId).Project
                                 .AddResource(resourceId1).Project
                                 .AddResource(resourceId2).Project
                                 .AddAssignment(taskId, resourceId1)
                                    .WithWork(TimeSpan.FromHours(40)).Project
                                 .AddAssignment(taskId, resourceId2)
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
                                 .AddTask()
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddTask()
                                    .WithDuration(TimeSpan.FromDays(5)).Project
                                 .AddTask(taskId3)
                                    .WithDuration(TimeSpan.FromDays(5)).Project
                                 .AddTask()
                                    .WithDuration(TimeSpan.FromDays(3))
                                    .AddPredecessorId(taskId3).Project
                                 .AddTask()
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
                                    .WithDuration(TimeSpan.FromDays(5)).Project
                                 .AddTask(taskIdB)
                                    .WithDuration(TimeSpan.FromDays(4))
                                    .AddPredecessorId(taskIdA).Project
                                 .AddTask(taskIdC)
                                    .WithDuration(TimeSpan.FromDays(5))
                                    .AddPredecessorId(taskIdA).Project
                                 .AddTask(taskIdD)
                                    .WithDuration(TimeSpan.FromDays(6))
                                    .AddPredecessorId(taskIdB).Project
                                 .AddTask(taskIdE)
                                    .WithDuration(TimeSpan.FromDays(3))
                                    .AddPredecessorId(taskIdC).Project
                                 .AddTask(taskIdF)
                                    .WithDuration(TimeSpan.FromDays(4))
                                    .AddPredecessorId(taskIdD)
                                    .AddPredecessorId(taskIdE).Project;

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
                              .AssertStartSlack(TimeSpan.FromDays(2))
                              .AssertFinishSlack(TimeSpan.FromDays(2))
                              .AssertTotalSlack(TimeSpan.FromDays(2))
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
                              .AssertStartSlack(TimeSpan.FromDays(2))
                              .AssertFinishSlack(TimeSpan.FromDays(2))
                              .AssertTotalSlack(TimeSpan.FromDays(2))
                              .AssertFreeSlack(TimeSpan.FromDays(2))
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
        public void Task_ResourceNames_Setting()
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
        public void Task_Removal_UpdatesOrdinal()
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
        public void Task_Removal_UpdatesPredecessors()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
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
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .AddPredecessorId(taskId1)
                                    .Project
                                 .AddResource(resourceId)
                                    .Project
                                 .AddAssignment(taskId1, resourceId)
                                    .Project
                                 .AddAssignment(taskId2, resourceId)
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
