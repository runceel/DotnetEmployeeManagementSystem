using Shared.Contracts.AttendanceService;

namespace BlazorWeb.Services;

/// <summary>
/// AttendanceService APIクライアントのインターフェース
/// </summary>
public interface IAttendanceApiClient
{
    /// <summary>
    /// 従業員の勤怠履歴を取得
    /// </summary>
    Task<IEnumerable<AttendanceDto>> GetAttendancesByEmployeeAsync(
        Guid employeeId, 
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員の月次勤怠集計を取得
    /// </summary>
    Task<MonthlyAttendanceSummaryDto> GetMonthlyAttendanceSummaryAsync(
        Guid employeeId, 
        int year, 
        int month,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 出勤を記録
    /// </summary>
    Task<AttendanceDto> CheckInAsync(
        CheckInRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 退勤を記録
    /// </summary>
    Task<AttendanceDto> CheckOutAsync(
        CheckOutRequest request,
        CancellationToken cancellationToken = default);
}
