namespace Shared.Contracts.AttendanceService;

/// <summary>
/// 勤怠記録更新リクエスト
/// </summary>
public record UpdateAttendanceRequest
{
    public string Type { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
