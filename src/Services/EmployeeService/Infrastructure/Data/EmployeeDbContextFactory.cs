using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EmployeeService.Infrastructure.Data;

/// <summary>
/// デザイン時のDbContextファクトリ（マイグレーション作成用）
/// </summary>
public class EmployeeDbContextFactory : IDesignTimeDbContextFactory<EmployeeDbContext>
{
    public EmployeeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EmployeeDbContext>();
        optionsBuilder.UseSqlite("Data Source=employees.db");

        return new EmployeeDbContext(optionsBuilder.Options);
    }
}
