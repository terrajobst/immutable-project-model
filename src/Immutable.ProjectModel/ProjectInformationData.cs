using System;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    internal sealed class ProjectInformationData
    {
        public static ProjectInformationData Create(ProjectId id)
        {
            Debug.Assert(!id.IsDefault);

            var start = DateTimeOffset.Now.Date;
            var finish = start;

            return new ProjectInformationData(id,
                                              string.Empty,
                                              start,
                                              finish,
                                              Calendar.Default,
                                              TimeConversion.Default);
        }

        private ProjectInformationData(ProjectId id,
                                       string name,
                                       DateTimeOffset start,
                                       DateTimeOffset finish,
                                       Calendar calendar,
                                       TimeConversion timeConversion)
        {
            Id = id;
            Name = name;
            Start = start;
            Finish = finish;
            Calendar = calendar;
            TimeConversion = timeConversion;
        }

        public ProjectId Id { get; }

        public string Name { get; }

        public DateTimeOffset Start { get; }

        public DateTimeOffset Finish { get; }

        public Calendar Calendar { get; }

        public TimeConversion TimeConversion { get; }

        public ProjectInformationData With(string name,
                                           DateTimeOffset start,
                                           DateTimeOffset finish,
                                           Calendar calendar,
                                           TimeConversion timeConversion)
        {
            if (name == Name &&
                start == Start &&
                finish == Finish &&
                calendar == Calendar &&
                timeConversion == TimeConversion)
                return this;

            return new ProjectInformationData(Id,
                                              name,
                                              start,
                                              finish,
                                              calendar,
                                              timeConversion);
        }

        public ProjectInformationData WithName(string name)
        {
            return With(name, Start, Finish, Calendar, TimeConversion);
        }

        public ProjectInformationData WithStart(DateTimeOffset start)
        {
            return With(Name, start, Finish, Calendar, TimeConversion);
        }

        public ProjectInformationData WithFinish(DateTimeOffset finish)
        {
            return With(Name, Start, finish, Calendar, TimeConversion);
        }

        public ProjectInformationData WithCalendar(Calendar calendar)
        {
            if (calendar == null)
                throw new ArgumentNullException(nameof(calendar));

            return With(Name, Start, Finish, calendar, TimeConversion);
        }

        public ProjectInformationData WithTimeConversion(TimeConversion timeConversion)
        {
            if (timeConversion == null)
                throw new ArgumentNullException(nameof(timeConversion));

            return With(Name, Start, Finish, Calendar, timeConversion);
        }
    }
}
