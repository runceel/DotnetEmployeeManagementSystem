using AttendanceService.API.Mappers;
using AttendanceService.Application.Services;
using AttendanceService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.AttendanceService;

namespace AttendanceService.API.Endpoints;

public static class LeaveRequestEndpoints
{
    public static IEndpointRouteBuilder MapLeaveRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var leaveRequests = app.MapGroup("/api/leaverequests")
            .WithTags("LeaveRequests");

        leaveRequests.MapGet("/", GetAllLeaveRequests)
            .WithName("GetAllLeaveRequests")
            .WithSummary("全休暇申請を取得")
            .WithDescription("システム内の全休暇申請を取得します。")
            .Produces<IEnumerable<LeaveRequestDto>>();

        leaveRequests.MapGet("/{id:guid}", GetLeaveRequestById)
            .WithName("GetLeaveRequestById")
            .WithSummary("IDで休暇申請を取得")
            .WithDescription("指定されたIDの休暇申請を取得します。")
            .Produces<LeaveRequestDto>()
            .Produces(StatusCodes.Status404NotFound);

        leaveRequests.MapGet("/employee/{employeeId:guid}", GetLeaveRequestsByEmployee)
            .WithName("GetLeaveRequestsByEmployee")
            .WithSummary("従業員別の休暇申請を取得")
            .WithDescription("指定された従業員の全休暇申請を取得します。")
            .Produces<IEnumerable<LeaveRequestDto>>();

        leaveRequests.MapGet("/status/{status}", GetLeaveRequestsByStatus)
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

        leaveRequests.MapPost("/", CreateLeaveRequest)
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

        leaveRequests.MapPost("/{id:guid}/approve", ApproveLeaveRequest)
            .WithName("ApproveLeaveRequest")
            .WithSummary("休暇申請を承認")
            .WithDescription("指定された休暇申請を承認します。承認者IDとコメントを指定できます。")
            .Produces<LeaveRequestDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        leaveRequests.MapPost("/{id:guid}/reject", RejectLeaveRequest)
            .WithName("RejectLeaveRequest")
            .WithSummary("休暇申請を却下")
            .WithDescription("指定された休暇申請を却下します。承認者IDと却下理由を指定できます。")
            .Produces<LeaveRequestDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        leaveRequests.MapPost("/{id:guid}/cancel", CancelLeaveRequest)
            .WithName("CancelLeaveRequest")
            .WithSummary("休暇申請をキャンセル")
            .WithDescription("指定された休暇申請をキャンセルします。申請者のみキャンセルできます。")
            .Produces<LeaveRequestDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetAllLeaveRequests(
        [FromServices] ILeaveRequestService leaveRequestService,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var requests = await leaveRequestService.GetAllLeaveRequestsAsync(cancellationToken);
            var dtos = requests.Select(LeaveRequestMapper.MapToDto);
            return Results.Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "全休暇申請取得中にエラーが発生しました");
            return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
        }
    }

    private static async Task<IResult> GetLeaveRequestById(
        Guid id,
        [FromServices] ILeaveRequestService leaveRequestService,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var leaveRequest = await leaveRequestService.GetLeaveRequestByIdAsync(id, cancellationToken);
            if (leaveRequest == null)
            {
                return Results.NotFound(new { error = "休暇申請が見つかりません。" });
            }

            return Results.Ok(LeaveRequestMapper.MapToDto(leaveRequest));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "休暇申請 {LeaveRequestId} 取得中にエラーが発生しました", id);
            return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
        }
    }

    private static async Task<IResult> GetLeaveRequestsByEmployee(
        Guid employeeId,
        [FromServices] ILeaveRequestService leaveRequestService,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var requests = await leaveRequestService.GetLeaveRequestsByEmployeeIdAsync(employeeId, cancellationToken);
            var dtos = requests.Select(LeaveRequestMapper.MapToDto);
            return Results.Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "従業員 {EmployeeId} の休暇申請取得中にエラーが発生しました", employeeId);
            return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
        }
    }

    private static async Task<IResult> GetLeaveRequestsByStatus(
        string status,
        [FromServices] ILeaveRequestService leaveRequestService,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<LeaveRequestStatus>(status, ignoreCase: true, out var leaveStatus))
            {
                return Results.BadRequest(new { error = "無効なステータスです。有効な値: Pending, Approved, Rejected, Cancelled" });
            }

            var requests = await leaveRequestService.GetLeaveRequestsByStatusAsync(leaveStatus, cancellationToken);
            var dtos = requests.Select(LeaveRequestMapper.MapToDto);
            return Results.Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ステータス {Status} の休暇申請取得中にエラーが発生しました", status);
            return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
        }
    }

    private static async Task<IResult> CreateLeaveRequest(
        [FromBody] CreateLeaveRequestRequest request,
        [FromServices] ILeaveRequestService leaveRequestService,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
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

            return Results.Created($"/api/leaverequests/{leaveRequest.Id}", LeaveRequestMapper.MapToDto(leaveRequest));
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

    private static async Task<IResult> ApproveLeaveRequest(
        Guid id,
        [FromBody] ApproveLeaveRequestRequest request,
        [FromServices] ILeaveRequestService leaveRequestService,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request);
            
            var leaveRequest = await leaveRequestService.ApproveLeaveRequestAsync(
                id,
                request.ApproverId,
                request.Comment,
                cancellationToken);

            return Results.Ok(LeaveRequestMapper.MapToDto(leaveRequest));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "休暇申請 {LeaveRequestId} の承認処理中にエラーが発生しました", id);
            return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
        }
    }

    private static async Task<IResult> RejectLeaveRequest(
        Guid id,
        [FromBody] RejectLeaveRequestRequest request,
        [FromServices] ILeaveRequestService leaveRequestService,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request);
            
            var leaveRequest = await leaveRequestService.RejectLeaveRequestAsync(
                id,
                request.ApproverId,
                request.Comment,
                cancellationToken);

            return Results.Ok(LeaveRequestMapper.MapToDto(leaveRequest));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "休暇申請 {LeaveRequestId} の却下処理中にエラーが発生しました", id);
            return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
        }
    }

    private static async Task<IResult> CancelLeaveRequest(
        Guid id,
        [FromServices] ILeaveRequestService leaveRequestService,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var leaveRequest = await leaveRequestService.CancelLeaveRequestAsync(id, cancellationToken);
            return Results.Ok(LeaveRequestMapper.MapToDto(leaveRequest));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "休暇申請 {LeaveRequestId} のキャンセル処理中にエラーが発生しました", id);
            return Results.BadRequest(new { error = ex.Message, traceId = Guid.NewGuid().ToString() });
        }
    }
}
