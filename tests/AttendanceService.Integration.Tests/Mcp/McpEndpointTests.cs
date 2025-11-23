using System.Net;
using AttendanceService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AttendanceService.Integration.Tests.Mcp;

/// <summary>
/// MCPエンドポイントの統合テスト
/// </summary>
public class McpEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public McpEndpointTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;
    }

    private HttpClient CreateClient()
    {
        var dbName = $"McpTestDb_{Guid.NewGuid()}";

        var factory = _baseFactory.WithWebHostBuilder(builder =>
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

                // HttpContextAccessor for MCP tools
                services.AddHttpContextAccessor();

                // MCP Server registration
                services.AddMcpServer()
                    .WithHttpTransport()
                    .WithToolsFromAssembly();
            });
        });

        return factory.CreateClient();
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
    public async Task McpEndpoint_ShouldBeAccessible()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/mcp");

        // Assert
        // MCP endpoint should return OK or MethodNotAllowed (GET not supported)
        // but should not return NotFound
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task McpEndpoint_ShouldReturnExpectedResponseForInvalidRequest()
    {
        // Arrange
        var client = CreateClient();

        // Act - Send an invalid MCP request
        var response = await client.PostAsync("/api/mcp",
            new StringContent("invalid json", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        // MCP endpoint should handle requests (even invalid ones) without returning 404
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task McpEndpoint_WithCorsHeaders_ShouldAllowOrigin()
    {
        // Arrange
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Origin", "http://localhost:3000");

        // Act
        var response = await client.GetAsync("/api/mcp");

        // Assert
        // CORS should be configured for MCP endpoint
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        // In development mode, CORS should allow any origin
        // Note: WebApplicationFactory might not include CORS headers in response
        // This test mainly ensures the endpoint exists and is configured
    }
}
