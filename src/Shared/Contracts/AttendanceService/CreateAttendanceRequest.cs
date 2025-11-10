namespace Shared.Contracts.AttendanceService;

/// <summary>
/// 勤怠記録作成リクエスト
/// </summary>
public record CreateAttendanceRequest
{
    public Guid EmployeeId { get; init; }
    public DateTime WorkDate { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
