using AuthService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.AuthService;

namespace AuthService.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        auth.MapPost("/login", Login)
            .WithName("Login")
            .Produces<AuthResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        auth.MapPost("/register", Register)
            .WithName("Register")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IAuthService authService)
    {
        var result = await authService.LoginAsync(request);
        return result is not null ? Results.Ok(result) : Results.Unauthorized();
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] IAuthService authService)
    {
        var result = await authService.RegisterAsync(request);
        if (result is null)
        {
            return Results.BadRequest(new { error = "ユーザー名またはメールアドレスが既に使用されています。" });
        }
        return Results.Created($"/api/auth/users/{result.UserId}", result);
    }
}
