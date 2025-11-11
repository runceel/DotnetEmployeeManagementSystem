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

    /// <summary>
    /// 勤怠記録を取得
    /// </summary>
    Task<AttendanceDto> GetAttendanceByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 勤怠記録を作成
    /// </summary>
    Task<AttendanceDto> CreateAttendanceAsync(
        CreateAttendanceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 勤怠記録を更新
    /// </summary>
    Task<AttendanceDto> UpdateAttendanceAsync(
        Guid id,
        UpdateAttendanceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 勤怠記録を削除
    /// </summary>
    Task DeleteAttendanceAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
