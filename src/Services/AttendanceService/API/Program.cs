using AttendanceService.Application.Services;
using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using AttendanceService.Infrastructure;
using AttendanceService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.AttendanceService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// データベース接続文字列とInfrastructure層の初期化
// Test環境ではテストコードでDbContextを設定するためスキップ
if (!builder.Environment.IsEnvironment("Test"))
{
    var connectionString = builder.Configuration.GetConnectionString("AttendanceDb")
        ?? "Data Source=attendance.db";

    // Infrastructure層のサービスを追加
    builder.Services.AddInfrastructure(connectionString);

    // Redis接続の追加
    builder.AddRedisClient("redis");
}

var app = builder.Build();

// データベース初期化 (Test環境では実行しない)
if (!app.Environment.IsEnvironment("Test"))
{
    await DbInitializer.InitializeAsync(app.Services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck")
    .WithTags("Health");

// Development-only endpoint to seed attendance data for specific employees
if (app.Environment.IsDevelopment())
{
    app.MapPost("/api/dev/seed-attendances", async (
        [FromBody] List<Guid> employeeIds,
        [FromServices] IAttendanceRepository attendanceRepository,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var attendances = new List<AttendanceService.Domain.Entities.Attendance>();
            var random = new Random(42);
            var today = DateTime.UtcNow.Date;
            var startDate = today.AddMonths(-3);

            foreach (var employeeId in employeeIds)
            {
                var currentDate = startDate;
                while (currentDate <= today)
                {
                    if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        if (random.Next(100) < 90)
                        {
                            var attendanceType = random.Next(100) switch
                            {
                                < 80 => AttendanceType.Normal,
                                < 90 => AttendanceType.Remote,
                                < 95 => AttendanceType.BusinessTrip,
                                _ => AttendanceType.HalfDay
                            };

                            var attendance = new AttendanceService.Domain.Entities.Attendance(employeeId, currentDate, attendanceType);

                            var checkInHour = 8;
                            var checkInMinute = random.Next(0, 61);
                            if (checkInMinute >= 30)
                            {
                                checkInHour = 9;
                                checkInMinute -= 30;
                            }
                            var checkInTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day,
                                checkInHour, checkInMinute, 0, DateTimeKind.Utc);

                            attendance.CheckIn(checkInTime);

                            var workHours = 7 + random.Next(0, 4) + (random.NextDouble() * 0.5);
                            var checkOutTime = checkInTime.AddHours(workHours);
                            attendance.CheckOut(checkOutTime);

                            if (random.Next(100) < 10)
                            {
                                var notes = new[] { "打ち合わせ多数", "顧客訪問", "社内研修", "定期健康診断" };
                                var selectedNote = notes[random.Next(notes.Length)];
                                attendance.Update(attendanceType, selectedNote);
                            }

                            attendances.Add(attendance);
                        }
                    }
                    currentDate = currentDate.AddDays(1);
                }
            }

            foreach (var attendance in attendances)
            {
                await attendanceRepository.AddAsync(attendance, cancellationToken);
            }

            return Results.Ok(new { message = $"Seeded {attendances.Count} attendance records for {employeeIds.Count} employees" });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    })
    .WithName("SeedAttendances")
    .WithTags("Development")
    .ExcludeFromDescription();
}

// Attendance API endpoints
var attendances = app.MapGroup("/api/attendances")
    .WithTags("Attendances")
    .WithOpenApi();

// 全勤怠記録を取得
attendances.MapGet("/", () =>
{
    return Results.Ok(Array.Empty<AttendanceDto>());
})
.WithName("GetAllAttendances")
.Produces<IEnumerable<AttendanceDto>>();

// IDで勤怠記録を取得
attendances.MapGet("/{id:guid}", (Guid id) =>
{
    return Results.NotFound();
})
.WithName("GetAttendanceById")
.Produces<AttendanceDto>()
.Produces(StatusCodes.Status404NotFound);

// 勤怠記録を作成
attendances.MapPost("/", ([FromBody] CreateAttendanceRequest request) =>
{
    return Results.Created($"/api/attendances/{Guid.NewGuid()}", new AttendanceDto());
})
.WithName("CreateAttendance")
.Produces<AttendanceDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

