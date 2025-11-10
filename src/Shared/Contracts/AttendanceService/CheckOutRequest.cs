namespace Shared.Contracts.AttendanceService;

/// <summary>
/// 退勤記録リクエスト
/// </summary>
public record CheckOutRequest
{
    public Guid EmployeeId { get; init; }
    public DateTime CheckOutTime { get; init; }
}
