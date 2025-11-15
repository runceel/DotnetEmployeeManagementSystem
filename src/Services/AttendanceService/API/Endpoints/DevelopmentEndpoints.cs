using AttendanceService.Domain.Enums;
using AttendanceService.Domain.Repositories;
using AttendanceService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceService.API.Endpoints;

public static class DevelopmentEndpoints
{
    public static IEndpointRouteBuilder MapDevelopmentEndpoints(this IEndpointRouteBuilder app)
    {
        // Health check endpoint
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("HealthCheck")
            .WithTags("Health");

        // Development-only endpoints
        var environment = app.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (environment.IsDevelopment())
        {
            // シードデータをクリア
            app.MapDelete("/api/dev/clear-seed-data", ClearSeedData)
                .WithName("ClearSeedData")
                .WithTags("Development")
                .WithSummary("シードデータをクリア")
                .WithDescription("開発環境専用：全ての勤怠記録と休暇申請を削除します。")
                .ExcludeFromDescription();

            // 特定の従業員に対してシードデータを生成
            app.MapPost("/api/dev/seed-attendances", SeedAttendances)
                .WithName("SeedAttendances")
                .WithTags("Development")
                .ExcludeFromDescription();
        }

        return app;
    }

    private static async Task<IResult> ClearSeedData(
        [FromServices] IServiceProvider services)
    {
        try
        {
            await DbInitializer.ClearSeedDataAsync(services);
            return Results.Ok(new { message = "Seed data cleared successfully" });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> SeedAttendances(
        [FromBody] List<Guid> employeeIds,
        [FromServices] IAttendanceRepository attendanceRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var attendances = new List<Domain.Entities.Attendance>();
            var random = new Random(42);
            var today = DateTime.UtcNow.Date;
            var startDate = today.AddMonths(-3);

            foreach (var employeeId in employeeIds)
            {
                var currentDate = startDate;
                while (currentDate <= today)
                {
                    if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        if (random.Next(100) < 90)
                        {
                            var attendanceType = random.Next(100) switch
                            {
                                < 80 => AttendanceType.Normal,
                                < 90 => AttendanceType.Remote,
                                < 95 => AttendanceType.BusinessTrip,
                                _ => AttendanceType.HalfDay
                            };

                            var attendance = new Domain.Entities.Attendance(employeeId, currentDate, attendanceType);

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

                            var workHours = 7 + random.Next(0, 4) + (random.NextDouble() * 0.5);
                            var checkOutTime = checkInTime.AddHours(workHours);
                            attendance.CheckOut(checkOutTime);

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

            foreach (var attendance in attendances)
            {
                await attendanceRepository.AddAsync(attendance, cancellationToken);
            }

            return Results.Ok(new { message = $"Seeded {attendances.Count} attendance records for {employeeIds.Count} employees" });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
