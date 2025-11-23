using System.Net;
using EmployeeService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace EmployeeService.Integration.Tests.Mcp;

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

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = "Development-Secret-Key-For-JWT-Token-Generation-Must-Be-At-Least-32-Characters-Long",
                    ["Jwt:Issuer"] = "EmployeeManagementSystem.AuthService",
                    ["Jwt:Audience"] = "EmployeeManagementSystem.API",
                    ["Jwt:ExpirationMinutes"] = "120"
                });
            });

            builder.ConfigureServices(services =>
            {
                // InMemory データベースを使用
                services.RemoveAll<DbContextOptions<EmployeeDbContext>>();
                services.AddDbContext<EmployeeDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                // ServiceDefaults からの OpenTelemetry を無効化
                services.RemoveAll<IHostedService>();
            });
        });

        return factory.CreateClient();
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
}
