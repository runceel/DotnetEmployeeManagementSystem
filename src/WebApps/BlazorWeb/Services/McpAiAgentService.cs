using System.Diagnostics;
using System.Text.Json;
using BlazorWeb.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace BlazorWeb.Services;

/// <summary>
/// AI Agent service that uses Microsoft.Extensions.AI IChatClient with automatic MCP tool calling.
/// The AI analyzes user requests and automatically calls appropriate MCP tools to complete tasks.
/// </summary>
public sealed class McpAiAgentService : IAsyncDisposable
{
    private static readonly ActivitySource ActivitySource = new("BlazorWeb.McpAiAgent");

    private readonly IChatClient _chatClient;
    private readonly ILogger<McpAiAgentService> _logger;
    private readonly McpOptions _mcpOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<string, McpClient> _mcpClients = new();
    private readonly Dictionary<string, IList<McpClientTool>> _serverTools = new();
    private readonly List<AIFunction> _aiFunctions = new();
    private bool _isInitialized;

    public const string SystemPrompt = """
        You are an intelligent assistant for the Employee Management System.
        You have access to various MCP (Model Context Protocol) tools that allow you to:
        - Manage employees (list, create, update, delete)
        - Manage departments
        - Handle authentication and users
        - Manage notifications
        - Track attendance

        When a user asks you to do something:
        1. Analyze the request and determine which tool(s) to use
        2. Call the appropriate tool(s) with the correct arguments
        3. Provide a clear, helpful response based on the results

        Always respond in a friendly, professional manner.
        If you need more information from the user, ask clarifying questions.
        If a tool call fails, explain what happened and suggest alternatives.

        Important: Do NOT make up data. Only use information from tool results.
        """;

    public McpAiAgentService(
        IChatClient chatClient,
        ILogger<McpAiAgentService> logger,
        IOptions<McpOptions> mcpOptions,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
    {
        _chatClient = chatClient;
        _logger = logger;
        _mcpOptions = mcpOptions.Value;
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Whether the service is initialized and ready to use
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// List of available MCP servers
    /// </summary>
    public IReadOnlyList<McpServerConfiguration> AvailableServers => _mcpOptions.Servers;

    /// <summary>
    /// Connected MCP server names
    /// </summary>
    public IReadOnlyList<string> ConnectedServers => _mcpClients.Keys.ToList();

    /// <summary>
    /// Number of available AI functions (tools)
    /// </summary>
    public int ToolCount => _aiFunctions.Count;

    /// <summary>
    /// Initialize the AI agent by connecting to all MCP servers and creating AI functions from tools
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("InitializeAiAgent");

        if (_isInitialized)
        {
            _logger.LogInformation("AI Agent already initialized");
            return;
        }

        _logger.LogInformation("Initializing AI Agent with MCP tools...");

        // Connect to all MCP servers
        foreach (var server in _mcpOptions.Servers)
        {
            try
            {
                await ConnectToServerAsync(server, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to MCP server: {ServerName}", server.Name);
            }
        }

        // Create AI functions from all discovered tools
        CreateAiFunctionsFromTools();

        _isInitialized = true;
        _logger.LogInformation("AI Agent initialized with {ToolCount} tools from {ServerCount} servers",
            _aiFunctions.Count, _mcpClients.Count);

        activity?.SetTag("tools.count", _aiFunctions.Count);
        activity?.SetTag("servers.count", _mcpClients.Count);
    }

    private async Task ConnectToServerAsync(McpServerConfiguration server, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("ConnectToMcpServer");
        activity?.SetTag("server.name", server.Name);

        var httpClientName = $"mcp-{server.Name.ToLowerInvariant()}";
        _logger.LogInformation("Connecting to MCP server: {ServerName} using HttpClient: {HttpClientName}", server.Name, httpClientName);

        var httpClient = _httpClientFactory.CreateClient(httpClientName);
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri("/api/mcp", UriKind.Relative),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient,
            _loggerFactory,
            ownsHttpClient: false);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_mcpOptions.ConnectionTimeoutMs);

        var client = await McpClient.CreateAsync(transport, cancellationToken: cts.Token);
        _mcpClients[server.Name] = client;

        // Get tools from server
        var tools = await client.ListToolsAsync(cancellationToken: cts.Token);
        _serverTools[server.Name] = tools;

        _logger.LogInformation("Connected to {ServerName}. Tools: {ToolCount}", server.Name, tools.Count);
        activity?.SetTag("tools.count", tools.Count);
    }

