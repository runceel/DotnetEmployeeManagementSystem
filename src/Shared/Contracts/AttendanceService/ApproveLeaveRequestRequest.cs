namespace Shared.Contracts.AttendanceService;

/// <summary>
/// 休暇申請承認リクエスト
/// </summary>
public record ApproveLeaveRequestRequest
{
    public Guid ApproverId { get; init; }
    public string? Comment { get; init; }
}
