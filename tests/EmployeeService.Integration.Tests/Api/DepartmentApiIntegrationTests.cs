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
using Shared.Contracts.DepartmentService;

namespace EmployeeService.Integration.Tests.Api;

public class DepartmentApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public DepartmentApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;
    }

    private HttpClient CreateClient()
    {
        var dbName = $"TestDb_{Guid.NewGuid()}";
        
        var client = _baseFactory.WithWebHostBuilder(builder =>
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
                services.RemoveAll<DbContextOptions<EmployeeDbContext>>();
                services.RemoveAll<EmployeeDbContext>();
                
                services.AddDbContext<EmployeeDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
                
                services.AddScoped<EmployeeService.Domain.Repositories.IDepartmentRepository, 
                    EmployeeService.Infrastructure.Repositories.DepartmentRepository>();
            });
        }).CreateClient();

        var token = JwtTokenHelper.GenerateToken("admin-test", "admin", "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    [Fact]
    public async Task GetAllDepartments_ShouldReturnEmptyList_WhenNoDepartments()
    {
        // Act
        var client = CreateClient();
        var response = await client.GetAsync("/api/departments");

        // Assert
        response.EnsureSuccessStatusCode();
        var departments = await response.Content.ReadFromJsonAsync<List<DepartmentDto>>();
        Assert.NotNull(departments);
        Assert.Empty(departments);
    }

    [Fact]
    public async Task CreateDepartment_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var client = CreateClient();
        var request = new CreateDepartmentRequest
        {
            Name = "開発部",
            Description = "ソフトウェア開発を担当する部署"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/departments", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var department = await response.Content.ReadFromJsonAsync<DepartmentDto>();
        Assert.NotNull(department);
        Assert.NotEqual(Guid.Empty, department.Id);
        Assert.Equal(request.Name, department.Name);
        Assert.Equal(request.Description, department.Description);
    }

    [Fact]
    public async Task GetDepartmentById_WithExistingId_ShouldReturnDepartment()
    {
        // Arrange
        var client = CreateClient();
        var createRequest = new CreateDepartmentRequest
        {
            Name = "開発部",
            Description = "ソフトウェア開発を担当する部署"
        };
        var createResponse = await client.PostAsJsonAsync("/api/departments", createRequest);
        var createdDepartment = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();
        Assert.NotNull(createdDepartment);

        // Act
        var response = await client.GetAsync($"/api/departments/{createdDepartment.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var department = await response.Content.ReadFromJsonAsync<DepartmentDto>();
        Assert.NotNull(department);
        Assert.Equal(createdDepartment.Id, department.Id);
        Assert.Equal(createdDepartment.Name, department.Name);
    }

    [Fact]
    public async Task GetDepartmentById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var client = CreateClient();
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/departments/{nonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDepartment_WithValidRequest_ShouldReturnUpdatedDepartment()
    {
        // Arrange
        var client = CreateClient();
        var createRequest = new CreateDepartmentRequest
        {
            Name = "開発部",
            Description = "ソフトウェア開発を担当する部署"
        };
        var createResponse = await client.PostAsJsonAsync("/api/departments", createRequest);
        var createdDepartment = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();
        Assert.NotNull(createdDepartment);

        var updateRequest = new UpdateDepartmentRequest
        {
            Name = "営業部",
            Description = "営業活動を担当する部署"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/departments/{createdDepartment.Id}", updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedDepartment = await response.Content.ReadFromJsonAsync<DepartmentDto>();
        Assert.NotNull(updatedDepartment);
        Assert.Equal(createdDepartment.Id, updatedDepartment.Id);
        Assert.Equal(updateRequest.Name, updatedDepartment.Name);
        Assert.Equal(updateRequest.Description, updatedDepartment.Description);
    }

    [Fact]
    public async Task UpdateDepartment_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var client = CreateClient();
        var nonExistingId = Guid.NewGuid();
        var updateRequest = new UpdateDepartmentRequest
        {
            Name = "営業部",
            Description = "営業活動を担当する部署"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/departments/{nonExistingId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDepartment_WithExistingId_ShouldReturnNoContent()
    {
        // Arrange
        var client = CreateClient();
        var createRequest = new CreateDepartmentRequest
        {
            Name = "開発部",
            Description = "ソフトウェア開発を担当する部署"
        };
        var createResponse = await client.PostAsJsonAsync("/api/departments", createRequest);
        var createdDepartment = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();
        Assert.NotNull(createdDepartment);

        // Act
        var response = await client.DeleteAsync($"/api/departments/{createdDepartment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deletion
        var getResponse = await client.GetAsync($"/api/departments/{createdDepartment.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteDepartment_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var client = CreateClient();
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/departments/{nonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllDepartments_ShouldReturnAllCreatedDepartments()
    {
        // Arrange
        var client = CreateClient();
        var departments = new[]
        {
            new CreateDepartmentRequest { Name = "開発部", Description = "ソフトウェア開発を担当する部署" },
            new CreateDepartmentRequest { Name = "営業部", Description = "営業活動を担当する部署" },
            new CreateDepartmentRequest { Name = "人事部", Description = "人事管理を担当する部署" }
        };

        foreach (var dept in departments)
        {
            await client.PostAsJsonAsync("/api/departments", dept);
        }

        // Act
        var response = await client.GetAsync("/api/departments");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<DepartmentDto>>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }
}
