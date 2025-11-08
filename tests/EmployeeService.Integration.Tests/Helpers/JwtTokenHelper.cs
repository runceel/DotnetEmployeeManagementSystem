using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeService.Integration.Tests.Helpers;

/// <summary>
/// Helper class for generating JWT tokens for integration tests
/// </summary>
public static class JwtTokenHelper
{
    private const string SecretKey = "Development-Secret-Key-For-JWT-Token-Generation-Must-Be-At-Least-32-Characters-Long";
    private const string Issuer = "EmployeeManagementSystem.AuthService";
    private const string Audience = "EmployeeManagementSystem.API";

    /// <summary>
    /// Generate a JWT token for testing
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userName">Username</param>
    /// <param name="roles">User roles</param>
    /// <returns>JWT token string</returns>
    public static string GenerateToken(string userId, string userName, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add role claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
