using System;
using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class DurationTests
    {
        [Fact]
        public void Duration_Constructor_And_Properties()
        {
            var duration1 = new Duration(TimeSpan.FromHours(8), TimeUnit.Days, true);
            Assert.Equal(TimeSpan.FromHours(8), duration1.Span);
            Assert.Equal(TimeUnit.Days, duration1.Unit);
            Assert.True(duration1.IsEstimated);

            var duration2 = new Duration(TimeSpan.FromHours(2), TimeUnit.Hours, false);
            Assert.Equal(TimeSpan.FromHours(2), duration2.Span);
            Assert.Equal(TimeUnit.Hours, duration2.Unit);
            Assert.False(duration2.IsEstimated);
        }

        [Fact]
        public void Duration_ToString()
        {
            Assert.Equal("1 day", new Duration(TimeSpan.FromHours(8), TimeUnit.Days, false).ToString());
            Assert.Equal("2 days?", new Duration(TimeSpan.FromHours(16), TimeUnit.Days, true).ToString());
        }

        [Fact]
        public void Duration_ToString_PrefersStoredUnits()
        {
            Assert.Equal("8 hrs", new Duration(TimeSpan.FromHours(8), TimeUnit.Hours, false).ToString());
            Assert.Equal("16 hrs?", new Duration(TimeSpan.FromHours(16), TimeUnit.Hours, true).ToString());
        }

        [Fact]
        public void Duration_ToString_HonorsTimeConversion()
        {
            var conversion = TimeConversion.Default.WithHoursPerDay(4);

            Assert.Equal("2 days", new Duration(TimeSpan.FromHours(8), TimeUnit.Days, false).ToString(conversion));
            Assert.Equal("4 days?", new Duration(TimeSpan.FromHours(16), TimeUnit.Days, true).ToString(conversion));
        }
    }
}
