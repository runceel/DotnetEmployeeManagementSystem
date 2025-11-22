# MCP Sample - Calculator Service

This sample demonstrates how to create an MCP (Model Context Protocol) server and client using .NET Aspire orchestration.

## Project Structure

```
McpSample/
├── McpSample.AppHost/      # Aspire AppHost for orchestration
├── McpSample.Server/       # MCP Server (ASP.NET Core Web API)
│   └── Mcp/
│       └── CalculatorTools.cs  # Sample MCP tools
├── McpSample.Client/       # MCP Client (Console app)
└── McpSample.slnx          # Solution file (XML format)
```

## Prerequisites

- .NET 10 SDK
- ModelContextProtocol NuGet packages (configured via Central Package Management in root Directory.Packages.props)

## Running the Sample

### Option 1: Using Aspire AppHost (Recommended)

Start the AppHost which will launch the MCP server and provide the Aspire dashboard:

```bash
dotnet run --project McpSample.AppHost
```

The Aspire dashboard will open automatically, showing:
- MCP Server running on https://localhost:7001
- Logs and traces from the server

Then in a separate terminal, run the client:

```bash
dotnet run --project McpSample.Client
```

### Option 2: Running Server and Client Separately

Start the MCP Server:

```bash
dotnet run --project McpSample.Server
```

In a separate terminal, start the Client:

```bash
dotnet run --project McpSample.Client https://localhost:7001/api/mcp
```

## What the Sample Demonstrates

### MCP Server (`McpSample.Server`)

The server exposes calculator operations as MCP tools:

- **Add**: Adds two numbers
- **Subtract**: Subtracts two numbers
- **Multiply**: Multiplies two numbers
- **Divide**: Divides two numbers (with error handling for division by zero)
- **Power**: Calculates power of a number
- **SquareRoot**: Calculates square root (with error handling for negative numbers)

**Key Features:**
- Attribute-based tool registration using `[McpServerToolType]` and `[McpServerTool]`
- HTTP/SSE transport for remote communication
- Automatic tool discovery via `WithToolsFromAssembly()`
- Swagger/OpenAPI integration for API documentation
- CORS configuration for client access

**Endpoints:**
- `/api/mcp` - MCP protocol endpoint
- `/` - Server info and available tools
- `/swagger` - API documentation

### MCP Client (`McpSample.Client`)

The client demonstrates:

- Connecting to an MCP server via HTTP transport
- Listing available tools
- Calling tools with parameters
- Error handling
- Logging tool calls

**Sample Output:**

```
=== MCP Sample Client ===

Connecting to MCP Server: https://localhost:7001/api/mcp
Successfully connected to MCP server

Listing available tools:
  - Add: Adds two numbers together and returns the result
  - Subtract: Subtracts the second number from the first number
  - Multiply: Multiplies two numbers together
  - Divide: Divides the first number by the second number
  - Power: Calculates a number raised to a power
  - SquareRoot: Calculates the square root of a number

=== Testing Calculator Tools ===

Calling Add(5, 3)...
Result: 5 + 3 = 8

Calling Multiply(4, 7)...
Result: 4 * 7 = 28

Calling Divide(10.0, 2.0)...
Result: 10.0 / 2.0 = 5

Calling Power(2, 8)...
Result: 2^8 = 256

Calling SquareRoot(16)...
Result: √16 = 4

=== Testing Error Handling ===

Calling Divide(10, 0) - this should fail...
Expected error caught: Cannot divide by zero

=== All tests completed successfully ===
```

## Code Highlights

### Server-Side Tool Definition

```csharp
[McpServerToolType]
public class CalculatorTools
{
    [McpServerTool(Description = "Adds two numbers together")]
    public int Add(
        [Description("The first number")] int a,
        [Description("The second number")] int b)
    {
        return a + b;
    }
}
```

### Server Configuration

```csharp
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

app.MapMcp("/api/mcp");
```

### Client Connection and Tool Call

```csharp
var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    BaseUrl = new Uri("https://localhost:7001/api/mcp")
});

var client = await McpClient.CreateAsync(transport);
var tools = await client.ListToolsAsync();

var result = await client.CallToolAsync("Add", new Dictionary<string, object?>
{
    ["a"] = 5,
    ["b"] = 3
});
```

## Extending the Sample

To add more tools:

1. Create a new class in `McpSample.Server/Mcp/` marked with `[McpServerToolType]`
2. Add methods marked with `[McpServerTool]`
3. Use `[Description]` attributes for tool and parameter documentation
4. The tools will be automatically discovered and available to clients

Example:

```csharp
[McpServerToolType]
public class StringTools
{
    [McpServerTool(Description = "Converts text to uppercase")]
    public string ToUpper([Description("Text to convert")] string text)
    {
        return text.ToUpperInvariant();
    }
}
```

## Related Documentation

See the main documentation for comprehensive MCP integration guidance:

- [MCP Integration Design](../../docs/mcp-integration-design.md)
- [MCP Implementation Guide](../../docs/mcp-implementation-guide.md)

## Troubleshooting

### "Cannot connect to MCP server"

- Ensure the server is running
- Check the URL and port (default: https://localhost:7001/api/mcp)
- Verify CORS is enabled on the server

### "Tools not found"

- Make sure the tools class is marked with `[McpServerToolType]`
- Verify `WithToolsFromAssembly()` is called in server configuration
- Check the server logs for any errors during tool discovery

### Build errors related to packages

- The sample uses Central Package Management from the root `Directory.Packages.props`
- Ensure all package versions are defined there
- Run `dotnet restore` at the solution level
