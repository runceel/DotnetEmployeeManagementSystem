using Aspire.Hosting.GitHub;

var builder = DistributedApplication.CreateBuilder(args);

// Add Redis for messaging
var redis = builder.AddRedis("redis");

// Add GitHub Model (GPT-4.1) for AI chat functionality
var chat = builder.AddGitHubModel("chat", GitHubModel.OpenAI.OpenAIGpt41);

// Add SQLite databases
var employeeDb = builder.AddSqlite("employeedb");
var authDb = builder.AddSqlite("authdb");
var notificationDb = builder.AddSqlite("notificationdb");
var attendanceDb = builder.AddSqlite("attendancedb");

// Application Insights and Log Analytics Workspace (Azure deployment only)
// Only provision these resources when publishing to Azure (not during local development)
var appInsights = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureApplicationInsights("appinsights")
        .WithLogAnalyticsWorkspace(
            builder.AddAzureLogAnalyticsWorkspace("loganalytics"))
    : null;

// Add services with database references
var employeeServiceApi = builder.AddProject<Projects.EmployeeService_API>("employeeservice-api")
    .WithReference(employeeDb)
    .WithReference(redis)
    .WithHttpHealthCheck("/health");

if (appInsights != null)
{
    employeeServiceApi.WithReference(appInsights);
}

var authServiceApi = builder.AddProject<Projects.AuthService_API>("authservice-api")
    .WithReference(authDb)
    .WithHttpHealthCheck("/health");

if (appInsights != null)
{
    authServiceApi.WithReference(appInsights);
}

var notificationServiceApi = builder.AddProject<Projects.NotificationService_API>("notificationservice-api")
    .WithReference(notificationDb)
    .WithReference(redis)
    .WithHttpHealthCheck("/health");

if (appInsights != null)
{
    notificationServiceApi.WithReference(appInsights);
}

var attendanceServiceApi = builder.AddProject<Projects.AttendanceService_API>("attendanceservice-api")
    .WithReference(attendanceDb)
    .WithReference(redis)
    .WithReference(employeeServiceApi)
    .WaitFor(employeeServiceApi)
    .WithHttpHealthCheck("/health");

if (appInsights != null)
{
    attendanceServiceApi.WithReference(appInsights);
}

// Add Blazor web app with service references
var blazorWeb = builder.AddProject<Projects.BlazorWeb>("blazorweb")
    .WithExternalHttpEndpoints()
    .WithReference(employeeServiceApi)
    .WithReference(authServiceApi)
    .WithReference(notificationServiceApi)
    .WithReference(attendanceServiceApi)
    .WithReference(chat);

if (appInsights != null)
{
    blazorWeb.WithReference(appInsights);
}

builder.Build().Run();
