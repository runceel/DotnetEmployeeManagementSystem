namespace AttendanceService.Domain.Enums;

/// <summary>
/// 休暇申請ステータス
/// </summary>
public enum LeaveRequestStatus
{
    /// <summary>
    /// 申請中
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 承認済み
    /// </summary>
    Approved = 1,

    /// <summary>
    /// 却下
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// キャンセル
    /// </summary>
    Cancelled = 3
}
