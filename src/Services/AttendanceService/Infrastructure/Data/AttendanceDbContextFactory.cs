using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AttendanceService.Infrastructure.Data;

/// <summary>
/// デザインタイム用のDbContextファクトリ
/// </summary>
public class AttendanceDbContextFactory : IDesignTimeDbContextFactory<AttendanceDbContext>
{
    public AttendanceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AttendanceDbContext>();
        optionsBuilder.UseSqlite("Data Source=attendance.db");

        return new AttendanceDbContext(optionsBuilder.Options);
    }
}
