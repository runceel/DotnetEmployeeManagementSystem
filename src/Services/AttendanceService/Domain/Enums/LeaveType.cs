namespace AttendanceService.Domain.Enums;

/// <summary>
/// 休暇種別
/// </summary>
public enum LeaveType
{
    /// <summary>
    /// 有給休暇
    /// </summary>
    PaidLeave = 0,

    /// <summary>
    /// 病気休暇
    /// </summary>
    SickLeave = 1,

    /// <summary>
    /// 特別休暇
    /// </summary>
    SpecialLeave = 2,

    /// <summary>
    /// 無給休暇
    /// </summary>
    UnpaidLeave = 3
}
