using EmployeeService.API.Endpoints;
using EmployeeService.API.Extensions;
using EmployeeService.Application.UseCases;
using EmployeeService.Infrastructure;
using EmployeeService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddEmployeeServiceOpenApi();

// 認証・認可の設定
builder.Services.AddEmployeeServiceAuthentication(builder.Configuration, builder.Environment);

// データベース接続文字列とInfrastructure層の初期化 (Test環境ではスキップ)
if (!builder.Environment.IsEnvironment("Test"))
{
    var connectionString = builder.Configuration.GetConnectionString("EmployeeDb") 
        ?? "Data Source=employees.db";
    
    // Infrastructure層のサービスを追加
    builder.Services.AddInfrastructure(connectionString, builder.Environment);

    // Redis接続の追加
    builder.AddRedisClient("redis");
}

// Application層のサービスを追加
builder.Services.AddScoped<IEmployeeService, EmployeeService.Application.UseCases.EmployeeService>();
builder.Services.AddScoped<IDepartmentService, EmployeeService.Application.UseCases.DepartmentService>();

// MCP Server 登録（共通設定）
builder.AddMcpServerDefaults();

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

// MCP エンドポイントとCORS設定（共通設定）
app.UseMcpServerDefaults();

// Map endpoints
app.MapEmployeeEndpoints();
app.MapDepartmentEndpoints();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
