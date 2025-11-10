using AttendanceService.Application.Services;
using AttendanceService.Domain.Enums;
using AttendanceService.Infrastructure;
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
