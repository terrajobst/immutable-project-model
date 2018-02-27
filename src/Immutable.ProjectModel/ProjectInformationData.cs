using System;
using System.Collections.Immutable;
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
            var calendar = Calendar.Default;
            var calendars = ImmutableArray.Create(Calendar.Default, Calendar.TwentyFourSeven, Calendar.NightShift);
            var timeConversion = TimeConversion.Default;

            return new ProjectInformationData(id,
                                              string.Empty,
                                              start,
                                              finish,
                                              calendar,
                                              calendars,
                                              timeConversion);
        }

        private ProjectInformationData(ProjectId id,
                                       string name,
                                       DateTimeOffset start,
                                       DateTimeOffset finish,
                                       Calendar calendar,
                                       ImmutableArray<Calendar> calendars,
                                       TimeConversion timeConversion)
        {
            Id = id;
            Name = name;
            Start = start;
            Finish = finish;
            Calendar = calendar;
            Calendars = calendars;
            TimeConversion = timeConversion;
        }

        public ProjectId Id { get; }

        public string Name { get; }

        public DateTimeOffset Start { get; }

        public DateTimeOffset Finish { get; }

        public Calendar Calendar { get; }

        public ImmutableArray<Calendar> Calendars { get; }

        public TimeConversion TimeConversion { get; }

        public ProjectInformationData With(string name,
                                           DateTimeOffset start,
                                           DateTimeOffset finish,
                                           Calendar calendar,
                                           ImmutableArray<Calendar> calendars,
                                           TimeConversion timeConversion)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (calendar == null)
                throw new ArgumentNullException(nameof(calendar));

            if (timeConversion == null)
                throw new ArgumentNullException(nameof(timeConversion));

            if (name == Name &&
                start == Start &&
                finish == Finish &&
                calendar == Calendar &&
                calendars == Calendars &&
                timeConversion == TimeConversion)
                return this;

            if (!calendars.Contains(calendar))
                calendars = calendars.Add(calendar);

            return new ProjectInformationData(Id,
                                              name,
                                              start,
                                              finish,
                                              calendar,
                                              calendars,
                                              timeConversion);
        }

        public ProjectInformationData WithName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return With(name, Start, Finish, Calendar, Calendars, TimeConversion);
        }

        public ProjectInformationData WithStart(DateTimeOffset start)
        {
            return With(Name, start, Finish, Calendar, Calendars, TimeConversion);
        }

        public ProjectInformationData WithFinish(DateTimeOffset finish)
        {
            return With(Name, Start, finish, Calendar, Calendars, TimeConversion);
        }

        public ProjectInformationData WithCalendar(Calendar calendar)
        {
            if (calendar == null)
                throw new ArgumentNullException(nameof(calendar));

            return With(Name, Start, Finish, calendar, Calendars, TimeConversion);
        }

        public ProjectInformationData WithCalendars(ImmutableArray<Calendar> calendars)
        {
            return With(Name, Start, Finish, Calendar, calendars, TimeConversion);
        }

        public ProjectInformationData WithTimeConversion(TimeConversion timeConversion)
        {
            if (timeConversion == null)
                throw new ArgumentNullException(nameof(timeConversion));

            return With(Name, Start, Finish, Calendar, Calendars, timeConversion);
        }
    }
}
