using System;

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
    }
}
