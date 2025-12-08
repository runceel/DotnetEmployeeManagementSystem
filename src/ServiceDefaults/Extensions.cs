using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        // Exclude health check requests from tracing
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath);

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    /// <summary>
    /// MCP サーバーの共通設定を追加します
    /// </summary>
    /// <param name="builder">ホストアプリケーションビルダー</param>
    /// <param name="additionalAllowedOrigins">追加で許可するオリジン（開発環境のみ）</param>
    /// <returns>ホストアプリケーションビルダー</returns>
    public static TBuilder AddMcpServerDefaults<TBuilder>(
        this TBuilder builder,
        string[]? additionalAllowedOrigins = null) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly();

        builder.Services.AddCors(options =>
        {
            ConfigureMcpCorsPolicy(options, builder.Environment, builder.Configuration, additionalAllowedOrigins);
        });

        return builder;
    }

    /// <summary>
    /// MCP 関連のミドルウェアとエンドポイントを設定します
    /// </summary>
    /// <param name="app">Web アプリケーション</param>
    /// <param name="mcpEndpoint">MCP エンドポイントのパス（デフォルト: /api/mcp）</param>
    /// <returns>Web アプリケーション</returns>
    public static WebApplication UseMcpServerDefaults(
        this WebApplication app,
        string mcpEndpoint = "/api/mcp")
    {
        app.UseCors("McpPolicy");
        app.MapMcp(mcpEndpoint);

        return app;
    }

    private static void ConfigureMcpCorsPolicy(
        Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions options,
        IHostEnvironment environment,
        IConfiguration configuration,
        string[]? additionalAllowedOrigins)
    {
        options.AddPolicy("McpPolicy", policy =>
        {
            if (environment.IsDevelopment())
            {
                // 開発環境でも特定オリジンのみ許可（セキュリティ改善）
                var defaultDevOrigins = configuration.GetSection("Cors:DevelopmentOrigins").Get<string[]>()
                    ?? ["http://localhost:5000", "https://localhost:5001", "http://localhost:3000", "https://localhost:3001"];
                var devAllowedOrigins = defaultDevOrigins.Concat(additionalAllowedOrigins ?? []).ToArray();
                
                policy.WithOrigins(devAllowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
            else
            {
                var configuredOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
                var prodAllowedOrigins = configuredOrigins.Concat(additionalAllowedOrigins ?? []).ToArray();
                
                policy.WithOrigins(prodAllowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
        });
    }
}
