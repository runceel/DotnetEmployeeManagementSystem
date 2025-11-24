namespace BlazorWeb.Models;

/// <summary>
/// MCPサーバーの設定情報
/// </summary>
public sealed class McpServerConfiguration
{
    /// <summary>
    /// サーバーの識別名（例: EmployeeService）
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// サーバーの表示名（例: 従業員サービス）
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// MCPエンドポイントURL
    /// </summary>
    public required string EndpointUrl { get; init; }

    /// <summary>
    /// サーバーの説明
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// サーバーのアイコン（MudBlazor Icons）
    /// </summary>
    public string? Icon { get; init; }
}

/// <summary>
/// MCP設定のオプション
/// </summary>
public sealed class McpOptions
{
    public const string SectionName = "Mcp";

    /// <summary>
    /// 利用可能なMCPサーバーのリスト
    /// </summary>
    public List<McpServerConfiguration> Servers { get; init; } = [];

    /// <summary>
    /// 接続タイムアウト（ミリ秒）
    /// </summary>
    public int ConnectionTimeoutMs { get; init; } = 5000;

    /// <summary>
    /// ツール実行タイムアウト（ミリ秒）
    /// </summary>
    public int ToolExecutionTimeoutMs { get; init; } = 30000;
}