// 出勤を記録
attendances.MapPost("/checkin", async (
    [FromBody] CheckInRequest request,
    [FromServices] IAttendanceService attendanceService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var attendance = await attendanceService.CheckInAsync(
            request.EmployeeId,
            request.CheckInTime,
            cancellationToken);

        var dto = new AttendanceDto
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            WorkDate = attendance.WorkDate,
            CheckInTime = attendance.CheckInTime,
            CheckOutTime = attendance.CheckOutTime,
            Type = attendance.Type.ToString(),
            Notes = attendance.Notes,
            WorkHours = attendance.CalculateWorkHours(),
            CreatedAt = attendance.CreatedAt,
            UpdatedAt = attendance.UpdatedAt
        };

        return Results.Ok(dto);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CheckIn")
.Produces<AttendanceDto>()
.Produces(StatusCodes.Status400BadRequest);

// 退勤を記録
attendances.MapPost("/checkout", async (
    [FromBody] CheckOutRequest request,
    [FromServices] IAttendanceService attendanceService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var attendance = await attendanceService.CheckOutAsync(
            request.EmployeeId,
            request.CheckOutTime,
            cancellationToken);

        var dto = new AttendanceDto
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            WorkDate = attendance.WorkDate,
            CheckInTime = attendance.CheckInTime,
            CheckOutTime = attendance.CheckOutTime,
            Type = attendance.Type.ToString(),
            Notes = attendance.Notes,
            WorkHours = attendance.CalculateWorkHours(),
            CreatedAt = attendance.CreatedAt,
            UpdatedAt = attendance.UpdatedAt
        };

        return Results.Ok(dto);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CheckOut")
.Produces<AttendanceDto>()
.Produces(StatusCodes.Status400BadRequest);

// 従業員の勤怠履歴を取得
attendances.MapGet("/employee/{employeeId:guid}", async (
    Guid employeeId,
    [FromServices] IAttendanceRepository attendanceRepository,
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate,
    CancellationToken cancellationToken) =>
{
    try
    {
        IEnumerable<AttendanceService.Domain.Entities.Attendance> attendanceRecords;

        if (startDate.HasValue && endDate.HasValue)
        {
            attendanceRecords = await attendanceRepository.GetByEmployeeIdAndDateRangeAsync(
                employeeId,
                startDate.Value,
                endDate.Value,
                cancellationToken);
        }
        else
        {
            attendanceRecords = await attendanceRepository.GetByEmployeeIdAsync(
                employeeId,
                cancellationToken);
        }

        var dtos = attendanceRecords.Select(a => new AttendanceDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            WorkDate = a.WorkDate,
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            Type = a.Type.ToString(),
            Notes = a.Notes,
            WorkHours = a.CalculateWorkHours(),
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        });

        return Results.Ok(dtos);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetAttendancesByEmployee")
.Produces<IEnumerable<AttendanceDto>>()
.Produces(StatusCodes.Status400BadRequest);

// 従業員の月次勤怠集計を取得
attendances.MapGet("/employee/{employeeId:guid}/summary/{year:int}/{month:int}", async (
    Guid employeeId,
    int year,
    int month,
    [FromServices] IAttendanceRepository attendanceRepository,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (month < 1 || month > 12)
        {
            return Results.BadRequest(new { error = "月は1から12の範囲で指定してください。" });
        }

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var attendanceRecords = await attendanceRepository.GetByEmployeeIdAndDateRangeAsync(
            employeeId,
            startDate,
            endDate,
            cancellationToken);

        var attendanceList = attendanceRecords.ToList();

        // 集計計算
        var workDays = attendanceList.Count(a => a.CheckInTime.HasValue && a.CheckOutTime.HasValue);
        var totalHours = attendanceList.Sum(a => a.CalculateWorkHours() ?? 0);
        var averageHours = workDays > 0 ? totalHours / workDays : 0;

        // 遅刻回数（例: 9:00より後の出勤）
        var lateDays = attendanceList.Count(a =>
            a.CheckInTime.HasValue &&
            a.CheckInTime.Value.TimeOfDay > new TimeSpan(9, 0, 0));

        // 欠勤日数と有給休暇日数は別途LeaveRequestから取得する必要があるため、ここでは0とする
        var absentDays = 0;
        var paidLeaveDays = 0;

        var summary = new MonthlyAttendanceSummaryDto
        {
            EmployeeId = employeeId,
            Year = year,
            Month = month,
            TotalWorkDays = workDays,
            TotalWorkHours = totalHours,
            AverageWorkHours = averageHours,
            LateDays = lateDays,
            AbsentDays = absentDays,
            PaidLeaveDays = paidLeaveDays,
            Attendances = attendanceList.Select(a => new AttendanceDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                WorkDate = a.WorkDate,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                Type = a.Type.ToString(),
                Notes = a.Notes,
                WorkHours = a.CalculateWorkHours(),
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
        };

        return Results.Ok(summary);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetMonthlyAttendanceSummary")
.Produces<MonthlyAttendanceSummaryDto>()
.Produces(StatusCodes.Status400BadRequest);

// Leave Request API endpoints
var leaveRequests = app.MapGroup("/api/leaverequests")
    .WithTags("LeaveRequests")
    .WithOpenApi();

// 全休暇申請を取得
leaveRequests.MapGet("/", async (
    [FromServices] ILeaveRequestService leaveRequestService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var requests = await leaveRequestService.GetAllLeaveRequestsAsync(cancellationToken);
        var dtos = requests.Select(MapToDto);
        return Results.Ok(dtos);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetAllLeaveRequests")
.Produces<IEnumerable<LeaveRequestDto>>();

// IDで休暇申請を取得
leaveRequests.MapGet("/{id:guid}", async (
    Guid id,
    [FromServices] ILeaveRequestService leaveRequestService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var leaveRequest = await leaveRequestService.GetLeaveRequestByIdAsync(id, cancellationToken);
        if (leaveRequest == null)
        {
            return Results.NotFound(new { error = "休暇申請が見つかりません。" });
        }

        return Results.Ok(MapToDto(leaveRequest));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetLeaveRequestById")
.Produces<LeaveRequestDto>()
.Produces(StatusCodes.Status404NotFound);

// 従業員別の休暇申請を取得
leaveRequests.MapGet("/employee/{employeeId:guid}", async (
    Guid employeeId,
    [FromServices] ILeaveRequestService leaveRequestService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var requests = await leaveRequestService.GetLeaveRequestsByEmployeeIdAsync(employeeId, cancellationToken);
        var dtos = requests.Select(MapToDto);
        return Results.Ok(dtos);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetLeaveRequestsByEmployee")
.Produces<IEnumerable<LeaveRequestDto>>();

// ステータス別の休暇申請を取得
leaveRequests.MapGet("/status/{status}", async (
    string status,
    [FromServices] ILeaveRequestService leaveRequestService,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (!Enum.TryParse<LeaveRequestStatus>(status, ignoreCase: true, out var leaveStatus))
        {
            return Results.BadRequest(new { error = "無効なステータスです。" });
        }

        var requests = await leaveRequestService.GetLeaveRequestsByStatusAsync(leaveStatus, cancellationToken);
        var dtos = requests.Select(MapToDto);
        return Results.Ok(dtos);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetLeaveRequestsByStatus")
.Produces<IEnumerable<LeaveRequestDto>>();

// 休暇申請を作成
leaveRequests.MapPost("/", async (
    [FromBody] CreateLeaveRequestRequest request,
    [FromServices] ILeaveRequestService leaveRequestService,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (!Enum.TryParse<LeaveType>(request.Type, ignoreCase: true, out var leaveType))
        {
            return Results.BadRequest(new { error = "無効な休暇種別です。" });
        }

        var leaveRequest = await leaveRequestService.CreateLeaveRequestAsync(
            request.EmployeeId,
            leaveType,
            request.StartDate,
            request.EndDate,
            request.Reason,
            cancellationToken);

        return Results.Created($"/api/leaverequests/{leaveRequest.Id}", MapToDto(leaveRequest));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateLeaveRequest")
.Produces<LeaveRequestDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

// 休暇申請を承認
leaveRequests.MapPost("/{id:guid}/approve", async (
    Guid id,
    [FromBody] ApproveLeaveRequestRequest request,
    [FromServices] ILeaveRequestService leaveRequestService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var leaveRequest = await leaveRequestService.ApproveLeaveRequestAsync(
            id,
            request.ApproverId,
            request.Comment,
            cancellationToken);

        return Results.Ok(MapToDto(leaveRequest));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("ApproveLeaveRequest")
.Produces<LeaveRequestDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

// 休暇申請を却下
leaveRequests.MapPost("/{id:guid}/reject", async (
    Guid id,
    [FromBody] RejectLeaveRequestRequest request,
    [FromServices] ILeaveRequestService leaveRequestService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var leaveRequest = await leaveRequestService.RejectLeaveRequestAsync(
            id,
            request.ApproverId,
            request.Comment,
            cancellationToken);

        return Results.Ok(MapToDto(leaveRequest));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("RejectLeaveRequest")
.Produces<LeaveRequestDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

// 休暇申請をキャンセル
leaveRequests.MapPost("/{id:guid}/cancel", async (
    Guid id,
    [FromServices] ILeaveRequestService leaveRequestService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var leaveRequest = await leaveRequestService.CancelLeaveRequestAsync(id, cancellationToken);
        return Results.Ok(MapToDto(leaveRequest));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CancelLeaveRequest")
.Produces<LeaveRequestDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

// DTO変換ヘルパー
static LeaveRequestDto MapToDto(AttendanceService.Domain.Entities.LeaveRequest leaveRequest)
{
    return new LeaveRequestDto
    {
        Id = leaveRequest.Id,
        EmployeeId = leaveRequest.EmployeeId,
        Type = leaveRequest.Type.ToString(),
        StartDate = leaveRequest.StartDate,
        EndDate = leaveRequest.EndDate,
        Reason = leaveRequest.Reason,
        Status = leaveRequest.Status.ToString(),
        ApproverId = leaveRequest.ApproverId,
        ApprovedAt = leaveRequest.ApprovedAt,
        ApproverComment = leaveRequest.ApproverComment,
        Days = leaveRequest.CalculateDays(),
        CreatedAt = leaveRequest.CreatedAt,
        UpdatedAt = leaveRequest.UpdatedAt
    };
}

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
