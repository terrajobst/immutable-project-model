﻿using System;

using Immutable.ProjectModel.Tests.Helpers;

using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class ResourceTests
    {
        [Fact]
        public void Resource_Removal_Decreases_Task_Work()
        {
            var taskId = TaskId.Create();
            var resourceId = ResourceId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .AddNewTask(taskId)
                                    .WithDuration(TimeSpan.FromDays(5)).Project
                                 .AddNewResource(resourceId).Project
                                 .AddNewAssignment(taskId, resourceId).Project
                                 .RemoveResource(resourceId);

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertWork(TimeSpan.Zero)
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 2, 17, 0, 0));
        }

        [Fact]
        public void Resource_Removal_RemovesAssignments()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();
            var resourceId1 = ResourceId.Create();
            var resourceId2 = ResourceId.Create();

            var project = Project.Create()
                                 .AddNewTask(taskId1)
                                    .Project
                                 .AddNewTask(taskId2)
                                    .AddPredecessorId(taskId1)
                                    .Project
                                 .AddNewResource(resourceId1)
                                    .Project
                                 .AddNewResource(resourceId2)
                                    .Project
                                 .AddNewAssignment(taskId1, resourceId1)
                                    .Project
                                 .AddNewAssignment(taskId2, resourceId2)
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
