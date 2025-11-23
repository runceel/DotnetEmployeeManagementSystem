using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using ModelContextProtocol.Server;

namespace AttendanceService.API.Mcp.Tools;

/// <summary>
/// 休暇申請管理に関するMCPツール
/// </summary>
[McpServerToolType]
public class LeaveRequestTools
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly ILogger<LeaveRequestTools> _logger;

    public LeaveRequestTools(
        ILeaveRequestRepository leaveRequestRepository,
        ILogger<LeaveRequestTools> logger)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _logger = logger;
    }

    /// <summary>
    /// 指定された休暇申請IDの詳細情報を取得します。休暇申請の種別、期間、ステータス、承認情報などが取得できます。
    /// </summary>
    [McpServerTool]
    public async Task<LeaveRequestDetailResponse> GetLeaveRequestAsync(
        string leaveRequestId)
    {
        ArgumentNullException.ThrowIfNull(leaveRequestId);

        if (!Guid.TryParse(leaveRequestId, out var id))
        {
            throw new ArgumentException("Invalid leave request ID format", nameof(leaveRequestId));
        }

        _logger.LogInformation("MCP Tool: GetLeaveRequest - LeaveRequestId: {LeaveRequestId}", id);

        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            throw new InvalidOperationException($"休暇申請ID {leaveRequestId} が見つかりませんでした。");
        }

        return MapToDetailResponse(leaveRequest);
    }

    /// <summary>
    /// 従業員の休暇申請一覧を取得します。ステータスを指定して絞り込むことができます。
    /// </summary>
    [McpServerTool]
    public async Task<LeaveRequestListResponse> ListLeaveRequestsAsync(
        string? employeeId = null,
        string? status = null)
    {
        _logger.LogInformation("MCP Tool: ListLeaveRequests - EmployeeId: {EmployeeId}, Status: {Status}",
            employeeId, status);

        IEnumerable<LeaveRequest> leaveRequests;

        if (!string.IsNullOrEmpty(employeeId))
        {
            if (!Guid.TryParse(employeeId, out var empId))
            {
                throw new ArgumentException("Invalid employee ID format", nameof(employeeId));
            }
            leaveRequests = await _leaveRequestRepository.GetByEmployeeIdAsync(empId);
        }
        else if (!string.IsNullOrEmpty(status))
        {
            if (!Enum.TryParse<LeaveRequestStatus>(status, true, out var statusEnum))
            {
                throw new ArgumentException("Invalid status. Valid values: Pending, Approved, Rejected, Cancelled", nameof(status));
            }
            leaveRequests = await _leaveRequestRepository.GetByStatusAsync(statusEnum);
        }
        else
        {
            leaveRequests = await _leaveRequestRepository.GetAllAsync();
        }

        var leaveRequestList = leaveRequests.Select(lr => new LeaveRequestSummary(
            Id: lr.Id.ToString(),
            EmployeeId: lr.EmployeeId.ToString(),
            Type: lr.Type.ToString(),
            StartDate: lr.StartDate,
            EndDate: lr.EndDate,
            Days: lr.CalculateDays(),
            Status: lr.Status.ToString(),
            CreatedAt: lr.CreatedAt
        )).ToList();

        return new LeaveRequestListResponse(
            LeaveRequests: leaveRequestList,
            TotalCount: leaveRequestList.Count
        );
    }

    /// <summary>
    /// 新しい休暇申請を作成します。休暇種別、期間、申請理由を指定して申請できます。
    /// </summary>
    [McpServerTool]
    public async Task<LeaveRequestDetailResponse> CreateLeaveRequestAsync(
        string employeeId,
        string leaveType,
        string startDate,
        string endDate,
        string reason)
    {
        ArgumentNullException.ThrowIfNull(employeeId);
        ArgumentNullException.ThrowIfNull(leaveType);
        ArgumentNullException.ThrowIfNull(startDate);
        ArgumentNullException.ThrowIfNull(endDate);
        ArgumentNullException.ThrowIfNull(reason);

        if (!Guid.TryParse(employeeId, out var empId))
        {
            throw new ArgumentException("Invalid employee ID format", nameof(employeeId));
        }

        if (!Enum.TryParse<LeaveType>(leaveType, true, out var type))
        {
            throw new ArgumentException("Invalid leave type. Valid values: PaidLeave, SickLeave, SpecialLeave, UnpaidLeave", nameof(leaveType));
        }

        if (!DateTime.TryParse(startDate, out var start))
        {
            throw new ArgumentException("Invalid start date format. Use ISO 8601 format (e.g., 2024-01-01)", nameof(startDate));
        }

        if (!DateTime.TryParse(endDate, out var end))
        {
            throw new ArgumentException("Invalid end date format. Use ISO 8601 format (e.g., 2024-01-05)", nameof(endDate));
        }

        _logger.LogInformation("MCP Tool: CreateLeaveRequest - EmployeeId: {EmployeeId}, Type: {Type}, StartDate: {StartDate}, EndDate: {EndDate}",
            empId, type, start, end);

        // Check for overlapping leave requests
        var overlappingRequests = await _leaveRequestRepository.GetOverlappingRequestsAsync(empId, start, end);
        var approvedOverlapping = overlappingRequests.Where(lr => lr.Status == LeaveRequestStatus.Approved).ToList();
        if (approvedOverlapping.Count != 0)
        {
            throw new InvalidOperationException($"指定期間に既に承認済みの休暇申請が存在します。");
        }

        var leaveRequest = new LeaveRequest(empId, type, start, end, reason);
        await _leaveRequestRepository.AddAsync(leaveRequest);

        _logger.LogInformation("MCP Tool: CreateLeaveRequest - Successfully created leave request {LeaveRequestId}", leaveRequest.Id);

        return MapToDetailResponse(leaveRequest);
    }

    /// <summary>
    /// 休暇申請を承認します。承認者IDとコメント（任意）を指定して承認できます。
    /// </summary>
    [McpServerTool]
    public async Task<LeaveRequestDetailResponse> ApproveLeaveRequestAsync(
        string leaveRequestId,
        string approverId,
        string? comment = null)
    {
        ArgumentNullException.ThrowIfNull(leaveRequestId);
        ArgumentNullException.ThrowIfNull(approverId);

        if (!Guid.TryParse(leaveRequestId, out var id))
        {
            throw new ArgumentException("Invalid leave request ID format", nameof(leaveRequestId));
        }

        if (!Guid.TryParse(approverId, out var approverGuid))
        {
            throw new ArgumentException("Invalid approver ID format", nameof(approverId));
        }

        _logger.LogInformation("MCP Tool: ApproveLeaveRequest - LeaveRequestId: {LeaveRequestId}, ApproverId: {ApproverId}",
            id, approverGuid);

        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            throw new InvalidOperationException($"休暇申請ID {leaveRequestId} が見つかりませんでした。");
        }

        leaveRequest.Approve(approverGuid, comment);
        await _leaveRequestRepository.UpdateAsync(leaveRequest);

        _logger.LogInformation("MCP Tool: ApproveLeaveRequest - Successfully approved leave request {LeaveRequestId}", id);

        return MapToDetailResponse(leaveRequest);
    }

    /// <summary>
    /// 休暇申請を却下します。承認者IDとコメント（任意）を指定して却下できます。
    /// </summary>
    [McpServerTool]
    public async Task<LeaveRequestDetailResponse> RejectLeaveRequestAsync(
        string leaveRequestId,
        string approverId,
        string? comment = null)
    {
        ArgumentNullException.ThrowIfNull(leaveRequestId);
        ArgumentNullException.ThrowIfNull(approverId);

        if (!Guid.TryParse(leaveRequestId, out var id))
        {
            throw new ArgumentException("Invalid leave request ID format", nameof(leaveRequestId));
        }

        if (!Guid.TryParse(approverId, out var approverGuid))
        {
            throw new ArgumentException("Invalid approver ID format", nameof(approverId));
        }

        _logger.LogInformation("MCP Tool: RejectLeaveRequest - LeaveRequestId: {LeaveRequestId}, ApproverId: {ApproverId}",
            id, approverGuid);

        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            throw new InvalidOperationException($"休暇申請ID {leaveRequestId} が見つかりませんでした。");
        }

        leaveRequest.Reject(approverGuid, comment);
        await _leaveRequestRepository.UpdateAsync(leaveRequest);

        _logger.LogInformation("MCP Tool: RejectLeaveRequest - Successfully rejected leave request {LeaveRequestId}", id);

        return MapToDetailResponse(leaveRequest);
    }

    /// <summary>
    /// 休暇申請をキャンセルします。申請者本人のみキャンセルできます。
    /// </summary>
    [McpServerTool]
    public async Task<LeaveRequestDetailResponse> CancelLeaveRequestAsync(
        string leaveRequestId)
    {
        ArgumentNullException.ThrowIfNull(leaveRequestId);

        if (!Guid.TryParse(leaveRequestId, out var id))
        {
            throw new ArgumentException("Invalid leave request ID format", nameof(leaveRequestId));
        }

        _logger.LogInformation("MCP Tool: CancelLeaveRequest - LeaveRequestId: {LeaveRequestId}", id);

        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            throw new InvalidOperationException($"休暇申請ID {leaveRequestId} が見つかりませんでした。");
        }

        leaveRequest.Cancel();
        await _leaveRequestRepository.UpdateAsync(leaveRequest);

        _logger.LogInformation("MCP Tool: CancelLeaveRequest - Successfully cancelled leave request {LeaveRequestId}", id);

        return MapToDetailResponse(leaveRequest);
    }

    private static LeaveRequestDetailResponse MapToDetailResponse(LeaveRequest leaveRequest)
    {
        return new LeaveRequestDetailResponse(
            Id: leaveRequest.Id.ToString(),
            EmployeeId: leaveRequest.EmployeeId.ToString(),
            Type: leaveRequest.Type.ToString(),
            StartDate: leaveRequest.StartDate,
            EndDate: leaveRequest.EndDate,
            Days: leaveRequest.CalculateDays(),
            Reason: leaveRequest.Reason,
            Status: leaveRequest.Status.ToString(),
            ApproverId: leaveRequest.ApproverId?.ToString(),
            ApprovedAt: leaveRequest.ApprovedAt,
            ApproverComment: leaveRequest.ApproverComment,
            CreatedAt: leaveRequest.CreatedAt,
            UpdatedAt: leaveRequest.UpdatedAt
        );
    }
}

// Response DTO definitions

public record LeaveRequestDetailResponse(
    string Id,
    string EmployeeId,
    string Type,
    DateTime StartDate,
    DateTime EndDate,
    int Days,
    string Reason,
    string Status,
    string? ApproverId,
    DateTime? ApprovedAt,
    string? ApproverComment,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record LeaveRequestSummary(
    string Id,
    string EmployeeId,
    string Type,
    DateTime StartDate,
    DateTime EndDate,
    int Days,
    string Status,
    DateTime CreatedAt
);

public record LeaveRequestListResponse(
    IReadOnlyList<LeaveRequestSummary> LeaveRequests,
    int TotalCount
);
