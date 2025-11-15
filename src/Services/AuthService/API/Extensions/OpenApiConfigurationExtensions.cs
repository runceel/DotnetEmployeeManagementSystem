using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AuthService.API.Extensions;

public static class OpenApiConfigurationExtensions
{
    public static IServiceCollection AddAuthServiceOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "AuthService API",
                    Version = "v1",
                    Description = "認証・ユーザー管理API",
                    Contact = new OpenApiContact
                    {
                        Name = "開発チーム",
                        Email = "dev@example.com"
                    }
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT認証トークンを入力してください",
                    In = ParameterLocation.Header,
                    Name = "Authorization"
                };

                return Task.CompletedTask;
            });
        });
        
        return services;
    }
}
