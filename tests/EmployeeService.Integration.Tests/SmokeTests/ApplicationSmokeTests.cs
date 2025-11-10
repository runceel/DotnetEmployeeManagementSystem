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
using Shared.Contracts.EmployeeService;

namespace EmployeeService.Integration.Tests.SmokeTests;

/// <summary>
/// スモークテスト - 代表的な画面操作を検証
/// Central Package Management 移行後の主要機能が正常に動作することを確認
/// </summary>
public class ApplicationSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public ApplicationSmokeTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;
    }

    private HttpClient CreateClient()
    {
        var dbName = $"SmokeTestDb_{Guid.NewGuid()}";
        
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
                
                services.AddScoped<EmployeeService.Domain.Repositories.IEmployeeRepository, 
                    EmployeeService.Infrastructure.Repositories.EmployeeRepository>();
                services.AddScoped<EmployeeService.Domain.Repositories.IDepartmentRepository,
                    EmployeeService.Infrastructure.Repositories.DepartmentRepository>();
            });
        }).CreateClient();

        var token = JwtTokenHelper.GenerateToken("admin-test", "admin", "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    /// <summary>
    /// スモークテスト1: 従業員一覧取得（初期状態）
    /// ダッシュボード/従業員一覧画面の表示に相当
    /// </summary>
    [Fact]
    public async Task SmokeTest_01_GetEmployeeList_InitialState()
    {
        // Arrange
        var client = CreateClient();

        // Act - 従業員一覧を取得
        var response = await client.GetAsync("/api/employees");

        // Assert - 正常に取得できることを確認
        response.EnsureSuccessStatusCode();
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        Assert.NotNull(employees);
    }

    /// <summary>
    /// スモークテスト2: 従業員登録→一覧表示の流れ
    /// 新規従業員登録フォームでの登録→一覧画面での確認に相当
    /// </summary>
    [Fact]
    public async Task SmokeTest_02_CreateEmployee_And_VerifyInList()
    {
        // Arrange
        var client = CreateClient();
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro.smoke@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "ソフトウェアエンジニア"
        };

        // Act - 従業員を登録
        var createResponse = await client.PostAsJsonAsync("/api/employees", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdEmployee = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();

        // 一覧を取得して確認
        var listResponse = await client.GetAsync("/api/employees");
        listResponse.EnsureSuccessStatusCode();
        var employees = await listResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>();

        // Assert - 登録した従業員が一覧に存在することを確認
        Assert.NotNull(createdEmployee);
        Assert.NotNull(employees);
        Assert.Contains(employees, e => e.Id == createdEmployee.Id);
    }

    /// <summary>
    /// スモークテスト3: 従業員詳細表示
    /// 従業員一覧から詳細画面への遷移に相当
    /// </summary>
    [Fact]
    public async Task SmokeTest_03_GetEmployeeDetails()
    {
        // Arrange
        var client = CreateClient();
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "花子",
            LastName = "佐藤",
            Email = "sato.hanako.smoke@example.com",
            HireDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "営業部",
            Position = "営業担当"
        };

        // 従業員を登録
        var createResponse = await client.PostAsJsonAsync("/api/employees", createRequest);
        var createdEmployee = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(createdEmployee);

        // Act - 詳細を取得
        var detailResponse = await client.GetAsync($"/api/employees/{createdEmployee.Id}");

        // Assert - 詳細が正常に取得できることを確認
        detailResponse.EnsureSuccessStatusCode();
        var employeeDetail = await detailResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(employeeDetail);
        Assert.Equal(createdEmployee.Id, employeeDetail.Id);
        Assert.Equal("花子", employeeDetail.FirstName);
        Assert.Equal("佐藤", employeeDetail.LastName);
    }

    /// <summary>
    /// スモークテスト4: 従業員情報更新
    /// 従業員詳細画面での情報編集に相当
    /// </summary>
    [Fact]
    public async Task SmokeTest_04_UpdateEmployee()
    {
        // Arrange
        var client = CreateClient();
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "次郎",
            LastName = "田中",
            Email = "tanaka.jiro.smoke@example.com",
            HireDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "総務部",
            Position = "一般社員"
        };

        var createResponse = await client.PostAsJsonAsync("/api/employees", createRequest);
        var createdEmployee = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(createdEmployee);

        // Act - 情報を更新（昇進）
        var updateRequest = new UpdateEmployeeRequest
        {
            FirstName = "次郎",
            LastName = "田中",
            Email = "tanaka.jiro.smoke@example.com",
            Department = "総務部",
            Position = "主任" // 昇進
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/employees/{createdEmployee.Id}", updateRequest);

        // Assert - 更新が成功し、情報が変更されていることを確認
        updateResponse.EnsureSuccessStatusCode();
        
        var detailResponse = await client.GetAsync($"/api/employees/{createdEmployee.Id}");
        var updatedEmployee = await detailResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(updatedEmployee);
        Assert.Equal("主任", updatedEmployee.Position);
    }

    /// <summary>
    /// スモークテスト5: 従業員削除
    /// 従業員削除操作に相当
    /// </summary>
    [Fact]
    public async Task SmokeTest_05_DeleteEmployee()
    {
        // Arrange
        var client = CreateClient();
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "三郎",
            LastName = "鈴木",
            Email = "suzuki.saburo.smoke@example.com",
            HireDate = new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "人事部",
            Position = "人事担当"
        };

        var createResponse = await client.PostAsJsonAsync("/api/employees", createRequest);
        var createdEmployee = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(createdEmployee);

        // Act - 従業員を削除
        var deleteResponse = await client.DeleteAsync($"/api/employees/{createdEmployee.Id}");

        // Assert - 削除が成功し、一覧から消えていることを確認
        deleteResponse.EnsureSuccessStatusCode();
        
        var detailResponse = await client.GetAsync($"/api/employees/{createdEmployee.Id}");
        Assert.Equal(HttpStatusCode.NotFound, detailResponse.StatusCode);
    }

    /// <summary>
    /// スモークテスト6: 部署一覧取得
    /// 部署管理画面の表示に相当
    /// </summary>
    [Fact]
    public async Task SmokeTest_06_GetDepartmentList()
    {
        // Arrange
        var client = CreateClient();

        // Act - 部署一覧を取得
        var response = await client.GetAsync("/api/departments");

        // Assert - 正常に取得できることを確認
        response.EnsureSuccessStatusCode();
        var departments = await response.Content.ReadFromJsonAsync<List<DepartmentDto>>();
        Assert.NotNull(departments);
    }

    /// <summary>
    /// スモークテスト7: エンドツーエンドシナリオ
    /// 複数従業員の登録→検索→更新の一連の操作
    /// </summary>
    [Fact]
    public async Task SmokeTest_07_EndToEndScenario_MultipleEmployees()
    {
        // Arrange
        var client = CreateClient();
        var employees = new[]
        {
            new CreateEmployeeRequest
            {
                FirstName = "一郎",
                LastName = "高橋",
                Email = "takahashi.ichiro@example.com",
                HireDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Department = "開発部",
                Position = "シニアエンジニア"
            },
            new CreateEmployeeRequest
            {
                FirstName = "美咲",
                LastName = "伊藤",
                Email = "ito.misaki@example.com",
                HireDate = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                Department = "開発部",
                Position = "エンジニア"
            },
            new CreateEmployeeRequest
            {
                FirstName = "健太",
                LastName = "渡辺",
                Email = "watanabe.kenta@example.com",
                HireDate = new DateTime(2023, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                Department = "営業部",
                Position = "営業マネージャー"
            }
        };

        // Act & Assert - 複数の従業員を登録
        foreach (var employeeRequest in employees)
        {
            var response = await client.PostAsJsonAsync("/api/employees", employeeRequest);
            response.EnsureSuccessStatusCode();
        }

        // 一覧を取得して全員が登録されていることを確認
        var listResponse = await client.GetAsync("/api/employees");
        listResponse.EnsureSuccessStatusCode();
        var employeeList = await listResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        
        Assert.NotNull(employeeList);
        Assert.Equal(3, employeeList.Count);
        
        // 開発部の従業員が2名いることを確認
        var devDeptCount = employeeList.Count(e => e.Department == "開発部");
        Assert.Equal(2, devDeptCount);
    }

    /// <summary>
    /// スモークテスト8: APIの健全性チェック
    /// 主要なエンドポイントが応答することを確認
    /// </summary>
    [Fact]
    public async Task SmokeTest_08_ApiHealthCheck()
    {
        // Arrange
        var client = CreateClient();

        // Act & Assert - 主要エンドポイントが応答することを確認
        var endpoints = new[]
        {
            "/api/employees",
            "/api/departments"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await client.GetAsync(endpoint);
            Assert.True(
                response.IsSuccessStatusCode, 
                $"エンドポイント {endpoint} が正常に応答しませんでした。ステータスコード: {response.StatusCode}"
            );
        }
    }
}
