using System.Net;
using System.Net.Http.Json;
using EmployeeService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
        return _baseFactory.WithWebHostBuilder(builder =>
        {
            // Skip database initialization by setting environment to Test
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                // Add in-memory database for testing with unique database name per test
                services.AddDbContext<EmployeeDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });
                
                // Register repository
                services.AddScoped<EmployeeService.Domain.Repositories.IEmployeeRepository, 
                    EmployeeService.Infrastructure.Repositories.EmployeeRepository>();
            });
        }).CreateClient();
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
}
