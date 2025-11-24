using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using ModelContextProtocol.Server;

namespace AuthService.API.Mcp.Tools;

/// <summary>
/// ロール管理に関するMCPツール
/// </summary>
[McpServerToolType]
public class RoleTools
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RoleTools> _logger;

    public RoleTools(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IHttpContextAccessor httpContextAccessor,
        ILogger<RoleTools> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// システムで利用可能な全ロールの一覧を取得します（管理者のみ）。
    /// </summary>
    [McpServerTool]
    public async Task<RoleListResponse> ListRolesAsync()
    {
        _logger.LogInformation("MCP Tool: ListRoles");

        // 管理者権限チェック
        EnsureAdminRole();

        var roles = _roleManager.Roles.ToList();
        var roleSummaries = new List<RoleSummary>();

        foreach (var role in roles)
        {
            // このロールを持つユーザー数を取得
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            
            roleSummaries.Add(new RoleSummary(
                RoleId: role.Id,
                RoleName: role.Name ?? "",
                UserCount: usersInRole.Count
            ));
        }

        return new RoleListResponse(
            Roles: roleSummaries,
            TotalCount: roleSummaries.Count
        );
    }

    /// <summary>
    /// 指定されたロールを持つユーザー一覧を取得します（管理者のみ）。
    /// </summary>
    [McpServerTool]
    public async Task<UsersInRoleResponse> GetUsersInRoleAsync(
        string roleName)
    {
        ArgumentNullException.ThrowIfNull(roleName);
        
        _logger.LogInformation("MCP Tool: GetUsersInRole - RoleName: {RoleName}", roleName);

        // 管理者権限チェック
        EnsureAdminRole();

        // ロールの存在確認
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            throw new InvalidOperationException($"ロール '{roleName}' が見つかりませんでした。");
        }

        var users = await _userManager.GetUsersInRoleAsync(roleName);
        var userSummaries = users.Select(u => new UserInRoleSummary(
            UserId: u.Id,
            UserName: u.UserName ?? "",
            Email: u.Email ?? ""
        )).ToList();

        return new UsersInRoleResponse(
            RoleName: roleName,
            Users: userSummaries,
            UserCount: userSummaries.Count
        );
    }

    /// <summary>
    /// ユーザーにロールを割り当てます（管理者のみ）。
    /// </summary>
    [McpServerTool]
    public async Task<AssignRoleResponse> AssignRoleAsync(
        string userId,
        string roleName)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(roleName);
        
        _logger.LogInformation("MCP Tool: AssignRole - UserId: {UserId}, RoleName: {RoleName}", 
            userId, roleName);

        // 管理者権限チェック
        EnsureAdminRole();

        // ユーザーの存在確認
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"ユーザーID {userId} が見つかりませんでした。");
        }

        // ロールの存在確認
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            throw new InvalidOperationException($"ロール '{roleName}' が見つかりませんでした。");
        }

        // 既にロールを持っているかチェック
        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            _logger.LogInformation("User {UserId} already has role {RoleName}", userId, roleName);
            return new AssignRoleResponse(
                Success: true,
                Message: $"ユーザー {user.UserName} は既にロール '{roleName}' を持っています。",
                UserId: user.Id,
                UserName: user.UserName ?? "",
                RoleName: roleName
            );
        }

        // ロールを割り当て
        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"ロールの割り当てに失敗しました: {errors}");
        }

        _logger.LogInformation("Successfully assigned role {RoleName} to user {UserId}", 
            roleName, userId);

        return new AssignRoleResponse(
            Success: true,
            Message: $"ユーザー {user.UserName} にロール '{roleName}' を割り当てました。",
            UserId: user.Id,
            UserName: user.UserName ?? "",
            RoleName: roleName
        );
    }

    /// <summary>
    /// ユーザーからロールを削除します（管理者のみ）。
    /// </summary>
    [McpServerTool]
    public async Task<RemoveRoleResponse> RemoveRoleAsync(
        string userId,
        string roleName)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(roleName);
        
        _logger.LogInformation("MCP Tool: RemoveRole - UserId: {UserId}, RoleName: {RoleName}", 
            userId, roleName);

        // 管理者権限チェック
        EnsureAdminRole();

        // ユーザーの存在確認
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"ユーザーID {userId} が見つかりませんでした。");
        }

        // ロールを持っているかチェック
        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            _logger.LogInformation("User {UserId} does not have role {RoleName}", userId, roleName);
            return new RemoveRoleResponse(
                Success: true,
                Message: $"ユーザー {user.UserName} はロール '{roleName}' を持っていません。",
                UserId: user.Id,
                UserName: user.UserName ?? "",
                RoleName: roleName
            );
        }

        // ロールを削除
        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"ロールの削除に失敗しました: {errors}");
        }

        _logger.LogInformation("Successfully removed role {RoleName} from user {UserId}", 
            roleName, userId);

        return new RemoveRoleResponse(
            Success: true,
            Message: $"ユーザー {user.UserName} からロール '{roleName}' を削除しました。",
            UserId: user.Id,
            UserName: user.UserName ?? "",
            RoleName: roleName
        );
    }

    // プライベートヘルパーメソッド

    private void EnsureAdminRole()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.IsInRole("Admin") != true)
        {
            throw new UnauthorizedAccessException("この操作には管理者権限が必要です。");
        }
    }
}

// レスポンスDTO定義

/// <summary>
/// ロールサマリー情報
/// </summary>
public record RoleSummary(
    string RoleId,
    string RoleName,
    int UserCount
);

/// <summary>
/// ロール一覧レスポンス
/// </summary>
public record RoleListResponse(
    IReadOnlyList<RoleSummary> Roles,
    int TotalCount
);

/// <summary>
/// ロール内のユーザーサマリー
/// </summary>
public record UserInRoleSummary(
    string UserId,
    string UserName,
    string Email
);

/// <summary>
/// ロール内のユーザー一覧レスポンス
/// </summary>
public record UsersInRoleResponse(
    string RoleName,
    IReadOnlyList<UserInRoleSummary> Users,
    int UserCount
);

/// <summary>
/// ロール割り当てレスポンス
/// </summary>
public record AssignRoleResponse(
    bool Success,
    string Message,
    string UserId,
    string UserName,
    string RoleName
);

/// <summary>
/// ロール削除レスポンス
/// </summary>
public record RemoveRoleResponse(
    bool Success,
    string Message,
    string UserId,
    string UserName,
    string RoleName
);
