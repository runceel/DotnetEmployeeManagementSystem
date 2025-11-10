using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using Shared.Contracts.Events;

namespace AttendanceService.Application.Services;

/// <summary>
/// 休暇申請サービス実装
/// </summary>
public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IEventPublisher _eventPublisher;

    public LeaveRequestService(
        ILeaveRequestRepository leaveRequestRepository,
        IEventPublisher eventPublisher)
    {
        _leaveRequestRepository = leaveRequestRepository ?? throw new ArgumentNullException(nameof(leaveRequestRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    /// <summary>
    /// 休暇申請を作成
    /// </summary>
    public async Task<LeaveRequest> CreateLeaveRequestAsync(
        Guid employeeId,
        LeaveType type,
        DateTime startDate,
        DateTime endDate,
        string reason,
        CancellationToken cancellationToken = default)
    {
        // 重複チェック
        var overlappingRequests = await _leaveRequestRepository.GetOverlappingRequestsAsync(
            employeeId,
            startDate,
            endDate,
            cancellationToken);

        if (overlappingRequests.Any())
        {
            throw new InvalidOperationException("指定された期間に既に休暇申請が存在します。");
        }

        // 休暇申請作成
        var leaveRequest = new LeaveRequest(employeeId, type, startDate, endDate, reason);
        await _leaveRequestRepository.AddAsync(leaveRequest, cancellationToken);

        // イベント発行
        var @event = new LeaveRequestCreatedEvent
        {
            LeaveRequestId = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            Type = leaveRequest.Type.ToString(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Reason = leaveRequest.Reason,
            Status = leaveRequest.Status.ToString(),
            CreatedAt = leaveRequest.CreatedAt
        };

        await _eventPublisher.PublishAsync("leave-requests", @event, cancellationToken);

        return leaveRequest;
    }

    /// <summary>
    /// 休暇申請をIDで取得
    /// </summary>
    public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _leaveRequestRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// 従業員の休暇申請を取得
    /// </summary>
    public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _leaveRequestRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
    }

    /// <summary>
    /// ステータス別の休暇申請を取得
    /// </summary>
    public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByStatusAsync(
        LeaveRequestStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _leaveRequestRepository.GetByStatusAsync(status, cancellationToken);
    }

    /// <summary>
    /// 全休暇申請を取得
    /// </summary>
    public async Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync(CancellationToken cancellationToken = default)
    {
        return await _leaveRequestRepository.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// 休暇申請を承認
    /// </summary>
    public async Task<LeaveRequest> ApproveLeaveRequestAsync(
        Guid id,
        Guid approverId,
        string? comment = null,
        CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (leaveRequest == null)
        {
            throw new InvalidOperationException("休暇申請が見つかりません。");
        }

        leaveRequest.Approve(approverId, comment);
        await _leaveRequestRepository.UpdateAsync(leaveRequest, cancellationToken);

        // イベント発行
        var @event = new LeaveRequestApprovedEvent
        {
            LeaveRequestId = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            ApproverId = leaveRequest.ApproverId!.Value,
            Type = leaveRequest.Type.ToString(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            ApproverComment = leaveRequest.ApproverComment,
            ApprovedAt = leaveRequest.ApprovedAt!.Value
        };

        await _eventPublisher.PublishAsync("leave-requests", @event, cancellationToken);

        return leaveRequest;
    }

    /// <summary>
    /// 休暇申請を却下
    /// </summary>
    public async Task<LeaveRequest> RejectLeaveRequestAsync(
        Guid id,
        Guid approverId,
        string? comment = null,
        CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (leaveRequest == null)
        {
            throw new InvalidOperationException("休暇申請が見つかりません。");
        }

        leaveRequest.Reject(approverId, comment);
        await _leaveRequestRepository.UpdateAsync(leaveRequest, cancellationToken);

        // イベント発行
        var @event = new LeaveRequestRejectedEvent
        {
            LeaveRequestId = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            ApproverId = leaveRequest.ApproverId!.Value,
            Type = leaveRequest.Type.ToString(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            ApproverComment = leaveRequest.ApproverComment,
            RejectedAt = leaveRequest.ApprovedAt!.Value
        };

        await _eventPublisher.PublishAsync("leave-requests", @event, cancellationToken);

        return leaveRequest;
    }

    /// <summary>
    /// 休暇申請をキャンセル
    /// </summary>
    public async Task<LeaveRequest> CancelLeaveRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (leaveRequest == null)
        {
            throw new InvalidOperationException("休暇申請が見つかりません。");
        }

        leaveRequest.Cancel();
        await _leaveRequestRepository.UpdateAsync(leaveRequest, cancellationToken);

        // イベント発行
        var @event = new LeaveRequestCancelledEvent
        {
            LeaveRequestId = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            Type = leaveRequest.Type.ToString(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            CancelledAt = leaveRequest.UpdatedAt
        };

        await _eventPublisher.PublishAsync("leave-requests", @event, cancellationToken);

        return leaveRequest;
    }
}
