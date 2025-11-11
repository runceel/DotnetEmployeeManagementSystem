using AttendanceService.Application.Services;
using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using AttendanceService.Infrastructure;
using AttendanceService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Shared.Contracts.AttendanceService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
// Configure OpenAPI with detailed documentation
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "AttendanceService API",
            Version = "v1",
            Description = """
                勤怠管理サービス API
                
                ## 概要
                従業員の勤怠記録、休暇申請、および月次集計を管理するためのRESTful APIです。
                
                ## 主要機能
                - **勤怠記録管理**: 出退勤の記録と勤務時間の自動計算
                - **休暇申請管理**: 有給休暇、病気休暇などの申請と承認フロー
                - **月次集計**: 総勤務時間、平均勤務時間、遅刻回数などの集計
                
                ## 認証
                APIは認証が必要です。リクエストヘッダーに適切な認証情報を含めてください。
                """,
            Contact = new OpenApiContact
            {
                Name = "開発チーム",
                Email = "dev@example.com"
            }
        };
        
        // Add security scheme
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT認証トークンを入力してください"
        });
        
        return Task.CompletedTask;
    });
});

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
    // Add Scalar UI for better OpenAPI documentation experience
    app.MapScalarApiReference(_ => _.Servers = []);
}

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "Unhandled exception occurred");
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = "内部サーバーエラーが発生しました。",
                message = app.Environment.IsDevelopment() ? error.Error.Message : "システム管理者にお問い合わせください。",
                traceId = context.TraceIdentifier
            });
        }
    });
});

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
.WithSummary("全勤怠記録を取得")
.WithDescription("システム内の全勤怠記録を取得します。（実装予定）")
.Produces<IEnumerable<AttendanceDto>>();

