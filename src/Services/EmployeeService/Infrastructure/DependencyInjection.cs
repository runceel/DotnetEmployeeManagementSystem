using EmployeeService.Application.Services;
using EmployeeService.Domain.Repositories;
using EmployeeService.Infrastructure.Data;
using EmployeeService.Infrastructure.Messaging;
using EmployeeService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmployeeService.Infrastructure;

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
        // Production環境でSQL Serverの接続文字列が存在する場合はSQL Serverを使用
        var useSqlServer = environment?.IsProduction() == true && 
                          IsSqlServerConnectionString(connectionString);

        services.AddDbContext<EmployeeDbContext>(options =>
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

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IEventPublisher, RedisEventPublisher>();

        return services;
    }

    /// <summary>
    /// 接続文字列がSQL Server用かどうかを判定
    /// </summary>
    private static bool IsSqlServerConnectionString(string connectionString)
    {
        return connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) && 
               !connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase);
    }
}
