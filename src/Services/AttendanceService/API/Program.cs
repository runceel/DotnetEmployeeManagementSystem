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
leaveRequests.MapGet("/", () =>
{
    return Results.Ok(Array.Empty<LeaveRequestDto>());
})
.WithName("GetAllLeaveRequests")
.Produces<IEnumerable<LeaveRequestDto>>();

// IDで休暇申請を取得
leaveRequests.MapGet("/{id:guid}", (Guid id) =>
{
    return Results.NotFound();
})
.WithName("GetLeaveRequestById")
.Produces<LeaveRequestDto>()
.Produces(StatusCodes.Status404NotFound);

// 休暇申請を作成
leaveRequests.MapPost("/", ([FromBody] CreateLeaveRequestRequest request) =>
{
    return Results.Created($"/api/leaverequests/{Guid.NewGuid()}", new LeaveRequestDto());
})
.WithName("CreateLeaveRequest")
.Produces<LeaveRequestDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

// 休暇申請を承認
leaveRequests.MapPost("/{id:guid}/approve", (Guid id, [FromBody] ApproveLeaveRequestRequest request) =>
{
    return Results.Ok(new LeaveRequestDto());
})
.WithName("ApproveLeaveRequest")
.Produces<LeaveRequestDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

// 休暇申請を却下
leaveRequests.MapPost("/{id:guid}/reject", (Guid id, [FromBody] RejectLeaveRequestRequest request) =>
{
    return Results.Ok(new LeaveRequestDto());
})
.WithName("RejectLeaveRequest")
.Produces<LeaveRequestDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

// 休暇申請をキャンセル
leaveRequests.MapPost("/{id:guid}/cancel", (Guid id) =>
{
    return Results.Ok(new LeaveRequestDto());
})
.WithName("CancelLeaveRequest")
.Produces<LeaveRequestDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
