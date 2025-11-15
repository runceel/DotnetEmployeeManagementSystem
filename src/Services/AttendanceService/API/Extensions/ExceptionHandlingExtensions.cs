using Microsoft.AspNetCore.Diagnostics;

namespace AttendanceService.API.Extensions;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
        
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                
                var error = context.Features.Get<IExceptionHandlerFeature>();
                if (error != null)
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(error.Error, "Unhandled exception occurred");
                    
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "内部サーバーエラーが発生しました。",
                        message = environment.IsDevelopment() ? error.Error.Message : "システム管理者にお問い合わせください。",
                        traceId = context.TraceIdentifier
                    });
                }
            });
        });
        
        return app;
    }
}
