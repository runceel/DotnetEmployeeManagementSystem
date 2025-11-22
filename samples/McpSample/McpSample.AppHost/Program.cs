var builder = DistributedApplication.CreateBuilder(args);

// Add MCP Server using proper AddProject<T> method
var mcpServer = builder.AddProject<Projects.McpSample_Server>("mcpserver")
    .WithHttpsEndpoint(port: 7001, name: "https");

builder.Build().Run();
