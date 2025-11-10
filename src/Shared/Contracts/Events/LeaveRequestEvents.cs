namespace Shared.Contracts.Events;

/// <summary>
/// 休暇申請作成イベント
/// </summary>
public record LeaveRequestCreatedEvent
{
    public Guid LeaveRequestId { get; init; }
    public Guid EmployeeId { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// 休暇申請承認イベント
/// </summary>
public record LeaveRequestApprovedEvent
{
    public Guid LeaveRequestId { get; init; }
    public Guid EmployeeId { get; init; }
    public Guid ApproverId { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string? ApproverComment { get; init; }
    public DateTime ApprovedAt { get; init; }
}

/// <summary>
/// 休暇申請却下イベント
/// </summary>
public record LeaveRequestRejectedEvent
{
    public Guid LeaveRequestId { get; init; }
    public Guid EmployeeId { get; init; }
    public Guid ApproverId { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string? ApproverComment { get; init; }
    public DateTime RejectedAt { get; init; }
}

/// <summary>
/// 休暇申請キャンセルイベント
/// </summary>
public record LeaveRequestCancelledEvent
{
    public Guid LeaveRequestId { get; init; }
    public Guid EmployeeId { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime CancelledAt { get; init; }
}
