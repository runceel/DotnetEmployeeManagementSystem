using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EmployeeService.API.Extensions;

public static class AuthenticationConfigurationExtensions
{
    private const string CustomAuthScheme = "CustomAuth";

    public static IServiceCollection AddEmployeeServiceAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IHostEnvironment environment)
    {
        // 認証・認可の設定
        // カスタム認証スキーム（X-User-*ヘッダー）とJWT Bearer認証の両方をサポート
        services.AddAuthentication(options =>
        {
            // デフォルトスキームをカスタム認証に設定（本番環境用）
            options.DefaultAuthenticateScheme = CustomAuthScheme;
            options.DefaultChallengeScheme = CustomAuthScheme;
        })
        .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>(CustomAuthScheme, null)
        .AddJwtBearer(options =>
        {
            // JWT Bearer認証はテスト環境で使用
            var secretKey = configuration["Jwt:SecretKey"];
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];

            // Test環境でのみJWT設定を必須にする
            if (environment.IsEnvironment("Test"))
            {
                if (string.IsNullOrEmpty(secretKey))
                    throw new InvalidOperationException("JWT SecretKey is not configured");
                if (string.IsNullOrEmpty(issuer))
                    throw new InvalidOperationException("JWT Issuer is not configured");
                if (string.IsNullOrEmpty(audience))
                    throw new InvalidOperationException("JWT Audience is not configured");
            }

            if (!string.IsNullOrEmpty(secretKey) && !string.IsNullOrEmpty(issuer) && !string.IsNullOrEmpty(audience))
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            }
        });

        // ポリシーベースの認可設定 - 複数の認証スキームをサポート
        services.AddAuthorization(options =>
        {
            // 管理者ロールが必要なポリシー
            options.AddPolicy("AdminPolicy", policy =>
            {
                policy.RequireRole("Admin");
                // CustomAuthとJWT Bearerの両方の認証スキームをサポート
                policy.AuthenticationSchemes.Add(CustomAuthScheme);
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
            });
        });

        return services;
    }
}
