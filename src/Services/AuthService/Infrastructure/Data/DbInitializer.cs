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
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthDbContext>>();

        try
        {
            // データベースマイグレーション適用
            await context.Database.MigrateAsync();
            logger.LogInformation("データベースマイグレーションが完了しました。");

            // ロールのシード
            await SeedRolesAsync(roleManager, logger);

            // ダミーユーザーのシード
            await SeedDummyUsersAsync(userManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "データベース初期化中にエラーが発生しました。");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        // Adminロールの作成
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var result = await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (result.Succeeded)
            {
                logger.LogInformation("ロール 'Admin' を作成しました。");
            }
            else
            {
                logger.LogWarning("ロール 'Admin' の作成に失敗しました: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Userロールの作成
        if (!await roleManager.RoleExistsAsync("User"))
        {
            var result = await roleManager.CreateAsync(new IdentityRole("User"));
            if (result.Succeeded)
            {
                logger.LogInformation("ロール 'User' を作成しました。");
            }
            else
            {
                logger.LogWarning("ロール 'User' の作成に失敗しました: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task SeedDummyUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        // テストユーザー1（一般ユーザー）
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
                await userManager.AddToRoleAsync(user, "User");
                logger.LogInformation("ダミーユーザー 'testuser' を作成し、Userロールを割り当てました。");
            }
            else
            {
                logger.LogWarning("ダミーユーザー 'testuser' の作成に失敗しました: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // テストユーザー2（管理者）
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
                await userManager.AddToRoleAsync(user, "Admin");
                logger.LogInformation("ダミーユーザー 'admin' を作成し、Adminロールを割り当てました。");
            }
            else
            {
                logger.LogWarning("ダミーユーザー 'admin' の作成に失敗しました: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
