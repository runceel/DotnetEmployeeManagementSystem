using AttendanceService.Domain.Enums;

namespace AttendanceService.Domain.Entities;

/// <summary>
/// 休暇申請エンティティ
/// </summary>
public class LeaveRequest
{
    /// <summary>
    /// 休暇申請ID
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// 従業員ID
    /// </summary>
    public Guid EmployeeId { get; private set; }

    /// <summary>
    /// 休暇種別
    /// </summary>
    public LeaveType Type { get; private set; }

    /// <summary>
    /// 開始日
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// 終了日
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// 申請理由
    /// </summary>
    public string Reason { get; private set; }

    /// <summary>
    /// ステータス
    /// </summary>
    public LeaveRequestStatus Status { get; private set; }

    /// <summary>
    /// 承認者ID
    /// </summary>
    public Guid? ApproverId { get; private set; }

    /// <summary>
    /// 承認日時
    /// </summary>
    public DateTime? ApprovedAt { get; private set; }

    /// <summary>
    /// 承認者コメント
    /// </summary>
    public string? ApproverComment { get; private set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    private LeaveRequest()
    {
        Reason = string.Empty;
    }

    public LeaveRequest(Guid employeeId, LeaveType type, DateTime startDate, DateTime endDate, string reason)
    {
        ArgumentNullException.ThrowIfNull(reason);

        if (employeeId == Guid.Empty)
            throw new ArgumentException("従業員IDは必須です。", nameof(employeeId));

        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        Type = type;
        StartDate = startDate.Date;
        EndDate = endDate.Date;
        Reason = reason;
        Status = LeaveRequestStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        ValidateLeaveRequest();
    }

    /// <summary>
    /// 休暇申請を承認
    /// </summary>
    public void Approve(Guid approverId, string? comment = null)
    {
        if (approverId == Guid.Empty)
            throw new ArgumentException("承認者IDは必須です。", nameof(approverId));

        if (Status != LeaveRequestStatus.Pending)
            throw new InvalidOperationException("申請中のステータスのみ承認できます。");

        Status = LeaveRequestStatus.Approved;
        ApproverId = approverId;
        ApprovedAt = DateTime.UtcNow;
        ApproverComment = comment;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 休暇申請を却下
    /// </summary>
    public void Reject(Guid approverId, string? comment = null)
    {
        if (approverId == Guid.Empty)
            throw new ArgumentException("承認者IDは必須です。", nameof(approverId));

        if (Status != LeaveRequestStatus.Pending)
            throw new InvalidOperationException("申請中のステータスのみ却下できます。");

        Status = LeaveRequestStatus.Rejected;
        ApproverId = approverId;
        ApprovedAt = DateTime.UtcNow;
        ApproverComment = comment;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 休暇申請をキャンセル
    /// </summary>
    public void Cancel()
    {
        if (Status == LeaveRequestStatus.Cancelled)
            throw new InvalidOperationException("既にキャンセル済みです。");

        if (Status == LeaveRequestStatus.Rejected)
            throw new InvalidOperationException("却下された申請はキャンセルできません。");

        Status = LeaveRequestStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 休暇日数を計算
    /// </summary>
    public int CalculateDays()
    {
        return (EndDate - StartDate).Days + 1;
    }

    private void ValidateLeaveRequest()
    {
        if (StartDate > EndDate)
            throw new ArgumentException("開始日は終了日以前である必要があります。");

        if (EndDate < DateTime.UtcNow.Date)
            throw new ArgumentException("終了日は現在または未来の日付である必要があります。");

        if (string.IsNullOrWhiteSpace(Reason))
            throw new ArgumentException("申請理由を入力してください。", nameof(Reason));

        if (Reason.Length > 1000)
            throw new ArgumentException("申請理由は1000文字以内で入力してください。", nameof(Reason));
    }
}