    private void CreateAiFunctionsFromTools()
    {
        _aiFunctions.Clear();

        foreach (var (serverName, tools) in _serverTools)
        {
            foreach (var tool in tools)
            {
                var aiFunction = CreateAiFunctionForTool(serverName, tool);
                _aiFunctions.Add(aiFunction);
            }
        }
    }

    private AIFunction CreateAiFunctionForTool(string serverName, McpClientTool tool)
    {
        // Create a unique name combining server and tool name
        var functionName = $"{serverName}_{tool.Name}";

        // Create the function that will be called by the AI
        var function = AIFunctionFactory.Create(
            async (IDictionary<string, object?>? arguments) =>
            {
                return await CallMcpToolAsync(serverName, tool.Name, arguments);
            },
            new AIFunctionFactoryOptions
            {
                Name = functionName,
                Description = $"[{serverName}] {tool.Description ?? tool.Name}"
            });

        return function;
    }

    private async Task<string> CallMcpToolAsync(
        string serverName,
        string toolName,
        IDictionary<string, object?>? arguments)
    {
        using var activity = ActivitySource.StartActivity("CallMcpTool");
        activity?.SetTag("server.name", serverName);
        activity?.SetTag("tool.name", toolName);

        if (!_mcpClients.TryGetValue(serverName, out var client))
        {
            var errorMsg = $"Not connected to server: {serverName}";
            _logger.LogError(errorMsg);
            activity?.SetStatus(ActivityStatusCode.Error, errorMsg);
            return JsonSerializer.Serialize(new { error = errorMsg });
        }

        try
        {
            var args = arguments?.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value) ?? new Dictionary<string, object?>();

            _logger.LogInformation("Calling tool {ToolName} on {ServerName} with args: {Args}",
                toolName, serverName, JsonSerializer.Serialize(args));

            using var cts = new CancellationTokenSource(_mcpOptions.ToolExecutionTimeoutMs);
            var result = await client.CallToolAsync(toolName, args, cancellationToken: cts.Token);

            var responseText = FormatToolResult(result);
            _logger.LogInformation("Tool {ToolName} completed successfully", toolName);

            return responseText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call tool {ToolName} on {ServerName}", toolName, serverName);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private string FormatToolResult(CallToolResult result)
    {
        if (result.Content.Count == 0)
            return "Operation completed successfully (no response data)";

        var sb = new System.Text.StringBuilder();
        foreach (var content in result.Content)
        {
            if (content is TextContentBlock textBlock)
            {
                sb.AppendLine(textBlock.Text);
            }
            else
            {
                sb.AppendLine(content.ToString());
            }
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Send a message to the AI agent and get a response.
    /// The AI will automatically call MCP tools as needed to complete the request.
    /// </summary>
    public async Task<AgentResponse> ChatAsync(
        string userMessage,
        List<ChatMessage>? conversationHistory = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("AgentChat");
        activity?.SetTag("message.length", userMessage.Length);

        if (!_isInitialized)
        {
            await InitializeAsync(cancellationToken);
        }

        var messages = conversationHistory ?? new List<ChatMessage>();

        // Add system prompt if this is a new conversation
        if (messages.Count == 0)
        {
            messages.Add(new ChatMessage(ChatRole.System, SystemPrompt));
        }

        // Add user message
        messages.Add(new ChatMessage(ChatRole.User, userMessage));

        _logger.LogInformation("Processing user message: {Message}", userMessage);

        try
        {
            // Create chat options with tools
            var options = new ChatOptions
            {
                Tools = _aiFunctions.Cast<AITool>().ToList()
            };

            // Create a chat client with function invocation enabled
            var clientWithFunctions = new ChatClientBuilder(_chatClient)
                .UseFunctionInvocation()
                .Build();

            // Get response from AI (it may make tool calls automatically)
            var response = await clientWithFunctions.GetResponseAsync(messages, options, cancellationToken);

            // Add response messages to history
            messages.AddRange(response.Messages);

            // Extract the final text response
            var responseText = response.Text ?? "I processed your request but have no additional information to provide.";

            // Track tool calls
            var toolCalls = ExtractToolCalls(response);

            _logger.LogInformation("AI response generated. Tool calls: {ToolCallCount}", toolCalls.Count);
            activity?.SetTag("response.length", responseText.Length);
            activity?.SetTag("tool.calls", toolCalls.Count);

            return new AgentResponse
            {
                Text = responseText,
                ToolCalls = toolCalls,
                UpdatedHistory = messages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process chat message");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            return new AgentResponse
            {
                Text = $"I encountered an error while processing your request: {ex.Message}",
                ToolCalls = new List<ToolCallInfo>(),
                UpdatedHistory = messages,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Stream a response from the AI agent
    /// </summary>
    public async IAsyncEnumerable<AgentStreamUpdate> ChatStreamAsync(
        string userMessage,
        List<ChatMessage>? conversationHistory = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("AgentChatStream");
        activity?.SetTag("message.length", userMessage.Length);

        if (!_isInitialized)
        {
            await InitializeAsync(cancellationToken);
        }

        var messages = conversationHistory ?? new List<ChatMessage>();

        // Add system prompt if this is a new conversation
        if (messages.Count == 0)
        {
            messages.Add(new ChatMessage(ChatRole.System, SystemPrompt));
        }

        // Add user message
        messages.Add(new ChatMessage(ChatRole.User, userMessage));

        var options = new ChatOptions
        {
            Tools = _aiFunctions.Cast<AITool>().ToList()
        };

        // Create a chat client with function invocation enabled
        var clientWithFunctions = new ChatClientBuilder(_chatClient)
            .UseFunctionInvocation()
            .Build();

        var updates = new List<ChatResponseUpdate>();

        await foreach (var update in clientWithFunctions.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            updates.Add(update);

            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return new AgentStreamUpdate
                {
                    Text = update.Text,
                    IsComplete = false
                };
            }
        }

        // Compose final response and update history
        var response = updates.ToChatResponse();
        messages.AddRange(response.Messages);

        yield return new AgentStreamUpdate
        {
            Text = "",
            IsComplete = true,
            ToolCalls = ExtractToolCalls(response),
            UpdatedHistory = messages
        };
    }

    private List<ToolCallInfo> ExtractToolCalls(ChatResponse response)
    {
        var toolCalls = new List<ToolCallInfo>();

        foreach (var message in response.Messages)
        {
            foreach (var content in message.Contents)
            {
                if (content is FunctionCallContent functionCall)
                {
                    toolCalls.Add(new ToolCallInfo
                    {
                        FunctionName = functionCall.Name,
                        Arguments = functionCall.Arguments?.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.ToString() ?? "") ?? new Dictionary<string, string>()
                    });
                }
            }
        }

        return toolCalls;
    }

    /// <summary>
    /// Get information about available tools
    /// </summary>
    public IReadOnlyList<ToolInfo> GetAvailableTools()
    {
        var tools = new List<ToolInfo>();

        foreach (var (serverName, serverTools) in _serverTools)
        {
            var server = _mcpOptions.Servers.FirstOrDefault(s => s.Name == serverName);
            foreach (var tool in serverTools)
            {
                tools.Add(new ToolInfo
                {
                    ServerName = serverName,
                    ServerDisplayName = server?.DisplayName ?? serverName,
                    Name = tool.Name,
                    Description = tool.Description ?? ""
                });
            }
        }

        return tools;
    }

    public async ValueTask DisposeAsync()
    {
        // Dispose MCP clients that implement IAsyncDisposable
        foreach (var client in _mcpClients.Values)
        {
            if (client is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (client is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _mcpClients.Clear();
        _serverTools.Clear();
        _aiFunctions.Clear();
        _isInitialized = false;
        ActivitySource.Dispose();
    }
}

/// <summary>
/// Response from the AI agent
/// </summary>
public sealed class AgentResponse
{
    public required string Text { get; init; }
    public required List<ToolCallInfo> ToolCalls { get; init; }
    public required List<ChatMessage> UpdatedHistory { get; init; }
    public string? Error { get; init; }
}

/// <summary>
/// Stream update from the AI agent
/// </summary>
public sealed class AgentStreamUpdate
{
    public required string Text { get; init; }
    public required bool IsComplete { get; init; }
    public List<ToolCallInfo>? ToolCalls { get; init; }
    public List<ChatMessage>? UpdatedHistory { get; init; }
}

/// <summary>
/// Information about a tool call made by the AI
/// </summary>
public sealed class ToolCallInfo
{
    public required string FunctionName { get; init; }
    public required Dictionary<string, string> Arguments { get; init; }
}

/// <summary>
/// Information about an available tool
/// </summary>
public sealed class ToolInfo
{
    public required string ServerName { get; init; }
    public required string ServerDisplayName { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
}
