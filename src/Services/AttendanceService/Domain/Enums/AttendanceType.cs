namespace AttendanceService.Domain.Enums;

/// <summary>
/// 勤怠種別
/// </summary>
public enum AttendanceType
{
    /// <summary>
    /// 通常勤務
    /// </summary>
    Normal = 0,

    /// <summary>
    /// リモートワーク
    /// </summary>
    Remote = 1,

    /// <summary>
    /// 出張
    /// </summary>
    BusinessTrip = 2,

    /// <summary>
    /// 半日勤務
    /// </summary>
    HalfDay = 3
}
