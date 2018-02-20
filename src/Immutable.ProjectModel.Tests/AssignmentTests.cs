using System;

using Immutable.ProjectModel.Tests.Helpers;

using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class AssignmentTests
    {
        [Fact]
        public void Assignment_Work_InitializedFrom_Task_Duration()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(10)).Project
                                 .AddResource(resourceId).Project
                                 .AddAssignment(taskId, resourceId).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(10))
                              .AssertWork(ProjectTime.FromHours(80))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId)
                              .AssertWork(ProjectTime.FromHours(80))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0));
        }

        [Fact]
        public void Assignment_Work_InitializedFrom_Task_Work()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(10))
                                    .WithWork(ProjectTime.FromHours(40)).Project
                                 .AddResource(resourceId).Project
                                 .AddAssignment(taskId, resourceId).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(5))
                              .AssertWork(ProjectTime.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId)
                              .AssertWork(ProjectTime.FromHours(40))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0));
        }

        [Fact]
        public void Assignment_Units()
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
                                    .WithWork(ProjectTime.FromHours(20)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(10))
                              .AssertWork(ProjectTime.FromHours(60))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(ProjectTime.FromHours(40))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0))
                              .Project
                         .ForAssignment(taskId, resourceId2)
                              .AssertWork(ProjectTime.FromHours(20))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 7, 12, 0, 0));
        }

        [Fact]
        public void Assignment_Start_Finish()
        {
            var taskId = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();
            var resourceId3 = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 5))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(10)).Project
                                 .AddResource(resourceId1).Project
                                 .AddResource(resourceId2).Project
                                 .AddResource(resourceId3).Project
                                 .AddAssignment(taskId, resourceId1)
                                    .WithWork(ProjectTime.FromHours(80)).Project
                                 .AddAssignment(taskId, resourceId2)
                                    .WithWork(ProjectTime.FromHours(40)).Project
                                 .AddAssignment(taskId, resourceId3)
                                    .WithWork(ProjectTime.FromHours(10))
                                    .WithUnits(.25).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(10))
                              .AssertWork(ProjectTime.FromHours(130))
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId1)
                              .AssertWork(ProjectTime.FromHours(80))
                              .AssertUnits(1.0)
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 16, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId2)
                              .AssertWork(ProjectTime.FromHours(40))
                              .AssertUnits(1.0)
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0)).Project
                         .ForAssignment(taskId, resourceId3)
                              .AssertWork(ProjectTime.FromHours(10))
                              .AssertUnits(.25)
                              .AssertStart(new DateTime(2018, 2, 5, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0));
        }

        [Fact]
        public void Assignment_TaskName_ResourceName_AreInitialized()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 5))
                                 .AddTask(taskId)
                                    .WithName("Some Task")
                                    .Project
                                 .AddResource(resourceId)
                                    .WithName("Some Resource")
                                    .Project
                                 .AddAssignment(taskId, resourceId)
                                    .Project;

            ProjectAssert.For(project)
                         .ForAssignment(taskId, resourceId)
                              .AssertTaskName("Some Task")
                              .AssertResourceName("Some Resource");
        }

        [Fact]
        public void Assignment_TaskName_ResourceName_AreUpdated()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 5))
                                 .AddTask(taskId)
                                    .WithName("Some Task")
                                    .Project
                                 .AddResource(resourceId)
                                    .WithName("Some Resource")
                                    .Project
                                 .AddAssignment(taskId, resourceId)
                                    .Project
                                 .GetTask(taskId)
                                    .WithName("Some New Task")
                                    .Project
                                 .GetResource(resourceId)
                                    .WithName("Some New Resource")
                                    .Project;

            ProjectAssert.For(project)
                         .ForAssignment(taskId, resourceId)
                              .AssertTaskName("Some New Task")
                              .AssertResourceName("Some New Resource");
        }

        [Fact]
        public void Assignment_IsRemoved_WhenTaskIsRemoved()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .AddPredecessorLink(taskId1)
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

        [Fact]
        public void Assignment_IsRemoved_WhenResourceIsRemoved()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();

            var project = Project.Create()
                                 .AddTask(taskId1)
                                    .Project
                                 .AddTask(taskId2)
                                    .AddPredecessorLink(taskId1)
                                    .Project
                                 .AddResource(resourceId1)
                                    .Project
                                 .AddResource(resourceId2)
                                    .Project
                                 .AddAssignment(taskId1, resourceId1)
                                    .Project
                                 .AddAssignment(taskId2, resourceId2)
                                    .Project
                                 .RemoveResource(resourceId1);

            ProjectAssert.For(project)
                         .HasTask(taskId1)
                         .HasTask(taskId2)
                         .HasNoResource(resourceId1)
                         .HasNoAssignment(taskId1, resourceId1)
                         .HasAssignment(taskId2, resourceId2);
        }
    }
}
