using System;
using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class TimeConversionTests
    {
        [Fact]
        public void TimeConversion_Default()
        {
            Assert.Equal(8, TimeConversion.Default.HoursPerDay);
            Assert.Equal(40, TimeConversion.Default.HoursPerWeek);
            Assert.Equal(20, TimeConversion.Default.DaysPerMonth);
            Assert.Equal(TimeUnit.Days, TimeConversion.Default.DurationUnit);
            Assert.Equal(TimeUnit.Hours, TimeConversion.Default.WorkUnit);
        }

        [Fact]
        public void TimeConversion_IsIdempotent()
        {
            var d = TimeConversion.Default;
            Assert.Same(d, d.WithHoursPerDay(d.HoursPerDay));
            Assert.Same(d, d.WithHoursPerWeek(d.HoursPerWeek));
            Assert.Same(d, d.WithDaysPerMonth(d.DaysPerMonth));
            Assert.Same(d, d.WithDurationUnit(d.DurationUnit));
            Assert.Same(d, d.WithWorkUnit(d.WorkUnit));
            Assert.Same(d, d.With(d.HoursPerDay, d.HoursPerWeek, d.DaysPerMonth, d.DurationUnit, d.WorkUnit));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void TimeConversion_WithHoursPerDay_Rejects_Invalid(double hoursPerDay)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TimeConversion.Default.WithHoursPerDay(hoursPerDay));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void TimeConversion_WithHoursPerWeek_Rejects_Invalid(double hoursPerWeek)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TimeConversion.Default.WithHoursPerWeek(hoursPerWeek));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void TimeConversion_WithDaysPerMonth_Rejects_Invalido(double daysPerMonth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TimeConversion.Default.WithDaysPerMonth(daysPerMonth));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(15)]
        public void TimeConversion_FromMinutes_ToMinutes(int minutes)
        {
            var timeConversion = TimeConversion.Default.WithHoursPerDay(99)
                                                       .WithHoursPerWeek(99)
                                                       .WithDaysPerMonth(99);

            Assert.Equal(minutes, timeConversion.FromMinutes(minutes).TotalMinutes);
            Assert.Equal(minutes, timeConversion.FromUnit(minutes, TimeUnit.Minutes).TotalMinutes);

            Assert.Equal(minutes, timeConversion.ToMinutes(timeConversion.FromUnit(minutes, TimeUnit.Minutes)));
            Assert.Equal(minutes, timeConversion.ToUnit(timeConversion.FromUnit(minutes, TimeUnit.Minutes), TimeUnit.Minutes));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(15)]
        public void TimeConversion_FromHours_ToHours(int hours)
        {
            var timeConversion = TimeConversion.Default.WithHoursPerDay(99)
                                                       .WithHoursPerWeek(99)
                                                       .WithDaysPerMonth(99);

            Assert.Equal(hours, timeConversion.FromHours(hours).TotalHours);
            Assert.Equal(hours, timeConversion.FromUnit(hours, TimeUnit.Hours).TotalHours);

            Assert.Equal(hours, timeConversion.ToHours(timeConversion.FromUnit(hours, TimeUnit.Hours)));
            Assert.Equal(hours, timeConversion.ToUnit(timeConversion.FromUnit(hours, TimeUnit.Hours), TimeUnit.Hours));
        }

        [Theory]
        [InlineData(0, 8)]
        [InlineData(-1, 8)]
        [InlineData(15, 8)]
        [InlineData(0, 12)]
        [InlineData(-1, 12)]
        [InlineData(15, 12)]
        public void TimeConversion_FromDays_ToDays(int days, int hoursPerDay)
        {
            var timeConversion = TimeConversion.Default.WithHoursPerDay(hoursPerDay)
                                                       .WithHoursPerWeek(99)
                                                       .WithDaysPerMonth(99);

            var expectedHours = days * hoursPerDay;
            Assert.Equal(expectedHours, timeConversion.FromDays(days).TotalHours);
            Assert.Equal(expectedHours, timeConversion.FromUnit(days, TimeUnit.Days).TotalHours);

            Assert.Equal(days, timeConversion.ToDays(timeConversion.FromUnit(days, TimeUnit.Days)));
            Assert.Equal(days, timeConversion.ToUnit(timeConversion.FromUnit(days, TimeUnit.Days), TimeUnit.Days));
        }

        [Theory]
        [InlineData(0, 20)]
        [InlineData(-1, 20)]
        [InlineData(15, 20)]
        [InlineData(0, 50)]
        [InlineData(-1, 50)]
        [InlineData(15, 50)]
        public void TimeConversion_FromWeeks_ToWeeks(int weeks, int hoursPerWeek)
        {
            var timeConversion = TimeConversion.Default.WithHoursPerDay(99)
                                                       .WithHoursPerWeek(hoursPerWeek)
                                                       .WithDaysPerMonth(99);

            var expectedHours = weeks * hoursPerWeek;
            Assert.Equal(expectedHours, timeConversion.FromWeeks(weeks).TotalHours);
            Assert.Equal(expectedHours, timeConversion.FromUnit(weeks, TimeUnit.Weeks).TotalHours);

            Assert.Equal(weeks, timeConversion.ToWeeks(timeConversion.FromUnit(weeks, TimeUnit.Weeks)));
            Assert.Equal(weeks, timeConversion.ToUnit(timeConversion.FromUnit(weeks, TimeUnit.Weeks), TimeUnit.Weeks));
        }

        [Theory]
        [InlineData(0, 8, 20)]
        [InlineData(-1, 8, 20)]
        [InlineData(15, 8, 20)]
        [InlineData(0, 12, 20)]
        [InlineData(-1, 12, 20)]
        [InlineData(15, 12, 20)]
        [InlineData(0, 8, 40)]
        [InlineData(-1, 8, 40)]
        [InlineData(15, 8, 40)]
        [InlineData(0, 12, 40)]
        [InlineData(-1, 12, 40)]
        [InlineData(15, 12, 40)]
        public void TimeConversion_FromMonths_ToMonths(int months, int hoursPerDay, int daysPerMonth)
        {
            var timeConversion = TimeConversion.Default.WithHoursPerDay(hoursPerDay)
                                                       .WithHoursPerWeek(99)
                                                       .WithDaysPerMonth(daysPerMonth);

            var expectedHours = months * daysPerMonth * hoursPerDay;
            Assert.Equal(expectedHours, timeConversion.FromMonths(months).TotalHours);
            Assert.Equal(expectedHours, timeConversion.FromUnit(months, TimeUnit.Months).TotalHours);

            Assert.Equal(months, timeConversion.ToMonths(timeConversion.FromUnit(months, TimeUnit.Months)));
            Assert.Equal(months, timeConversion.ToUnit(timeConversion.FromUnit(months, TimeUnit.Months), TimeUnit.Months));
        }

        [Theory]
        [InlineData("-1", TimeUnit.Hours, -1)]
        [InlineData("-1", TimeUnit.Days, -8)]
        [InlineData("-1h", TimeUnit.Days, -1)]
        [InlineData("-1d", TimeUnit.Hours, -8)]
        [InlineData("0", TimeUnit.Hours, 0)]
        [InlineData("1", TimeUnit.Hours, 1)]
        [InlineData("1", TimeUnit.Days, 8)]
        [InlineData("1h", TimeUnit.Days, 1)]
        [InlineData("1d", TimeUnit.Hours, 8)]
        public void TimeConversion_ParseWork_HonorsDefaultUnits(string text, TimeUnit workUnit, double expectedHours)
        {
            var timeConversion = TimeConversion.Default.WithWorkUnit(workUnit);
            var expectedValue = TimeSpan.FromHours(expectedHours);

            Assert.Equal(expectedValue, timeConversion.ParseWork(text));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("asdsd")]
        [InlineData("1 zz")]
        public void TimeConversion_TryParseWork_DoesNotThrowForInvalid(string text)
        {
            Assert.False(TimeConversion.Default.TryParseWork(text, out var result));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("asdsd")]
        [InlineData("1 zz")]
        public void TimeConversion_ParseWork_ThrowsForInvalid(string text)
        {
            Assert.Throws<FormatException>(() => TimeConversion.Default.ParseWork(text));
        }


        [Theory]
        [InlineData("1 min", 1.0 / 60.0, TimeUnit.Minutes)]
        [InlineData("2 mins", 2.0 / 60.0, TimeUnit.Minutes)]
        [InlineData("1 hr", 1, TimeUnit.Hours)]
        [InlineData("8 hrs", 8, TimeUnit.Hours)]
        [InlineData("1 day", 8, TimeUnit.Days)]
        [InlineData("2 days", 16, TimeUnit.Days)]
        [InlineData("1 wk", 40, TimeUnit.Weeks)]
        [InlineData("2 wks", 80, TimeUnit.Weeks)]
        [InlineData("1 mon", 160, TimeUnit.Months)]
        [InlineData("2 mons", 320, TimeUnit.Months)]
        public void TimeConversion_FormatWork_HonorsDefaultUnits(string text, double hours, TimeUnit workUnit)
        {
            var tc = TimeConversion.Default.WithWorkUnit(workUnit);
            Assert.Equal(text, tc.FormatWork(tc.FromHours(hours)));
        }

        [Theory]
        [InlineData("-1", TimeUnit.Hours, -1)]
        [InlineData("-1", TimeUnit.Days, -8)]
        [InlineData("-1h", TimeUnit.Days, -1)]
        [InlineData("-1d", TimeUnit.Hours, -8)]
        [InlineData("0", TimeUnit.Hours, 0)]
        [InlineData("1", TimeUnit.Hours, 1)]
        [InlineData("1", TimeUnit.Days, 8)]
        [InlineData("1h", TimeUnit.Days, 1)]
        [InlineData("1d", TimeUnit.Hours, 8)]
        public void TimeConversion_ParseDuration_HonorsDefaultUnits(string text, TimeUnit durationUnit, double expectedHours)
        {
            var timeConversion = TimeConversion.Default.WithDurationUnit(durationUnit);
            var expectedValue = TimeSpan.FromHours(expectedHours);

            Assert.Equal(expectedValue, timeConversion.ParseDuration(text));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("asdsd")]
        [InlineData("1 zz")]
        public void TimeConversion_TryParseDuration_DoesNotThrowForInvalid(string text)
        {
            Assert.False(TimeConversion.Default.TryParseDuration(text, out var result));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("asdsd")]
        [InlineData("1 zz")]
        public void TimeConversion_ParseDuration_ThrowsForInvalid(string text)
        {
            Assert.Throws<FormatException>(() => TimeConversion.Default.ParseDuration(text));
        }

        [Theory]
        [InlineData("1 min", 1.0 / 60.0, TimeUnit.Minutes)]
        [InlineData("2 mins", 2.0 / 60.0, TimeUnit.Minutes)]
        [InlineData("1 hr", 1, TimeUnit.Hours)]
        [InlineData("8 hrs", 8, TimeUnit.Hours)]
        [InlineData("1 day", 8, TimeUnit.Days)]
        [InlineData("2 days", 16, TimeUnit.Days)]
        [InlineData("1 wk", 40, TimeUnit.Weeks)]
        [InlineData("2 wks", 80, TimeUnit.Weeks)]
        [InlineData("1 mon", 160, TimeUnit.Months)]
        [InlineData("2 mons", 320, TimeUnit.Months)]
        public void TimeConversion_FormatDuration_HonorsDefaultUnits(string text, double hours, TimeUnit durationUnit)
        {
            var tc = TimeConversion.Default.WithDurationUnit(durationUnit);
            Assert.Equal(text, tc.FormatDuration(tc.FromHours(hours)));
        }
    }
}
