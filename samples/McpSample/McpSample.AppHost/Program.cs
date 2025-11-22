using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add MCP Server as a project resource
var mcpServer = builder.AddProject("mcpserver", "../McpSample.Server/McpSample.Server.csproj")
    .WithHttpsEndpoint(port: 7001, name: "https");

builder.Build().Run();
