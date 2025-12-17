using AttendanceService.API.Endpoints;
using AttendanceService.API.Extensions;
using AttendanceService.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddAttendanceServiceOpenApi();

// データベース接続文字列とInfrastructure層の初期化
// Test環境ではテストコードでDbContextを設定するためスキップ
if (!builder.Environment.IsEnvironment("Test"))
{
    var connectionString = builder.Configuration.GetConnectionString("AttendanceDb")
        ?? "Data Source=attendance.db";

    // Infrastructure層のサービスを追加
    builder.Services.AddInfrastructure(connectionString, builder.Environment);

    // Redis接続の追加
    builder.AddRedisClient("redis");

    // EmployeeService API呼び出し用のHttpClientを追加
    builder.Services.AddHttpClient("EmployeeService", client =>
    {
        // Aspire Service Discoveryを使用する場合
        client.BaseAddress = new Uri("http://employeeservice-api");
    });
}

// MCP Server 登録（共通設定）
builder.AddMcpServerDefaults();

var app = builder.Build();

// データベース初期化 (Test環境では実行しない)
if (!app.Environment.IsEnvironment("Test"))
{
    await app.InitializeDatabaseAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Add Scalar UI for better OpenAPI documentation experience
    app.MapScalarApiReference(_ => _.Servers = []);
}

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseExceptionHandling();

// MCP エンドポイントとCORS設定（共通設定）
app.UseMcpServerDefaults();

// Map endpoints
app.MapAttendanceEndpoints();
app.MapLeaveRequestEndpoints();
app.MapDevelopmentEndpoints();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
