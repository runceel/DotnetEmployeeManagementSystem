using AttendanceService.Application.Services;
using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using Moq;
using Shared.Contracts.Events;

namespace AttendanceService.Application.Tests.Services;

public class AttendanceServiceTests
{
    private readonly Mock<IAttendanceRepository> _mockRepository;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly Application.Services.AttendanceService _service;

    public AttendanceServiceTests()
    {
        _mockRepository = new Mock<IAttendanceRepository>();
        _mockEventPublisher = new Mock<IEventPublisher>();
        _service = new Application.Services.AttendanceService(_mockRepository.Object, _mockEventPublisher.Object);
    }

    [Fact]
    public async Task CheckInAsync_WhenNoExistingRecord_ShouldCreateNewAttendance()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var workDate = checkInTime.Date;

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance?)null);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance a, CancellationToken _) => a);

        // Act
        var result = await _service.CheckInAsync(employeeId, checkInTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(workDate, result.WorkDate);
        Assert.Equal(checkInTime, result.CheckInTime);
        Assert.Null(result.CheckOutTime);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(e => e.PublishAsync("attendance:checkin", It.IsAny<CheckInRecordedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_WhenDuplicateCheckIn_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var workDate = checkInTime.Date;

        var existingAttendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        existingAttendance.CheckIn(checkInTime);

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAttendance);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CheckInAsync(employeeId, checkInTime.AddMinutes(30)));
    }

    [Fact]
    public async Task CheckInAsync_WhenEmptyEmployeeId_ShouldThrowArgumentException()
    {
        // Arrange
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CheckInAsync(Guid.Empty, checkInTime));
    }

    [Fact]
    public async Task CheckOutAsync_WhenValidCheckOut_ShouldUpdateAttendance()
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

        // Act
        var result = await _service.CheckOutAsync(employeeId, checkOutTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(checkOutTime, result.CheckOutTime);
        Assert.Equal(9.0, result.CalculateWorkHours());

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(e => e.PublishAsync("attendance:checkout", It.IsAny<CheckOutRecordedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckOutAsync_WhenNoCheckInRecord_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkOutTime = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
        var workDate = checkOutTime.Date;

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CheckOutAsync(employeeId, checkOutTime));
    }

    [Fact]
    public async Task CheckOutAsync_WhenEmptyEmployeeId_ShouldThrowArgumentException()
    {
        // Arrange
        var checkOutTime = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CheckOutAsync(Guid.Empty, checkOutTime));
    }

    [Fact]
    public async Task CheckOutAsync_WhenDuplicateCheckOut_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var checkOutTime = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
        var workDate = checkOutTime.Date;

        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        attendance.CheckIn(checkInTime);
        attendance.CheckOut(checkOutTime);

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendance);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CheckOutAsync(employeeId, checkOutTime.AddMinutes(30)));
    }

    [Fact]
    public async Task CheckInAsync_WhenExistingRecordWithoutCheckIn_ShouldUpdateExisting()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var workDate = checkInTime.Date;

        var existingAttendance = new Attendance(employeeId, workDate, AttendanceType.Normal);

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAndDateAsync(employeeId, workDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAttendance);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CheckInAsync(employeeId, checkInTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(checkInTime, result.CheckInTime);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(e => e.PublishAsync("attendance:checkin", It.IsAny<CheckInRecordedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
