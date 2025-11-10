using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;

namespace AttendanceService.Domain.Repositories;

/// <summary>
/// 休暇申請リポジトリインターフェース
/// </summary>
public interface ILeaveRequestRepository
{
    /// <summary>
    /// 休暇申請を取得
    /// </summary>
    Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 全休暇申請を取得
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員の休暇申請を取得
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// ステータス別の休暇申請を取得
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveRequestStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員の特定期間に重複する休暇申請を取得
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetOverlappingRequestsAsync(
        Guid employeeId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 休暇申請を追加
    /// </summary>
    Task<LeaveRequest> AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// 休暇申請を更新
    /// </summary>
    Task UpdateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// 休暇申請を削除
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
