using System;
using System.Globalization;

using Immutable.ProjectModel.Tests.Helpers;

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
                                 .WithStart(new DateTime(2018, 1, 29))
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
        public void Calendar_SixDayWorkWeek()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var workingSaturday = WorkingDay.CreateEightToFive(DayOfWeek.Saturday);
            var sixDayWorkWeek = WorkingWeek.Default.WithDay(workingSaturday);
            var calendar = Calendar.Default.WithWorkingWeek(sixDayWorkWeek);

            var project = Project.Create()                                 
                                 .WithStart(new DateTime(2018, 1, 29))
                                 .WithCalendar(calendar)
                                 .AddTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(10)).Project
                                 .AddTask(taskId2)
                                    .WithDuration(ProjectTime.FromDays(5))
                                    .AddPredecessorLink(taskId1).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(10))
                              .AssertStart(new DateTime(2018, 1, 29, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 8, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(ProjectTime.FromDays(5))
                              .AssertStart(new DateTime(2018, 2, 9, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 14, 17, 0, 0))
                              .AssertWork(TimeSpan.Zero);
        }

        [Fact]
        public void Calendar_NightShift()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .WithStart(new DateTime(2018, 2, 1))
                                 .WithCalendar(Calendar.NightShift)
                                 .AddTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(10)).Project
                                 .AddTask(taskId2)
                                    .WithDuration(ProjectTime.FromDays(5))
                                    .AddPredecessorLink(taskId1).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                             .AssertDuration(ProjectTime.FromDays(10))
                             .AssertStart(new DateTime(2018, 2, 1, 0, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 15, 0, 0, 0))
                             .AssertWork(ProjectTime.FromDays(0)).Project
                         .ForTask(1)
                             .AssertDuration(ProjectTime.FromDays(5))
                             .AssertStart(new DateTime(2018, 2, 15, 0, 0, 0))
                             .AssertFinish(new DateTime(2018, 2, 22, 0, 0, 0))
                             .AssertWork(ProjectTime.FromDays(0));
        }

        [Fact]
        public void Calendar_TwentyFourSeven()
        {
            var taskId1 = TaskId.Create();
            var taskId2 = TaskId.Create();

            var project = Project.Create()
                                 .WithStart(new DateTime(2018, 2, 1))
                                 .WithCalendar(Calendar.TwentyFourSeven)
                                 .AddTask(taskId1)
                                    .WithDuration(ProjectTime.FromDays(10)).Project
                                 .AddTask(taskId2)
                                    .WithDuration(ProjectTime.FromDays(5))
                                    .AddPredecessorLink(taskId1).Project;

            ProjectAssert.For(project)
                         .ForTask(0)
                              .AssertDuration(ProjectTime.FromDays(10))
                              .AssertStart(new DateTime(2018, 2, 1, 0, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 4, 8, 0, 0))
                              .AssertWork(TimeSpan.Zero).Project
                         .ForTask(1)
                              .AssertDuration(ProjectTime.FromDays(5))
                              .AssertStart(new DateTime(2018, 2, 4, 8, 0, 0))
                              .AssertFinish(new DateTime(2018, 2, 6, 0, 0, 0))
                              .AssertWork(TimeSpan.Zero);
        }

        [Theory]
        [InlineData("")]
        [InlineData("New Name")]
        public void Calendar_Name_Set(string name)
        {
            var calendar = Calendar.Default.WithName(name);
            Assert.Equal(name, calendar.Name);
        }

        [Fact]
        public void Calendar_Name_Set_Throws_WhenNull()
        {
            Assert.Throws<ArgumentNullException>(() => Calendar.Default.WithName(null));
            Assert.Throws<ArgumentNullException>(() => Calendar.Default.With(null, Calendar.Default.WorkingWeek));
        }

        [Fact]
        public void Calendar_WithWorkingWeek_RejectsNonWorkingWeek()
        {
            var week = WorkingWeek.NonWorking;
            Assert.Throws<ArgumentException>(() => Calendar.Default.WithWorkingWeek(week));
            Assert.Throws<ArgumentException>(() => Calendar.Default.With(Calendar.Default.Name, week));
        }

        [Theory]
        [InlineData("2/4/2018 07:00", 0.0, "2/5/2018 08:00")]
        [InlineData("2/4/2018 08:00", 0.0, "2/5/2018 08:00")]
        [InlineData("2/4/2018 07:00", 1.0, "2/5/2018 09:00")]
        [InlineData("2/4/2018 08:00", 1.0, "2/5/2018 09:00")]
        [InlineData("2/4/2018 07:00", 4.0, "2/5/2018 12:00")]
        [InlineData("2/4/2018 08:00", 4.0, "2/5/2018 12:00")]
        [InlineData("2/4/2018 07:00", 5.5, "2/5/2018 14:30")]
        [InlineData("2/4/2018 08:00", 5.5, "2/5/2018 14:30")]
        [InlineData("2/5/2018 07:00", 0.0, "2/5/2018 08:00")]
        [InlineData("2/5/2018 08:00", 0.0, "2/5/2018 08:00")]
        [InlineData("2/5/2018 07:00", 1.0, "2/5/2018 09:00")]
        [InlineData("2/5/2018 08:00", 1.0, "2/5/2018 09:00")]
        [InlineData("2/5/2018 07:00", 4.0, "2/5/2018 12:00")]
        [InlineData("2/5/2018 08:00", 4.0, "2/5/2018 12:00")]
        [InlineData("2/5/2018 07:00", 5.5, "2/5/2018 14:30")]
        [InlineData("2/5/2018 08:00", 5.5, "2/5/2018 14:30")]
        [InlineData("2/5/2018 07:00", 76, "2/16/2018 12:00")]
        [InlineData("2/5/2018 07:00", 76.5, "2/16/2018 13:30")]
        [InlineData("2/5/2018 07:00", 80, "2/16/2018 17:00")]
        [InlineData("2/5/2018 08:00", 88, "2/19/2018 17:00")]
        public void Calendar_Add(string dateText, double hours, string expectedDateText)
        {
            var date = DateTime.Parse(dateText, CultureInfo.InvariantCulture);
            var expectedDate = DateTime.Parse(expectedDateText, CultureInfo.InvariantCulture);
            var timeSpan = TimeSpan.FromHours(hours);

            Assert.Equal(expectedDate, Calendar.Default.AddWork(date, timeSpan));
        }

        [Theory]
        [InlineData("2/5/2018 18:00", 1.0, "2/5/2018 16:00")]
        [InlineData("2/5/2018 17:00", 1.0, "2/5/2018 16:00")]
        [InlineData("2/5/2018 18:00", 4.0, "2/5/2018 13:00")]
        [InlineData("2/5/2018 17:00", 4.0, "2/5/2018 13:00")]
        [InlineData("2/5/2018 18:00", 5.5, "2/5/2018 10:30")]
        [InlineData("2/5/2018 17:00", 5.5, "2/5/2018 10:30")]
        [InlineData("2/5/2018 18:00", 76, "1/23/2018 13:00")]
        [InlineData("2/5/2018 18:00", 76.5, "1/23/2018 11:30")]
        [InlineData("2/5/2018 18:00", 80, "1/23/2018 08:00")]
        [InlineData("2/5/2018 17:00", 88, "1/22/2018 08:00")]
        public void Calendar_Subtract(string dateText, double hours, string expectedDateText)
        {
            var date = DateTime.Parse(dateText, CultureInfo.InvariantCulture);
            var expectedDate = DateTime.Parse(expectedDateText, CultureInfo.InvariantCulture);
            var timeSpan = TimeSpan.FromHours(hours);

            Assert.Equal(expectedDate, Calendar.Default.AddWork(date, -timeSpan));
        }

        [Theory]
        [InlineData("2/2/2018 16:59", "2/2/2018 16:59")]
        [InlineData("2/2/2018 17:00", "2/5/2018 08:00")]
        [InlineData("2/4/2018 07:00", "2/5/2018 08:00")]
        [InlineData("2/4/2018 08:00", "2/5/2018 08:00")]
        [InlineData("2/4/2018 09:00", "2/5/2018 08:00")]
        [InlineData("2/5/2018 07:00", "2/5/2018 08:00")]
        [InlineData("2/5/2018 08:00", "2/5/2018 08:00")]
        [InlineData("2/5/2018 09:00", "2/5/2018 09:00")]
        public void Calendar_FindWorkStart(string dateText, string expectedDateText)
        {
            var date = DateTime.Parse(dateText, CultureInfo.InvariantCulture);
            var expectedDate = DateTime.Parse(expectedDateText, CultureInfo.InvariantCulture);
            Assert.Equal(expectedDate, Calendar.Default.FindWorkStart(date));
        }

        [Theory]
        [InlineData("2/2/2018 17:01", "2/2/2018 17:00")]
        [InlineData("2/5/2018 08:00", "2/2/2018 17:00")]
        [InlineData("2/3/2018 18:00", "2/2/2018 17:00")]
        [InlineData("2/3/2018 17:00", "2/2/2018 17:00")]
        [InlineData("2/3/2018 16:00", "2/2/2018 17:00")]
        [InlineData("2/2/2018 18:00", "2/2/2018 17:00")]
        [InlineData("2/2/2018 17:00", "2/2/2018 17:00")]
        [InlineData("2/2/2018 16:00", "2/2/2018 16:00")]
        public void Calendar_FindWorkEnd(string dateText, string expectedDateText)
        {
            var date = DateTime.Parse(dateText, CultureInfo.InvariantCulture);
            var expectedDate = DateTime.Parse(expectedDateText, CultureInfo.InvariantCulture);
            Assert.Equal(expectedDate, Calendar.Default.FindWorkEnd(date));
        }

        [Theory]
        [InlineData("2/5/2018 12:00", "2/5/2018 13:00", 0.0)]
        [InlineData("2/5/2018 12:15", "2/5/2018 12:45", 0.0)]
        [InlineData("2/5/2018 13:00", "2/5/2018 14:00", 1.0)]
        [InlineData("2/5/2018 12:30", "2/5/2018 14:00", 1.0)]
        [InlineData("2/5/2018 11:00", "2/5/2018 12:00", 1.0)]
        [InlineData("2/5/2018 11:00", "2/5/2018 12:30", 1.0)]
        [InlineData("2/5/2018 11:00", "2/5/2018 14:00", 2.0)]
        [InlineData("2/5/2018 08:00", "2/5/2018 17:00", 8.0)]
        [InlineData("2/5/2018 00:00", "2/5/2018 23:59", 8.0)]
        [InlineData("2/5/2018 17:00", "2/4/2018 08:00", -8.0)]
        public void Calendar_GetWork(string startDateText, string endDateText, double expectedHours)
        {
            var startDate = DateTime.Parse(startDateText, CultureInfo.InvariantCulture);
            var endDate = DateTime.Parse(endDateText, CultureInfo.InvariantCulture);
            var expected = TimeSpan.FromHours(expectedHours);

            Assert.Equal(expected, Calendar.Default.GetWork(startDate, endDate));
            Assert.Equal(-expected, Calendar.Default.GetWork(endDate, startDate));
        }
    }
}
