using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthService.Infrastructure;

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

        // DbContextの登録
        services.AddDbContext<AuthDbContext>(options =>
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

        // ASP.NET Core Identityの登録
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // パスワードの要件（開発用に緩和）
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // ユーザー設定
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AuthDbContext>()
        .AddDefaultTokenProviders();

        // Application層のサービス実装を登録
        services.AddScoped<IAuthService, Services.AuthService>();
        services.AddScoped<IJwtTokenGenerator, Services.JwtTokenGenerator>();

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
