using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;

namespace AttendanceService.Application.Services;

/// <summary>
/// 休暇申請サービスインターフェース
/// </summary>
public interface ILeaveRequestService
{
    /// <summary>
    /// 休暇申請を作成
    /// </summary>
    Task<LeaveRequest> CreateLeaveRequestAsync(
        Guid employeeId,
        LeaveType type,
        DateTime startDate,
        DateTime endDate,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 休暇申請をIDで取得
    /// </summary>
    Task<LeaveRequest?> GetLeaveRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員の休暇申請を取得
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// ステータス別の休暇申請を取得
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByStatusAsync(LeaveRequestStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 全休暇申請を取得
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 休暇申請を承認
    /// </summary>
    Task<LeaveRequest> ApproveLeaveRequestAsync(Guid id, Guid approverId, string? comment = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 休暇申請を却下
    /// </summary>
    Task<LeaveRequest> RejectLeaveRequestAsync(Guid id, Guid approverId, string? comment = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 休暇申請をキャンセル
    /// </summary>
    Task<LeaveRequest> CancelLeaveRequestAsync(Guid id, CancellationToken cancellationToken = default);
}
