using System;

namespace Immutable.ProjectModel.Tests.Helpers
{
    internal static class ProjectTime
    {
        public static TimeSpan FromDays(double days)
        {
            return TimeConversion.Default.FromDays(days);
        }

        public static TimeSpan FromHours(double hours)
        {
            return TimeConversion.Default.FromHours(hours);
        }
    }
}
