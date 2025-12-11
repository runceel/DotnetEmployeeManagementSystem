using AuthService.API.Mcp.Tools;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AuthService.Tests.Mcp;

public class UserToolsTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<UserTools>> _loggerMock;
    private readonly UserTools _userTools;

    public UserToolsTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
        
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<UserTools>>();

        _userTools = new UserTools(
            _userManagerMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserExists_ReturnsUserDetails()
    {
        // Arrange
        var userId = "test-user-id";
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        SetupAuthenticatedUser(userId, "User");
        _userManagerMock.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _userTools.GetCurrentUserAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("testuser", result.UserName);
        Assert.Equal("test@example.com", result.Email);
        Assert.True(result.EmailConfirmed);
        Assert.Single(result.Roles);
        Assert.Contains("User", result.Roles);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userTools.GetCurrentUserAsync());
    }

    [Fact]
    public async Task GetUserAsync_WithAdminRole_ReturnsUserDetails()
    {
        // Arrange
        var targetUserId = "target-user-id";
        var user = new ApplicationUser
        {
            Id = targetUserId,
            UserName = "targetuser",
            Email = "target@example.com",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        SetupAuthenticatedUser("admin-id", "Admin");
        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _userTools.GetUserAsync(targetUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetUserId, result.UserId);
        Assert.Equal("targetuser", result.UserName);
    }

    [Fact]
    public async Task GetUserAsync_WithoutAdminRole_ThrowsUnauthorizedException()
    {
        // Arrange
        SetupAuthenticatedUser("user-id", "User");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userTools.GetUserAsync("other-user-id"));
    }

    [Fact]
    public async Task GetUserAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthenticatedUser("admin-id", "Admin");
        _userManagerMock.Setup(m => m.FindByIdAsync("nonexistent-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userTools.GetUserAsync("nonexistent-id"));
    }

    [Fact]
    public async Task SearchUserByEmailAsync_WithAdminRole_ReturnsUserWhenFound()
    {
        // Arrange
        var email = "search@example.com";
        var user = new ApplicationUser
        {
            Id = "found-user-id",
            UserName = "founduser",
            Email = email,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        SetupAuthenticatedUser("admin-id", "Admin");
        _userManagerMock.Setup(m => m.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _userTools.SearchUserByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result!.Email);
        Assert.Equal("founduser", result.UserName);
    }

    [Fact]
    public async Task SearchUserByEmailAsync_WhenUserNotFound_ReturnsNull()
    {
        // Arrange
        SetupAuthenticatedUser("admin-id", "Admin");
        _userManagerMock.Setup(m => m.FindByEmailAsync("nonexistent@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _userTools.SearchUserByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ListUsersAsync_WithAdminRole_ReturnsUserList()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "1", UserName = "user1", Email = "user1@example.com", CreatedAt = DateTime.UtcNow },
            new ApplicationUser { Id = "2", UserName = "user2", Email = "user2@example.com", CreatedAt = DateTime.UtcNow }
        };

        SetupAuthenticatedUser("admin-id", "Admin");
        _userManagerMock.Setup(m => m.Users)
            .Returns(users.AsQueryable());
        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _userTools.ListUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Users.Count);
        Assert.Contains(result.Users, u => u.UserName == "user1");
        Assert.Contains(result.Users, u => u.UserName == "user2");
    }

    [Fact]
    public async Task ListUsersAsync_WithoutAdminRole_ThrowsUnauthorizedException()
    {
        // Arrange
        SetupAuthenticatedUser("user-id", "User");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userTools.ListUsersAsync());
    }

    [Fact]
    public async Task GetUserRolesAsync_ForSelf_ReturnsRoles()
    {
        // Arrange
        var userId = "current-user-id";
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com"
        };

        SetupAuthenticatedUser(userId, "User");
        _userManagerMock.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User", "Manager" });

        // Act
        var result = await _userTools.GetUserRolesAsync(null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(2, result.Roles.Count);
        Assert.Contains("User", result.Roles);
        Assert.Contains("Manager", result.Roles);
    }

    [Fact]
    public async Task GetUserRolesAsync_ForOtherUserWithoutAdminRole_ThrowsUnauthorizedException()
    {
        // Arrange
        SetupAuthenticatedUser("user-id", "User");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userTools.GetUserRolesAsync("other-user-id"));
    }

    [Fact]
    public async Task GetUserRolesAsync_ForOtherUserWithAdminRole_ReturnsRoles()
    {
        // Arrange
        var targetUserId = "target-user-id";
        var user = new ApplicationUser
        {
            Id = targetUserId,
            UserName = "targetuser",
            Email = "target@example.com"
        };

        SetupAuthenticatedUser("admin-id", "Admin");
        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _userTools.GetUserRolesAsync(targetUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetUserId, result.UserId);
        Assert.Single(result.Roles);
    }

    // Helper methods

    private void SetupAuthenticatedUser(string userId, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
    }

    private void SetupUnauthenticatedUser()
    {
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
    }
}
