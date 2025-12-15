using AttendanceService.Application.Services;
using AttendanceService.Domain.Repositories;
using AttendanceService.Infrastructure.Data;
using AttendanceService.Infrastructure.Messaging;
using AttendanceService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        string connectionString,
        IHostEnvironment? environment = null)
    {
        // 環境に応じてデータベースプロバイダーを切り替え
        var useSqlServer = environment?.IsProduction() == true && 
                          IsSqlServerConnectionString(connectionString);

        services.AddDbContext<AttendanceDbContext>(options =>
        {
            if (useSqlServer)
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IEventPublisher, RedisEventPublisher>();
        services.AddScoped<Application.Services.IAttendanceService, Application.Services.AttendanceService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        
        // 勤怠異常検知サービスの登録
        services.AddScoped<Domain.Services.IAttendanceAnomalyDetector, Domain.Services.AttendanceAnomalyDetector>();

        return services;
    }

    /// <summary>
    /// 接続文字列がSQL Server用かどうかを判定
    /// </summary>
    private static bool IsSqlServerConnectionString(string connectionString)
    {
        return (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase)) && 
               !connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase);
    }
}
