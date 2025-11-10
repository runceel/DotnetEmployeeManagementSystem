using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;
using Xunit;

namespace AttendanceService.Domain.Tests.Entities;

public class LeaveRequestTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateLeaveRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var type = LeaveType.PaidLeave;
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = DateTime.UtcNow.Date.AddDays(3);
        var reason = "有給休暇取得";

        // Act
        var leaveRequest = new LeaveRequest(employeeId, type, startDate, endDate, reason);

        // Assert
        Assert.NotEqual(Guid.Empty, leaveRequest.Id);
        Assert.Equal(employeeId, leaveRequest.EmployeeId);
        Assert.Equal(type, leaveRequest.Type);
        Assert.Equal(startDate.Date, leaveRequest.StartDate);
        Assert.Equal(endDate.Date, leaveRequest.EndDate);
        Assert.Equal(reason, leaveRequest.Reason);
        Assert.Equal(LeaveRequestStatus.Pending, leaveRequest.Status);
        Assert.Null(leaveRequest.ApproverId);
        Assert.Null(leaveRequest.ApprovedAt);
        Assert.True(leaveRequest.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithEmptyEmployeeId_ShouldThrowArgumentException()
    {
        // Arrange
        var startDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2024, 2, 3, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new LeaveRequest(Guid.Empty, LeaveType.PaidLeave, startDate, endDate, "理由"));
    }

    [Fact]
    public void Constructor_WithStartDateAfterEndDate_ShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var startDate = new DateTime(2024, 2, 5, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2024, 2, 3, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, "理由"));
    }

    [Fact]
    public void Constructor_WithEmptyReason_ShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, ""));
    }

    [Fact]
    public void Approve_WithValidParameters_ShouldApproveRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);
        var leaveRequest = new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, "有給休暇");
        var comment = "承認します";

        // Act
        leaveRequest.Approve(approverId, comment);

        // Assert
        Assert.Equal(LeaveRequestStatus.Approved, leaveRequest.Status);
        Assert.Equal(approverId, leaveRequest.ApproverId);
        Assert.NotNull(leaveRequest.ApprovedAt);
        Assert.Equal(comment, leaveRequest.ApproverComment);
    }

    [Fact]
    public void Approve_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);
        var leaveRequest = new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, "有給休暇");
        leaveRequest.Approve(approverId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            leaveRequest.Approve(Guid.NewGuid()));
    }

    [Fact]
    public void Reject_WithValidParameters_ShouldRejectRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);
        var leaveRequest = new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, "有給休暇");
        var comment = "業務都合により却下";

        // Act
        leaveRequest.Reject(approverId, comment);

        // Assert
        Assert.Equal(LeaveRequestStatus.Rejected, leaveRequest.Status);
        Assert.Equal(approverId, leaveRequest.ApproverId);
        Assert.NotNull(leaveRequest.ApprovedAt);
        Assert.Equal(comment, leaveRequest.ApproverComment);
    }

    [Fact]
    public void Reject_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);
        var leaveRequest = new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, "有給休暇");
        leaveRequest.Approve(approverId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            leaveRequest.Reject(Guid.NewGuid()));
    }

    [Fact]
    public void Cancel_WhenPending_ShouldCancelRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);
        var leaveRequest = new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, "有給休暇");

        // Act
        leaveRequest.Cancel();

        // Assert
        Assert.Equal(LeaveRequestStatus.Cancelled, leaveRequest.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);
        var leaveRequest = new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, "有給休暇");
        leaveRequest.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            leaveRequest.Cancel());
    }

    [Fact]
    public void Cancel_WhenRejected_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);
        var leaveRequest = new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, "有給休暇");
        leaveRequest.Reject(approverId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            leaveRequest.Cancel());
    }

    [Fact]
    public void CalculateDays_ShouldReturnCorrectNumberOfDays()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = DateTime.UtcNow.Date.AddDays(5);
        var leaveRequest = new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, "有給休暇");

        // Act
        var days = leaveRequest.CalculateDays();

        // Assert
        Assert.Equal(5, days);
    }

    [Fact]
    public void CalculateDays_ForSingleDay_ShouldReturnOne()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = DateTime.UtcNow.Date.AddDays(1);
        var leaveRequest = new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, "有給休暇");

        // Act
        var days = leaveRequest.CalculateDays();

        // Assert
        Assert.Equal(1, days);
    }
}
