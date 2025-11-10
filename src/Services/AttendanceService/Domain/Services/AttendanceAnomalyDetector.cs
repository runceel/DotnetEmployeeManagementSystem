namespace AttendanceService.Domain.Services;

/// <summary>
/// 勤怠異常検知サービス実装
/// </summary>
public class AttendanceAnomalyDetector : IAttendanceAnomalyDetector
{
    // 標準開始時刻: 9:00
    private static readonly TimeSpan StandardStartTime = new(9, 0, 0);
    
    // 標準終了時刻: 17:00
    private static readonly TimeSpan StandardEndTime = new(17, 0, 0);
    
    // 最小勤務時間（早退判定用）: 4時間
    private const double MinimumWorkHours = 4.0;
    
    // 標準勤務時間: 8時間
    private const double StandardWorkHours = 8.0;
    
    // 長時間労働の閾値: 10時間
    private const double OvertimeThreshold = 10.0;

    /// <summary>
    /// 遅刻を検知
    /// </summary>
    public bool IsLateArrival(DateTime checkInTime)
    {
        var checkInTimeOfDay = checkInTime.TimeOfDay;
        return checkInTimeOfDay > StandardStartTime;
    }

    /// <summary>
    /// 早退を検知
    /// </summary>
    public bool IsEarlyLeaving(DateTime checkInTime, DateTime checkOutTime)
    {
        // 勤務時間が最小勤務時間以上でないと早退とは判定しない
        var workHours = (checkOutTime - checkInTime).TotalHours;
        if (workHours < MinimumWorkHours)
        {
            return false;
        }

        var checkOutTimeOfDay = checkOutTime.TimeOfDay;
        return checkOutTimeOfDay < StandardEndTime;
    }

    /// <summary>
    /// 長時間労働を検知
    /// </summary>
    public bool IsOvertime(double workHours)
    {
        return workHours >= OvertimeThreshold;
    }

    /// <summary>
    /// 遅刻時間を計算（分単位）
    /// </summary>
    public int CalculateLateMinutes(DateTime checkInTime)
    {
        if (!IsLateArrival(checkInTime))
        {
            return 0;
        }

        var checkInTimeOfDay = checkInTime.TimeOfDay;
        var lateTime = checkInTimeOfDay - StandardStartTime;
        return (int)lateTime.TotalMinutes;
    }

    /// <summary>
    /// 超過労働時間を計算（時間単位）
    /// </summary>
    public double CalculateOvertimeHours(double workHours)
    {
        if (!IsOvertime(workHours))
        {
            return 0;
        }

        return Math.Round(workHours - StandardWorkHours, 2);
    }
}
