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

// MCP Server 登録（共通設定）
builder.AddMcpServerDefaults();

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

// MCP エンドポイントとCORS設定（共通設定）
app.UseMcpServerDefaults();

// Map endpoints
app.MapAuthEndpoints();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
