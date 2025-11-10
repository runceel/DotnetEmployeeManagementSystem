using AttendanceService.Application.Services;
using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using Moq;
using Shared.Contracts.Events;

namespace AttendanceService.Application.Tests.Services;

public class LeaveRequestServiceTests
{
    private readonly Mock<ILeaveRequestRepository> _mockRepository;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly LeaveRequestService _service;

    public LeaveRequestServiceTests()
    {
        _mockRepository = new Mock<ILeaveRequestRepository>();
        _mockEventPublisher = new Mock<IEventPublisher>();
        _service = new LeaveRequestService(_mockRepository.Object, _mockEventPublisher.Object);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_WhenNoOverlap_ShouldCreateLeaveRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);
        var reason = "有給休暇";

        _mockRepository
            .Setup(r => r.GetOverlappingRequestsAsync(employeeId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LeaveRequest>());

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveRequest lr, CancellationToken _) => lr);

        // Act
        var result = await _service.CreateLeaveRequestAsync(
            employeeId,
            LeaveType.PaidLeave,
            startDate,
            endDate,
            reason);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(LeaveType.PaidLeave, result.Type);
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
        Assert.Equal(reason, result.Reason);
        Assert.Equal(LeaveRequestStatus.Pending, result.Status);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(e => e.PublishAsync("leaverequest:created", It.IsAny<LeaveRequestCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_WhenOverlapExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);
        var reason = "有給休暇";

        var existingRequest = new LeaveRequest(employeeId, LeaveType.PaidLeave, startDate, endDate, "既存の申請");

        _mockRepository
            .Setup(r => r.GetOverlappingRequestsAsync(employeeId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LeaveRequest> { existingRequest });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateLeaveRequestAsync(employeeId, LeaveType.PaidLeave, startDate, endDate, reason));
    }

    [Fact]
    public async Task GetLeaveRequestByIdAsync_WhenExists_ShouldReturnLeaveRequest()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var leaveRequest = new LeaveRequest(
            employeeId,
            LeaveType.PaidLeave,
            DateTime.UtcNow.Date.AddDays(1),
            DateTime.UtcNow.Date.AddDays(3),
            "有給休暇");

        _mockRepository
            .Setup(r => r.GetByIdAsync(leaveRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);

        // Act
        var result = await _service.GetLeaveRequestByIdAsync(leaveRequestId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
    }

    [Fact]
    public async Task GetLeaveRequestByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetByIdAsync(leaveRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveRequest?)null);

        // Act
        var result = await _service.GetLeaveRequestByIdAsync(leaveRequestId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_WhenPending_ShouldApproveAndPublishEvent()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var comment = "承認します";

        var leaveRequest = new LeaveRequest(
            employeeId,
            LeaveType.PaidLeave,
            DateTime.UtcNow.Date.AddDays(1),
            DateTime.UtcNow.Date.AddDays(3),
            "有給休暇");

        _mockRepository
            .Setup(r => r.GetByIdAsync(leaveRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ApproveLeaveRequestAsync(leaveRequestId, approverId, comment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(LeaveRequestStatus.Approved, result.Status);
        Assert.Equal(approverId, result.ApproverId);
        Assert.Equal(comment, result.ApproverComment);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(e => e.PublishAsync("leaverequest:approved", It.IsAny<LeaveRequestApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_WhenNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetByIdAsync(leaveRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveRequest?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ApproveLeaveRequestAsync(leaveRequestId, approverId));
    }

    [Fact]
    public async Task RejectLeaveRequestAsync_WhenPending_ShouldRejectAndPublishEvent()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var comment = "業務都合により却下";

        var leaveRequest = new LeaveRequest(
            employeeId,
            LeaveType.PaidLeave,
            DateTime.UtcNow.Date.AddDays(1),
            DateTime.UtcNow.Date.AddDays(3),
            "有給休暇");

        _mockRepository
            .Setup(r => r.GetByIdAsync(leaveRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RejectLeaveRequestAsync(leaveRequestId, approverId, comment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(LeaveRequestStatus.Rejected, result.Status);
        Assert.Equal(approverId, result.ApproverId);
        Assert.Equal(comment, result.ApproverComment);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(e => e.PublishAsync("leaverequest:rejected", It.IsAny<LeaveRequestRejectedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectLeaveRequestAsync_WhenNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetByIdAsync(leaveRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveRequest?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RejectLeaveRequestAsync(leaveRequestId, approverId));
    }

    [Fact]
    public async Task CancelLeaveRequestAsync_WhenPending_ShouldCancelAndPublishEvent()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var leaveRequest = new LeaveRequest(
            employeeId,
            LeaveType.PaidLeave,
            DateTime.UtcNow.Date.AddDays(1),
            DateTime.UtcNow.Date.AddDays(3),
            "有給休暇");

        _mockRepository
            .Setup(r => r.GetByIdAsync(leaveRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CancelLeaveRequestAsync(leaveRequestId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(LeaveRequestStatus.Cancelled, result.Status);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(e => e.PublishAsync("leaverequest:cancelled", It.IsAny<LeaveRequestCancelledEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelLeaveRequestAsync_WhenNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetByIdAsync(leaveRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveRequest?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CancelLeaveRequestAsync(leaveRequestId));
    }

    [Fact]
    public async Task GetLeaveRequestsByEmployeeIdAsync_ShouldReturnEmployeeLeaveRequests()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var leaveRequests = new List<LeaveRequest>
        {
            new LeaveRequest(employeeId, LeaveType.PaidLeave, DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(2), "理由1"),
            new LeaveRequest(employeeId, LeaveType.SickLeave, DateTime.UtcNow.Date.AddDays(5), DateTime.UtcNow.Date.AddDays(6), "理由2")
        };

        _mockRepository
            .Setup(r => r.GetByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequests);

        // Act
        var result = await _service.GetLeaveRequestsByEmployeeIdAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetLeaveRequestsByStatusAsync_ShouldReturnFilteredLeaveRequests()
    {
        // Arrange
        var status = LeaveRequestStatus.Pending;
        var leaveRequests = new List<LeaveRequest>
        {
            new LeaveRequest(Guid.NewGuid(), LeaveType.PaidLeave, DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(2), "理由1"),
            new LeaveRequest(Guid.NewGuid(), LeaveType.SickLeave, DateTime.UtcNow.Date.AddDays(5), DateTime.UtcNow.Date.AddDays(6), "理由2")
        };

        _mockRepository
            .Setup(r => r.GetByStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequests);

        // Act
        var result = await _service.GetLeaveRequestsByStatusAsync(status);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllLeaveRequestsAsync_ShouldReturnAllLeaveRequests()
    {
        // Arrange
        var leaveRequests = new List<LeaveRequest>
        {
            new LeaveRequest(Guid.NewGuid(), LeaveType.PaidLeave, DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(2), "理由1"),
            new LeaveRequest(Guid.NewGuid(), LeaveType.SickLeave, DateTime.UtcNow.Date.AddDays(5), DateTime.UtcNow.Date.AddDays(6), "理由2"),
            new LeaveRequest(Guid.NewGuid(), LeaveType.SpecialLeave, DateTime.UtcNow.Date.AddDays(10), DateTime.UtcNow.Date.AddDays(11), "理由3")
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequests);

        // Act
        var result = await _service.GetAllLeaveRequestsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }
}
