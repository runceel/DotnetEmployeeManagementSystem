namespace Shared.Contracts.AttendanceService;

/// <summary>
/// 勤怠記録DTO
/// </summary>
public record AttendanceDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public DateTime WorkDate { get; init; }
    public DateTime? CheckInTime { get; init; }
    public DateTime? CheckOutTime { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public double? WorkHours { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
