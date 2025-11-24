using AuthService.API.Mcp.Tools;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AuthService.Tests.Mcp;

public class RoleToolsTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<RoleTools>> _loggerMock;
    private readonly RoleTools _roleTools;

    public RoleToolsTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null, null, null, null, null, null, null, null);

        var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            roleStoreMock.Object,
            null, null, null, null);
        
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<RoleTools>>();

        _roleTools = new RoleTools(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ListRolesAsync_WithAdminRole_ReturnsRoleList()
    {
        // Arrange
        var roles = new List<IdentityRole>
        {
            new IdentityRole { Id = "1", Name = "Admin" },
            new IdentityRole { Id = "2", Name = "User" },
            new IdentityRole { Id = "3", Name = "Manager" }
        };

        SetupAuthenticatedAdmin();
        _roleManagerMock.Setup(m => m.Roles)
            .Returns(roles.AsQueryable());
        _userManagerMock.Setup(m => m.GetUsersInRoleAsync("Admin"))
            .ReturnsAsync(new List<ApplicationUser> { new ApplicationUser() });
        _userManagerMock.Setup(m => m.GetUsersInRoleAsync("User"))
            .ReturnsAsync(new List<ApplicationUser> { new ApplicationUser(), new ApplicationUser() });
        _userManagerMock.Setup(m => m.GetUsersInRoleAsync("Manager"))
            .ReturnsAsync(new List<ApplicationUser>());

        // Act
        var result = await _roleTools.ListRolesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Roles.Count);
        
        var adminRole = result.Roles.First(r => r.RoleName == "Admin");
        Assert.Equal(1, adminRole.UserCount);
        
        var userRole = result.Roles.First(r => r.RoleName == "User");
        Assert.Equal(2, userRole.UserCount);
    }

    [Fact]
    public async Task ListRolesAsync_WithoutAdminRole_ThrowsUnauthorizedException()
    {
        // Arrange
        SetupAuthenticatedUser("user-id", "User");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _roleTools.ListRolesAsync());
    }

    [Fact]
    public async Task GetUsersInRoleAsync_WithAdminRole_ReturnsUsersInRole()
    {
        // Arrange
        var roleName = "Manager";
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "1", UserName = "manager1", Email = "manager1@example.com" },
            new ApplicationUser { Id = "2", UserName = "manager2", Email = "manager2@example.com" }
        };

        SetupAuthenticatedAdmin();
        _roleManagerMock.Setup(m => m.RoleExistsAsync(roleName))
            .ReturnsAsync(true);
        _userManagerMock.Setup(m => m.GetUsersInRoleAsync(roleName))
            .ReturnsAsync(users);

        // Act
        var result = await _roleTools.GetUsersInRoleAsync(roleName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roleName, result.RoleName);
        Assert.Equal(2, result.UserCount);
        Assert.Equal(2, result.Users.Count);
        Assert.Contains(result.Users, u => u.UserName == "manager1");
        Assert.Contains(result.Users, u => u.UserName == "manager2");
    }

    [Fact]
    public async Task GetUsersInRoleAsync_WhenRoleNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthenticatedAdmin();
        _roleManagerMock.Setup(m => m.RoleExistsAsync("NonexistentRole"))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _roleTools.GetUsersInRoleAsync("NonexistentRole"));
    }

    [Fact]
    public async Task AssignRoleAsync_WithAdminRole_AssignsRoleSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var roleName = "Manager";
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com"
        };

        SetupAuthenticatedAdmin();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(user);
        _roleManagerMock.Setup(m => m.RoleExistsAsync(roleName))
            .ReturnsAsync(true);
        _userManagerMock.Setup(m => m.IsInRoleAsync(user, roleName))
            .ReturnsAsync(false);
        _userManagerMock.Setup(m => m.AddToRoleAsync(user, roleName))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _roleTools.AssignRoleAsync(userId, roleName);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(roleName, result.RoleName);
        Assert.Contains("割り当てました", result.Message);
        _userManagerMock.Verify(m => m.AddToRoleAsync(user, roleName), Times.Once);
    }

    [Fact]
    public async Task AssignRoleAsync_WhenUserAlreadyHasRole_ReturnsSuccessMessage()
    {
        // Arrange
        var userId = "user-id";
        var roleName = "Manager";
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com"
        };

        SetupAuthenticatedAdmin();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(user);
        _roleManagerMock.Setup(m => m.RoleExistsAsync(roleName))
            .ReturnsAsync(true);
        _userManagerMock.Setup(m => m.IsInRoleAsync(user, roleName))
            .ReturnsAsync(true);

        // Act
        var result = await _roleTools.AssignRoleAsync(userId, roleName);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("既に", result.Message);
        _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AssignRoleAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthenticatedAdmin();
        _userManagerMock.Setup(m => m.FindByIdAsync("nonexistent-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _roleTools.AssignRoleAsync("nonexistent-id", "Manager"));
    }

    [Fact]
    public async Task AssignRoleAsync_WhenRoleNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-id", UserName = "testuser" };

        SetupAuthenticatedAdmin();
        _userManagerMock.Setup(m => m.FindByIdAsync("user-id"))
            .ReturnsAsync(user);
        _roleManagerMock.Setup(m => m.RoleExistsAsync("NonexistentRole"))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _roleTools.AssignRoleAsync("user-id", "NonexistentRole"));
    }

    [Fact]
    public async Task AssignRoleAsync_WithoutAdminRole_ThrowsUnauthorizedException()
    {
        // Arrange
        SetupAuthenticatedUser("user-id", "User");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _roleTools.AssignRoleAsync("target-user-id", "Manager"));
    }

    [Fact]
    public async Task RemoveRoleAsync_WithAdminRole_RemovesRoleSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var roleName = "Manager";
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com"
        };

        SetupAuthenticatedAdmin();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.IsInRoleAsync(user, roleName))
            .ReturnsAsync(true);
        _userManagerMock.Setup(m => m.RemoveFromRoleAsync(user, roleName))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _roleTools.RemoveRoleAsync(userId, roleName);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(roleName, result.RoleName);
        Assert.Contains("削除しました", result.Message);
        _userManagerMock.Verify(m => m.RemoveFromRoleAsync(user, roleName), Times.Once);
    }

    [Fact]
    public async Task RemoveRoleAsync_WhenUserDoesNotHaveRole_ReturnsSuccessMessage()
    {
        // Arrange
        var userId = "user-id";
        var roleName = "Manager";
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com"
        };

        SetupAuthenticatedAdmin();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.IsInRoleAsync(user, roleName))
            .ReturnsAsync(false);

        // Act
        var result = await _roleTools.RemoveRoleAsync(userId, roleName);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("持っていません", result.Message);
        _userManagerMock.Verify(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RemoveRoleAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthenticatedAdmin();
        _userManagerMock.Setup(m => m.FindByIdAsync("nonexistent-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _roleTools.RemoveRoleAsync("nonexistent-id", "Manager"));
    }

    [Fact]
    public async Task RemoveRoleAsync_WithoutAdminRole_ThrowsUnauthorizedException()
    {
        // Arrange
        SetupAuthenticatedUser("user-id", "User");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _roleTools.RemoveRoleAsync("target-user-id", "Manager"));
    }

    // Helper methods

    private void SetupAuthenticatedAdmin()
    {
        SetupAuthenticatedUser("admin-id", "Admin");
    }

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
}
