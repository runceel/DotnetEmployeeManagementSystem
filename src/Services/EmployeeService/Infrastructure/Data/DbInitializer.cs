using EmployeeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmployeeService.Infrastructure.Data;

/// <summary>
/// データベース初期化
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// データベースを初期化してサンプルデータを投入
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<EmployeeDbContext>>();

        try
        {
            // データベースが存在しない場合は作成し、マイグレーションを適用
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed.");

            // データが既に存在する場合はスキップ
            if (await context.Employees.Take(1).AnyAsync())
            {
                logger.LogInformation("Database already seeded.");
                return;
            }

            // サンプルデータを投入
            var employees = new[]
            {
                new Employee(
                    "太郎",
                    "山田",
                    "yamada.taro@example.com",
                    new DateTime(2020, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                    "開発部",
                    "シニアエンジニア"
                ),
                new Employee(
                    "花子",
                    "佐藤",
                    "sato.hanako@example.com",
                    new DateTime(2019, 7, 15, 0, 0, 0, DateTimeKind.Utc),
                    "営業部",
                    "マネージャー"
                ),
                new Employee(
                    "次郎",
                    "田中",
                    "tanaka.jiro@example.com",
                    new DateTime(2021, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    "開発部",
                    "ジュニアエンジニア"
                ),
                new Employee(
                    "美咲",
                    "鈴木",
                    "suzuki.misaki@example.com",
                    new DateTime(2018, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    "人事部",
                    "ディレクター"
                ),
                new Employee(
                    "健太",
                    "高橋",
                    "takahashi.kenta@example.com",
                    new DateTime(2022, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                    "マーケティング部",
                    "アシスタント"
                )
            };

            await context.Employees.AddRangeAsync(employees);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Database seeded with {Count} employees.", employees.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
