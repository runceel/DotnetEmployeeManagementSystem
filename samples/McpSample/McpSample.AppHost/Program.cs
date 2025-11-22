var builder = DistributedApplication.CreateBuilder(args);

// Add MCP Server using proper AddProject<T> method
var mcpServer = builder.AddProject<Projects.McpSample_Server>("mcpserver")
    .WithHttpsEndpoint(port: 7001, name: "https");

// Add Blazor Chat Web App (MCP Client)
var chatWeb = builder.AddProject<Projects.McpSample_ChatWeb>("chatweb")
    .WithReference(mcpServer);

builder.Build().Run();
