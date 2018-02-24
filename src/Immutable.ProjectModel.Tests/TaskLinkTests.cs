using System;

using Immutable.ProjectModel.Tests.Helpers;

using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class TaskLinkTests
    {
        [Fact]
        public void TaskLink_FinishToStart()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors("0")
                             .AssertStart(new DateTime(2018, 2, 28, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 28, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 28, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0));
        }

        [Fact]
        public void TaskLink_FinishToStart_PositiveSlack()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId, lag: ProjectTime.FromDays(1)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors("0FS+1 day")
                             .AssertStart(new DateTime(2018, 3, 1, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 3, 5, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 3, 1, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 3, 5, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 3, 1, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 3, 5, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0));
        }

        [Fact]
        public void TaskLink_FinishToStart_NegativeSlack()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId, lag: ProjectTime.FromDays(-1)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors("0FS-1 days")
                             .AssertStart(new DateTime(2018, 2, 27, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 3, 1, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 27, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 3, 1, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 27, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 3, 1, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0));
        }

        [Fact]
        public void TaskLink_FinishToStart_NegativeSlack_BeforeProjectStart()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId, lag: ProjectTime.FromDays(-3)).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors("0FS-3 days")
                             .AssertStart(new DateTime(2018, 2, 23, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 23, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 23, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0));
        }

        [Fact]
        public void TaskLink_StartToStart()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId,
                                                        TaskLinkType.StartToStart)
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors("0SS")
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0));
        }

        [Fact]
        public void TaskLink_StartToStart_PositiveSlack()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId,
                                                        TaskLinkType.StartToStart,
                                                        ProjectTime.FromDays(1))
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors("0SS+1 day")
                             .AssertStart(new DateTime(2018, 2, 27, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 3, 1, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 27, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 3, 1, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 27, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 3, 1, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0));
        }

        [Fact]
        public void TaskLink_StartToStart_Negativelack()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId,
                                                        TaskLinkType.StartToStart,
                                                        ProjectTime.FromDays(-1))
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors("0SS-1 days")
                             .AssertStart(new DateTime(2018, 2, 23, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 23, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 23, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0));
        }

        [Fact]
        public void TaskLink_FinishToFinish()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId,
                                                        TaskLinkType.FinishToFinish)
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 27, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(1))
                             .AssertFinishSlack(ProjectTime.FromDays(1))
                             .AssertTotalSlack(ProjectTime.FromDays(1))
                             .AssertFreeSlack(ProjectTime.FromDays(1)).Project
                         .ForTask(1)
                             .AssertPredecessors("0FF")
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0));
        }

        [Fact]
        public void TaskLink_FinishToFinish_PositiveSlack()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId,
                                                        TaskLinkType.FinishToFinish,
                                                        ProjectTime.FromDays(1))
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors("0FF+1 day")
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0));
        }

        [Fact]
        public void TaskLink_FinishToFinish_NegativeSlack()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId,
                                                        TaskLinkType.FinishToFinish,
                                                        ProjectTime.FromDays(-1))
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 27, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(1))
                             .AssertFinishSlack(ProjectTime.FromDays(1))
                             .AssertTotalSlack(ProjectTime.FromDays(1))
                             .AssertFreeSlack(ProjectTime.FromDays(1)).Project
                         .ForTask(1)
                             .AssertPredecessors("0FF-1 days")
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0));
        }

        [Fact]
        public void TaskLink_FinishToFinish_NegativeSlack_AtProjectStart()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(5)).Project
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(3)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(2))
                                    .AddPredecessorLink(taskId,
                                                        TaskLinkType.FinishToFinish,
                                                        ProjectTime.FromDays(-1))
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 28, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(2))
                             .AssertFinishSlack(ProjectTime.FromDays(2))
                             .AssertTotalSlack(ProjectTime.FromDays(2))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(2)
                             .AssertPredecessors("1FF-1 days")
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 3, 1, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(3))
                             .AssertFinishSlack(ProjectTime.FromDays(3))
                             .AssertTotalSlack(ProjectTime.FromDays(3))
                             .AssertFreeSlack(ProjectTime.FromDays(3));
        }

        [Fact]
        public void TaskLink_FinishToFinish_NegativeSlack_BeforeProjectStart()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(5)).Project
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(3)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(2))
                                    .AddPredecessorLink(taskId,
                                                        TaskLinkType.FinishToFinish,
                                                        ProjectTime.FromDays(-2))
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 28, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 28, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(2))
                             .AssertFinishSlack(ProjectTime.FromDays(2))
                             .AssertTotalSlack(ProjectTime.FromDays(2))
                             .AssertFreeSlack(ProjectTime.FromDays(1)).Project
                         .ForTask(2)
                             .AssertPredecessors("1FF-2 days")
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 3, 1, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 3, 2, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(3))
                             .AssertFinishSlack(ProjectTime.FromDays(3))
                             .AssertTotalSlack(ProjectTime.FromDays(3))
                             .AssertFreeSlack(ProjectTime.FromDays(3));
        }

        [Fact]
        public void TaskLink_StartToFinish()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId,
                                                        TaskLinkType.StartToFinish)
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors("0SF")
                             .AssertStart(new DateTime(2018, 2, 21, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 21, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 23, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(2))
                             .AssertFinishSlack(ProjectTime.FromDays(2))
                             .AssertTotalSlack(ProjectTime.FromDays(2))
                             .AssertFreeSlack(ProjectTime.FromDays(2));
        }

        [Fact]
        public void TaskLink_StartToFinish_PositiveSlack()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId,
                                                        TaskLinkType.StartToFinish,
                                                        ProjectTime.FromDays(1))
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors("0SF+1 day")
                             .AssertStart(new DateTime(2018, 2, 22, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 26, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 22, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 26, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 23, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(1))
                             .AssertFinishSlack(ProjectTime.FromDays(1))
                             .AssertTotalSlack(ProjectTime.FromDays(1))
                             .AssertFreeSlack(ProjectTime.FromDays(1));
        }

        [Fact]
        public void TaskLink_StartToFinish_NegativeSlack()
        {
            var taskId = TaskId.Create();
            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 26, 8, 0, 0))
                                 .AddTask(taskId)
                                    .WithDuration(ProjectTime.FromDays(2)).Project
                                 .AddTask()
                                    .WithDuration(ProjectTime.FromDays(3))
                                    .AddPredecessorLink(taskId,
                                                        TaskLinkType.StartToFinish,
                                                        ProjectTime.FromDays(-1))
                                    .Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertPredecessors(null)
                             .AssertStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 26, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(0))
                             .AssertFinishSlack(ProjectTime.FromDays(0))
                             .AssertTotalSlack(ProjectTime.FromDays(0))
                             .AssertFreeSlack(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertPredecessors("0SF-1 days")
                             .AssertStart(new DateTime(2018, 2, 20, 8, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 23, 8, 0, 0))
                             .AssertEarlyStart(new DateTime(2018, 2, 20, 8, 0, 0))
                             .AssertEarlyFinish(new DateTime(2018, 2, 23, 8, 0, 0))
                             .AssertLateStart(new DateTime(2018, 2, 23, 8, 0, 0))
                             .AssertLateFinish(new DateTime(2018, 2, 27, 17, 0, 0))
                             .AssertStartSlack(ProjectTime.FromDays(3))
                             .AssertFinishSlack(ProjectTime.FromDays(3))
                             .AssertTotalSlack(ProjectTime.FromDays(3))
                             .AssertFreeSlack(ProjectTime.FromDays(3));
        }
    }
}
