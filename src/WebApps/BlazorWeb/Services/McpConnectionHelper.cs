using System.Diagnostics;
using BlazorWeb.Models;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;

namespace BlazorWeb.Services;

/// <summary>
/// MCP サーバーへの接続とツール取得を共通化するヘルパークラス
/// </summary>
public sealed class McpConnectionHelper
{
    private static readonly ActivitySource ActivitySource = new("BlazorWeb.McpConnection");

    private readonly ILogger<McpConnectionHelper> _logger;
    private readonly McpOptions _mcpOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;

    public McpConnectionHelper(
        ILogger<McpConnectionHelper> logger,
        IOptions<McpOptions> mcpOptions,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _mcpOptions = mcpOptions.Value;
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// 利用可能なMCPサーバーのリストを取得
    /// </summary>
    public IReadOnlyList<McpServerConfiguration> AvailableServers => _mcpOptions.Servers;

    /// <summary>
    /// 接続タイムアウト（ミリ秒）
    /// </summary>
    public int ConnectionTimeoutMs => _mcpOptions.ConnectionTimeoutMs;

    /// <summary>
    /// ツール実行タイムアウト（ミリ秒）
    /// </summary>
    public int ToolExecutionTimeoutMs => _mcpOptions.ToolExecutionTimeoutMs;

    /// <summary>
    /// 指定されたサーバーに接続し、McpClientとツールリストを返す
    /// </summary>
    /// <param name="server">接続するサーバーの設定</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>接続結果（McpClientとツールリスト）。接続失敗時はnull</returns>
    public async Task<McpConnectionResult?> ConnectToServerAsync(
        McpServerConfiguration server,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("ConnectToMcpServer");
        activity?.SetTag("server.name", server.Name);

        var httpClientName = $"mcp-{server.Name.ToLowerInvariant()}";
        _logger.LogInformation("Connecting to MCP server: {ServerName} using HttpClient: {HttpClientName}",
            server.Name, httpClientName);

        var httpClient = _httpClientFactory.CreateClient(httpClientName);

        if (httpClient.BaseAddress is null)
        {
            _logger.LogError("HttpClient for {ServerName} does not have a BaseAddress configured", server.Name);
            activity?.SetStatus(ActivityStatusCode.Error, "HttpClient BaseAddress not configured");
            return null;
        }

        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress, "api/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient,
            _loggerFactory,
            ownsHttpClient: false);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_mcpOptions.ConnectionTimeoutMs);

        var client = await McpClient.CreateAsync(transport, cancellationToken: cts.Token);

        // ツールリストを取得
        var tools = await client.ListToolsAsync(cancellationToken: cts.Token);

        _logger.LogInformation("Connected to {ServerName}. Tools available: {ToolCount}",
            server.Name, tools.Count);
        activity?.SetTag("tools.count", tools.Count);

        return new McpConnectionResult(client, tools);
    }
}

/// <summary>
/// MCP接続の結果を格納するレコード
/// </summary>
/// <param name="Client">接続したMcpClient</param>
/// <param name="Tools">サーバーから取得したツールリスト</param>
public sealed record McpConnectionResult(McpClient Client, IList<McpClientTool> Tools);
