using Shared.Contracts.AuthService;

namespace BlazorWeb.Services;

/// <summary>
/// 認証状態管理サービス
/// </summary>
public class AuthStateService
{
    private AuthResponse? _currentUser;
    private readonly ILogger<AuthStateService> _logger;

    public AuthStateService(ILogger<AuthStateService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 現在のユーザー情報
    /// </summary>
    public AuthResponse? CurrentUser => _currentUser;

    /// <summary>
    /// ユーザーがログインしているかどうか
    /// </summary>
    public bool IsAuthenticated => _currentUser != null;

    /// <summary>
    /// 認証状態変更イベント
    /// </summary>
    public event Action? OnAuthStateChanged;

    /// <summary>
    /// ログイン処理
    /// </summary>
    public void Login(AuthResponse user)
    {
        _currentUser = user;
        _logger.LogInformation("User logged in: {UserName}", user.UserName);
        NotifyAuthStateChanged();
    }

    /// <summary>
    /// ログアウト処理
    /// </summary>
    public void Logout()
    {
        var userName = _currentUser?.UserName;
        _currentUser = null;
        _logger.LogInformation("User logged out: {UserName}", userName);
        NotifyAuthStateChanged();
    }

    /// <summary>
    /// 認証状態変更を通知
    /// </summary>
    private void NotifyAuthStateChanged()
    {
        OnAuthStateChanged?.Invoke();
    }
}
