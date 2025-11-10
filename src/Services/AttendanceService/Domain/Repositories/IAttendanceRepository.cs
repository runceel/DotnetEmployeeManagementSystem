using AttendanceService.Domain.Entities;

namespace AttendanceService.Domain.Repositories;

/// <summary>
/// 勤怠記録リポジトリインターフェース
/// </summary>
public interface IAttendanceRepository
{
    /// <summary>
    /// 勤怠記録を取得
    /// </summary>
    Task<Attendance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 全勤怠記録を取得
    /// </summary>
    Task<IEnumerable<Attendance>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員の勤怠記録を取得
    /// </summary>
    Task<IEnumerable<Attendance>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員の特定期間の勤怠記録を取得
    /// </summary>
    Task<IEnumerable<Attendance>> GetByEmployeeIdAndDateRangeAsync(
        Guid employeeId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 特定日の勤怠記録を取得
    /// </summary>
    Task<Attendance?> GetByEmployeeIdAndDateAsync(
        Guid employeeId, 
        DateTime workDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 勤怠記録を追加
    /// </summary>
    Task<Attendance> AddAsync(Attendance attendance, CancellationToken cancellationToken = default);

    /// <summary>
    /// 勤怠記録を更新
    /// </summary>
    Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default);

    /// <summary>
    /// 勤怠記録を削除
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
