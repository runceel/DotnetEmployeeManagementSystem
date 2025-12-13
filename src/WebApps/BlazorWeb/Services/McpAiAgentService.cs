using System.Diagnostics;
using System.Text.Json;
using BlazorWeb.Models;
using Microsoft.Extensions.AI;
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
    private readonly McpConnectionHelper _connectionHelper;
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
        McpConnectionHelper connectionHelper)
    {
        _chatClient = chatClient;
        _logger = logger;
        _connectionHelper = connectionHelper;
    }

    /// <summary>
    /// Whether the service is initialized and ready to use
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// List of available MCP servers
    /// </summary>
    public IReadOnlyList<McpServerConfiguration> AvailableServers => _connectionHelper.AvailableServers;

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
        foreach (var server in _connectionHelper.AvailableServers)
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

        var result = await _connectionHelper.ConnectToServerAsync(server, cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException($"Failed to connect to MCP server: {server.Name}");
        }

        _mcpClients[server.Name] = result.Client;
        _serverTools[server.Name] = result.Tools;

        _logger.LogInformation("Connected to {ServerName}. Tools: {ToolCount}", server.Name, result.Tools.Count);
        activity?.SetTag("tools.count", result.Tools.Count);
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

        // Create the base function that will be called by the AI
        var baseFunction = AIFunctionFactory.Create(
            async (IDictionary<string, object?>? arguments) =>
            {
                return await CallMcpToolAsync(serverName, tool.Name, arguments);
            },
            new AIFunctionFactoryOptions
            {
                Name = functionName,
                Description = $"[{serverName}] {tool.Description ?? tool.Name}"
            });

        // If the tool has an InputSchema, wrap the function to expose it to the AI
        // McpClientTool has a JsonSchema property, but we should use the ProtocolTool.InputSchema
        var inputSchema = tool.ProtocolTool?.InputSchema;
        if (inputSchema != null && inputSchema.Value.ValueKind != JsonValueKind.Undefined)
        {
            _logger.LogDebug("Creating AI function with InputSchema for tool: {ToolName}", tool.Name);
            return new McpToolAIFunction(baseFunction, inputSchema.Value, _logger);
        }

        return baseFunction;
    }

    /// <summary>
    /// Validate tool arguments against the InputSchema from MCP tool definition
    /// </summary>
    private string? ValidateToolArguments(JsonElement inputSchema, Dictionary<string, object?> arguments)
    {
        try
        {
            // Parse the schema to extract required fields
            if (inputSchema.TryGetProperty("required", out var requiredElement) && 
                requiredElement.ValueKind == JsonValueKind.Array)
            {
                var missingArgs = new List<string>();
                
                foreach (var requiredField in requiredElement.EnumerateArray())
                {
                    var fieldName = requiredField.GetString();
                    if (fieldName != null && !arguments.ContainsKey(fieldName))
                    {
                        missingArgs.Add(fieldName);
                    }
                }
                
                if (missingArgs.Count > 0)
                {
                    return $"Missing required arguments: {string.Join(", ", missingArgs)}";
                }
            }
            
            return null; // Validation passed
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate tool arguments against schema");
            return null; // Don't block execution if validation fails
        }
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

            // Validate required parameters if we have the tool definition
            if (_serverTools.TryGetValue(serverName, out var tools))
            {
                var tool = tools.FirstOrDefault(t => t.Name == toolName);
                var inputSchema = tool?.ProtocolTool?.InputSchema;
                if (inputSchema != null && inputSchema.Value.ValueKind != JsonValueKind.Undefined)
                {
                    var validationError = ValidateToolArguments(inputSchema.Value, args);
                    if (validationError != null)
                    {
                        _logger.LogWarning("Tool {ToolName} argument validation failed: {Error}", toolName, validationError);
                        return JsonSerializer.Serialize(new { error = validationError });
                    }
                }
            }

            _logger.LogInformation("Calling tool {ToolName} on {ServerName} with args: {Args}",
                toolName, serverName, JsonSerializer.Serialize(args));

            using var cts = new CancellationTokenSource(_connectionHelper.ToolExecutionTimeoutMs);
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
            var server = _connectionHelper.AvailableServers.FirstOrDefault(s => s.Name == serverName);
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

/// <summary>
/// AIFunction wrapper that exposes MCP tool's InputSchema to the AI.
/// This allows the AI to understand the required parameters and their types.
/// </summary>
internal sealed class McpToolAIFunction : AIFunction
{
    private readonly AIFunction _innerFunction;
    private readonly JsonElement _mcpInputSchema;
    private readonly ILogger? _logger;

    public McpToolAIFunction(AIFunction innerFunction, JsonElement mcpInputSchema, ILogger? logger = null)
    {
        _innerFunction = innerFunction;
        _mcpInputSchema = mcpInputSchema;
        _logger = logger;
    }

    public override string Name => _innerFunction.Name;
    public override string? Description => _innerFunction.Description;
    
    /// <summary>
    /// Expose the MCP InputSchema directly instead of wrapping parameters
    /// </summary>
    public override JsonElement JsonSchema => _mcpInputSchema;
    
    public override IReadOnlyDictionary<string, object?> AdditionalProperties => _innerFunction.AdditionalProperties;

    protected override async ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments,
        CancellationToken cancellationToken)
    {
        // AI will call with flat arguments based on InputSchema: { employeeId: "123", firstName: "John", ... }
        // The inner function expects: { arguments: { employeeId: "123", firstName: "John", ... } }
        // So we need to wrap the arguments
        
        var wrappedArgs = new AIFunctionArguments();
        var argumentsDict = new Dictionary<string, object?>();
        
        foreach (var kvp in arguments)
        {
            argumentsDict[kvp.Key] = kvp.Value;
        }
        
        _logger?.LogDebug("Wrapping {ArgCount} arguments for tool {ToolName}", 
            argumentsDict.Count, Name);
        
        wrappedArgs["arguments"] = argumentsDict;
        
        return await _innerFunction.InvokeAsync(wrappedArgs, cancellationToken);
    }
}
