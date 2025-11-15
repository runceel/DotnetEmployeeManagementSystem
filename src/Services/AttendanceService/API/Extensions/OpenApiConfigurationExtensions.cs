using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AttendanceService.API.Extensions;

public static class OpenApiConfigurationExtensions
{
    public static IServiceCollection AddAttendanceServiceOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "AttendanceService API",
                    Version = "v1",
                    Description = """
                        勤怠管理サービス API
                        
                        ## 概要
                        従業員の勤怠記録、休暇申請、および月次集計を管理するためのRESTful APIです。
                        
                        ## 主要機能
                        - **勤怠記録管理**: 出退勤の記録と勤務時間の自動計算
                        - **休暇申請管理**: 有給休暇、病気休暇などの申請と承認フロー
                        - **月次集計**: 総勤務時間、平均勤務時間、遅刻回数などの集計
                        
                        ## 認証
                        APIは認証が必要です。リクエストヘッダーに適切な認証情報を含めてください。
                        """,
                    Contact = new OpenApiContact
                    {
                        Name = "開発チーム",
                        Email = "dev@example.com"
                    }
                };
                
                // Add security scheme (Bearer JWT)
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
