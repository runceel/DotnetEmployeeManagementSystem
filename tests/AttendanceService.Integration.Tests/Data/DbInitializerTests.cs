using AttendanceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AttendanceService.Integration.Tests.Data;

/// <summary>
/// DbInitializerのテスト
/// </summary>
public class DbInitializerTests
{
    [Fact]
    public async Task InitializeAsync_WithEmployeeIds_ShouldGenerateSeedData()
    {
        // Arrange
        var dbName = $"TestDb_{Guid.NewGuid()}";
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder => builder.AddConsole());
        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseInMemoryDatabase(dbName), ServiceLifetime.Scoped);

        var serviceProvider = services.BuildServiceProvider();

        var employeeIds = new[]
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        // Act
        await DbInitializer.InitializeAsync(serviceProvider, employeeIds);

        // Assert - Create a new service provider scope and verify the data persisted
        using var verificationScope = serviceProvider.CreateScope();
        var context = verificationScope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
        
        var attendanceCount = await context.Attendances.CountAsync();
        var leaveRequestCount = await context.LeaveRequests.CountAsync();

        Assert.True(attendanceCount > 0, $"Should have generated attendance records but got {attendanceCount}");
        Assert.True(leaveRequestCount > 0, $"Should have generated leave requests but got {leaveRequestCount}");
        
        // Verify that attendances are for the provided employee IDs
        var distinctEmployeeIds = await context.Attendances
            .Select(a => a.EmployeeId)
            .Distinct()
            .ToListAsync();
        
        Assert.Equal(employeeIds.Length, distinctEmployeeIds.Count);
        Assert.All(employeeIds, id => Assert.Contains(id, distinctEmployeeIds));
    }

    [Fact]
    public async Task InitializeAsync_WithoutEmployeeIds_ShouldSkipSeedData()
    {
        // Arrange
        var dbName = $"TestDb_{Guid.NewGuid()}";
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder => builder.AddConsole());
        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseInMemoryDatabase(dbName), ServiceLifetime.Scoped);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        await DbInitializer.InitializeAsync(serviceProvider, null);

        // Assert
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
        
        var attendanceCount = await context.Attendances.CountAsync();
        var leaveRequestCount = await context.LeaveRequests.CountAsync();

        Assert.Equal(0, attendanceCount);
        Assert.Equal(0, leaveRequestCount);
    }

    [Fact]
    public async Task InitializeAsync_WithSkipSeedDataConfig_ShouldSkipSeedData()
    {
        // Arrange
        var dbName = $"TestDb_{Guid.NewGuid()}";
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AttendanceService:SkipSeedData", "true" }
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder => builder.AddConsole());
        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseInMemoryDatabase(dbName), ServiceLifetime.Scoped);

        var serviceProvider = services.BuildServiceProvider();

        var employeeIds = new[] { Guid.NewGuid() };

        // Act
        await DbInitializer.InitializeAsync(serviceProvider, employeeIds);

        // Assert
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
        
        var attendanceCount = await context.Attendances.CountAsync();
        Assert.Equal(0, attendanceCount);
    }

    [Fact]
    public async Task InitializeAsync_WhenDataAlreadyExists_ShouldSkipSeedData()
    {
        // Arrange
        var dbName = $"TestDb_{Guid.NewGuid()}";
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder => builder.AddConsole());
        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseInMemoryDatabase(dbName), ServiceLifetime.Scoped);

        var serviceProvider = services.BuildServiceProvider();

        var employeeIds = new[] { Guid.NewGuid() };

        // Act - First initialization
        await DbInitializer.InitializeAsync(serviceProvider, employeeIds);

        int initialCount;
        using (var scope1 = serviceProvider.CreateScope())
        {
            var context1 = scope1.ServiceProvider.GetRequiredService<AttendanceDbContext>();
            initialCount = await context1.Attendances.CountAsync();
        }

        // Act - Second initialization (should skip)
        await DbInitializer.InitializeAsync(serviceProvider, employeeIds);

        // Assert
        int finalCount;
        using (var scope2 = serviceProvider.CreateScope())
        {
            var context2 = scope2.ServiceProvider.GetRequiredService<AttendanceDbContext>();
            finalCount = await context2.Attendances.CountAsync();
        }

        Assert.Equal(initialCount, finalCount);
    }

    [Fact]
    public async Task ClearSeedDataAsync_ShouldRemoveAllData()
    {
        // Arrange
        var dbName = $"TestDb_{Guid.NewGuid()}";
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder => builder.AddConsole());
        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseInMemoryDatabase(dbName), ServiceLifetime.Scoped);

        var serviceProvider = services.BuildServiceProvider();

        var employeeIds = new[] { Guid.NewGuid() };

        // Seed data first
        await DbInitializer.InitializeAsync(serviceProvider, employeeIds);

        // Verify data was seeded
        using (var scope1 = serviceProvider.CreateScope())
        {
            var context1 = scope1.ServiceProvider.GetRequiredService<AttendanceDbContext>();
            var initialAttendanceCount = await context1.Attendances.CountAsync();
            var initialLeaveRequestCount = await context1.LeaveRequests.CountAsync();

            Assert.True(initialAttendanceCount > 0, $"Should have attendance data but got {initialAttendanceCount}");
            Assert.True(initialLeaveRequestCount > 0, $"Should have leave request data but got {initialLeaveRequestCount}");
        }

        // Act - Clear data
        await DbInitializer.ClearSeedDataAsync(serviceProvider);

        // Assert - Verify data was cleared
        using (var scope2 = serviceProvider.CreateScope())
        {
            var context2 = scope2.ServiceProvider.GetRequiredService<AttendanceDbContext>();
            var finalAttendanceCount = await context2.Attendances.CountAsync();
            var finalLeaveRequestCount = await context2.LeaveRequests.CountAsync();

            Assert.Equal(0, finalAttendanceCount);
            Assert.Equal(0, finalLeaveRequestCount);
        }
    }
}
