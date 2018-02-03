using System;

using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class CalendarTests
    {
        [Fact]
        public void Calendar_Default()
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
        public void Calendar_SixDayWorkWeek()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var workingSaturday = WorkingDay.CreateEightToFive(DayOfWeek.Saturday);
            var sixDayWorkWeek = WorkingWeek.Default.WithDay(workingSaturday);
            var calendar = Calendar.Default.WithWorkingWeek(sixDayWorkWeek);

            var project = Project.Create()                                 
                                 .WithStartDate(new DateTime(2018, 1, 29))
                                 .WithCalendar(calendar)
                                 .AddNewTask(taskId1)
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddNewTask(taskId2)
                                    .WithDuration(TimeSpan.FromDays(5))
                                    .AddPredecessorId(taskId1).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(10))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 8, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertStart(new DateTime(2018, 2, 9, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 14, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero);
        }

        [Fact]
        public void Calendar_TwentyFourSeven()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .WithStartDate(new DateTime(2018, 2, 1))
                                 .WithCalendar(Calendar.TwentyFourSeven)
                                 .AddNewTask(taskId1)
                                    .WithDuration(TimeSpan.FromDays(10)).Project
                                 .AddNewTask(taskId2)
                                    .WithDuration(TimeSpan.FromDays(5))
                                    .AddPredecessorId(taskId1).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(TimeSpan.FromDays(10))
                              .AssertStart(new DateTime(2018, 2, 1, 0, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 4, 8, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(TimeSpan.FromDays(5))
                              .AssertStart(new DateTime(2018, 2, 4, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 6, 0, 0, 0))
                              .AssertWork(TimeSpan.Zero);
        }
    }
}
