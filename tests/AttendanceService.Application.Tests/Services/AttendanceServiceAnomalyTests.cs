using AttendanceService.Application.Services;
using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using AttendanceService.Domain.Services;
using Moq;
using Shared.Contracts.Events;

namespace AttendanceService.Application.Tests.Services;

/// <summary>
/// 勤怠異常検知機能の統合テスト
/// </summary>
public class AttendanceServiceAnomalyTests
{
    private readonly Mock<IAttendanceRepository> _mockRepository;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly Mock<IAttendanceAnomalyDetector> _mockAnomalyDetector;
    private readonly Application.Services.AttendanceService _service;

    public AttendanceServiceAnomalyTests()
    {
        _mockRepository = new Mock<IAttendanceRepository>();
        _mockEventPublisher = new Mock<IEventPublisher>();
        _mockAnomalyDetector = new Mock<IAttendanceAnomalyDetector>();
        _service = new Application.Services.AttendanceService(
            _mockRepository.Object,
            _mockEventPublisher.Object,
            _mockAnomalyDetector.Object);
    }

    #region CheckIn - Late Arrival Tests

    [Fact]
    public async Task CheckInAsync_WhenLateArrival_ShouldPublishLateArrivalEvent()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkInTime = new DateTime(2024, 1, 15, 9, 30, 0, DateTimeKind.Utc);
        var workDate = checkInTime.Date;
        var lateMinutes = 30;

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance?)null);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance a, CancellationToken _) => a);

        _mockAnomalyDetector
            .Setup(d => d.IsLateArrival(checkInTime))
            .Returns(true);

        _mockAnomalyDetector
            .Setup(d => d.CalculateLateMinutes(checkInTime))
            .Returns(lateMinutes);

        // Act
        var result = await _service.CheckInAsync(employeeId, checkInTime);

        // Assert
        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:checkin", It.IsAny<CheckInRecordedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:late-arrival", 
                It.Is<LateArrivalDetectedEvent>(evt =>
                    evt.EmployeeId == employeeId &&
                    evt.CheckInTime == checkInTime &&
                    evt.WorkDate == workDate &&
                    evt.LateMinutes == lateMinutes),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_WhenNotLate_ShouldNotPublishLateArrivalEvent()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkInTime = new DateTime(2024, 1, 15, 8, 59, 0, DateTimeKind.Utc);
        var workDate = checkInTime.Date;

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance?)null);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance a, CancellationToken _) => a);

        _mockAnomalyDetector
            .Setup(d => d.IsLateArrival(checkInTime))
            .Returns(false);

        // Act
        var result = await _service.CheckInAsync(employeeId, checkInTime);

        // Assert
        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:checkin", It.IsAny<CheckInRecordedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:late-arrival", It.IsAny<LateArrivalDetectedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region CheckOut - Early Leaving Tests

    [Fact]
    public async Task CheckOutAsync_WhenEarlyLeaving_ShouldPublishEarlyLeavingEvent()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var checkOutTime = new DateTime(2024, 1, 15, 16, 0, 0, DateTimeKind.Utc);
        var workDate = checkOutTime.Date;

        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        attendance.CheckIn(checkInTime);

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendance);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockAnomalyDetector
            .Setup(d => d.IsEarlyLeaving(checkInTime, checkOutTime))
            .Returns(true);

        _mockAnomalyDetector
            .Setup(d => d.IsOvertime(It.IsAny<double>()))
            .Returns(false);

        // Act
        var result = await _service.CheckOutAsync(employeeId, checkOutTime);

        // Assert
        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:checkout", It.IsAny<CheckOutRecordedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:early-leaving",
                It.Is<EarlyLeavingDetectedEvent>(evt =>
                    evt.EmployeeId == employeeId &&
                    evt.CheckInTime == checkInTime &&
                    evt.CheckOutTime == checkOutTime &&
                    evt.WorkDate == workDate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckOutAsync_WhenNotEarlyLeaving_ShouldNotPublishEarlyLeavingEvent()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var checkOutTime = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
        var workDate = checkOutTime.Date;

        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        attendance.CheckIn(checkInTime);

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendance);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockAnomalyDetector
            .Setup(d => d.IsEarlyLeaving(checkInTime, checkOutTime))
            .Returns(false);

        _mockAnomalyDetector
            .Setup(d => d.IsOvertime(It.IsAny<double>()))
            .Returns(false);

        // Act
        var result = await _service.CheckOutAsync(employeeId, checkOutTime);

        // Assert
        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:early-leaving", It.IsAny<EarlyLeavingDetectedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region CheckOut - Overtime Tests

    [Fact]
    public async Task CheckOutAsync_WhenOvertime_ShouldPublishOvertimeEvent()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var checkOutTime = new DateTime(2024, 1, 15, 21, 0, 0, DateTimeKind.Utc);
        var workDate = checkOutTime.Date;
        var overtimeHours = 4.0;

        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        attendance.CheckIn(checkInTime);

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendance);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockAnomalyDetector
            .Setup(d => d.IsEarlyLeaving(checkInTime, checkOutTime))
            .Returns(false);

        _mockAnomalyDetector
            .Setup(d => d.IsOvertime(It.IsAny<double>()))
            .Returns(true);

        _mockAnomalyDetector
            .Setup(d => d.CalculateOvertimeHours(It.IsAny<double>()))
            .Returns(overtimeHours);

        // Act
        var result = await _service.CheckOutAsync(employeeId, checkOutTime);

        // Assert
        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:checkout", It.IsAny<CheckOutRecordedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:overtime",
                It.Is<OvertimeDetectedEvent>(evt =>
                    evt.EmployeeId == employeeId &&
                    evt.CheckInTime == checkInTime &&
                    evt.CheckOutTime == checkOutTime &&
                    evt.WorkDate == workDate &&
                    evt.OvertimeHours == overtimeHours),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckOutAsync_WhenNotOvertime_ShouldNotPublishOvertimeEvent()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var checkOutTime = new DateTime(2024, 1, 15, 17, 0, 0, DateTimeKind.Utc);
        var workDate = checkOutTime.Date;

        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        attendance.CheckIn(checkInTime);

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendance);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockAnomalyDetector
            .Setup(d => d.IsEarlyLeaving(checkInTime, checkOutTime))
            .Returns(false);

        _mockAnomalyDetector
            .Setup(d => d.IsOvertime(It.IsAny<double>()))
            .Returns(false);

        // Act
        var result = await _service.CheckOutAsync(employeeId, checkOutTime);

        // Assert
        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:overtime", It.IsAny<OvertimeDetectedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Multiple Anomalies Tests

    [Fact]
    public async Task CheckOutAsync_WhenBothEarlyLeavingAndNotOvertime_ShouldPublishOnlyEarlyLeavingEvent()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var checkOutTime = new DateTime(2024, 1, 15, 15, 0, 0, DateTimeKind.Utc);
        var workDate = checkOutTime.Date;

        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        attendance.CheckIn(checkInTime);

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendance);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockAnomalyDetector
            .Setup(d => d.IsEarlyLeaving(checkInTime, checkOutTime))
            .Returns(true);

        _mockAnomalyDetector
            .Setup(d => d.IsOvertime(It.IsAny<double>()))
            .Returns(false);

        // Act
        var result = await _service.CheckOutAsync(employeeId, checkOutTime);

        // Assert
        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:early-leaving", It.IsAny<EarlyLeavingDetectedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockEventPublisher.Verify(
            e => e.PublishAsync("attendance:overtime", It.IsAny<OvertimeDetectedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion
}
