namespace Shared.Contracts.AttendanceService;

/// <summary>
/// 退勤記録リクエスト
/// </summary>
public record CheckOutRequest
{
    public DateTime CheckOutTime { get; init; }
}
