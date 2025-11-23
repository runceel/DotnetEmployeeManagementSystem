using Microsoft.EntityFrameworkCore;
using NotificationService.API.Endpoints;
using NotificationService.Application.Services;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Services;
using NotificationService.Infrastructure.Workers;
using StackExchange.Redis;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "NotificationService API",
            Version = "v1",
            Description = "通知送信とイベント処理のAPI",
            Contact = new OpenApiContact
            {
                Name = "開発チーム",
                Email = "dev@example.com"
            }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT認証トークンを入力してください",
            In = ParameterLocation.Header,
            Name = "Authorization"
        };

        return Task.CompletedTask;
    });
});

// データベース接続文字列 (Test環境ではスキップ)
if (!builder.Environment.IsEnvironment("Test"))
{
    var connectionString = builder.Configuration.GetConnectionString("NotificationDb") 
        ?? "Data Source=notifications.db";
    
    builder.Services.AddDbContext<NotificationDbContext>(options =>
        options.UseSqlite(connectionString));
}

// Redis接続 (Test環境ではスキップ)
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.AddRedisClient("redis");
}

// リポジトリとサービスの登録
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService.Application.UseCases.NotificationService>();
builder.Services.AddScoped<IEmailService, ConsoleEmailService>();
builder.Services.AddScoped<IEventPublisher, RedisEventPublisher>();

// MCP Server 登録 (Test環境ではスキップ)
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddMcpServer()
        .WithHttpTransport()           // HTTP/SSE transport
        .WithToolsFromAssembly();      // 自動的に[McpServerToolType]を検出

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
                policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        }
    });
}

// バックグラウンドワーカーの登録 (Test環境ではスキップ)
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddHostedService<EmployeeEventConsumer>();
    builder.Services.AddHostedService<AttendanceEventConsumer>();
    builder.Services.AddHostedService<NotificationProcessorWorker>();
}

var app = builder.Build();

// データベースマイグレーション (Test環境ではスキップ)
if (!app.Environment.IsEnvironment("Test"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// CORS有効化 (Test環境ではスキップ)
if (!app.Environment.IsEnvironment("Test"))
{
    app.UseCors("McpPolicy");
}

// Map endpoints
app.MapNotificationEndpoints();

// MCP エンドポイントマッピング (Test環境ではスキップ)
if (!app.Environment.IsEnvironment("Test"))
{
    app.MapMcp("/api/mcp");
}

app.Run();
