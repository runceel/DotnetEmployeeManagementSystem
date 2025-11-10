using AttendanceService.Application.Services;
using AttendanceService.Domain.Repositories;
using AttendanceService.Infrastructure.Data;
using AttendanceService.Infrastructure.Messaging;
using AttendanceService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceService.Infrastructure;

/// <summary>
/// Infrastructure層の依存性注入設定
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Infrastructure層のサービスを追加
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IEventPublisher, RedisEventPublisher>();
        services.AddScoped<Application.Services.IAttendanceService, Application.Services.AttendanceService>();

        return services;
    }
}
