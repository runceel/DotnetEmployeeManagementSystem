using AuthService.API.Endpoints;
using AuthService.API.Extensions;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddAuthServiceOpenApi();

// データベース接続文字列とInfrastructure層の初期化
var connectionString = builder.Configuration.GetConnectionString("AuthDb") 
    ?? "Data Source=auth.db";

// Infrastructure層のサービスを追加
builder.Services.AddInfrastructure(connectionString);

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

// データベース初期化
await DbInitializer.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// CORS有効化
app.UseCors("McpPolicy");

// MCP エンドポイントマッピング
app.MapMcp("/api/mcp");

// Map endpoints
app.MapAuthEndpoints();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
