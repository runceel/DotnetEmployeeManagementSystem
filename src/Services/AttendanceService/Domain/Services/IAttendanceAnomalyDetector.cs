namespace AttendanceService.Domain.Services;

/// <summary>
/// 勤怠異常検知サービスインターフェース
/// </summary>
public interface IAttendanceAnomalyDetector
{
    /// <summary>
    /// 遅刻を検知
    /// </summary>
    /// <param name="checkInTime">出勤時刻</param>
    /// <returns>遅刻の場合true</returns>
    bool IsLateArrival(DateTime checkInTime);

    /// <summary>
    /// 早退を検知
    /// </summary>
    /// <param name="checkInTime">出勤時刻</param>
    /// <param name="checkOutTime">退勤時刻</param>
    /// <returns>早退の場合true</returns>
    bool IsEarlyLeaving(DateTime checkInTime, DateTime checkOutTime);

    /// <summary>
    /// 長時間労働を検知
    /// </summary>
    /// <param name="workHours">勤務時間（時間単位）</param>
    /// <returns>長時間労働の場合true</returns>
    bool IsOvertime(double workHours);

    /// <summary>
    /// 遅刻時間を計算（分単位）
    /// </summary>
    /// <param name="checkInTime">出勤時刻</param>
    /// <returns>遅刻時間（分）、遅刻していない場合は0</returns>
    int CalculateLateMinutes(DateTime checkInTime);

    /// <summary>
    /// 超過労働時間を計算（時間単位）
    /// </summary>
    /// <param name="workHours">勤務時間（時間単位）</param>
    /// <returns>超過労働時間（時間）、超過していない場合は0</returns>
    double CalculateOvertimeHours(double workHours);
}
