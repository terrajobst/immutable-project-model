using System;
using System.Collections.Immutable;
using System.Linq;

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

        [Fact]
        public void Projet_Calendar_DefaultsToStandard()
        {
            var project = Project.Create();
            Assert.Equal(Calendar.Default, project.Calendar);
        }

        [Fact]
        public void Projet_Calendars_DefaultsToStandardSet()
        {
            var project = Project.Create();
            var defaultSet = ImmutableArray.Create(Calendar.Default, Calendar.TwentyFourSeven, Calendar.NightShift);
            Assert.Equal(defaultSet.AsEnumerable(), project.Calendars);
        }

        [Fact]
        public void Projet_WithCalendar_CannotRemoveCurrent()
        {
            var project = Project.Create();
            var current = project.Calendar;
            var calendars = project.Calendars.Remove(current);
            project = project.WithCalendars(calendars);

            calendars = calendars.Add(current);

            Assert.Equal(calendars.AsEnumerable(), project.Calendars);
        }

        [Fact]
        public void Projet_WithCalendar_AddsToSet()
        {
            var project = Project.Create();
            var calendars = project.Calendars.Remove(Calendar.NightShift);
            project = project.WithCalendars(calendars);

            Assert.Equal(calendars.AsEnumerable(), project.Calendars);

            project = project.WithCalendar(Calendar.NightShift);
            calendars = calendars.Add(Calendar.NightShift);

            Assert.Equal(calendars.AsEnumerable(), project.Calendars);
        }
    }
}
