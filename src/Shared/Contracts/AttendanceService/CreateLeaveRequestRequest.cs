namespace Shared.Contracts.AttendanceService;

/// <summary>
/// 休暇申請作成リクエスト
/// </summary>
public record CreateLeaveRequestRequest
{
    public Guid EmployeeId { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Reason { get; init; } = string.Empty;
}
