using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using ModelContextProtocol.Server;
using System.Security.Claims;

namespace AuthService.API.Mcp.Tools;

/// <summary>
/// ユーザー管理に関するMCPツール
/// </summary>
[McpServerToolType]
public class UserTools
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserTools> _logger;

    public UserTools(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserTools> logger)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// 現在ログインしているユーザーの情報を取得します。
    /// </summary>
    [McpServerTool]
    public async Task<UserDetailResponse> GetCurrentUserAsync()
    {
        _logger.LogInformation("MCP Tool: GetCurrentUser");

        var userId = GetCurrentUserId();
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("ユーザー情報が見つかりませんでした。");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDetailResponse(
            UserId: user.Id,
            UserName: user.UserName ?? "",
            Email: user.Email ?? "",
            EmailConfirmed: user.EmailConfirmed,
            Roles: roles.ToList(),
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt
        );
    }

    /// <summary>
    /// 指定されたユーザーIDのユーザー情報を取得します（管理者のみ）。
    /// </summary>
    [McpServerTool]
    public async Task<UserDetailResponse> GetUserAsync(
        string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);
        
        _logger.LogInformation("MCP Tool: GetUser - UserId: {UserId}", userId);

        // 管理者権限チェック
        EnsureAdminRole();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"ユーザーID {userId} が見つかりませんでした。");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDetailResponse(
            UserId: user.Id,
            UserName: user.UserName ?? "",
            Email: user.Email ?? "",
            EmailConfirmed: user.EmailConfirmed,
            Roles: roles.ToList(),
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt
        );
    }

    /// <summary>
    /// メールアドレスでユーザーを検索します（管理者のみ）。
    /// </summary>
    [McpServerTool]
    public async Task<UserDetailResponse?> SearchUserByEmailAsync(
        string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        _logger.LogInformation("MCP Tool: SearchUserByEmail - Email: {Email}", email);

        // 管理者権限チェック
        EnsureAdminRole();

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogInformation("User not found with email: {Email}", email);
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDetailResponse(
            UserId: user.Id,
            UserName: user.UserName ?? "",
            Email: user.Email ?? "",
            EmailConfirmed: user.EmailConfirmed,
            Roles: roles.ToList(),
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt
        );
    }

    /// <summary>
    /// 全ユーザーの一覧を取得します（管理者のみ）。
    /// </summary>
    [McpServerTool]
    public async Task<UserListResponse> ListUsersAsync()
    {
        _logger.LogInformation("MCP Tool: ListUsers");

        // 管理者権限チェック
        EnsureAdminRole();

        var users = _userManager.Users.ToList();
        var userSummaries = new List<UserSummary>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userSummaries.Add(new UserSummary(
                UserId: user.Id,
                UserName: user.UserName ?? "",
                Email: user.Email ?? "",
                Roles: roles.ToList(),
                CreatedAt: user.CreatedAt
            ));
        }

        return new UserListResponse(
            Users: userSummaries,
            TotalCount: userSummaries.Count
        );
    }

    /// <summary>
    /// ユーザーのロール一覧を取得します。
    /// </summary>
    [McpServerTool]
    public async Task<UserRolesResponse> GetUserRolesAsync(
        string? userId = null)
    {
        var currentUserId = GetCurrentUserId();
        var targetUserId = userId ?? currentUserId;

        _logger.LogInformation("MCP Tool: GetUserRoles - UserId: {UserId}", targetUserId);

        // 他人のロールを確認する場合は管理者権限が必要
        if (targetUserId != currentUserId && !IsAdmin())
        {
            throw new UnauthorizedAccessException("他のユーザーのロール情報を取得するには管理者権限が必要です。");
        }

        var user = await _userManager.FindByIdAsync(targetUserId);
        if (user == null)
        {
            throw new InvalidOperationException($"ユーザーID {targetUserId} が見つかりませんでした。");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserRolesResponse(
            UserId: user.Id,
            UserName: user.UserName ?? "",
            Roles: roles.ToList()
        );
    }

    // プライベートヘルパーメソッド

    private string GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("認証が必要です。ログインしてください。");
        }

        return userId;
    }

    private bool IsAdmin()
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") == true;
    }

    private void EnsureAdminRole()
    {
        if (!IsAdmin())
        {
            throw new UnauthorizedAccessException("この操作には管理者権限が必要です。");
        }
    }
}

// レスポンスDTO定義

/// <summary>
/// ユーザー詳細情報レスポンス
/// </summary>
public record UserDetailResponse(
    string UserId,
    string UserName,
    string Email,
    bool EmailConfirmed,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>
/// ユーザーサマリー情報
/// </summary>
public record UserSummary(
    string UserId,
    string UserName,
    string Email,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt
);

/// <summary>
/// ユーザー一覧レスポンス
/// </summary>
public record UserListResponse(
    IReadOnlyList<UserSummary> Users,
    int TotalCount
);

/// <summary>
/// ユーザーロールレスポンス
/// </summary>
public record UserRolesResponse(
    string UserId,
    string UserName,
    IReadOnlyList<string> Roles
);
