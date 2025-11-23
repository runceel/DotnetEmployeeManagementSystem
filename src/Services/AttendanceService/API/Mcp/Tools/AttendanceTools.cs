using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using ModelContextProtocol.Server;

namespace AttendanceService.API.Mcp.Tools;

/// <summary>
/// 勤怠管理に関するMCPツール
/// </summary>
[McpServerToolType]
public class AttendanceTools
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILogger<AttendanceTools> _logger;

    public AttendanceTools(
        IAttendanceRepository attendanceRepository,
        ILogger<AttendanceTools> logger)
    {
        _attendanceRepository = attendanceRepository;
        _logger = logger;
    }

    /// <summary>
    /// 指定された勤怠記録IDの詳細情報を取得します。勤怠記録の出勤・退勤時刻、勤務時間などが取得できます。
    /// </summary>
    [McpServerTool]
    public async Task<AttendanceDetailResponse> GetAttendanceAsync(
        string attendanceId)
    {
        ArgumentNullException.ThrowIfNull(attendanceId);

        if (!Guid.TryParse(attendanceId, out var id))
        {
            throw new ArgumentException("Invalid attendance ID format", nameof(attendanceId));
        }

        _logger.LogInformation("MCP Tool: GetAttendance - AttendanceId: {AttendanceId}", id);

        var attendance = await _attendanceRepository.GetByIdAsync(id);
        if (attendance == null)
        {
            throw new InvalidOperationException($"勤怠記録ID {attendanceId} が見つかりませんでした。");
        }

        return new AttendanceDetailResponse(
            Id: attendance.Id.ToString(),
            EmployeeId: attendance.EmployeeId.ToString(),
            WorkDate: attendance.WorkDate,
            CheckInTime: attendance.CheckInTime,
            CheckOutTime: attendance.CheckOutTime,
            Type: attendance.Type.ToString(),
            Notes: attendance.Notes,
            WorkHours: attendance.CalculateWorkHours(),
            CreatedAt: attendance.CreatedAt,
            UpdatedAt: attendance.UpdatedAt
        );
    }

    /// <summary>
    /// 従業員の勤怠記録一覧を取得します。期間を指定して絞り込むことができます。
    /// </summary>
    [McpServerTool]
    public async Task<AttendanceListResponse> ListAttendancesAsync(
        string employeeId,
        string? startDate = null,
        string? endDate = null)
    {
        ArgumentNullException.ThrowIfNull(employeeId);

        if (!Guid.TryParse(employeeId, out var empId))
        {
            throw new ArgumentException("Invalid employee ID format", nameof(employeeId));
        }

        _logger.LogInformation("MCP Tool: ListAttendances - EmployeeId: {EmployeeId}, StartDate: {StartDate}, EndDate: {EndDate}",
            empId, startDate, endDate);

        IEnumerable<Attendance> attendances;

        if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
        {
            if (!DateTime.TryParse(startDate, out var start))
            {
                throw new ArgumentException("Invalid start date format. Use ISO 8601 format (e.g., 2024-01-01)", nameof(startDate));
            }

            if (!DateTime.TryParse(endDate, out var end))
            {
                throw new ArgumentException("Invalid end date format. Use ISO 8601 format (e.g., 2024-01-31)", nameof(endDate));
            }

            attendances = await _attendanceRepository.GetByEmployeeIdAndDateRangeAsync(empId, start, end);
        }
        else
        {
            attendances = await _attendanceRepository.GetByEmployeeIdAsync(empId);
        }

        var attendanceList = attendances.Select(a => new AttendanceSummary(
            Id: a.Id.ToString(),
            EmployeeId: a.EmployeeId.ToString(),
            WorkDate: a.WorkDate,
            CheckInTime: a.CheckInTime,
            CheckOutTime: a.CheckOutTime,
            Type: a.Type.ToString(),
            WorkHours: a.CalculateWorkHours()
        )).ToList();

        return new AttendanceListResponse(
            Attendances: attendanceList,
            TotalCount: attendanceList.Count
        );
    }

    /// <summary>
    /// 従業員の出勤を記録します。出勤時刻を指定して記録できます。
    /// </summary>
    [McpServerTool]
    public async Task<AttendanceDetailResponse> CheckInAsync(
        string employeeId,
        string checkInTime,
        string? attendanceType = null,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(employeeId);
        ArgumentNullException.ThrowIfNull(checkInTime);

        if (!Guid.TryParse(employeeId, out var empId))
        {
            throw new ArgumentException("Invalid employee ID format", nameof(employeeId));
        }

        if (!DateTime.TryParse(checkInTime, out var parsedCheckInTime))
        {
            throw new ArgumentException("Invalid check-in time format. Use ISO 8601 format (e.g., 2024-01-15T09:00:00Z)", nameof(checkInTime));
        }

        _logger.LogInformation("MCP Tool: CheckIn - EmployeeId: {EmployeeId}, CheckInTime: {CheckInTime}",
            empId, parsedCheckInTime);

        var type = AttendanceType.Normal;
        if (!string.IsNullOrEmpty(attendanceType) && !Enum.TryParse<AttendanceType>(attendanceType, true, out type))
        {
            throw new ArgumentException("Invalid attendance type. Valid values: Normal, Remote, BusinessTrip, HalfDay", nameof(attendanceType));
        }

        var workDate = parsedCheckInTime.Date;

        // Check if attendance record already exists for this date
        var existingAttendance = await _attendanceRepository.GetByEmployeeIdAndDateAsync(empId, workDate);
        if (existingAttendance != null)
        {
            throw new InvalidOperationException($"従業員ID {employeeId} の {workDate:yyyy-MM-dd} の勤怠記録は既に存在します。");
        }

        var attendance = new Attendance(empId, workDate, type, notes);
        attendance.CheckIn(parsedCheckInTime);

        await _attendanceRepository.AddAsync(attendance);

        _logger.LogInformation("MCP Tool: CheckIn - Successfully recorded check-in for employee {EmployeeId}", empId);

        return new AttendanceDetailResponse(
            Id: attendance.Id.ToString(),
            EmployeeId: attendance.EmployeeId.ToString(),
            WorkDate: attendance.WorkDate,
            CheckInTime: attendance.CheckInTime,
            CheckOutTime: attendance.CheckOutTime,
            Type: attendance.Type.ToString(),
            Notes: attendance.Notes,
            WorkHours: attendance.CalculateWorkHours(),
            CreatedAt: attendance.CreatedAt,
            UpdatedAt: attendance.UpdatedAt
        );
    }

    /// <summary>
    /// 従業員の退勤を記録します。退勤時刻を指定して記録できます。
    /// </summary>
    [McpServerTool]
    public async Task<AttendanceDetailResponse> CheckOutAsync(
        string employeeId,
        string checkOutTime)
    {
        ArgumentNullException.ThrowIfNull(employeeId);
        ArgumentNullException.ThrowIfNull(checkOutTime);

        if (!Guid.TryParse(employeeId, out var empId))
        {
            throw new ArgumentException("Invalid employee ID format", nameof(employeeId));
        }

        if (!DateTime.TryParse(checkOutTime, out var parsedCheckOutTime))
        {
            throw new ArgumentException("Invalid check-out time format. Use ISO 8601 format (e.g., 2024-01-15T18:00:00Z)", nameof(checkOutTime));
        }

        _logger.LogInformation("MCP Tool: CheckOut - EmployeeId: {EmployeeId}, CheckOutTime: {CheckOutTime}",
            empId, parsedCheckOutTime);

        var workDate = parsedCheckOutTime.Date;

        // Find today's attendance record
        var attendance = await _attendanceRepository.GetByEmployeeIdAndDateAsync(empId, workDate);
        if (attendance == null)
        {
            throw new InvalidOperationException($"従業員ID {employeeId} の {workDate:yyyy-MM-dd} の勤怠記録が見つかりません。先に出勤を記録してください。");
        }

        attendance.CheckOut(parsedCheckOutTime);
        await _attendanceRepository.UpdateAsync(attendance);

        _logger.LogInformation("MCP Tool: CheckOut - Successfully recorded check-out for employee {EmployeeId}", empId);

        return new AttendanceDetailResponse(
            Id: attendance.Id.ToString(),
            EmployeeId: attendance.EmployeeId.ToString(),
            WorkDate: attendance.WorkDate,
            CheckInTime: attendance.CheckInTime,
            CheckOutTime: attendance.CheckOutTime,
            Type: attendance.Type.ToString(),
            Notes: attendance.Notes,
            WorkHours: attendance.CalculateWorkHours(),
            CreatedAt: attendance.CreatedAt,
            UpdatedAt: attendance.UpdatedAt
        );
    }

    /// <summary>
    /// 従業員の月次勤怠集計を取得します。指定した年月の出勤日数、総勤務時間、平均勤務時間、遅刻回数などが取得できます。
    /// </summary>
    [McpServerTool]
    public async Task<MonthlySummaryResponse> GetMonthlySummaryAsync(
        string employeeId,
        int year,
        int month)
    {
        ArgumentNullException.ThrowIfNull(employeeId);

        if (!Guid.TryParse(employeeId, out var empId))
        {
            throw new ArgumentException("Invalid employee ID format", nameof(employeeId));
        }

        if (year < 2000 || year > 2100)
        {
            throw new ArgumentException("Year must be between 2000 and 2100", nameof(year));
        }

        if (month < 1 || month > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12", nameof(month));
        }

        _logger.LogInformation("MCP Tool: GetMonthlySummary - EmployeeId: {EmployeeId}, Year: {Year}, Month: {Month}",
            empId, year, month);

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var attendances = await _attendanceRepository.GetByEmployeeIdAndDateRangeAsync(empId, startDate, endDate);
        var attendanceList = attendances.ToList();

        var totalWorkDays = attendanceList.Count(a => a.CheckInTime.HasValue && a.CheckOutTime.HasValue);
        var totalWorkHours = attendanceList
            .Select(a => a.CalculateWorkHours())
            .Where(h => h.HasValue)
            .Sum(h => h!.Value);
        var averageWorkHours = totalWorkDays > 0 ? totalWorkHours / totalWorkDays : 0;
        var lateDays = attendanceList.Count(a =>
            a.CheckInTime.HasValue &&
            a.CheckInTime.Value.TimeOfDay > new TimeSpan(9, 0, 0));

        return new MonthlySummaryResponse(
            EmployeeId: empId.ToString(),
            Year: year,
            Month: month,
            TotalWorkDays: totalWorkDays,
            TotalWorkHours: totalWorkHours,
            AverageWorkHours: averageWorkHours,
            LateDays: lateDays
        );
    }
}

// Response DTO definitions

public record AttendanceDetailResponse(
    string Id,
    string EmployeeId,
    DateTime WorkDate,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    string Type,
    string? Notes,
    double? WorkHours,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record AttendanceSummary(
    string Id,
    string EmployeeId,
    DateTime WorkDate,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    string Type,
    double? WorkHours
);

public record AttendanceListResponse(
    IReadOnlyList<AttendanceSummary> Attendances,
    int TotalCount
);

public record MonthlySummaryResponse(
    string EmployeeId,
    int Year,
    int Month,
    int TotalWorkDays,
    double TotalWorkHours,
    double AverageWorkHours,
    int LateDays
);
