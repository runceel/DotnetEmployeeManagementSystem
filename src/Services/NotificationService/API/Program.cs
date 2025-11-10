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

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();

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

// Map endpoints
app.MapNotificationEndpoints();

app.Run();
