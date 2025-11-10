using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using AttendanceService.Domain.Services;
using Shared.Contracts.Events;

namespace AttendanceService.Application.Services;

/// <summary>
/// 勤怠サービス実装
/// </summary>
public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IAttendanceAnomalyDetector _anomalyDetector;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        IEventPublisher eventPublisher,
        IAttendanceAnomalyDetector anomalyDetector)
    {
        _attendanceRepository = attendanceRepository ?? throw new ArgumentNullException(nameof(attendanceRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _anomalyDetector = anomalyDetector ?? throw new ArgumentNullException(nameof(anomalyDetector));
    }

    /// <summary>
    /// 出勤を記録
    /// </summary>
    public async Task<Attendance> CheckInAsync(Guid employeeId, DateTime checkInTime, CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("従業員IDは必須です。", nameof(employeeId));

        var workDate = checkInTime.Date;

        // 同じ日の打刻が既に存在するかチェック
        var existingAttendance = await _attendanceRepository.GetByEmployeeIdAndDateAsync(
            employeeId, workDate, cancellationToken);

        if (existingAttendance != null)
        {
            // 既に出勤記録が存在する場合はエラー
            if (existingAttendance.CheckInTime.HasValue)
                throw new InvalidOperationException("既に出勤記録が存在します。");

            // 出勤記録がない場合は打刻
            existingAttendance.CheckIn(checkInTime);
            await _attendanceRepository.UpdateAsync(existingAttendance, cancellationToken);

            // イベント発行
            await _eventPublisher.PublishAsync("attendance:checkin", new CheckInRecordedEvent
            {
                AttendanceId = existingAttendance.Id,
                EmployeeId = existingAttendance.EmployeeId,
                CheckInTime = checkInTime,
                WorkDate = workDate
            }, cancellationToken);

            return existingAttendance;
        }

        // 新規勤怠記録を作成
        var attendance = new Attendance(employeeId, workDate, AttendanceType.Normal);
        attendance.CheckIn(checkInTime);
        
        var result = await _attendanceRepository.AddAsync(attendance, cancellationToken);

        // イベント発行
        await _eventPublisher.PublishAsync("attendance:checkin", new CheckInRecordedEvent
        {
            AttendanceId = result.Id,
            EmployeeId = result.EmployeeId,
            CheckInTime = checkInTime,
            WorkDate = workDate
        }, cancellationToken);

        // 遅刻検知
        if (_anomalyDetector.IsLateArrival(checkInTime))
        {
            var lateMinutes = _anomalyDetector.CalculateLateMinutes(checkInTime);
            await _eventPublisher.PublishAsync("attendance:late-arrival", new LateArrivalDetectedEvent
            {
                AttendanceId = result.Id,
                EmployeeId = result.EmployeeId,
                CheckInTime = checkInTime,
                WorkDate = workDate,
                LateMinutes = lateMinutes
            }, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// 退勤を記録
    /// </summary>
    public async Task<Attendance> CheckOutAsync(Guid employeeId, DateTime checkOutTime, CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("従業員IDは必須です。", nameof(employeeId));

        var workDate = checkOutTime.Date;

        // 当日の勤怠記録を取得
        var attendance = await _attendanceRepository.GetByEmployeeIdAndDateAsync(
            employeeId, workDate, cancellationToken);

        if (attendance == null)
            throw new InvalidOperationException("出勤記録が見つかりません。先に出勤打刻を行ってください。");

        // 退勤記録
        attendance.CheckOut(checkOutTime);
        await _attendanceRepository.UpdateAsync(attendance, cancellationToken);

        var workHours = attendance.CalculateWorkHours() ?? 0;

        // イベント発行
        await _eventPublisher.PublishAsync("attendance:checkout", new CheckOutRecordedEvent
        {
            AttendanceId = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            CheckOutTime = checkOutTime,
            WorkDate = workDate,
            WorkHours = workHours
        }, cancellationToken);

        // 早退検知
        if (_anomalyDetector.IsEarlyLeaving(attendance.CheckInTime!.Value, checkOutTime))
        {
            await _eventPublisher.PublishAsync("attendance:early-leaving", new EarlyLeavingDetectedEvent
            {
                AttendanceId = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                CheckInTime = attendance.CheckInTime!.Value,
                CheckOutTime = checkOutTime,
                WorkDate = workDate,
                WorkHours = workHours
            }, cancellationToken);
        }

        // 長時間労働検知
        if (_anomalyDetector.IsOvertime(workHours))
        {
            var overtimeHours = _anomalyDetector.CalculateOvertimeHours(workHours);
            await _eventPublisher.PublishAsync("attendance:overtime", new OvertimeDetectedEvent
            {
                AttendanceId = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                CheckInTime = attendance.CheckInTime!.Value,
                CheckOutTime = checkOutTime,
                WorkDate = workDate,
                WorkHours = workHours,
                OvertimeHours = overtimeHours
            }, cancellationToken);
        }

        return attendance;
    }
}
