using AuthService.Application.Services;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Contracts.AuthService;

namespace AuthService.Infrastructure.Services;

/// <summary>
/// 認証サービス実装（ダミー認証）
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            // ユーザー名またはメールアドレスでユーザーを検索
            var user = await _userManager.FindByNameAsync(request.UserNameOrEmail)
                ?? await _userManager.FindByEmailAsync(request.UserNameOrEmail);

            if (user == null)
            {
                _logger.LogWarning("ログイン失敗: ユーザーが見つかりません - {UserNameOrEmail}", request.UserNameOrEmail);
                return null;
            }

            // パスワード検証
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("ログイン失敗: パスワードが正しくありません - {UserName}", user.UserName);
                return null;
            }

            _logger.LogInformation("ログイン成功: {UserName}", user.UserName);

            // ダミートークンを生成（実際のJWT実装は今後の課題）
            var token = GenerateDummyToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                Token = token
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ログイン処理中にエラーが発生しました");
            throw;
        }
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // ユーザー名の重複チェック
            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null)
            {
                _logger.LogWarning("ユーザー登録失敗: ユーザー名が既に存在します - {UserName}", request.UserName);
                return null;
            }

            // メールアドレスの重複チェック
            var existingEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                _logger.LogWarning("ユーザー登録失敗: メールアドレスが既に存在します - {Email}", request.Email);
                return null;
            }

            // 新しいユーザーを作成
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                EmailConfirmed = true, // ダミー実装のため自動確認
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("ユーザー登録失敗: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }

            _logger.LogInformation("ユーザー登録成功: {UserName}", user.UserName);

            // ダミートークンを生成
            var token = GenerateDummyToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Token = token
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ユーザー登録処理中にエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// ダミートークンを生成（実際のJWT実装は今後の課題）
    /// </summary>
    private static string GenerateDummyToken(ApplicationUser user)
    {
        // Base64エンコードされたダミートークン
        var tokenData = $"{user.Id}:{user.UserName}:{DateTime.UtcNow:O}";
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
        return Convert.ToBase64String(tokenBytes);
    }
}