// IDで勤怠記録を取得
attendances.MapGet("/{id:guid}", async (
    Guid id,
    [FromServices] IAttendanceRepository attendanceRepository,
    [FromServices] ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        var attendance = await attendanceRepository.GetByIdAsync(id, cancellationToken);
        if (attendance == null)
        {
            return Results.NotFound(new { error = "勤怠記録が見つかりません。" });
        }

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
    catch (Exception ex)
    {
        logger.LogError(ex, "勤怠記録 {AttendanceId} の取得中にエラーが発生しました", id);
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("GetAttendanceById")
.WithSummary("IDで勤怠記録を取得")
.WithDescription("指定されたIDの勤怠記録を取得します。")
.Produces<AttendanceDto>()
.Produces(StatusCodes.Status404NotFound);

// 勤怠記録を作成
attendances.MapPost("/", async (
    [FromBody] CreateAttendanceRequest request,
    [FromServices] IAttendanceRepository attendanceRepository,
    [FromServices] ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Enum.TryParse<AttendanceType>(request.Type, ignoreCase: true, out var attendanceType))
        {
            return Results.BadRequest(new { error = "無効な勤怠種別です。有効な値: Normal, Remote, BusinessTrip, HalfDay" });
        }

        // 同じ日の勤怠記録が既に存在するかチェック
        var existingAttendance = await attendanceRepository.GetByEmployeeIdAndDateAsync(
            request.EmployeeId,
            request.WorkDate,
            cancellationToken);

        if (existingAttendance != null)
        {
            return Results.BadRequest(new { error = "指定された日付の勤怠記録は既に存在します。" });
        }

        var attendance = new AttendanceService.Domain.Entities.Attendance(
            request.EmployeeId,
            request.WorkDate,
            attendanceType,
            request.Notes);

        // 出勤時刻が指定されている場合は記録
        if (request.CheckInTime.HasValue)
        {
            attendance.CheckIn(request.CheckInTime.Value);
        }

        // 退勤時刻が指定されている場合は記録
        if (request.CheckOutTime.HasValue)
        {
            if (!request.CheckInTime.HasValue)
            {
                return Results.BadRequest(new { error = "退勤時刻を記録するには出勤時刻が必要です。" });
            }
            attendance.CheckOut(request.CheckOutTime.Value);
        }

        var result = await attendanceRepository.AddAsync(attendance, cancellationToken);

        var dto = new AttendanceDto
        {
            Id = result.Id,
            EmployeeId = result.EmployeeId,
            WorkDate = result.WorkDate,
            CheckInTime = result.CheckInTime,
            CheckOutTime = result.CheckOutTime,
            Type = result.Type.ToString(),
            Notes = result.Notes,
            WorkHours = result.CalculateWorkHours(),
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };

        return Results.Created($"/api/attendances/{dto.Id}", dto);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "勤怠記録の作成中にエラーが発生しました");
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("CreateAttendance")
.WithSummary("勤怠記録を作成")
.WithDescription("""
    新しい勤怠記録を作成します。
    
    **勤怠種別:**
    - Normal: 通常勤務
    - Remote: リモートワーク
    - BusinessTrip: 出張
    - HalfDay: 半日勤務
    
    **バリデーション:**
    - 従業員IDは有効なGUIDである必要があります
    - 同じ日付の勤怠記録が既に存在する場合はエラーを返します
    - 勤務日は現在または過去の日付である必要があります
    """)
.Produces<AttendanceDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

// 勤怠記録を更新
attendances.MapPut("/{id:guid}", async (
    Guid id,
    [FromBody] UpdateAttendanceRequest request,
    [FromServices] IAttendanceRepository attendanceRepository,
    [FromServices] ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        ArgumentNullException.ThrowIfNull(request);

        var attendance = await attendanceRepository.GetByIdAsync(id, cancellationToken);
        if (attendance == null)
        {
            return Results.NotFound(new { error = "勤怠記録が見つかりません。" });
        }

        if (!Enum.TryParse<AttendanceType>(request.Type, ignoreCase: true, out var attendanceType))
        {
            return Results.BadRequest(new { error = "無効な勤怠種別です。有効な値: Normal, Remote, BusinessTrip, HalfDay" });
        }

        attendance.Update(attendanceType, request.Notes);
        await attendanceRepository.UpdateAsync(attendance, cancellationToken);

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
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "勤怠記録 {AttendanceId} の更新中にエラーが発生しました", id);
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("UpdateAttendance")
.WithSummary("勤怠記録を更新")
.WithDescription("""
    既存の勤怠記録を更新します。
    
    **更新可能な項目:**
    - 勤怠種別 (Type)
    - 備考 (Notes)
    
    **注意:**
    - 出勤時刻・退勤時刻は更新できません
    - 従業員IDや勤務日は変更できません
    """)
.Produces<AttendanceDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

// 勤怠記録を削除
attendances.MapDelete("/{id:guid}", async (
    Guid id,
    [FromServices] IAttendanceRepository attendanceRepository,
    [FromServices] ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        var attendance = await attendanceRepository.GetByIdAsync(id, cancellationToken);
        if (attendance == null)
        {
            return Results.NotFound(new { error = "勤怠記録が見つかりません。" });
        }

        await attendanceRepository.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "勤怠記録 {AttendanceId} の削除中にエラーが発生しました", id);
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("DeleteAttendance")
.WithSummary("勤怠記録を削除")
.WithDescription("指定されたIDの勤怠記録を削除します。")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

// 出勤を記録
attendances.MapPost("/checkin", async (
    [FromBody] CheckInRequest request,
    [FromServices] IAttendanceService attendanceService,
    CancellationToken cancellationToken) =>
{
    try
    {
        ArgumentNullException.ThrowIfNull(request);
        
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
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("CheckIn")
.WithSummary("出勤を記録")
.WithDescription("""
    従業員の出勤時刻を記録します。
    
    **バリデーション:**
    - 従業員IDは有効なGUIDである必要があります
    - 出勤時刻は未来の日時であってはなりません
    - 既にその日の出勤記録が存在する場合はエラーを返します
    """)
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
        ArgumentNullException.ThrowIfNull(request);
        
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
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("CheckOut")
.WithSummary("退勤を記録")
.WithDescription("""
    従業員の退勤時刻を記録します。
    
    **バリデーション:**
    - 従業員IDは有効なGUIDである必要があります
    - 退勤時刻は出勤時刻より後である必要があります
    - その日の出勤記録が存在しない場合はエラーを返します
    """)
.Produces<AttendanceDto>()
.Produces(StatusCodes.Status400BadRequest);

// 従業員の勤怠履歴を取得
attendances.MapGet("/employee/{employeeId:guid}", async (
    Guid employeeId,
    [FromServices] IAttendanceRepository attendanceRepository,
    [FromServices] ILogger<Program> logger,
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate,
    CancellationToken cancellationToken) =>
{
    try
    {
        IEnumerable<AttendanceService.Domain.Entities.Attendance> attendanceRecords;

        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate.Value > endDate.Value)
            {
                return Results.BadRequest(new { error = "開始日は終了日より前である必要があります。" });
            }
            
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
        logger.LogError(ex, "従業員 {EmployeeId} の勤怠履歴取得中にエラーが発生しました", employeeId);
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("GetAttendancesByEmployee")
.WithSummary("従業員の勤怠履歴を取得")
.WithDescription("""
    指定された従業員の勤怠履歴を取得します。
    
    **クエリパラメータ:**
    - startDate: 開始日（オプション）
    - endDate: 終了日（オプション）
    
    両方を指定すると期間フィルタリングが適用されます。
    """)
.Produces<IEnumerable<AttendanceDto>>()
.Produces(StatusCodes.Status400BadRequest);

// 従業員の月次勤怠集計を取得
attendances.MapGet("/employee/{employeeId:guid}/summary/{year:int}/{month:int}", async (
    Guid employeeId,
    int year,
    int month,
    [FromServices] IAttendanceRepository attendanceRepository,
    [FromServices] ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (month < 1 || month > 12)
        {
            return Results.BadRequest(new { error = "月は1から12の範囲で指定してください。" });
        }
        
        if (year < 2000 || year > 2100)
        {
            return Results.BadRequest(new { error = "年は2000から2100の範囲で指定してください。" });
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
        logger.LogError(ex, "従業員 {EmployeeId} の月次集計取得中にエラーが発生しました (Year: {Year}, Month: {Month})", employeeId, year, month);
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("GetMonthlyAttendanceSummary")
.WithSummary("従業員の月次勤怠集計を取得")
.WithDescription("""
    指定された従業員の月次勤怠集計を取得します。
    
    **集計項目:**
    - 総出勤日数
    - 総勤務時間
    - 平均勤務時間
    - 遅刻回数（9:00以降の出勤）
    
    **パスパラメータ:**
    - employeeId: 従業員ID
    - year: 年（2000-2100）
    - month: 月（1-12）
    """)
.Produces<MonthlyAttendanceSummaryDto>()
.Produces(StatusCodes.Status400BadRequest);

// Leave Request API endpoints
var leaveRequests = app.MapGroup("/api/leaverequests")
    .WithTags("LeaveRequests")
    .WithOpenApi();

// 全休暇申請を取得
leaveRequests.MapGet("/", async (
    [FromServices] ILeaveRequestService leaveRequestService,
    [FromServices] ILogger<Program> logger,
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
        logger.LogError(ex, "全休暇申請取得中にエラーが発生しました");
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("GetAllLeaveRequests")
.WithSummary("全休暇申請を取得")
.WithDescription("システム内の全休暇申請を取得します。")
.Produces<IEnumerable<LeaveRequestDto>>();

// IDで休暇申請を取得
leaveRequests.MapGet("/{id:guid}", async (
    Guid id,
    [FromServices] ILeaveRequestService leaveRequestService,
    [FromServices] ILogger<Program> logger,
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
        logger.LogError(ex, "休暇申請 {LeaveRequestId} 取得中にエラーが発生しました", id);
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("GetLeaveRequestById")
.WithSummary("IDで休暇申請を取得")
.WithDescription("指定されたIDの休暇申請を取得します。")
.Produces<LeaveRequestDto>()
.Produces(StatusCodes.Status404NotFound);

// 従業員別の休暇申請を取得
leaveRequests.MapGet("/employee/{employeeId:guid}", async (
    Guid employeeId,
    [FromServices] ILeaveRequestService leaveRequestService,
    [FromServices] ILogger<Program> logger,
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
        logger.LogError(ex, "従業員 {EmployeeId} の休暇申請取得中にエラーが発生しました", employeeId);
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("GetLeaveRequestsByEmployee")
.WithSummary("従業員別の休暇申請を取得")
.WithDescription("指定された従業員の全休暇申請を取得します。")
.Produces<IEnumerable<LeaveRequestDto>>();

// ステータス別の休暇申請を取得
leaveRequests.MapGet("/status/{status}", async (
    string status,
    [FromServices] ILeaveRequestService leaveRequestService,
    [FromServices] ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (!Enum.TryParse<LeaveRequestStatus>(status, ignoreCase: true, out var leaveStatus))
        {
            return Results.BadRequest(new { error = "無効なステータスです。有効な値: Pending, Approved, Rejected, Cancelled" });
        }

        var requests = await leaveRequestService.GetLeaveRequestsByStatusAsync(leaveStatus, cancellationToken);
        var dtos = requests.Select(MapToDto);
        return Results.Ok(dtos);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "ステータス {Status} の休暇申請取得中にエラーが発生しました", status);
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("GetLeaveRequestsByStatus")
.WithSummary("ステータス別の休暇申請を取得")
.WithDescription("""
    指定されたステータスの休暇申請を取得します。
    
    **有効なステータス:**
    - Pending: 承認待ち
    - Approved: 承認済み
    - Rejected: 却下
    - Cancelled: キャンセル
    """)
.Produces<IEnumerable<LeaveRequestDto>>();

// 休暇申請を作成
leaveRequests.MapPost("/", async (
    [FromBody] CreateLeaveRequestRequest request,
    [FromServices] ILeaveRequestService leaveRequestService,
    [FromServices] ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        ArgumentNullException.ThrowIfNull(request);
        
        if (!Enum.TryParse<LeaveType>(request.Type, ignoreCase: true, out var leaveType))
        {
            return Results.BadRequest(new { error = "無効な休暇種別です。有効な値: PaidLeave, SickLeave, SpecialLeave, Unpaid" });
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
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("CreateLeaveRequest")
.WithSummary("休暇申請を作成")
.WithDescription("""
    新しい休暇申請を作成します。
    
    **休暇種別:**
    - PaidLeave: 有給休暇
    - SickLeave: 病気休暇
    - SpecialLeave: 特別休暇
    - Unpaid: 無給休暇
    
    **バリデーション:**
    - 開始日は終了日より前である必要があります
    - 過去の日付は指定できません
    """)
.Produces<LeaveRequestDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

// 休暇申請を承認
leaveRequests.MapPost("/{id:guid}/approve", async (
    Guid id,
    [FromBody] ApproveLeaveRequestRequest request,
    [FromServices] ILeaveRequestService leaveRequestService,
    [FromServices] ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var leaveRequest = await leaveRequestService.ApproveLeaveRequestAsync(
            id,
            request.ApproverId,
            request.Comment,
            cancellationToken);

        return Results.Ok(MapToDto(leaveRequest));
    }
    catch (InvalidOperationException ex)
    {
        logger.LogError(ex, "休暇申請 {LeaveRequestId} の承認処理中にエラーが発生しました", id);
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("ApproveLeaveRequest")
.WithSummary("休暇申請を承認")
.WithDescription("指定された休暇申請を承認します。承認者IDとコメントを指定できます。")
.Produces<LeaveRequestDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

// 休暇申請を却下
leaveRequests.MapPost("/{id:guid}/reject", async (
    Guid id,
    [FromBody] RejectLeaveRequestRequest request,
    [FromServices] ILeaveRequestService leaveRequestService,
    [FromServices] ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var leaveRequest = await leaveRequestService.RejectLeaveRequestAsync(
            id,
            request.ApproverId,
            request.Comment,
            cancellationToken);

        return Results.Ok(MapToDto(leaveRequest));
    }
    catch (InvalidOperationException ex)
    {
        logger.LogError(ex, "休暇申請 {LeaveRequestId} の却下処理中にエラーが発生しました", id);
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("RejectLeaveRequest")
.WithSummary("休暇申請を却下")
.WithDescription("指定された休暇申請を却下します。承認者IDと却下理由を指定できます。")
.Produces<LeaveRequestDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

// 休暇申請をキャンセル
leaveRequests.MapPost("/{id:guid}/cancel", async (
    Guid id,
    [FromServices] ILeaveRequestService leaveRequestService,
    [FromServices] ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        var leaveRequest = await leaveRequestService.CancelLeaveRequestAsync(id, cancellationToken);
        return Results.Ok(MapToDto(leaveRequest));
    }
    catch (InvalidOperationException ex)
    {
        logger.LogError(ex, "休暇申請 {LeaveRequestId} のキャンセル処理中にエラーが発生しました", id);
        return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
    }
})
.WithName("CancelLeaveRequest")
.WithSummary("休暇申請をキャンセル")
.WithDescription("指定された休暇申請をキャンセルします。申請者のみキャンセルできます。")
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
