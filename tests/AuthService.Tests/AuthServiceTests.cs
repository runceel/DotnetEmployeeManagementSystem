using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Contracts.AuthService;

namespace AuthService.Tests;

/// <summary>
/// AuthService統合テスト
/// インメモリデータベースを使用した実際の認証フローのテスト
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Infrastructure.Services.AuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthServiceTests()
    {
        // テスト用のサービスコレクションを構築
        var services = new ServiceCollection();
        
        // インメモリデータベースの設定
        services.AddDbContext<AuthDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
        
        // ASP.NET Core Identityの設定
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<AuthDbContext>()
        .AddDefaultTokenProviders();
        
        // ロギングの設定
        services.AddLogging();
        
        // AuthServiceの登録
        services.AddScoped<Infrastructure.Services.AuthService>();
        
        _serviceProvider = services.BuildServiceProvider();
        
        // データベースの初期化
        var context = _serviceProvider.GetRequiredService<AuthDbContext>();
        context.Database.EnsureCreated();
        
        // ロールの初期化
        var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
        roleManager.CreateAsync(new IdentityRole("User")).GetAwaiter().GetResult();
        
        _authService = _serviceProvider.GetRequiredService<Infrastructure.Services.AuthService>();
        _userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var password = "Password123!";
        var user = new ApplicationUser
        {
            UserName = "testuser",
            Email = "test@example.com"
        };
        await _userManager.CreateAsync(user, password);

        var request = new LoginRequest
        {
            UserNameOrEmail = "testuser",
            Password = password
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal(user.Email, result.Email);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            UserNameOrEmail = "nonexistent",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserName = "testuser2",
            Email = "test2@example.com"
        };
        await _userManager.CreateAsync(user, "CorrectPassword123!");

        var request = new LoginRequest
        {
            UserNameOrEmail = "testuser2",
            Password = "WrongPassword"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            UserName = "newuser",
            Email = "newuser@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.UserName, result.UserName);
        Assert.Equal(request.Email, result.Email);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsNull()
    {
        // Arrange
        var existingUser = new ApplicationUser
        {
            UserName = "existinguser",
            Email = "existing@example.com"
        };
        await _userManager.CreateAsync(existingUser, "Password123!");

        var request = new RegisterRequest
        {
            UserName = "existinguser",
            Email = "newemail@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsNull()
    {
        // Arrange
        var existingUser = new ApplicationUser
        {
            UserName = "existinguser2",
            Email = "existing@example.com"
        };
        await _userManager.CreateAsync(existingUser, "Password123!");

        var request = new RegisterRequest
        {
            UserName = "newuser2",
            Email = "existing@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithEmail_ReturnsAuthResponse()
    {
        // Arrange
        var password = "Password123!";
        var user = new ApplicationUser
        {
            UserName = "testuser3",
            Email = "test3@example.com"
        };
        await _userManager.CreateAsync(user, password);

        var request = new LoginRequest
        {
            UserNameOrEmail = "test3@example.com",
            Password = password
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal(user.Email, result.Email);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
