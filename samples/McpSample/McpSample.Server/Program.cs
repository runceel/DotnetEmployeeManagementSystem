var builder = WebApplication.CreateBuilder(args);

// Add MCP Server
builder.Services.AddMcpServer()
    .WithHttpTransport()           // Enable HTTP/SSE transport
    .WithToolsFromAssembly();      // Auto-discover [McpServerToolType] classes

// Add CORS for MCP client access
builder.Services.AddCors(options =>
{
    options.AddPolicy("McpPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("McpPolicy");

// Map MCP endpoint
app.MapMcp("/api/mcp");

// Sample health check endpoint
app.MapGet("/", () => new
{
    Message = "MCP Sample Server is running",
    McpEndpoint = "/api/mcp",
    AvailableTools = new[] { "Add", "Subtract", "Multiply", "Divide", "Power", "SquareRoot" }
})
.WithName("GetServerInfo");

app.Run();
