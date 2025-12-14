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

// HttpContextAccessor登録（MCPツールで使用）
builder.Services.AddHttpContextAccessor();

// MCP Server 登録
builder.Services.AddMcpServer()
    .WithHttpTransport()           // HTTP/SSE transport有効化
    .WithToolsFromAssembly();      // 自動的に[McpServerToolType]クラスを検出

// CORS設定（MCP用）
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("McpPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    }
    else
    {
        options.AddPolicy("McpPolicy", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    }
});

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

// CORS有効化
app.UseCors("McpPolicy");

// MCP エンドポイントマッピング
app.MapMcp("/api/mcp");

// Map endpoints
app.MapAttendanceEndpoints();
app.MapLeaveRequestEndpoints();
app.MapDevelopmentEndpoints();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
