var builder = DistributedApplication.CreateBuilder(args);

// Add SQLite databases
var employeeDb = builder.AddSqlite("employeedb");

var authDb = builder.AddSqlite("authdb");

// Add services with database references
var employeeServiceApi = builder.AddProject<Projects.EmployeeService_API>("employeeservice-api")
    .WithReference(employeeDb);

var authServiceApi = builder.AddProject<Projects.AuthService_API>("authservice-api")
    .WithReference(authDb);

// Add Blazor web app with service references
builder.AddProject<Projects.BlazorWeb>("blazorweb")
    .WithExternalHttpEndpoints()
    .WithReference(employeeServiceApi)
    .WithReference(authServiceApi);

builder.Build().Run();
