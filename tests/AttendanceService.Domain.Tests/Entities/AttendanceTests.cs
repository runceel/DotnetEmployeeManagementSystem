using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;
using Xunit;

namespace AttendanceService.Domain.Tests.Entities;

public class AttendanceTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAttendance()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var type = AttendanceType.Normal;
        var notes = "通常勤務";

        // Act
        var attendance = new Attendance(employeeId, workDate, type, notes);

        // Assert
        Assert.NotEqual(Guid.Empty, attendance.Id);
        Assert.Equal(employeeId, attendance.EmployeeId);
        Assert.Equal(workDate.Date, attendance.WorkDate);
        Assert.Equal(type, attendance.Type);
        Assert.Equal(notes, attendance.Notes);
        Assert.Null(attendance.CheckInTime);
        Assert.Null(attendance.CheckOutTime);
        Assert.True(attendance.CreatedAt <= DateTime.UtcNow);
        Assert.True(attendance.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithEmptyEmployeeId_ShouldThrowArgumentException()
    {
        // Arrange
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Attendance(Guid.Empty, workDate, AttendanceType.Normal));
    }

    [Fact]
    public void CheckIn_WhenNotCheckedIn_ShouldRecordCheckIn()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);

        // Act
        attendance.CheckIn(checkInTime);

        // Assert
        Assert.Equal(checkInTime, attendance.CheckInTime);
    }

    [Fact]
    public void CheckIn_WhenAlreadyCheckedIn_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        attendance.CheckIn(checkInTime);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            attendance.CheckIn(checkInTime.AddMinutes(10)));
    }

    [Fact]
    public void CheckIn_WithDifferentDate_ShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var wrongDateCheckIn = new DateTime(2024, 1, 16, 9, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            attendance.CheckIn(wrongDateCheckIn));
    }

    [Fact]
    public void CheckOut_WhenCheckedIn_ShouldRecordCheckOut()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var checkOutTime = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
        attendance.CheckIn(checkInTime);

        // Act
        attendance.CheckOut(checkOutTime);

        // Assert
        Assert.Equal(checkOutTime, attendance.CheckOutTime);
    }

    [Fact]
    public void CheckOut_WhenNotCheckedIn_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var checkOutTime = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            attendance.CheckOut(checkOutTime));
    }

    [Fact]
    public void CheckOut_BeforeCheckIn_ShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var checkOutTime = new DateTime(2024, 1, 15, 8, 0, 0, DateTimeKind.Utc);
        attendance.CheckIn(checkInTime);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            attendance.CheckOut(checkOutTime));
    }

    [Fact]
    public void CalculateWorkHours_WithCheckInAndOut_ShouldReturnCorrectHours()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var checkOutTime = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
        attendance.CheckIn(checkInTime);
        attendance.CheckOut(checkOutTime);

        // Act
        var workHours = attendance.CalculateWorkHours();

        // Assert
        Assert.NotNull(workHours);
        Assert.Equal(9.0, workHours.Value);
    }

    [Fact]
    public void CalculateWorkHours_WithoutCheckOut_ShouldReturnNull()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        attendance.CheckIn(checkInTime);

        // Act
        var workHours = attendance.CalculateWorkHours();

        // Assert
        Assert.Null(workHours);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateAttendance()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var newType = AttendanceType.Remote;
        var newNotes = "リモートワークに変更";

        // Act
        attendance.Update(newType, newNotes);

        // Assert
        Assert.Equal(newType, attendance.Type);
        Assert.Equal(newNotes, attendance.Notes);
    }

    [Fact]
    public void Constructor_WithFutureWorkDate_ShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.Date.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Attendance(employeeId, futureDate, AttendanceType.Normal));
    }

    [Fact]
    public void Constructor_WithNotesOver500Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var longNotes = new string('あ', 501);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Attendance(employeeId, workDate, AttendanceType.Normal, longNotes));
    }

    [Fact]
    public void Constructor_WithNotesExactly500Characters_ShouldCreateAttendance()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var exactNotes = new string('あ', 500);

        // Act
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal, exactNotes);

        // Assert
        Assert.Equal(exactNotes, attendance.Notes);
    }

    [Fact]
    public void Update_WithNotesOver500Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var longNotes = new string('あ', 501);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            attendance.Update(AttendanceType.Remote, longNotes));
    }

    [Fact]
    public void CheckOut_WhenAlreadyCheckedOut_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        var checkOutTime = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
        attendance.CheckIn(checkInTime);
        attendance.CheckOut(checkOutTime);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            attendance.CheckOut(checkOutTime.AddMinutes(30)));
    }

    [Fact]
    public void CheckOut_AtExactSameTimeAsCheckIn_ShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        var checkInTime = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        attendance.CheckIn(checkInTime);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            attendance.CheckOut(checkInTime));
    }

    [Fact]
    public void CalculateWorkHours_WithoutCheckIn_ShouldReturnNull()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);

        // Act
        var workHours = attendance.CalculateWorkHours();

        // Assert
        Assert.Null(workHours);
    }

    [Fact]
    public void Constructor_WithNullNotes_ShouldCreateAttendance()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal, null);

        // Assert
        Assert.Null(attendance.Notes);
    }

    [Fact]
    public void Constructor_ShouldStoreOnlyDatePartOfWorkDate()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDateWithTime = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc);

        // Act
        var attendance = new Attendance(employeeId, workDateWithTime, AttendanceType.Normal);

        // Assert
        Assert.Equal(workDateWithTime.Date, attendance.WorkDate);
        Assert.Equal(0, attendance.WorkDate.Hour);
        Assert.Equal(0, attendance.WorkDate.Minute);
        Assert.Equal(0, attendance.WorkDate.Second);
    }

    [Fact]
    public void Update_WithNullNotes_ShouldClearNotes()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal, "初期の備考");

        // Act
        attendance.Update(AttendanceType.Remote, null);

        // Assert
        Assert.Null(attendance.Notes);
    }

    [Fact]
    public void Constructor_WithRemoteType_ShouldCreateAttendance()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Remote);

        // Assert
        Assert.Equal(AttendanceType.Remote, attendance.Type);
    }

    [Fact]
    public void Constructor_WithBusinessTripType_ShouldCreateAttendance()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var attendance = new Attendance(employeeId, workDate, AttendanceType.BusinessTrip);

        // Assert
        Assert.Equal(AttendanceType.BusinessTrip, attendance.Type);
    }

    [Fact]
    public void Constructor_WithHalfDayType_ShouldCreateAttendance()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var workDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var attendance = new Attendance(employeeId, workDate, AttendanceType.HalfDay);

        // Assert
        Assert.Equal(AttendanceType.HalfDay, attendance.Type);
    }
}
