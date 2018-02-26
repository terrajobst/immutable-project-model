
using System;
using Immutable.ProjectModel.Tests.Helpers;
using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class ProjectTests
    {
        [Fact]
        public void Project_FinishDate()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStart(new DateTime(2018, 2, 5))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2))
                                    .Project
                                 .AddTask()
                                    .AddPredecessorLink(taskId)
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .Project;

            ProjectAssert.For(project)
                         .AssertFinish(new DateTime(2018, 2, 9, 17, 0, 0));
        }
    }
}
