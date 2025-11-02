namespace AuthService.Application.DTOs;

/// <summary>
/// ログインリクエスト
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// ユーザー名またはメールアドレス
    /// </summary>
    public required string UserNameOrEmail { get; init; }

    /// <summary>
    /// パスワード
    /// </summary>
    public required string Password { get; init; }
}
