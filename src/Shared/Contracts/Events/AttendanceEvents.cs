namespace Shared.Contracts.Events;

/// <summary>
/// 勤怠記録作成イベント
/// </summary>
public record AttendanceCreatedEvent
{
    public Guid AttendanceId { get; init; }
    public Guid EmployeeId { get; init; }
    public DateTime WorkDate { get; init; }
    public DateTime? CheckInTime { get; init; }
    public DateTime? CheckOutTime { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// 勤怠記録更新イベント
/// </summary>
public record AttendanceUpdatedEvent
{
    public Guid AttendanceId { get; init; }
    public Guid EmployeeId { get; init; }
    public DateTime WorkDate { get; init; }
    public DateTime? CheckInTime { get; init; }
    public DateTime? CheckOutTime { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// 出勤記録イベント
/// </summary>
public record CheckInRecordedEvent
{
    public Guid AttendanceId { get; init; }
    public Guid EmployeeId { get; init; }
    public DateTime CheckInTime { get; init; }
    public DateTime WorkDate { get; init; }
}

/// <summary>
/// 退勤記録イベント
/// </summary>
public record CheckOutRecordedEvent
{
    public Guid AttendanceId { get; init; }
    public Guid EmployeeId { get; init; }
    public DateTime CheckOutTime { get; init; }
    public DateTime WorkDate { get; init; }
    public double WorkHours { get; init; }
}
