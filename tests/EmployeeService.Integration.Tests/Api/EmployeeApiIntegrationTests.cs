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

public class EmployeeApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public EmployeeApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;
    }

    private HttpClient CreateClient()
    {
        // Use a unique database name per test to isolate test data
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
                // Remove existing DbContext registration
                services.RemoveAll<DbContextOptions<EmployeeDbContext>>();
                services.RemoveAll<EmployeeDbContext>();
                
                // Add in-memory database for testing - same DB instance for all requests from this client
                services.AddDbContext<EmployeeDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
                
                // Register repositories
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

        // Add admin JWT Bearer token for create/update tests
        var token = JwtTokenHelper.GenerateToken("admin-test", "admin", "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    [Fact]
    public async Task GetAllEmployees_ShouldReturnEmptyList_WhenNoEmployees()
    {
        // Act
        var client = CreateClient();
        var response = await client.GetAsync("/api/employees");

        // Assert
        response.EnsureSuccessStatusCode();
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        Assert.NotNull(employees);
        Assert.Empty(employees);
    }

    [Fact]
    public async Task CreateEmployee_ShouldReturnCreated_WithValidRequest()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro.test@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };

        // Act
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/employees", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(request.FirstName, created.FirstName);
        Assert.Equal(request.LastName, created.LastName);
        Assert.Equal(request.Email, created.Email);
    }

    [Fact]
    public async Task GetEmployeeById_ShouldReturnEmployee_WhenExists()
    {
        // Arrange - Create an employee first
        var client = CreateClient();
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro.get@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };
        var createResponse = await client.PostAsJsonAsync("/api/employees", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(created);

        // Act
        var response = await client.GetAsync($"/api/employees/{created.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(employee);
        Assert.Equal(created.Id, employee.Id);
        Assert.Equal(createRequest.Email, employee.Email);
    }

    [Fact]
    public async Task GetEmployeeById_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var client = CreateClient();
        var response = await client.GetAsync($"/api/employees/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEmployee_ShouldReturnOk_WithValidRequest()
    {
        // Arrange - Create an employee first
        var client = CreateClient();
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro.update@example.com",
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
            Email = "tanaka.jiro.update@example.com",
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
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(updateRequest.FirstName, updated.FirstName);
        Assert.Equal(updateRequest.LastName, updated.LastName);
        Assert.Equal(updateRequest.Email, updated.Email);
    }

    [Fact]
    public async Task UpdateEmployee_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
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
        var client = CreateClient();
        var response = await client.PutAsJsonAsync($"/api/employees/{nonExistentId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_ShouldReturnNoContent_WhenExists()
    {
        // Arrange - Create an employee first
        var client = CreateClient();
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro.delete@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };
        var createResponse = await client.PostAsJsonAsync("/api/employees", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(created);

        // Act
        var response = await client.DeleteAsync($"/api/employees/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deletion
        var getResponse = await client.GetAsync($"/api/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var client = CreateClient();
        var response = await client.DeleteAsync($"/api/employees/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_ShouldReturnBadRequest_WithDuplicateEmail()
    {
        // Arrange - Create first employee
        var client = CreateClient();
        var request1 = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "duplicate@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };
        await client.PostAsJsonAsync("/api/employees", request1);

        // Try to create another employee with the same email
        var request2 = new CreateEmployeeRequest
        {
            FirstName = "花子",
            LastName = "佐藤",
            Email = "duplicate@example.com",
            HireDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "営業部",
            Position = "マネージャー"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/employees", request2);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboardStatistics_ShouldReturnEmptyStatistics_WhenNoEmployees()
    {
        // Act
        var client = CreateClient();
        var response = await client.GetAsync("/api/employees/dashboard/statistics");

        // Assert
        response.EnsureSuccessStatusCode();
        var statistics = await response.Content.ReadFromJsonAsync<DashboardStatisticsDto>();
        Assert.NotNull(statistics);
        Assert.Equal(0, statistics.TotalEmployees);
        Assert.Equal(0, statistics.DepartmentCount);
        Assert.Equal(0, statistics.NewEmployeesThisMonth);
    }

    [Fact]
    public async Task GetDashboardStatistics_ShouldReturnCorrectStatistics_WithMultipleEmployees()
    {
        // Arrange - Create multiple employees
        var client = CreateClient();
        
        // Create employees in different departments
        var request1 = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "taro.yamada@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };
        await client.PostAsJsonAsync("/api/employees", request1);

        var request2 = new CreateEmployeeRequest
        {
            FirstName = "花子",
            LastName = "佐藤",
            Email = "hanako.sato@example.com",
            HireDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "営業部",
            Position = "マネージャー"
        };
        await client.PostAsJsonAsync("/api/employees", request2);

        var request3 = new CreateEmployeeRequest
        {
            FirstName = "次郎",
            LastName = "鈴木",
            Email = "jiro.suzuki@example.com",
            HireDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "シニアエンジニア"
        };
        await client.PostAsJsonAsync("/api/employees", request3);

        // Act
        var response = await client.GetAsync("/api/employees/dashboard/statistics");

        // Assert
        response.EnsureSuccessStatusCode();
        var statistics = await response.Content.ReadFromJsonAsync<DashboardStatisticsDto>();
        Assert.NotNull(statistics);
        Assert.Equal(3, statistics.TotalEmployees);
        Assert.Equal(2, statistics.DepartmentCount); // 開発部 and 営業部
        Assert.Equal(3, statistics.NewEmployeesThisMonth); // All created this month
    }

    [Fact]
    public async Task GetRecentActivities_ShouldReturnEmptyList_WhenNoEmployees()
    {
        // Act
        var client = CreateClient();
        var response = await client.GetAsync("/api/employees/dashboard/recent-activities");

        // Assert
        response.EnsureSuccessStatusCode();
        var activities = await response.Content.ReadFromJsonAsync<IEnumerable<RecentActivityDto>>();
        Assert.NotNull(activities);
        Assert.Empty(activities);
    }

    [Fact]
    public async Task GetRecentActivities_ShouldReturnCreatedActivities_ForNewEmployees()
    {
        // Arrange - Create employees
        var client = CreateClient();
        var request = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "taro.yamada@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };
        await client.PostAsJsonAsync("/api/employees", request);

        // Act
        var response = await client.GetAsync("/api/employees/dashboard/recent-activities?count=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var activities = await response.Content.ReadFromJsonAsync<List<RecentActivityDto>>();
        Assert.NotNull(activities);
        Assert.Single(activities);
        Assert.Equal("Created", activities[0].ActivityType);
        Assert.Contains("山田 太郎", activities[0].EmployeeName);
    }

    [Fact]
    public async Task GetRecentActivities_ShouldLimitResults_WhenCountParameterIsProvided()
    {
        // Arrange - Create multiple employees
        var client = CreateClient();
        for (int i = 0; i < 5; i++)
        {
            var request = new CreateEmployeeRequest
            {
                FirstName = $"太郎{i}",
                LastName = "山田",
                Email = $"employee{i}@example.com",
                HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Department = "開発部",
                Position = "エンジニア"
            };
            await client.PostAsJsonAsync("/api/employees", request);
        }

        // Act - Request only 3 activities
        var response = await client.GetAsync("/api/employees/dashboard/recent-activities?count=3");

        // Assert
        response.EnsureSuccessStatusCode();
        var activities = await response.Content.ReadFromJsonAsync<List<RecentActivityDto>>();
        Assert.NotNull(activities);
        Assert.Equal(3, activities.Count);
    }
}
