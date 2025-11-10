using AttendanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceService.Infrastructure.Data;

/// <summary>
/// 勤怠データベースコンテキスト
/// </summary>
public class AttendanceDbContext : DbContext
{
    public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 勤怠記録
    /// </summary>
    public DbSet<Attendance> Attendances => Set<Attendance>();

    /// <summary>
    /// 休暇申請
    /// </summary>
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.EmployeeId)
                .IsRequired();

            entity.Property(a => a.WorkDate)
                .IsRequired();

            entity.Property(a => a.Type)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(a => a.Notes)
                .HasMaxLength(500);

            entity.Property(a => a.CreatedAt)
                .IsRequired();

            entity.Property(a => a.UpdatedAt)
                .IsRequired();

            entity.HasIndex(a => a.EmployeeId);
            entity.HasIndex(a => a.WorkDate);
            entity.HasIndex(a => new { a.EmployeeId, a.WorkDate })
                .IsUnique();
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(lr => lr.Id);

            entity.Property(lr => lr.EmployeeId)
                .IsRequired();

            entity.Property(lr => lr.Type)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(lr => lr.StartDate)
                .IsRequired();

            entity.Property(lr => lr.EndDate)
                .IsRequired();

            entity.Property(lr => lr.Reason)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(lr => lr.Status)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(lr => lr.ApproverComment)
                .HasMaxLength(1000);

            entity.Property(lr => lr.CreatedAt)
                .IsRequired();

            entity.Property(lr => lr.UpdatedAt)
                .IsRequired();

            entity.HasIndex(lr => lr.EmployeeId);
            entity.HasIndex(lr => lr.Status);
            entity.HasIndex(lr => new { lr.StartDate, lr.EndDate });
        });
    }
}
