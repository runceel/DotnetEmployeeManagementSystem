namespace Shared.Contracts.AttendanceService;

/// <summary>
/// 休暇申請DTO
/// </summary>
public record LeaveRequestDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid? ApproverId { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public string? ApproverComment { get; init; }
    public int Days { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
