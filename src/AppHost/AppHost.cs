var builder = DistributedApplication.CreateBuilder(args);

// Add Redis for messaging
var redis = builder.AddRedis("redis");

// Add SQLite databases
var employeeDb = builder.AddSqlite("employeedb");
var authDb = builder.AddSqlite("authdb");
var notificationDb = builder.AddSqlite("notificationdb");

// Add services with database references
var employeeServiceApi = builder.AddProject<Projects.EmployeeService_API>("employeeservice-api")
    .WithReference(employeeDb)
    .WithReference(redis);

var authServiceApi = builder.AddProject<Projects.AuthService_API>("authservice-api")
    .WithReference(authDb);

var notificationServiceApi = builder.AddProject<Projects.NotificationService_API>("notificationservice-api")
    .WithReference(notificationDb)
    .WithReference(redis);

// Add Blazor web app with service references
builder.AddProject<Projects.BlazorWeb>("blazorweb")
    .WithExternalHttpEndpoints()
    .WithReference(employeeServiceApi)
    .WithReference(authServiceApi)
    .WithReference(notificationServiceApi);

builder.Build().Run();
