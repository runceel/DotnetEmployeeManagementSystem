using AuthService.Application.DTOs;

namespace AuthService.Application.Services;

/// <summary>
/// 認証サービスインターフェース
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// ユーザーログイン
    /// </summary>
    /// <param name="request">ログインリクエスト</param>
    /// <returns>認証レスポンス、失敗時はnull</returns>
    Task<AuthResponse?> LoginAsync(LoginRequest request);

    /// <summary>
    /// ユーザー登録
    /// </summary>
    /// <param name="request">登録リクエスト</param>
    /// <returns>認証レスポンス、失敗時はnull</returns>
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
}
