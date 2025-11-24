using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using BlazorWeb.Models;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace BlazorWeb.Services;

/// <summary>
/// 複数のMCPサーバーを管理し、ツール呼び出しを提供するサービス
/// </summary>
public sealed class McpChatService : IAsyncDisposable
{
    private static readonly ActivitySource ActivitySource = new("BlazorWeb.McpChat");
    
    private readonly ILogger<McpChatService> _logger;
    private readonly McpOptions _mcpOptions;
    private readonly ConcurrentDictionary<string, McpClient> _connectedClients = new();
    private readonly ConcurrentDictionary<string, IList<McpClientTool>> _serverTools = new();

    public McpChatService(
        ILogger<McpChatService> logger,
        IOptions<McpOptions> mcpOptions)
    {
        _logger = logger;
        _mcpOptions = mcpOptions.Value;
    }

    /// <summary>
    /// 利用可能なサーバーリストを取得
    /// </summary>
    public IReadOnlyList<McpServerConfiguration> AvailableServers => _mcpOptions.Servers;

    /// <summary>
    /// 接続済みサーバーの名前リストを取得
    /// </summary>
    public IReadOnlyList<string> ConnectedServers => _connectedClients.Keys.ToList();

    /// <summary>
    /// 指定されたサーバーに接続
    /// </summary>
    public async Task<bool> ConnectToServerAsync(string serverName, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("ConnectToServer");
        activity?.SetTag("server.name", serverName);

        var serverConfig = _mcpOptions.Servers.FirstOrDefault(s => s.Name == serverName);
        if (serverConfig == null)
        {
            _logger.LogWarning("Server configuration not found: {ServerName}", serverName);
            activity?.SetStatus(ActivityStatusCode.Error, "Server configuration not found");
            return false;
        }

        if (_connectedClients.ContainsKey(serverName))
        {
            _logger.LogInformation("Already connected to server: {ServerName}", serverName);
            return true;
        }

        try
        {
            _logger.LogInformation("Connecting to MCP server: {ServerName} at {Url}", serverName, serverConfig.EndpointUrl);

            var transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(serverConfig.EndpointUrl),
                TransportMode = HttpTransportMode.StreamableHttp
            });

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_mcpOptions.ConnectionTimeoutMs);

            var client = await McpClient.CreateAsync(transport, cancellationToken: cts.Token);
            _connectedClients[serverName] = client;

            // ツールリストを取得してキャッシュ
            var tools = await client.ListToolsAsync(cancellationToken: cts.Token);
            _serverTools[serverName] = tools;

            _logger.LogInformation("Successfully connected to {ServerName}. Tools available: {ToolCount}",
                serverName, tools.Count);
            activity?.SetTag("tools.count", tools.Count);

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Connection to {ServerName} was cancelled", serverName);
            activity?.SetStatus(ActivityStatusCode.Error, "Connection cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MCP server: {ServerName}", serverName);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 全てのサーバーに接続
    /// </summary>
    public async Task<Dictionary<string, bool>> ConnectToAllServersAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("ConnectToAllServers");
        
        var results = new Dictionary<string, bool>();
        var tasks = _mcpOptions.Servers.Select(async server =>
        {
            var success = await ConnectToServerAsync(server.Name, cancellationToken);
            return (server.Name, success);
        });

        var taskResults = await Task.WhenAll(tasks);
        foreach (var (name, success) in taskResults)
        {
            results[name] = success;
        }

        var successCount = results.Values.Count(x => x);
        _logger.LogInformation("Connected to {SuccessCount}/{TotalCount} servers",
            successCount, _mcpOptions.Servers.Count);
        activity?.SetTag("servers.connected", successCount);
        activity?.SetTag("servers.total", _mcpOptions.Servers.Count);

        return results;
    }

    /// <summary>
    /// サーバーから切断
    /// </summary>
    public void DisconnectFromServer(string serverName)
    {
        using var activity = ActivitySource.StartActivity("DisconnectFromServer");
        activity?.SetTag("server.name", serverName);

        if (_connectedClients.TryRemove(serverName, out _))
        {
            _serverTools.TryRemove(serverName, out _);
            _logger.LogInformation("Disconnected from server: {ServerName}", serverName);
        }
    }

    /// <summary>
    /// 全サーバーから切断
    /// </summary>
    public void DisconnectAll()
    {
        using var activity = ActivitySource.StartActivity("DisconnectAll");
        
        var serverCount = _connectedClients.Count;
        _connectedClients.Clear();
        _serverTools.Clear();
        
        _logger.LogInformation("Disconnected from all servers. Count: {Count}", serverCount);
        activity?.SetTag("servers.disconnected", serverCount);
    }

    /// <summary>
    /// 特定のサーバーの利用可能なツールを取得
    /// </summary>
    public IReadOnlyList<McpClientTool>? GetToolsForServer(string serverName)
    {
        _serverTools.TryGetValue(serverName, out var tools);
        return tools?.ToList();
    }

    /// <summary>
    /// 全サーバーの利用可能なツールを取得（サーバー名付き）
    /// </summary>
    public Dictionary<string, IReadOnlyList<McpClientTool>> GetAllTools()
    {
        return _serverTools.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<McpClientTool>)kvp.Value.ToList()
        );
    }

    /// <summary>
    /// ツールを実行
    /// </summary>
    public async Task<CallToolResult> CallToolAsync(
        string serverName,
        string toolName,
        Dictionary<string, object?> arguments,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CallTool");
        activity?.SetTag("server.name", serverName);
        activity?.SetTag("tool.name", toolName);

        if (!_connectedClients.TryGetValue(serverName, out var client))
        {
            var errorMsg = $"Not connected to server: {serverName}";
            _logger.LogError(errorMsg);
            activity?.SetStatus(ActivityStatusCode.Error, errorMsg);
            throw new InvalidOperationException(errorMsg);
        }

        try
        {
            _logger.LogInformation("Calling tool {ToolName} on server {ServerName} with arguments: {Arguments}",
                toolName, serverName, JsonSerializer.Serialize(arguments));

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_mcpOptions.ToolExecutionTimeoutMs);

            var result = await client.CallToolAsync(toolName, arguments, cancellationToken: cts.Token);

            _logger.LogInformation("Tool {ToolName} executed successfully on {ServerName}",
                toolName, serverName);

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Tool execution cancelled: {ToolName} on {ServerName}", toolName, serverName);
            activity?.SetStatus(ActivityStatusCode.Error, "Tool execution cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute tool {ToolName} on server {ServerName}",
                toolName, serverName);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// ツールをリフレッシュ（再取得）
    /// </summary>
    public async Task RefreshToolsAsync(string serverName, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("RefreshTools");
        activity?.SetTag("server.name", serverName);

        if (!_connectedClients.TryGetValue(serverName, out var client))
        {
            throw new InvalidOperationException($"Not connected to server: {serverName}");
        }

        var tools = await client.ListToolsAsync(cancellationToken: cancellationToken);
        _serverTools[serverName] = tools;

        _logger.LogInformation("Refreshed tools for {ServerName}. Tools count: {Count}",
            serverName, tools.Count);
        activity?.SetTag("tools.count", tools.Count);
    }

    public async ValueTask DisposeAsync()
    {
        _connectedClients.Clear();
        _serverTools.Clear();
        ActivitySource.Dispose();
    }
}
