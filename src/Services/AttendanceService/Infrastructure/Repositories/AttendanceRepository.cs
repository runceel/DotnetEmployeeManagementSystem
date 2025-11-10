using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Repositories;
using AttendanceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceService.Infrastructure.Repositories;

/// <summary>
/// 勤怠記録リポジトリ実装
/// </summary>
public class AttendanceRepository : IAttendanceRepository
{
    private readonly AttendanceDbContext _context;

    public AttendanceRepository(AttendanceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Attendance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Attendance>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .OrderByDescending(a => a.WorkDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Attendance>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.WorkDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Attendance>> GetByEmployeeIdAndDateRangeAsync(
        Guid employeeId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .Where(a => a.EmployeeId == employeeId &&
                       a.WorkDate >= startDate.Date &&
                       a.WorkDate <= endDate.Date)
            .OrderBy(a => a.WorkDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Attendance?> GetByEmployeeIdAndDateAsync(
        Guid employeeId,
        DateTime workDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.WorkDate == workDate.Date, cancellationToken);
    }

    public async Task<Attendance> AddAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        await _context.Attendances.AddAsync(attendance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return attendance;
    }

    public async Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        _context.Attendances.Update(attendance);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (attendance != null)
        {
            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
