using AuthService.Domain.Entities;

namespace AuthService.Application.Services;

/// <summary>
/// JWTトークン生成サービスインターフェース
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// ユーザーとロール情報からJWTトークンを生成
    /// </summary>
    /// <param name="user">ユーザー情報</param>
    /// <param name="roles">ユーザーのロール</param>
    /// <returns>生成されたJWTトークン</returns>
    string GenerateToken(ApplicationUser user, IList<string> roles);
}
