using AuthService.Application.Services;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Contracts.AuthService;

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
            Title = "AuthService API",
            Version = "v1",
            Description = "認証・ユーザー管理API",
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
