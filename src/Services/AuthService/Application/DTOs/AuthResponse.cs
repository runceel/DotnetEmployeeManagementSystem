namespace AuthService.Application.DTOs;

/// <summary>
/// 認証レスポンス
/// </summary>
public record AuthResponse
{
    /// <summary>
    /// ユーザーID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// ユーザー名
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// メールアドレス
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// 認証トークン（ダミー実装のため固定値）
    /// </summary>
    public required string Token { get; init; }
}
