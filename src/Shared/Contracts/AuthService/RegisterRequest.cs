namespace Shared.Contracts.AuthService;

/// <summary>
/// ユーザー登録リクエスト
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// ユーザー名
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// メールアドレス
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// パスワード
    /// </summary>
    public required string Password { get; init; }
}
