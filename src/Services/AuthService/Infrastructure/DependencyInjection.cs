using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure;

/// <summary>
/// Infrastructure層の依存性注入設定
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Infrastructure層のサービスを追加
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // DbContextの登録
        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlite(connectionString));

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

        return services;
    }
}
