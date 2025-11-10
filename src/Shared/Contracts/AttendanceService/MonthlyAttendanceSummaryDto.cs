namespace Shared.Contracts.AttendanceService;

/// <summary>
/// 月次勤怠集計DTO
/// </summary>
public record MonthlyAttendanceSummaryDto
{
    /// <summary>
    /// 従業員ID
    /// </summary>
    public Guid EmployeeId { get; init; }

    /// <summary>
    /// 対象年
    /// </summary>
    public int Year { get; init; }

    /// <summary>
    /// 対象月
    /// </summary>
    public int Month { get; init; }

    /// <summary>
    /// 総出勤日数
    /// </summary>
    public int TotalWorkDays { get; init; }

    /// <summary>
    /// 総勤務時間
    /// </summary>
    public double TotalWorkHours { get; init; }

    /// <summary>
    /// 平均勤務時間
    /// </summary>
    public double AverageWorkHours { get; init; }

    /// <summary>
    /// 遅刻回数
    /// </summary>
    public int LateDays { get; init; }

    /// <summary>
    /// 欠勤日数
    /// </summary>
    public int AbsentDays { get; init; }

    /// <summary>
    /// 有給休暇取得日数
    /// </summary>
    public int PaidLeaveDays { get; init; }

    /// <summary>
    /// 詳細な勤怠記録リスト
    /// </summary>
    public IEnumerable<AttendanceDto> Attendances { get; init; } = [];
}
