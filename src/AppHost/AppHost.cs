var builder = DistributedApplication.CreateBuilder(args);

// Add Redis for messaging
var redis = builder.AddRedis("redis");

// Add SQLite databases
var employeeDb = builder.AddSqlite("employeedb");
var authDb = builder.AddSqlite("authdb");
var notificationDb = builder.AddSqlite("notificationdb");
var attendanceDb = builder.AddSqlite("attendancedb");

// Add services with database references
var employeeServiceApi = builder.AddProject<Projects.EmployeeService_API>("employeeservice-api")
    .WithReference(employeeDb)
    .WithReference(redis)
    .WithHttpHealthCheck("/health");

var authServiceApi = builder.AddProject<Projects.AuthService_API>("authservice-api")
    .WithReference(authDb)
    .WithHttpHealthCheck("/health");

var notificationServiceApi = builder.AddProject<Projects.NotificationService_API>("notificationservice-api")
    .WithReference(notificationDb)
    .WithReference(redis)
    .WithHttpHealthCheck("/health");

var attendanceServiceApi = builder.AddProject<Projects.AttendanceService_API>("attendanceservice-api")
    .WithReference(attendanceDb)
    .WithReference(redis)
    .WithHttpHealthCheck("/health");

// Add Blazor web app with service references
builder.AddProject<Projects.BlazorWeb>("blazorweb")
    .WithExternalHttpEndpoints()
    .WithReference(employeeServiceApi)
    .WithReference(authServiceApi)
    .WithReference(notificationServiceApi)
    .WithReference(attendanceServiceApi);

builder.Build().Run();
