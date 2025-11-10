using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using AttendanceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceService.Infrastructure.Repositories;

/// <summary>
/// 休暇申請リポジトリ実装
/// </summary>
public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly AttendanceDbContext _context;

    public LeaveRequestRepository(AttendanceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveRequests
            .FirstOrDefaultAsync(lr => lr.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LeaveRequests
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveRequests
            .Where(lr => lr.EmployeeId == employeeId)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveRequestStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveRequests
            .Where(lr => lr.Status == status)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeaveRequest>> GetOverlappingRequestsAsync(
        Guid employeeId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.LeaveRequests
            .Where(lr => lr.EmployeeId == employeeId &&
                        lr.Status != LeaveRequestStatus.Rejected &&
                        lr.Status != LeaveRequestStatus.Cancelled &&
                        lr.StartDate <= endDate.Date &&
                        lr.EndDate >= startDate.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeaveRequest> AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        await _context.LeaveRequests.AddAsync(leaveRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return leaveRequest;
    }

    public async Task UpdateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        _context.LeaveRequests.Update(leaveRequest);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(lr => lr.Id == id, cancellationToken);

        if (leaveRequest != null)
        {
            _context.LeaveRequests.Remove(leaveRequest);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
