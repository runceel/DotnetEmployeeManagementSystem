using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmployeeService.Infrastructure.Data;
using EmployeeService.Integration.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Contracts.EmployeeService;

namespace EmployeeService.Integration.Tests.Api;

public class EmployeeAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public EmployeeAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;
    }

    private HttpClient CreateClient(string? userId = null, string? userName = null, string? roles = null)
    {
        var dbName = $"TestDb_{Guid.NewGuid()}";
        
        var factory = _baseFactory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Add JWT configuration for testing
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
                services.RemoveAll<DbContextOptions<EmployeeDbContext>>();
                services.RemoveAll<EmployeeDbContext>();
                
                services.AddDbContext<EmployeeDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
                
                services.AddScoped<EmployeeService.Domain.Repositories.IEmployeeRepository, 
                    EmployeeService.Infrastructure.Repositories.EmployeeRepository>();
                services.AddScoped<EmployeeService.Domain.Repositories.IDepartmentRepository, 
                    EmployeeService.Infrastructure.Repositories.DepartmentRepository>();
            });
        });
        
        // Seed test department
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
            var dept = new EmployeeService.Domain.Entities.Department("開発部", "ソフトウェア開発を担当する部署");
            context.Departments.Add(dept);
            context.SaveChanges();
        }
        
        var client = factory.CreateClient();

        // Add JWT Bearer token if authentication info is provided
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName))
        {
            var roleArray = string.IsNullOrEmpty(roles) ? Array.Empty<string>() : roles.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray();
            var token = JwtTokenHelper.GenerateToken(userId, userName, roleArray);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    [Fact]
    public async Task CreateEmployee_ShouldReturnForbidden_WhenUserIsNotAdmin()
    {
        // Arrange
        var client = CreateClient(userId: "user-123", userName: "testuser", roles: "User");
        var request = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/employees", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_ShouldReturnCreated_WhenUserIsAdmin()
    {
        // Arrange
        var client = CreateClient(userId: "admin-123", userName: "admin", roles: "Admin");
        var request = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/employees", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(created);
        Assert.Equal(request.FirstName, created.FirstName);
    }

    [Fact]
    public async Task UpdateEmployee_ShouldReturnForbidden_WhenUserIsNotAdmin()
    {
        // Arrange - First create an employee as admin
        var adminClient = CreateClient(userId: "admin-123", userName: "admin", roles: "Admin");
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro.update@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };
        var createResponse = await adminClient.PostAsJsonAsync("/api/employees", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(created);

        // Now try to update as regular user
        var userClient = CreateClient(userId: "user-123", userName: "testuser", roles: "User");
        var updateRequest = new UpdateEmployeeRequest
        {
            FirstName = "次郎",
            LastName = "田中",
            Email = "tanaka.jiro@example.com",
            HireDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "営業部",
            Position = "マネージャー"
        };

        // Act
        var response = await userClient.PutAsJsonAsync($"/api/employees/{created.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEmployee_ShouldReturnOk_WhenUserIsAdmin()
    {
        // Arrange - Create and update as admin
        var client = CreateClient(userId: "admin-123", userName: "admin", roles: "Admin");
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro.update2@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };
        var createResponse = await client.PostAsJsonAsync("/api/employees", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(created);

        var updateRequest = new UpdateEmployeeRequest
        {
            FirstName = "次郎",
            LastName = "田中",
            Email = "tanaka.jiro@example.com",
            HireDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "営業部",
            Position = "マネージャー"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/employees/{created.Id}", updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(updated);
        Assert.Equal(updateRequest.FirstName, updated.FirstName);
    }

    [Fact]
    public async Task GetEmployees_ShouldReturnOk_ForAnyAuthenticatedUser()
    {
        // Arrange - Create an employee as admin first using same DB
        var dbName = $"TestDb_{Guid.NewGuid()}";
        
        var adminClient = CreateClientWithDb(dbName, userId: "admin-123", userName: "admin", roles: "Admin");
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro.get@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };
        await adminClient.PostAsJsonAsync("/api/employees", createRequest);

        // Act - Get employees as regular user with same DB
        var userClient = CreateClientWithDb(dbName, userId: "user-123", userName: "testuser", roles: "User");
        var response = await userClient.GetAsync("/api/employees");

        // Assert
        response.EnsureSuccessStatusCode();
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        Assert.NotNull(employees);
        Assert.NotEmpty(employees);
    }

    private HttpClient CreateClientWithDb(string dbName, string? userId = null, string? userName = null, string? roles = null)
    {
        var client = _baseFactory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Add JWT configuration for testing
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
                services.RemoveAll<DbContextOptions<EmployeeDbContext>>();
                services.RemoveAll<EmployeeDbContext>();
                
                services.AddDbContext<EmployeeDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
                
                services.AddScoped<EmployeeService.Domain.Repositories.IEmployeeRepository, 
                    EmployeeService.Infrastructure.Repositories.EmployeeRepository>();
            });
        }).CreateClient();

        // Add JWT Bearer token if authentication info is provided
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName))
        {
            var roleArray = string.IsNullOrEmpty(roles) ? Array.Empty<string>() : roles.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray();
            var token = JwtTokenHelper.GenerateToken(userId, userName, roleArray);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    [Fact]
    public async Task GetEmployees_ShouldReturnOk_ForUnauthenticatedUser()
    {
        // Arrange
        var client = CreateClient(); // No auth headers

        // Act
        var response = await client.GetAsync("/api/employees");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateEmployee_ShouldReturnUnauthorizedOrForbidden_WhenNoAuthHeaders()
    {
        // Arrange
        var client = CreateClient(); // No auth headers
        var request = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro.noauth@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/employees", request);

        // Assert - Without auth headers, the response should be either Unauthorized or Forbidden
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden,
            $"Expected Unauthorized or Forbidden, but got {response.StatusCode}");
    }
}
