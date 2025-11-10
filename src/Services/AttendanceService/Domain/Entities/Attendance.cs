using AttendanceService.Domain.Enums;

namespace AttendanceService.Domain.Entities;

/// <summary>
/// 勤怠記録エンティティ
/// </summary>
public class Attendance
{
    /// <summary>
    /// 勤怠記録ID
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// 従業員ID
    /// </summary>
    public Guid EmployeeId { get; private set; }

    /// <summary>
    /// 勤務日
    /// </summary>
    public DateTime WorkDate { get; private set; }

    /// <summary>
    /// 出勤時刻
    /// </summary>
    public DateTime? CheckInTime { get; private set; }

    /// <summary>
    /// 退勤時刻
    /// </summary>
    public DateTime? CheckOutTime { get; private set; }

    /// <summary>
    /// 勤怠種別
    /// </summary>
    public AttendanceType Type { get; private set; }

    /// <summary>
    /// 備考
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    private Attendance()
    {
    }

    public Attendance(Guid employeeId, DateTime workDate, AttendanceType type, string? notes = null)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("従業員IDは必須です。", nameof(employeeId));

        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        WorkDate = workDate.Date; // 日付部分のみを保持
        Type = type;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        ValidateAttendance();
    }

    /// <summary>
    /// 出勤を記録
    /// </summary>
    public void CheckIn(DateTime checkInTime)
    {
        if (CheckInTime.HasValue)
            throw new InvalidOperationException("既に出勤記録が存在します。");

        if (checkInTime.Date != WorkDate.Date)
            throw new ArgumentException("出勤時刻は勤務日と同じ日付である必要があります。", nameof(checkInTime));

        CheckInTime = checkInTime;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 退勤を記録
    /// </summary>
    public void CheckOut(DateTime checkOutTime)
    {
        if (!CheckInTime.HasValue)
            throw new InvalidOperationException("出勤記録がありません。");

        if (CheckOutTime.HasValue)
            throw new InvalidOperationException("既に退勤記録が存在します。");

        if (checkOutTime <= CheckInTime.Value)
            throw new ArgumentException("退勤時刻は出勤時刻より後である必要があります。", nameof(checkOutTime));

        CheckOutTime = checkOutTime;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 勤怠記録を更新
    /// </summary>
    public void Update(AttendanceType type, string? notes = null)
    {
        Type = type;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;

        ValidateAttendance();
    }

    /// <summary>
    /// 労働時間を計算（時間単位）
    /// </summary>
    public double? CalculateWorkHours()
    {
        if (!CheckInTime.HasValue || !CheckOutTime.HasValue)
            return null;

        var timeSpan = CheckOutTime.Value - CheckInTime.Value;
        return timeSpan.TotalHours;
    }

    private void ValidateAttendance()
    {
        if (WorkDate > DateTime.UtcNow.Date)
            throw new ArgumentException("勤務日は現在または過去の日付である必要があります。", nameof(WorkDate));

        if (!string.IsNullOrEmpty(Notes) && Notes.Length > 500)
            throw new ArgumentException("備考は500文字以内で入力してください。", nameof(Notes));
    }
}
