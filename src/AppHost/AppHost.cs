var builder = DistributedApplication.CreateBuilder(args);

// Add services
var employeeServiceApi = builder.AddProject<Projects.EmployeeService_API>("employeeservice-api");
var authServiceApi = builder.AddProject<Projects.AuthService_API>("authservice-api");

// Add Blazor web app with service references
builder.AddProject<Projects.BlazorWeb>("blazorweb")
    .WithExternalHttpEndpoints()
    .WithReference(employeeServiceApi)
    .WithReference(authServiceApi);

builder.Build().Run();
