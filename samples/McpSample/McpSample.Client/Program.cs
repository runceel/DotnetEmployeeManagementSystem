using ModelContextProtocol.Client;
using Microsoft.Extensions.Logging;
using System.Text.Json;

Console.WriteLine("=== MCP Sample Client ===\n");

// Configure logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<Program>();

// MCP Server URL (adjust based on your server configuration)
var serverUrl = args.Length > 0 ? args[0] : "https://localhost:7001/api/mcp";

logger.LogInformation("Connecting to MCP Server: {ServerUrl}", serverUrl);

try
{
    // Create HTTP transport with correct Endpoint property
    var transport = new HttpClientTransport(new HttpClientTransportOptions
    {
        Endpoint = new Uri(serverUrl),
        TransportMode = HttpTransportMode.StreamableHttp
    });

    // Create MCP client
    var client = await McpClient.CreateAsync(transport);
    logger.LogInformation("Successfully connected to MCP server");

    // List available tools
    logger.LogInformation("\nListing available tools:");
    var tools = await client.ListToolsAsync();
    
    foreach (var tool in tools)
    {
        Console.WriteLine($"  - {tool.Name}: {tool.Description}");
    }

    // Test calculator tools
    Console.WriteLine("\n=== Testing Calculator Tools ===\n");

    // Test Add
    logger.LogInformation("Calling Add(5, 3)...");
    var addResult = await client.CallToolAsync("Add", new Dictionary<string, object?>
    {
        ["a"] = 5,
        ["b"] = 3
    });
    Console.WriteLine($"Result: 5 + 3 = {JsonSerializer.Serialize(addResult)}");

    // Test Multiply
    logger.LogInformation("\nCalling Multiply(4, 7)...");
    var multiplyResult = await client.CallToolAsync("Multiply", new Dictionary<string, object?>
    {
        ["a"] = 4,
        ["b"] = 7
    });
    Console.WriteLine($"Result: 4 * 7 = {JsonSerializer.Serialize(multiplyResult)}");

    // Test Divide
    logger.LogInformation("\nCalling Divide(10.0, 2.0)...");
    var divideResult = await client.CallToolAsync("Divide", new Dictionary<string, object?>
    {
        ["a"] = 10.0,
        ["b"] = 2.0
    });
    Console.WriteLine($"Result: 10.0 / 2.0 = {JsonSerializer.Serialize(divideResult)}");

    // Test Power
    logger.LogInformation("\nCalling Power(2, 8)...");
    var powerResult = await client.CallToolAsync("Power", new Dictionary<string, object?>
    {
        ["baseNumber"] = 2.0,
        ["exponent"] = 8.0
    });
    Console.WriteLine($"Result: 2^8 = {JsonSerializer.Serialize(powerResult)}");

    // Test SquareRoot
    logger.LogInformation("\nCalling SquareRoot(16)...");
    var sqrtResult = await client.CallToolAsync("SquareRoot", new Dictionary<string, object?>
    {
        ["number"] = 16.0
    });
    Console.WriteLine($"Result: √16 = {JsonSerializer.Serialize(sqrtResult)}");

    // Test error handling - divide by zero
    Console.WriteLine("\n=== Testing Error Handling ===\n");
    try
    {
        logger.LogInformation("Calling Divide(10, 0) - this should fail...");
        await client.CallToolAsync("Divide", new Dictionary<string, object?>
        {
            ["a"] = 10.0,
            ["b"] = 0.0
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Expected error caught: {ex.Message}");
    }

    Console.WriteLine("\n=== All tests completed successfully ===");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error communicating with MCP server");
    Console.WriteLine($"\nError: {ex.Message}");
    Console.WriteLine("\nMake sure the MCP server is running. You can start it with:");
    Console.WriteLine("  dotnet run --project McpSample.Server");
    Console.WriteLine($"\nOr via Aspire AppHost:");
    Console.WriteLine("  dotnet run --project McpSample.AppHost");
    return 1;
}

return 0;
