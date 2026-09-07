using FluentAssertions;
using Nailify.Domain.Booking;
using Nailify.Domain.Enums;
using Xunit;

namespace Nailify.Application.Tests;

public class BookingRulesTests
{
    [Theory]
    [InlineData(AppointmentStatus.Pending)]
    [InlineData(AppointmentStatus.AwaitingDeposit)]
    [InlineData(AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.InProgress)]
    public void Active_appointment_statuses_block_time(AppointmentStatus status)
    {
        BookingRules.BlocksTime(status).Should().BeTrue();
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    public void Finished_appointment_statuses_release_time(AppointmentStatus status)
    {
        BookingRules.BlocksTime(status).Should().BeFalse();
    }

    [Theory]
    [InlineData("10:00", "11:00", "10:30", "11:30", true)]
    [InlineData("10:00", "11:00", "11:00", "11:30", false)]
    [InlineData("10:00", "11:00", "09:00", "10:00", false)]
    [InlineData("10:00", "11:00", "10:00", "11:00", true)]
    public void Overlap_uses_half_open_time_ranges(string firstStart, string firstEnd, string secondStart, string secondEnd, bool expected)
    {
        BookingRules.Overlaps(TimeOnly.Parse(firstStart), TimeOnly.Parse(firstEnd), TimeOnly.Parse(secondStart), TimeOnly.Parse(secondEnd)).Should().Be(expected);
    }

    [Fact]
    public void Total_duration_includes_design_duration()
    {
        BookingRules.TotalDurationMinutes(45, 15).Should().Be(60);
    }

    [Fact]
    public void Total_price_includes_design_surcharge()
    {
        BookingRules.TotalPrice(35m, 12m).Should().Be(47m);
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending, false, true)]
    [InlineData(AppointmentStatus.AwaitingDeposit, false, true)]
    [InlineData(AppointmentStatus.Confirmed, false, true)]
    [InlineData(AppointmentStatus.InProgress, false, false)]
    [InlineData(AppointmentStatus.Completed, false, false)]
    [InlineData(AppointmentStatus.Cancelled, false, false)]
    [InlineData(AppointmentStatus.Pending, true, false)]
    public void Customer_cancellation_follows_status_and_start_time(AppointmentStatus status, bool hasStarted, bool expected)
    {
        BookingRules.CanCustomerCancel(status, hasStarted).Should().Be(expected);
    }

    [Theory]
    [InlineData(330000, 0.30, 99000)]
    [InlineData(250001, 0.30, 75000)]
    public void Deposit_amount_uses_configured_percentage_and_vnd_rounding(decimal totalPrice, decimal percent, decimal expected)
    {
        BookingRules.DepositAmount(totalPrice, percent).Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, 0.30)]
    [InlineData(100000, 0)]
    [InlineData(100000, 1.01)]
    public void Deposit_amount_rejects_invalid_values(decimal totalPrice, decimal percent)
    {
        var action = () => BookingRules.DepositAmount(totalPrice, percent);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
