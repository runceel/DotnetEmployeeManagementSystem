using System.Diagnostics;
using Microsoft.Extensions.AI;

namespace BlazorWeb.Services;

/// <summary>
/// Service for interacting with AI chat functionality using Microsoft.Extensions.AI.IChatClient.
/// Aspire's AddOllamaApiClient with AddChatClient chains the IChatClient registration,
/// so this service uses the standard IChatClient interface for Aspire integration.
/// </summary>
public sealed class AiChatService
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<AiChatService> _logger;
    private static readonly ActivitySource ActivitySource = new("BlazorWeb.AiChat");

    /// <summary>
    /// Default model to use for chat interactions.
    /// Note: The actual model is typically configured via Aspire orchestration.
    /// </summary>
    public const string DefaultModel = "phi4-mini";

    public AiChatService(IChatClient chatClient, ILogger<AiChatService> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    /// <summary>
    /// Check if the AI chat client is available by attempting a simple request.
    /// </summary>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CheckAiAvailability");

        try
        {
            // Try a simple request to verify the client is operational
            var response = await _chatClient.GetResponseAsync("ping", cancellationToken: cancellationToken);
            var isAvailable = response != null;

            activity?.SetTag("available", isAvailable);
            _logger.LogInformation("AI chat client available: {IsAvailable}", isAvailable);

            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI chat client is not available");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Generate a response from the AI model.
    /// </summary>
    public async Task<string> GenerateAsync(string prompt, string? model = null, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GenerateAiResponse");

        activity?.SetTag("prompt.length", prompt.Length);
        if (model != null)
        {
            activity?.SetTag("model", model);
        }

        try
        {
            _logger.LogInformation("Generating AI response for prompt of length {Length}", prompt.Length);

            var options = model != null ? new ChatOptions { ModelId = model } : null;
            var response = await _chatClient.GetResponseAsync(prompt, options, cancellationToken);
            var result = response.Text ?? string.Empty;

            activity?.SetTag("response.length", result.Length);
            _logger.LogInformation("AI response generated, length: {Length}", result.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI response");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Stream a response from the AI model.
    /// </summary>
    public async IAsyncEnumerable<string> GenerateStreamAsync(
        string prompt,
        string? model = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GenerateAiResponseStream");

        activity?.SetTag("prompt.length", prompt.Length);
        if (model != null)
        {
            activity?.SetTag("model", model);
        }

        _logger.LogInformation("Starting streaming AI response");

        var options = model != null ? new ChatOptions { ModelId = model } : null;

        await foreach (var update in _chatClient.GetStreamingResponseAsync(prompt, options, cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return update.Text;
            }
        }
    }

    /// <summary>
    /// Generate JSON for MCP tool arguments using AI.
    /// </summary>
    public async Task<string> GenerateMcpToolArgumentsAsync(
        string toolName,
        string toolDescription,
        string userRequest,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GenerateMcpToolArguments");
        activity?.SetTag("tool.name", toolName);

        var prompt = $$"""
            You are an assistant that generates JSON arguments for MCP (Model Context Protocol) tools.

            Tool Name: {{toolName}}
            Tool Description: {{toolDescription}}

            User Request: {{userRequest}}

            Generate ONLY a valid JSON object with the required arguments. Do not include any explanation, just the JSON.
            If the tool doesn't require any arguments, return an empty object: {}

            JSON:
            """;

        _logger.LogInformation("Generating MCP tool arguments for tool: {ToolName}", toolName);

        var response = await GenerateAsync(prompt, cancellationToken: cancellationToken);

        // Extract JSON from response (in case the model adds extra text)
        var jsonStart = response.IndexOf('{');
        var jsonEnd = response.LastIndexOf('}');

        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            return response.Substring(jsonStart, jsonEnd - jsonStart + 1);
        }

        return "{}";
    }

    /// <summary>
    /// Get metadata about the chat client if available.
    /// Note: Model listing is not available through the standard IChatClient interface.
    /// Use Aspire's configuration to manage available models.
    /// </summary>
    public ChatClientMetadata? GetMetadata()
    {
        try
        {
            return _chatClient.GetService<ChatClientMetadata>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get chat client metadata");
            return null;
        }
    }
}
