using System.Net;
using System.Net.Http.Json;
using AttendanceService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Contracts.AttendanceService;

namespace AttendanceService.Integration.Tests.Api;

public class AttendanceApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public AttendanceApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;
    }

    private HttpClient CreateClient()
    {
        var dbName = $"TestDb_{Guid.NewGuid()}";

        var client = _baseFactory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                // Remove ALL DbContext related descriptors including the options
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AttendanceDbContext>));
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);

                var dbContextOptionsDescriptor = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions)).ToList();
                foreach (var descriptor in dbContextOptionsDescriptor)
                    services.Remove(descriptor);

                services.RemoveAll<AttendanceDbContext>();

                // Add in-memory database for testing
                services.AddDbContext<AttendanceDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                }, ServiceLifetime.Scoped);

                // Remove Redis-related services and replace with mock
                services.RemoveAll<AttendanceService.Application.Services.IEventPublisher>();
                services.AddScoped<AttendanceService.Application.Services.IEventPublisher, MockEventPublisher>();
            });
        }).CreateClient();

        return client;
    }

    // Mock event publisher for testing
    private class MockEventPublisher : AttendanceService.Application.Services.IEventPublisher
    {
        public Task PublishAsync<T>(string channel, T @event, CancellationToken cancellationToken = default) where T : class
        {
            // No-op for tests
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task CheckIn_WhenFirstTime_ShouldReturnOk()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        // Use yesterday to avoid any date validation issues
        var checkInTime = DateTime.UtcNow.AddDays(-1).Date.AddHours(9);
        var request = new CheckInRequest
        {
            EmployeeId = employeeId,
            CheckInTime = checkInTime
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/attendances/checkin", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AttendanceDto>();
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(checkInTime, result.CheckInTime);
        Assert.Null(result.CheckOutTime);
        Assert.Equal(checkInTime.Date, result.WorkDate);
    }

    [Fact]
    public async Task CheckIn_WhenDuplicate_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var checkInTime = DateTime.UtcNow.AddDays(-1).Date.AddHours(9);
        var request = new CheckInRequest
        {
            EmployeeId = employeeId,
            CheckInTime = checkInTime
        };

        // First check-in
        await client.PostAsJsonAsync("/api/attendances/checkin", request);

        // Act - Second check-in
        var response = await client.PostAsJsonAsync("/api/attendances/checkin", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckOut_AfterCheckIn_ShouldReturnOk()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var baseDate = DateTime.UtcNow.AddDays(-1).Date;
        var checkInTime = baseDate.AddHours(9);
        var checkOutTime = baseDate.AddHours(18);

        // Check in first
        var checkInRequest = new CheckInRequest
        {
            EmployeeId = employeeId,
            CheckInTime = checkInTime
        };
        await client.PostAsJsonAsync("/api/attendances/checkin", checkInRequest);

        // Act - Check out
        var checkOutRequest = new CheckOutRequest
        {
            EmployeeId = employeeId,
            CheckOutTime = checkOutTime
        };
        var response = await client.PostAsJsonAsync("/api/attendances/checkout", checkOutRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AttendanceDto>();
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(checkInTime, result.CheckInTime);
        Assert.Equal(checkOutTime, result.CheckOutTime);
        Assert.Equal(9.0, result.WorkHours);
    }

    [Fact]
    public async Task CheckOut_WithoutCheckIn_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var checkOutTime = DateTime.UtcNow.AddDays(-1).Date.AddHours(18);

        var request = new CheckOutRequest
        {
            EmployeeId = employeeId,
            CheckOutTime = checkOutTime
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/attendances/checkout", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckOut_WhenDuplicate_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var baseDate = DateTime.UtcNow.AddDays(-1).Date;
        var checkInTime = baseDate.AddHours(9);
        var checkOutTime = baseDate.AddHours(18);

        // Check in and out
        var checkInRequest = new CheckInRequest
        {
            EmployeeId = employeeId,
            CheckInTime = checkInTime
        };
        await client.PostAsJsonAsync("/api/attendances/checkin", checkInRequest);

        var checkOutRequest = new CheckOutRequest
        {
            EmployeeId = employeeId,
            CheckOutTime = checkOutTime
        };
        await client.PostAsJsonAsync("/api/attendances/checkout", checkOutRequest);

        // Act - Second check-out
        var response = await client.PostAsJsonAsync("/api/attendances/checkout", checkOutRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckIn_WithEmptyEmployeeId_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var checkInTime = DateTime.UtcNow.AddDays(-1).Date.AddHours(9);
        var request = new CheckInRequest
        {
            EmployeeId = Guid.Empty,
            CheckInTime = checkInTime
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/attendances/checkin", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MultipleEmployees_CanCheckInOnSameDay()
    {
        // Arrange
        var client = CreateClient();
        var employee1Id = Guid.NewGuid();
        var employee2Id = Guid.NewGuid();
        var checkInTime = DateTime.UtcNow.AddDays(-1).Date.AddHours(9);

        var request1 = new CheckInRequest
        {
            EmployeeId = employee1Id,
            CheckInTime = checkInTime
        };

        var request2 = new CheckInRequest
        {
            EmployeeId = employee2Id,
            CheckInTime = checkInTime.AddMinutes(30)
        };

        // Act
        var response1 = await client.PostAsJsonAsync("/api/attendances/checkin", request1);
        var response2 = await client.PostAsJsonAsync("/api/attendances/checkin", request2);

        // Assert
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();

        var result1 = await response1.Content.ReadFromJsonAsync<AttendanceDto>();
        var result2 = await response2.Content.ReadFromJsonAsync<AttendanceDto>();

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotEqual(result1.Id, result2.Id);
    }

    [Fact]
    public async Task CheckInCheckOut_AcrossMidnight_ShouldFail()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var baseDate = DateTime.UtcNow.AddDays(-2).Date; // Use day before yesterday
        var checkInTime = baseDate.AddHours(22); // 22:00
        var checkOutTime = baseDate.AddDays(1).AddHours(2); // 02:00 next day

        // Check in
        var checkInRequest = new CheckInRequest
        {
            EmployeeId = employeeId,
            CheckInTime = checkInTime
        };
        await client.PostAsJsonAsync("/api/attendances/checkin", checkInRequest);

        // Act - Check out next day
        var checkOutRequest = new CheckOutRequest
        {
            EmployeeId = employeeId,
            CheckOutTime = checkOutTime
        };
        var response = await client.PostAsJsonAsync("/api/attendances/checkout", checkOutRequest);

        // Assert
        // This should fail because checkout date is different from check-in date
        // Our domain logic requires checkout to be for the same work date
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
