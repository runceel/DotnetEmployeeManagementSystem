using EmployeeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Infrastructure.Data;

/// <summary>
/// 従業員データベースコンテキスト
/// </summary>
public class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 従業員
    /// </summary>
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.Department)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Position)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.HireDate)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();
        });
    }
}
