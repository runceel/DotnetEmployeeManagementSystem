using AttendanceService.Application.Services;
using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.AttendanceService;

namespace AttendanceService.API.Endpoints;

public static class AttendanceEndpoints
{
    public static IEndpointRouteBuilder MapAttendanceEndpoints(this IEndpointRouteBuilder app)
    {
        var attendances = app.MapGroup("/api/attendances")
            .WithTags("Attendances");

        attendances.MapGet("/", GetAllAttendances)
            .WithName("GetAllAttendances")
            .WithSummary("全勤怠記録を取得")
            .WithDescription("システム内の全勤怠記録を取得します。（実装予定）")
            .Produces<IEnumerable<AttendanceDto>>();

        attendances.MapGet("/{id:guid}", GetAttendanceById)
            .WithName("GetAttendanceById")
            .WithSummary("IDで勤怠記録を取得")
            .WithDescription("指定されたIDの勤怠記録を取得します。")
            .Produces<AttendanceDto>()
            .Produces(StatusCodes.Status404NotFound);

        attendances.MapPost("/", CreateAttendance)
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

        attendances.MapPut("/{id:guid}", UpdateAttendance)
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

        attendances.MapDelete("/{id:guid}", DeleteAttendance)
            .WithName("DeleteAttendance")
            .WithSummary("勤怠記録を削除")
            .WithDescription("指定されたIDの勤怠記録を削除します。")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        attendances.MapPost("/checkin", CheckIn)
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

        attendances.MapPost("/checkout", CheckOut)
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

        attendances.MapGet("/employee/{employeeId:guid}", GetAttendancesByEmployee)
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

        attendances.MapGet("/employee/{employeeId:guid}/summary/{year:int}/{month:int}", GetMonthlyAttendanceSummary)
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

        return app;
    }

    private static IResult GetAllAttendances()
    {
        return Results.Ok(Array.Empty<AttendanceDto>());
    }

    private static async Task<IResult> GetAttendanceById(
        Guid id,
        [FromServices] IAttendanceRepository attendanceRepository,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var attendance = await attendanceRepository.GetByIdAsync(id, cancellationToken);
            if (attendance == null)
            {
                return Results.NotFound(new { error = "勤怠記録が見つかりません。" });
            }

            var dto = MapToDto(attendance);
            return Results.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "勤怠記録 {AttendanceId} の取得中にエラーが発生しました", id);
            return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
        }
    }

    private static async Task<IResult> CreateAttendance(
        [FromBody] CreateAttendanceRequest request,
        [FromServices] IAttendanceRepository attendanceRepository,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
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

            var attendance = new Domain.Entities.Attendance(
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
            var dto = MapToDto(result);

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
    }

    private static async Task<IResult> UpdateAttendance(
        Guid id,
        [FromBody] UpdateAttendanceRequest request,
        [FromServices] IAttendanceRepository attendanceRepository,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
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

            var dto = MapToDto(attendance);
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
    }

    private static async Task<IResult> DeleteAttendance(
        Guid id,
        [FromServices] IAttendanceRepository attendanceRepository,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
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
    }

    private static async Task<IResult> CheckIn(
        [FromBody] CheckInRequest request,
        [FromServices] IAttendanceService attendanceService,
        CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request);
            
            var attendance = await attendanceService.CheckInAsync(
                request.EmployeeId,
                request.CheckInTime,
                cancellationToken);

            var dto = MapToDto(attendance);
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
    }

    private static async Task<IResult> CheckOut(
        [FromBody] CheckOutRequest request,
        [FromServices] IAttendanceService attendanceService,
        CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request);
            
            var attendance = await attendanceService.CheckOutAsync(
                request.EmployeeId,
                request.CheckOutTime,
                cancellationToken);

            var dto = MapToDto(attendance);
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
    }

    private static async Task<IResult> GetAttendancesByEmployee(
        Guid employeeId,
        [FromServices] IAttendanceRepository attendanceRepository,
        [FromServices] ILogger<Program> logger,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Domain.Entities.Attendance> attendanceRecords;

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

            var dtos = attendanceRecords.Select(MapToDto);
            return Results.Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "従業員 {EmployeeId} の勤怠履歴取得中にエラーが発生しました", employeeId);
            return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
        }
    }

    private static async Task<IResult> GetMonthlyAttendanceSummary(
        Guid employeeId,
        int year,
        int month,
        [FromServices] IAttendanceRepository attendanceRepository,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
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
                Attendances = attendanceList.Select(MapToDto)
            };

            return Results.Ok(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "従業員 {EmployeeId} の月次集計取得中にエラーが発生しました (Year: {Year}, Month: {Month})", employeeId, year, month);
            return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
        }
    }

    private static AttendanceDto MapToDto(Domain.Entities.Attendance attendance)
    {
        return new AttendanceDto
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
    }
}
