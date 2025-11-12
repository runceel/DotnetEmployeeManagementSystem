using AttendanceService.Domain.Entities;
using AttendanceService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AttendanceService.Infrastructure.Data;

/// <summary>
/// データベース初期化
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// データベースを初期化してサンプルデータを投入
    /// </summary>
    /// <param name="serviceProvider">サービスプロバイダー</param>
    /// <param name="employeeIds">従業員IDのリスト（指定しない場合は自動取得またはスキップ）</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider, IEnumerable<Guid>? employeeIds = null)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AttendanceDbContext>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        try
        {
            // データベースが存在しない場合は作成し、マイグレーションを適用
            // InMemoryDatabaseの場合はEnsureCreatedを使用
            var isInMemory = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
            if (isInMemory)
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("In-memory database created.");
            }
            else
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migration completed.");
            }

            // シードデータ生成を無効化するオプション
            var skipSeedingStr = configuration["AttendanceService:SkipSeedData"];
            if (!string.IsNullOrEmpty(skipSeedingStr) && bool.TryParse(skipSeedingStr, out var skipSeeding) && skipSeeding)
            {
                logger.LogInformation("Seed data generation is disabled by configuration.");
                return;
            }

            // データが既に存在する場合はスキップ
            if (await context.Attendances.Take(1).AnyAsync())
            {
                logger.LogInformation("Database already seeded.");
                return;
            }

            // 従業員IDが指定されていない場合はスキップ
            if (employeeIds == null || !employeeIds.Any())
            {
                logger.LogWarning("No employee IDs provided. Skipping seed data generation. " +
                    "Call InitializeAsync with employee IDs from EmployeeService to generate demo data.");
                return;
            }

            var employeeIdList = employeeIds.ToList();

            logger.LogInformation("Starting seed data generation for {EmployeeCount} employees.", employeeIdList.Count);

            var attendances = new List<Attendance>();
            var random = new Random(42); // 固定シード値で再現可能なデータを生成

            // 過去3ヶ月分のサンプルデータを生成
            var today = DateTime.UtcNow.Date;
            var startDate = today.AddMonths(-3);

            foreach (var employeeId in employeeIdList)
            {
                var currentDate = startDate;

                while (currentDate <= today)
                {
                    // 土日をスキップ
                    if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        // 90%の確率で出勤
                        if (random.Next(100) < 90)
                        {
                            var attendanceType = random.Next(100) switch
                            {
                                < 80 => AttendanceType.Normal,      // 80%
                                < 90 => AttendanceType.Remote,      // 10%
                                < 95 => AttendanceType.BusinessTrip, // 5%
                                _ => AttendanceType.HalfDay         // 5%
                            };

                            var attendance = new Attendance(employeeId, currentDate, attendanceType);

                            // 出勤時刻: 8:30 - 9:30の間でランダム
                            var checkInHour = 8;
                            var checkInMinute = random.Next(0, 61);
                            if (checkInMinute >= 30)
                            {
                                checkInHour = 9;
                                checkInMinute -= 30;
                            }
                            var checkInTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 
                                checkInHour, checkInMinute, 0, DateTimeKind.Utc);

                            attendance.CheckIn(checkInTime);

                            // 退勤時刻: 出勤時刻から7-10時間後
                            var workHours = 7 + random.Next(0, 4) + (random.NextDouble() * 0.5);
                            var checkOutTime = checkInTime.AddHours(workHours);
                            attendance.CheckOut(checkOutTime);

                            // たまに備考を追加
                            if (random.Next(100) < 10)
                            {
                                var notes = new[] { "打ち合わせ多数", "顧客訪問", "社内研修", "定期健康診断" };
                                var selectedNote = notes[random.Next(notes.Length)];
                                attendance.Update(attendanceType, selectedNote);
                            }

                            attendances.Add(attendance);
                        }
                    }

                    currentDate = currentDate.AddDays(1);
                }
            }

            // サンプル休暇申請データを生成
            var leaveRequests = new List<LeaveRequest>();
            
            // 各従業員に2-3件の休暇申請を追加
            foreach (var employeeId in employeeIdList)
            {
                var leaveCount = random.Next(2, 4);
                for (int i = 0; i < leaveCount; i++)
                {
                    var leaveType = random.Next(4) switch
                    {
                        0 => LeaveType.PaidLeave,
                        1 => LeaveType.SickLeave,
                        2 => LeaveType.SpecialLeave,
                        _ => LeaveType.UnpaidLeave
                    };

                    // 未来の日付で休暇申請を作成（現在日から+10日～+100日の範囲）
                    var leaveStartDate = DateTime.UtcNow.Date.AddDays(10 + random.Next(90));
                    var leaveDays = random.Next(1, 4);
                    var leaveEndDate = leaveStartDate.AddDays(leaveDays - 1);

                    var leaveRequest = new LeaveRequest(
                        employeeId,
                        leaveType,
                        leaveStartDate,
                        leaveEndDate,
                        "サンプル休暇申請"
                    );

                    // 75%の確率で承認済み
                    if (random.Next(100) < 75)
                    {
                        var approverId = employeeIdList[random.Next(employeeIdList.Count)];
                        leaveRequest.Approve(approverId, "承認しました");
                    }
                    // 10%の確率で却下
                    else if (random.Next(100) < 10)
                    {
                        var approverId = employeeIdList[random.Next(employeeIdList.Count)];
                        leaveRequest.Reject(approverId, "理由が不十分です");
                    }

                    leaveRequests.Add(leaveRequest);
                }
            }

            await context.Attendances.AddRangeAsync(attendances);
            await context.LeaveRequests.AddRangeAsync(leaveRequests);
            await context.SaveChangesAsync();

            logger.LogInformation("Database seeded with {AttendanceCount} attendances and {LeaveRequestCount} leave requests.",
                attendances.Count, leaveRequests.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    /// <summary>
    /// シードデータをクリア（開発環境専用）
    /// </summary>
    /// <param name="serviceProvider">サービスプロバイダー</param>
    public static async Task ClearSeedDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AttendanceDbContext>>();

        try
        {
            // 全ての勤怠記録と休暇申請を削除
            var attendanceCount = await context.Attendances.CountAsync();
            var leaveRequestCount = await context.LeaveRequests.CountAsync();

            context.Attendances.RemoveRange(context.Attendances);
            context.LeaveRequests.RemoveRange(context.LeaveRequests);
            
            await context.SaveChangesAsync();

            logger.LogInformation("Cleared {AttendanceCount} attendances and {LeaveRequestCount} leave requests.",
                attendanceCount, leaveRequestCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while clearing seed data.");
            throw;
        }
    }
}
