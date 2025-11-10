using AttendanceService.Domain.Services;
using Xunit;

namespace AttendanceService.Domain.Tests.Services;

public class AttendanceAnomalyDetectorTests
{
    private readonly IAttendanceAnomalyDetector _detector;

    public AttendanceAnomalyDetectorTests()
    {
        _detector = new AttendanceAnomalyDetector();
    }

    #region IsLateArrival Tests

    [Fact]
    public void IsLateArrival_WhenCheckInAt0859_ReturnsFalse()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 8, 59, 0);

        // Act
        var result = _detector.IsLateArrival(checkInTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsLateArrival_WhenCheckInAt0900_ReturnsFalse()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 9, 0, 0);

        // Act
        var result = _detector.IsLateArrival(checkInTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsLateArrival_WhenCheckInAt0901_ReturnsTrue()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 9, 1, 0);

        // Act
        var result = _detector.IsLateArrival(checkInTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsLateArrival_WhenCheckInAt0930_ReturnsTrue()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 9, 30, 0);

        // Act
        var result = _detector.IsLateArrival(checkInTime);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region IsEarlyLeaving Tests

    [Fact]
    public void IsEarlyLeaving_WhenWorkingLessThan4Hours_ReturnsFalse()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 9, 0, 0);
        var checkOutTime = new DateTime(2025, 1, 15, 12, 30, 0); // 3.5 hours

        // Act
        var result = _detector.IsEarlyLeaving(checkInTime, checkOutTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEarlyLeaving_WhenCheckOutAt1659And4HoursWork_ReturnsTrue()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 9, 0, 0);
        var checkOutTime = new DateTime(2025, 1, 15, 16, 59, 0); // 7.98 hours

        // Act
        var result = _detector.IsEarlyLeaving(checkInTime, checkOutTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEarlyLeaving_WhenCheckOutAt1700_ReturnsFalse()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 9, 0, 0);
        var checkOutTime = new DateTime(2025, 1, 15, 17, 0, 0); // 8 hours

        // Act
        var result = _detector.IsEarlyLeaving(checkInTime, checkOutTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEarlyLeaving_WhenCheckOutAt1701_ReturnsFalse()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 9, 0, 0);
        var checkOutTime = new DateTime(2025, 1, 15, 17, 1, 0); // 8.01 hours

        // Act
        var result = _detector.IsEarlyLeaving(checkInTime, checkOutTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEarlyLeaving_WhenCheckOutAt1600And7HoursWork_ReturnsTrue()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 9, 0, 0);
        var checkOutTime = new DateTime(2025, 1, 15, 16, 0, 0); // 7 hours

        // Act
        var result = _detector.IsEarlyLeaving(checkInTime, checkOutTime);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region IsOvertime Tests

    [Fact]
    public void IsOvertime_WhenWorkHoursIs8_ReturnsFalse()
    {
        // Arrange
        var workHours = 8.0;

        // Act
        var result = _detector.IsOvertime(workHours);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOvertime_WhenWorkHoursIs9_ReturnsFalse()
    {
        // Arrange
        var workHours = 9.0;

        // Act
        var result = _detector.IsOvertime(workHours);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOvertime_WhenWorkHoursIs10_ReturnsTrue()
    {
        // Arrange
        var workHours = 10.0;

        // Act
        var result = _detector.IsOvertime(workHours);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOvertime_WhenWorkHoursIs12_ReturnsTrue()
    {
        // Arrange
        var workHours = 12.0;

        // Act
        var result = _detector.IsOvertime(workHours);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region CalculateLateMinutes Tests

    [Fact]
    public void CalculateLateMinutes_WhenNotLate_Returns0()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 8, 59, 0);

        // Act
        var result = _detector.CalculateLateMinutes(checkInTime);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateLateMinutes_WhenLate1Minute_Returns1()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 9, 1, 0);

        // Act
        var result = _detector.CalculateLateMinutes(checkInTime);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void CalculateLateMinutes_WhenLate30Minutes_Returns30()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 9, 30, 0);

        // Act
        var result = _detector.CalculateLateMinutes(checkInTime);

        // Assert
        Assert.Equal(30, result);
    }

    [Fact]
    public void CalculateLateMinutes_WhenLate90Minutes_Returns90()
    {
        // Arrange
        var checkInTime = new DateTime(2025, 1, 15, 10, 30, 0);

        // Act
        var result = _detector.CalculateLateMinutes(checkInTime);

        // Assert
        Assert.Equal(90, result);
    }

    #endregion

    #region CalculateOvertimeHours Tests

    [Fact]
    public void CalculateOvertimeHours_WhenNoOvertime_Returns0()
    {
        // Arrange
        var workHours = 8.0;

        // Act
        var result = _detector.CalculateOvertimeHours(workHours);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateOvertimeHours_When9Hours_Returns0()
    {
        // Arrange
        var workHours = 9.0;

        // Act
        var result = _detector.CalculateOvertimeHours(workHours);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateOvertimeHours_When10Hours_Returns2()
    {
        // Arrange
        var workHours = 10.0;

        // Act
        var result = _detector.CalculateOvertimeHours(workHours);

        // Assert
        Assert.Equal(2.0, result);
    }

    [Fact]
    public void CalculateOvertimeHours_When12Hours_Returns4()
    {
        // Arrange
        var workHours = 12.0;

        // Act
        var result = _detector.CalculateOvertimeHours(workHours);

        // Assert
        Assert.Equal(4.0, result);
    }

    [Fact]
    public void CalculateOvertimeHours_When10Point5Hours_Returns2Point5()
    {
        // Arrange
        var workHours = 10.5;

        // Act
        var result = _detector.CalculateOvertimeHours(workHours);

        // Assert
        Assert.Equal(2.5, result);
    }

    #endregion
}
