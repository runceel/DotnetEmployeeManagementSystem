using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace McpSample.ChatWeb.Services;

/// <summary>
/// Service for managing MCP client connections and tool invocations
/// </summary>
public class McpChatService
{
    private readonly ILogger<McpChatService> _logger;
    private readonly IConfiguration _configuration;
    private McpClient? _mcpClient;
    private bool _isConnected;

    public McpChatService(ILogger<McpChatService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public bool IsConnected => _isConnected;

    /// <summary>
    /// Connect to the MCP server
    /// </summary>
    public async Task<bool> ConnectAsync(string serverUrl)
    {
        try
        {
            _logger.LogInformation("Connecting to MCP server: {ServerUrl}", serverUrl);

            var transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(serverUrl),
                TransportMode = HttpTransportMode.StreamableHttp
            });

            _mcpClient = await McpClient.CreateAsync(transport);
            _isConnected = true;

            _logger.LogInformation("Successfully connected to MCP server");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MCP server");
            _isConnected = false;
            return false;
        }
    }

    /// <summary>
    /// Get available tools from the MCP server
    /// </summary>
    public async Task<IList<McpClientTool>> GetToolsAsync()
    {
        if (_mcpClient == null || !_isConnected)
        {
            throw new InvalidOperationException("Not connected to MCP server");
        }

        return await _mcpClient.ListToolsAsync();
    }

    /// <summary>
    /// Call a tool on the MCP server
    /// </summary>
    public async Task<CallToolResult> CallToolAsync(string toolName, Dictionary<string, object?> arguments)
    {
        if (_mcpClient == null || !_isConnected)
        {
            throw new InvalidOperationException("Not connected to MCP server");
        }

        _logger.LogInformation("Calling tool: {ToolName}", toolName);
        return await _mcpClient.CallToolAsync(toolName, arguments);
    }

    /// <summary>
    /// Disconnect from the MCP server
    /// </summary>
    public void Disconnect()
    {
        _mcpClient = null;
        _isConnected = false;
        _logger.LogInformation("Disconnected from MCP server");
    }
}
