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
    builder.Services.AddInfrastructure(connectionString);

    // Redis接続の追加
    builder.AddRedisClient("redis");
}

// Application層のサービスを追加
builder.Services.AddScoped<IEmployeeService, EmployeeService.Application.UseCases.EmployeeService>();
builder.Services.AddScoped<IDepartmentService, EmployeeService.Application.UseCases.DepartmentService>();

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

// CORS有効化
app.UseCors("McpPolicy");

// MCP エンドポイントマッピング
app.MapMcp("/api/mcp");

// Map endpoints
app.MapEmployeeEndpoints();
app.MapDepartmentEndpoints();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
