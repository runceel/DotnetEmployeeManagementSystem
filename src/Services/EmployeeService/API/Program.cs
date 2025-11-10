using EmployeeService.Application.UseCases;
using EmployeeService.Infrastructure;
using EmployeeService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.EmployeeService;
using Shared.Contracts.DepartmentService;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 認証・認可の設定
// カスタム認証スキーム（X-User-*ヘッダー）とJWT Bearer認証の両方をサポート
const string CustomAuthScheme = "CustomAuth";

builder.Services.AddAuthentication(options =>
{
    // デフォルトスキームをカスタム認証に設定（本番環境用）
    options.DefaultAuthenticateScheme = CustomAuthScheme;
    options.DefaultChallengeScheme = CustomAuthScheme;
})
.AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>(CustomAuthScheme, null)
.AddJwtBearer(options =>
{
    // JWT Bearer認証はテスト環境で使用
    var secretKey = builder.Configuration["Jwt:SecretKey"];
    var issuer = builder.Configuration["Jwt:Issuer"];
    var audience = builder.Configuration["Jwt:Audience"];

    // Test環境でのみJWT設定を必須にする
    if (builder.Environment.IsEnvironment("Test"))
    {
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured");
        if (string.IsNullOrEmpty(issuer))
            throw new InvalidOperationException("JWT Issuer is not configured");
        if (string.IsNullOrEmpty(audience))
            throw new InvalidOperationException("JWT Audience is not configured");
    }

    if (!string.IsNullOrEmpty(secretKey) && !string.IsNullOrEmpty(issuer) && !string.IsNullOrEmpty(audience))
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(secretKey))
        };
    }
});

// ポリシーベースの認可設定 - 複数の認証スキームをサポート
builder.Services.AddAuthorization(options =>
{
    // 管理者ロールが必要なポリシー
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireRole("Admin");
        // CustomAuthとJWT Bearerの両方の認証スキームをサポート
        policy.AuthenticationSchemes.Add(CustomAuthScheme);
        policy.AuthenticationSchemes.Add(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme);
    });
});

// データベース接続文字列とInfrastructure層の初期化 (Test環境ではスキップ)
if (!builder.Environment.IsEnvironment("Test"))
{
    var connectionString = builder.Configuration.GetConnectionString("EmployeeDb") 
        ?? "Data Source=employees.db";
    
    // Infrastructure層のサービスを追加
    builder.Services.AddInfrastructure(connectionString);

    // Redis接続の追加
    builder.AddRedisClient("redis");
}

// Application層のサービスを追加
builder.Services.AddScoped<IEmployeeService, EmployeeService.Application.UseCases.EmployeeService>();
builder.Services.AddScoped<IDepartmentService, EmployeeService.Application.UseCases.DepartmentService>();

var app = builder.Build();

// データベース初期化 (Test環境では実行しない)
if (!app.Environment.IsEnvironment("Test"))
{
    await DbInitializer.InitializeAsync(app.Services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Employee API endpoints
var employees = app.MapGroup("/api/employees")
    .WithTags("Employees")
    .WithOpenApi();

// 全従業員を取得
employees.MapGet("/", async (IEmployeeService employeeService) =>
{
    var result = await employeeService.GetAllAsync();
    return Results.Ok(result);
})
.WithName("GetAllEmployees")
.Produces<IEnumerable<EmployeeDto>>();

// IDで従業員を取得
employees.MapGet("/{id:guid}", async (Guid id, IEmployeeService employeeService) =>
{
    var result = await employeeService.GetByIdAsync(id);
    return result is not null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetEmployeeById")
.Produces<EmployeeDto>()
.Produces(StatusCodes.Status404NotFound);

// 従業員を作成
employees.MapPost("/", async ([FromBody] CreateEmployeeRequest request, IEmployeeService employeeService) =>
{
    try
    {
        var result = await employeeService.CreateAsync(request);
        return Results.Created($"/api/employees/{result.Id}", result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateEmployee")
.Produces<EmployeeDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status403Forbidden)
.RequireAuthorization("AdminPolicy");

// 従業員を更新
employees.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateEmployeeRequest request, IEmployeeService employeeService) =>
{
    try
    {
        var result = await employeeService.UpdateAsync(id, request);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("UpdateEmployee")
.Produces<EmployeeDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status403Forbidden)
.RequireAuthorization("AdminPolicy");

// 従業員を削除
employees.MapDelete("/{id:guid}", async (Guid id, IEmployeeService employeeService) =>
{
    var result = await employeeService.DeleteAsync(id);
    return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteEmployee")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

// ダッシュボード統計情報を取得
employees.MapGet("/dashboard/statistics", async (IEmployeeService employeeService) =>
{
    var result = await employeeService.GetDashboardStatisticsAsync();
    return Results.Ok(result);
})
.WithName("GetDashboardStatistics")
.Produces<DashboardStatisticsDto>();

// 最近のアクティビティを取得
employees.MapGet("/dashboard/recent-activities", async (IEmployeeService employeeService, [FromQuery] int count = 10) =>
{
    var result = await employeeService.GetRecentActivitiesAsync(count);
    return Results.Ok(result);
})
.WithName("GetRecentActivities")
.Produces<IEnumerable<RecentActivityDto>>();

// Department API endpoints
var departments = app.MapGroup("/api/departments")
    .WithTags("Departments")
    .WithOpenApi();

// 全部署を取得
departments.MapGet("/", async (IDepartmentService departmentService) =>
{
    var result = await departmentService.GetAllAsync();
    return Results.Ok(result);
})
.WithName("GetAllDepartments")
.Produces<IEnumerable<DepartmentDto>>();

// IDで部署を取得
departments.MapGet("/{id:guid}", async (Guid id, IDepartmentService departmentService) =>
{
    var result = await departmentService.GetByIdAsync(id);
    return result is not null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetDepartmentById")
.Produces<DepartmentDto>()
.Produces(StatusCodes.Status404NotFound);

// 部署を作成
departments.MapPost("/", async ([FromBody] CreateDepartmentRequest request, IDepartmentService departmentService) =>
{
    try
    {
        var result = await departmentService.CreateAsync(request);
        return Results.Created($"/api/departments/{result.Id}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateDepartment")
.Produces<DepartmentDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status403Forbidden)
.RequireAuthorization("AdminPolicy");

// 部署を更新
departments.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateDepartmentRequest request, IDepartmentService departmentService) =>
{
    try
    {
        var result = await departmentService.UpdateAsync(id, request);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("UpdateDepartment")
.Produces<DepartmentDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status403Forbidden)
.RequireAuthorization("AdminPolicy");

// 部署を削除
departments.MapDelete("/{id:guid}", async (Guid id, IDepartmentService departmentService) =>
{
    var result = await departmentService.DeleteAsync(id);
    return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteDepartment")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.RequireAuthorization("AdminPolicy");

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
