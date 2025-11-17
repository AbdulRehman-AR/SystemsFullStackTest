using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests;

public class CanteenTests
{
    [Test]
    public void GetCutoffTimeForDay_WithExistingSchedule_ShouldReturnCutoffTime()
    {
        var canteen = new Canteen
        {
            Id = Guid.NewGuid(),
            Schedules = new List<CanteenSchedule>
            {
                new CanteenSchedule
                {
                    DayOfWeek = DayOfWeek.Monday,
                    CutoffTime = new TimeSpan(9, 30, 0)
                }
            }
        };

        var cutoff = canteen.GetCutoffTimeForDay(DayOfWeek.Monday);

        cutoff.Should().Be(new TimeSpan(9, 30, 0));
    }

    [Test]
    public void GetCutoffTimeForDay_WithNonExistingSchedule_ShouldReturnNull()
    {
        var canteen = new Canteen
        {
            Id = Guid.NewGuid(),
            Schedules = new List<CanteenSchedule>()
        };

        var cutoff = canteen.GetCutoffTimeForDay(DayOfWeek.Monday);

        cutoff.Should().BeNull();
    }

    [Test]
    public void IsOpenOnDay_WithExistingSchedule_ShouldReturnTrue()
    {
        var canteen = new Canteen
        {
            Id = Guid.NewGuid(),
            Schedules = new List<CanteenSchedule>
            {
                new CanteenSchedule { DayOfWeek = DayOfWeek.Monday }
            }
        };

        canteen.IsOpenOnDay(DayOfWeek.Monday).Should().BeTrue();
    }

    [Test]
    public void IsOpenOnDay_WithNonExistingSchedule_ShouldReturnFalse()
    {
        var canteen = new Canteen
        {
            Id = Guid.NewGuid(),
            Schedules = new List<CanteenSchedule>()
        };

        canteen.IsOpenOnDay(DayOfWeek.Monday).Should().BeFalse();
    }
}

