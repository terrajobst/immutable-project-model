using System;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    internal sealed class ProjectInformationData
    {
        public static ProjectInformationData Create(ProjectId id)
        {
            Debug.Assert(!id.IsDefault);

            return new ProjectInformationData(id, string.Empty, DateTimeOffset.Now.Date, Calendar.Default);
        }

        private ProjectInformationData(ProjectId id, string name, DateTimeOffset startDate, Calendar calendar)
        {
            Id = id;
            Name = name;
            StartDate = startDate;
            Calendar = calendar;
        }

        public ProjectId Id { get; }

        public string Name { get; }

        public DateTimeOffset StartDate { get; }

        public Calendar Calendar { get; }

        public ProjectInformationData With(string name, DateTimeOffset startDate, Calendar calendar)
        {
            if (name == Name &&
                startDate == StartDate &&
                calendar == Calendar)
                return this;

            return new ProjectInformationData(Id, name, startDate, calendar);
        }

        public ProjectInformationData WithName(string name)
        {
            return With(name, StartDate, Calendar);
        }

        public ProjectInformationData WithStartDate(DateTimeOffset startDate)
        {
            return With(Name, startDate, Calendar);
        }

        public ProjectInformationData WithCalendar(Calendar calendar)
        {
            return With(Name, StartDate, calendar);
        }
    }
}
