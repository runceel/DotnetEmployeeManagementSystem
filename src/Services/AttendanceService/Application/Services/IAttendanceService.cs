using AttendanceService.Domain.Entities;

namespace AttendanceService.Application.Services;

/// <summary>
/// 勤怠サービスインターフェース
/// </summary>
public interface IAttendanceService
{
    /// <summary>
    /// 出勤を記録
    /// </summary>
    Task<Attendance> CheckInAsync(Guid employeeId, DateTime checkInTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// 退勤を記録
    /// </summary>
    Task<Attendance> CheckOutAsync(Guid employeeId, DateTime checkOutTime, CancellationToken cancellationToken = default);
}
