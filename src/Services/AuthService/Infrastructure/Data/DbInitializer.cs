using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Data;

/// <summary>
/// データベース初期化クラス
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// データベースを初期化し、ダミーユーザーをシード
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthDbContext>>();

        try
        {
            // データベースマイグレーション適用
            await context.Database.MigrateAsync();
            logger.LogInformation("データベースマイグレーションが完了しました。");

            // ダミーユーザーのシード
            await SeedDummyUsersAsync(userManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "データベース初期化中にエラーが発生しました。");
            throw;
        }
    }

    private static async Task SeedDummyUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        // テストユーザー1
        if (await userManager.FindByNameAsync("testuser") == null)
        {
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded)
            {
                logger.LogInformation("ダミーユーザー 'testuser' を作成しました。");
            }
            else
            {
                logger.LogWarning("ダミーユーザー 'testuser' の作成に失敗しました: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // テストユーザー2
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@example.com",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Admin123!");
            if (result.Succeeded)
            {
                logger.LogInformation("ダミーユーザー 'admin' を作成しました。");
            }
            else
            {
                logger.LogWarning("ダミーユーザー 'admin' の作成に失敗しました: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
