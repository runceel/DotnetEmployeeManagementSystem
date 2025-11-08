using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

/// <summary>
/// カスタム認証ハンドラー（ダミートークン検証用）
/// </summary>
public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public CustomAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // X-User-Id, X-User-Name, X-User-Rolesヘッダーから認証情報を取得
        if (!Request.Headers.TryGetValue("X-User-Id", out var userId) ||
            !Request.Headers.TryGetValue("X-User-Name", out var userName))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userName.ToString())
        };

        // ロール情報を追加
        if (Request.Headers.TryGetValue("X-User-Roles", out var roles))
        {
            var roleList = roles.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
            claims.AddRange(roleList.Select(role => new Claim(ClaimTypes.Role, role.Trim())));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
