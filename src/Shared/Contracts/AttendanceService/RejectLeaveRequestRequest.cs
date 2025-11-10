namespace Shared.Contracts.AttendanceService;

/// <summary>
/// 休暇申請却下リクエスト
/// </summary>
public record RejectLeaveRequestRequest
{
    public Guid ApproverId { get; init; }
    public string? Comment { get; init; }
}
