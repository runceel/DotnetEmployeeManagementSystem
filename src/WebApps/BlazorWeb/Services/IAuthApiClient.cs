using Shared.Contracts.AuthService;

namespace BlazorWeb.Services;

/// <summary>
/// AuthService APIクライアントのインターフェース
/// </summary>
public interface IAuthApiClient
{
    /// <summary>
    /// ユーザーログイン
    /// </summary>
    /// <param name="request">ログインリクエスト</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>認証レスポンス、失敗時はnull</returns>
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// ユーザー登録
    /// </summary>
    /// <param name="request">登録リクエスト</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>認証レスポンス、失敗時はnull</returns>
    Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
