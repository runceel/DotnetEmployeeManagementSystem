using OllamaSharp;
using System.Diagnostics;

namespace BlazorWeb.Services;

/// <summary>
/// Service for interacting with Ollama AI chat functionality.
/// Uses OllamaSharp client with Aspire service discovery.
/// </summary>
public sealed class AiChatService
{
    private readonly IOllamaApiClient _ollamaClient;
    private readonly ILogger<AiChatService> _logger;
    private static readonly ActivitySource ActivitySource = new("BlazorWeb.AiChat");

    /// <summary>
    /// Default model to use for chat interactions
    /// </summary>
    public const string DefaultModel = "phi3";

    public AiChatService(IOllamaApiClient ollamaClient, ILogger<AiChatService> logger)
    {
        _ollamaClient = ollamaClient;
        _logger = logger;
    }

    /// <summary>
    /// Check if Ollama is available and the model is ready
    /// </summary>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CheckOllamaAvailability");
        
        try
        {
            var models = await _ollamaClient.ListLocalModelsAsync(cancellationToken);
            var modelList = models.ToList();
            var hasModel = modelList.Any(m => m.Name.Contains(DefaultModel, StringComparison.OrdinalIgnoreCase));
            
            activity?.SetTag("models.count", modelList.Count);
            activity?.SetTag("has.default.model", hasModel);
            
            _logger.LogInformation("Ollama available with {ModelCount} models, default model available: {HasModel}", 
                modelList.Count, hasModel);
            
            return hasModel;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama is not available");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Generate a response from the AI model
    /// </summary>
    public async Task<string> GenerateAsync(string prompt, string? model = null, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GenerateAiResponse");
        var modelToUse = model ?? DefaultModel;
        
        activity?.SetTag("model", modelToUse);
        activity?.SetTag("prompt.length", prompt.Length);

        try
        {
            _logger.LogInformation("Generating AI response with model {Model} for prompt of length {Length}", 
                modelToUse, prompt.Length);

            var responseBuilder = new System.Text.StringBuilder();
            
            await foreach (var chunk in _ollamaClient.GenerateAsync(new OllamaSharp.Models.GenerateRequest
            {
                Model = modelToUse,
                Prompt = prompt,
                Stream = false
            }, cancellationToken))
            {
                if (chunk?.Response != null)
                {
                    responseBuilder.Append(chunk.Response);
                }
            }

            var result = responseBuilder.ToString();
            
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
    /// Stream a response from the AI model
    /// </summary>
    public async IAsyncEnumerable<string> GenerateStreamAsync(
        string prompt, 
        string? model = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GenerateAiResponseStream");
        var modelToUse = model ?? DefaultModel;
        
        activity?.SetTag("model", modelToUse);
        activity?.SetTag("prompt.length", prompt.Length);

        _logger.LogInformation("Starting streaming AI response with model {Model}", modelToUse);

        var request = new OllamaSharp.Models.GenerateRequest
        {
            Model = modelToUse,
            Prompt = prompt,
            Stream = true
        };

        await foreach (var response in _ollamaClient.GenerateAsync(request, cancellationToken))
        {
            if (!string.IsNullOrEmpty(response?.Response))
            {
                yield return response.Response;
            }
        }
    }

    /// <summary>
    /// Generate JSON for MCP tool arguments using AI
    /// </summary>
    public async Task<string> GenerateMcpToolArgumentsAsync(
        string toolName, 
        string toolDescription, 
        string userRequest,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GenerateMcpToolArguments");
        activity?.SetTag("tool.name", toolName);

        var prompt = $@"You are an assistant that generates JSON arguments for MCP (Model Context Protocol) tools.

Tool Name: {toolName}
Tool Description: {toolDescription}

User Request: {userRequest}

Generate ONLY a valid JSON object with the required arguments. Do not include any explanation, just the JSON.
If the tool doesn't require any arguments, return an empty object: {{}}

JSON:";

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
    /// List available models
    /// </summary>
    public async Task<IEnumerable<string>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("ListOllamaModels");
        
        try
        {
            var models = await _ollamaClient.ListLocalModelsAsync(cancellationToken);
            var modelNames = models.Select(m => m.Name).ToList();
            
            activity?.SetTag("models.count", modelNames.Count);
            _logger.LogInformation("Listed {Count} Ollama models", modelNames.Count);
            
            return modelNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list Ollama models");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return Enumerable.Empty<string>();
        }
    }
}
