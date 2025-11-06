using AuthService.Application.Services;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.AuthService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// データベース接続文字列とInfrastructure層の初期化
var connectionString = builder.Configuration.GetConnectionString("AuthDb") 
    ?? "Data Source=auth.db";

// Infrastructure層のサービスを追加
builder.Services.AddInfrastructure(connectionString);

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

// Auth API endpoints
var auth = app.MapGroup("/api/auth")
    .WithTags("Authentication")
    .WithOpenApi();

// ログインエンドポイント
auth.MapPost("/login", async ([FromBody] LoginRequest request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request);
    return result is not null ? Results.Ok(result) : Results.Unauthorized();
})
.WithName("Login")
.Produces<AuthResponse>()
.Produces(StatusCodes.Status401Unauthorized);

// ユーザー登録エンドポイント
auth.MapPost("/register", async ([FromBody] RegisterRequest request, IAuthService authService) =>
{
    var result = await authService.RegisterAsync(request);
    if (result is null)
    {
        return Results.BadRequest(new { error = "ユーザー名またはメールアドレスが既に使用されています。" });
    }
    return Results.Created($"/api/auth/users/{result.UserId}", result);
})
.WithName("Register")
.Produces<AuthResponse>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
