using System.Net;
using System.Net.Http.Json;
using AttendanceService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.AttendanceService;

namespace AttendanceService.Integration.Tests.Api;

public class LeaveRequestApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public LeaveRequestApiIntegrationTests(WebApplicationFactory<Program> factory)
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
                // Add in-memory database for testing
                services.AddDbContext<AttendanceDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                // Register repositories
                services.AddScoped<AttendanceService.Domain.Repositories.IAttendanceRepository,
                    AttendanceService.Infrastructure.Repositories.AttendanceRepository>();
                services.AddScoped<AttendanceService.Domain.Repositories.ILeaveRequestRepository,
                    AttendanceService.Infrastructure.Repositories.LeaveRequestRepository>();

                // Register application services
                services.AddScoped<AttendanceService.Application.Services.IAttendanceService,
                    AttendanceService.Application.Services.AttendanceService>();
                services.AddScoped<AttendanceService.Application.Services.ILeaveRequestService,
                    AttendanceService.Application.Services.LeaveRequestService>();

                // Register domain services
                services.AddScoped<AttendanceService.Domain.Services.IAttendanceAnomalyDetector,
                    AttendanceService.Domain.Services.AttendanceAnomalyDetector>();

                // Mock event publisher
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
    public async Task CreateLeaveRequest_WhenValid_ShouldReturnCreated()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);

        var request = new CreateLeaveRequestRequest
        {
            EmployeeId = employeeId,
            Type = "PaidLeave",
            StartDate = startDate,
            EndDate = endDate,
            Reason = "有給休暇を取得します"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/leaverequests", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LeaveRequestDto>();
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal("PaidLeave", result.Type);
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(3, result.Days);
    }

    [Fact]
    public async Task CreateLeaveRequest_WhenOverlap_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);

        var request = new CreateLeaveRequestRequest
        {
            EmployeeId = employeeId,
            Type = "PaidLeave",
            StartDate = startDate,
            EndDate = endDate,
            Reason = "有給休暇"
        };

        // Create first request
        await client.PostAsJsonAsync("/api/leaverequests", request);

        // Act - Try to create overlapping request
        var response = await client.PostAsJsonAsync("/api/leaverequests", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetLeaveRequestById_WhenExists_ShouldReturnOk()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);

        var createRequest = new CreateLeaveRequestRequest
        {
            EmployeeId = employeeId,
            Type = "SickLeave",
            StartDate = startDate,
            EndDate = endDate,
            Reason = "病気のため"
        };

        var createResponse = await client.PostAsJsonAsync("/api/leaverequests", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<LeaveRequestDto>();

        // Act
        var response = await client.GetAsync($"/api/leaverequests/{created!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LeaveRequestDto>();
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(employeeId, result.EmployeeId);
    }

    [Fact]
    public async Task GetLeaveRequestById_WhenNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var client = CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/leaverequests/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ApproveLeaveRequest_WhenPending_ShouldReturnOk()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);

        var createRequest = new CreateLeaveRequestRequest
        {
            EmployeeId = employeeId,
            Type = "PaidLeave",
            StartDate = startDate,
            EndDate = endDate,
            Reason = "有給休暇"
        };

        var createResponse = await client.PostAsJsonAsync("/api/leaverequests", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<LeaveRequestDto>();

        var approveRequest = new ApproveLeaveRequestRequest
        {
            ApproverId = approverId,
            Comment = "承認します"
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/leaverequests/{created!.Id}/approve", approveRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LeaveRequestDto>();
        Assert.NotNull(result);
        Assert.Equal("Approved", result.Status);
        Assert.Equal(approverId, result.ApproverId);
        Assert.Equal("承認します", result.ApproverComment);
        Assert.NotNull(result.ApprovedAt);
    }

    [Fact]
    public async Task RejectLeaveRequest_WhenPending_ShouldReturnOk()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);

        var createRequest = new CreateLeaveRequestRequest
        {
            EmployeeId = employeeId,
            Type = "PaidLeave",
            StartDate = startDate,
            EndDate = endDate,
            Reason = "有給休暇"
        };

        var createResponse = await client.PostAsJsonAsync("/api/leaverequests", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<LeaveRequestDto>();

        var rejectRequest = new RejectLeaveRequestRequest
        {
            ApproverId = approverId,
            Comment = "業務都合により却下"
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/leaverequests/{created!.Id}/reject", rejectRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LeaveRequestDto>();
        Assert.NotNull(result);
        Assert.Equal("Rejected", result.Status);
        Assert.Equal(approverId, result.ApproverId);
        Assert.Equal("業務都合により却下", result.ApproverComment);
    }

    [Fact]
    public async Task CancelLeaveRequest_WhenPending_ShouldReturnOk()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);

        var createRequest = new CreateLeaveRequestRequest
        {
            EmployeeId = employeeId,
            Type = "PaidLeave",
            StartDate = startDate,
            EndDate = endDate,
            Reason = "有給休暇"
        };

        var createResponse = await client.PostAsJsonAsync("/api/leaverequests", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<LeaveRequestDto>();

        // Act
        var response = await client.PostAsync($"/api/leaverequests/{created!.Id}/cancel", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LeaveRequestDto>();
        Assert.NotNull(result);
        Assert.Equal("Cancelled", result.Status);
    }

    [Fact]
    public async Task GetAllLeaveRequests_ShouldReturnOk()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();

        // Create a couple of leave requests
        for (int i = 0; i < 3; i++)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(1 + (i * 5));
            var request = new CreateLeaveRequestRequest
            {
                EmployeeId = employeeId,
                Type = "PaidLeave",
                StartDate = startDate,
                EndDate = startDate.AddDays(1),
                Reason = $"休暇理由{i + 1}"
            };
            await client.PostAsJsonAsync("/api/leaverequests", request);
        }

        // Act
        var response = await client.GetAsync("/api/leaverequests");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<LeaveRequestDto>>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetLeaveRequestsByEmployee_ShouldReturnFilteredResults()
    {
        // Arrange
        var client = CreateClient();
        var employee1Id = Guid.NewGuid();
        var employee2Id = Guid.NewGuid();

        // Create requests for employee 1
        for (int i = 0; i < 2; i++)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(1 + (i * 5));
            var request = new CreateLeaveRequestRequest
            {
                EmployeeId = employee1Id,
                Type = "PaidLeave",
                StartDate = startDate,
                EndDate = startDate.AddDays(1),
                Reason = "休暇"
            };
            await client.PostAsJsonAsync("/api/leaverequests", request);
        }

        // Create request for employee 2
        var employee2Request = new CreateLeaveRequestRequest
        {
            EmployeeId = employee2Id,
            Type = "SickLeave",
            StartDate = DateTime.UtcNow.Date.AddDays(15),
            EndDate = DateTime.UtcNow.Date.AddDays(16),
            Reason = "病気"
        };
        await client.PostAsJsonAsync("/api/leaverequests", employee2Request);

        // Act
        var response = await client.GetAsync($"/api/leaverequests/employee/{employee1Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<LeaveRequestDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(employee1Id, r.EmployeeId));
    }

    [Fact]
    public async Task GetLeaveRequestsByStatus_ShouldReturnFilteredResults()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        // Create and approve one request
        var request1 = new CreateLeaveRequestRequest
        {
            EmployeeId = employeeId,
            Type = "PaidLeave",
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(2),
            Reason = "休暇1"
        };
        var response1 = await client.PostAsJsonAsync("/api/leaverequests", request1);
        var created1 = await response1.Content.ReadFromJsonAsync<LeaveRequestDto>();
        await client.PostAsJsonAsync($"/api/leaverequests/{created1!.Id}/approve",
            new ApproveLeaveRequestRequest { ApproverId = approverId });

        // Create a pending request
        var request2 = new CreateLeaveRequestRequest
        {
            EmployeeId = employeeId,
            Type = "SickLeave",
            StartDate = DateTime.UtcNow.Date.AddDays(10),
            EndDate = DateTime.UtcNow.Date.AddDays(11),
            Reason = "休暇2"
        };
        await client.PostAsJsonAsync("/api/leaverequests", request2);

        // Act - Get pending requests
        var response = await client.GetAsync("/api/leaverequests/status/pending");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<LeaveRequestDto>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Pending", result[0].Status);
    }

    [Fact]
    public async Task ApproveLeaveRequest_WhenAlreadyApproved_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);

        var createRequest = new CreateLeaveRequestRequest
        {
            EmployeeId = employeeId,
            Type = "PaidLeave",
            StartDate = startDate,
            EndDate = endDate,
            Reason = "有給休暇"
        };

        var createResponse = await client.PostAsJsonAsync("/api/leaverequests", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<LeaveRequestDto>();

        var approveRequest = new ApproveLeaveRequestRequest
        {
            ApproverId = approverId,
            Comment = "承認します"
        };

        // First approval
        await client.PostAsJsonAsync($"/api/leaverequests/{created!.Id}/approve", approveRequest);

        // Act - Try to approve again
        var response = await client.PostAsJsonAsync($"/api/leaverequests/{created.Id}/approve", approveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLeaveRequest_WithInvalidLeaveType_ShouldReturnBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var request = new CreateLeaveRequestRequest
        {
            EmployeeId = Guid.NewGuid(),
            Type = "InvalidType",
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(2),
            Reason = "休暇"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/leaverequests", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
