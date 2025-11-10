namespace Shared.Contracts.AttendanceService;

/// <summary>
/// 出勤記録リクエスト
/// </summary>
public record CheckInRequest
{
    public Guid EmployeeId { get; init; }
    public DateTime CheckInTime { get; init; }
}
