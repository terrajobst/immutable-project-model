using System;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    internal sealed class ProjectInformationData
    {
        public static ProjectInformationData Create(ProjectId id)
        {
            Debug.Assert(!id.IsDefault);

            return new ProjectInformationData(id,
                                              string.Empty,
                                              DateTimeOffset.Now.Date,
                                              Calendar.Default,
                                              TimeConversion.Default);
        }

        private ProjectInformationData(ProjectId id,
                                       string name,
                                       DateTimeOffset startDate,
                                       Calendar calendar,
                                       TimeConversion timeConversion)
        {
            Id = id;
            Name = name;
            StartDate = startDate;
            Calendar = calendar;
            TimeConversion = timeConversion;
        }

        public ProjectId Id { get; }

        public string Name { get; }

        public DateTimeOffset StartDate { get; }

        public Calendar Calendar { get; }

        public TimeConversion TimeConversion { get; }

        public ProjectInformationData With(string name,
                                           DateTimeOffset startDate,
                                           Calendar calendar,
                                           TimeConversion timeConversion)
        {
            if (name == Name &&
                startDate == StartDate &&
                calendar == Calendar &&
                timeConversion == TimeConversion)
                return this;

            return new ProjectInformationData(Id,
                                              name,
                                              startDate,
                                              calendar,
                                              timeConversion);
        }

        public ProjectInformationData WithName(string name)
        {
            return With(name, StartDate, Calendar, TimeConversion);
        }

        public ProjectInformationData WithStartDate(DateTimeOffset startDate)
        {
            return With(Name, startDate, Calendar, TimeConversion);
        }

        public ProjectInformationData WithCalendar(Calendar calendar)
        {
            if (calendar == null)
                throw new ArgumentNullException(nameof(calendar));

            return With(Name, StartDate, calendar, TimeConversion);
        }

        public ProjectInformationData WithTimeConversion(TimeConversion timeConversion)
        {
            if (timeConversion == null)
                throw new ArgumentNullException(nameof(timeConversion));

            return With(Name, StartDate, Calendar, timeConversion);
        }
    }
}
