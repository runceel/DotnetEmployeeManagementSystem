using Shared.Contracts.AttendanceService;

namespace AttendanceService.API.Mappers;

public static class LeaveRequestMapper
{
    public static LeaveRequestDto MapToDto(Domain.Entities.LeaveRequest leaveRequest)
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
}
