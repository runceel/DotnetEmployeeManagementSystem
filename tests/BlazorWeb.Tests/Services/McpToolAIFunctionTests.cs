using System.Text.Json;
using BlazorWeb.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlazorWeb.Tests.Services;

/// <summary>
/// Tests for McpToolAIFunction wrapper that exposes MCP InputSchema to AI
/// </summary>
public class McpToolAIFunctionTests
{
    [Fact]
    public void JsonSchema_ShouldExposeMcpInputSchema_InsteadOfWrappingInArguments()
    {
        // Arrange
        var mcpInputSchema = JsonSerializer.Deserialize<JsonElement>("""
        {
            "type": "object",
            "properties": {
                "employeeId": {"type": "string", "description": "The employee ID"},
                "firstName": {"type": "string", "description": "First name"}
            },
            "required": ["employeeId", "firstName"]
        }
        """);

        var baseFunction = AIFunctionFactory.Create(
            (IDictionary<string, object?>? arguments) => "OK",
            new AIFunctionFactoryOptions
            {
                Name = "testTool",
                Description = "Test tool"
            });

        // Act
        var wrappedFunction = new McpToolAIFunction(baseFunction, mcpInputSchema, NullLogger.Instance);

        // Assert
        Assert.Equal("testTool", wrappedFunction.Name);
        Assert.Equal("Test tool", wrappedFunction.Description);
        
        // The schema should be the MCP InputSchema, not wrapped with "arguments"
        var schema = wrappedFunction.JsonSchema;
        Assert.True(schema.TryGetProperty("properties", out var properties));
        Assert.True(properties.TryGetProperty("employeeId", out _));
        Assert.True(properties.TryGetProperty("firstName", out _));
        Assert.False(properties.TryGetProperty("arguments", out _)); // Should NOT have "arguments" wrapper
        
        // Required fields should be preserved
        Assert.True(schema.TryGetProperty("required", out var required));
        var requiredArray = required.EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("employeeId", requiredArray);
        Assert.Contains("firstName", requiredArray);
    }

    [Fact]
    public async Task InvokeAsync_ShouldWrapFlatArgumentsForInnerFunction()
    {
        // Arrange
        Dictionary<string, object?>? receivedArguments = null;
        
        var baseFunction = AIFunctionFactory.Create(
            (IDictionary<string, object?>? arguments) =>
            {
                receivedArguments = arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                return "OK";
            },
            new AIFunctionFactoryOptions
            {
                Name = "testTool",
                Description = "Test tool"
            });

        var mcpInputSchema = JsonSerializer.Deserialize<JsonElement>("""
        {
            "type": "object",
            "properties": {
                "employeeId": {"type": "string"}
            },
            "required": ["employeeId"]
        }
        """);

        var wrappedFunction = new McpToolAIFunction(baseFunction, mcpInputSchema, NullLogger.Instance);

        // Act - AI calls with flat arguments
        var arguments = new AIFunctionArguments
        {
            ["employeeId"] = "123",
            ["firstName"] = "John"
        };
        
        await wrappedFunction.InvokeAsync(arguments);

        // Assert - AIFunctionFactory automatically unwraps the "arguments" parameter
        // and binds it directly to the IDictionary<string, object?> parameter
        Assert.NotNull(receivedArguments);
        Assert.Equal(2, receivedArguments.Count);
        Assert.Equal("123", receivedArguments["employeeId"]);
        Assert.Equal("John", receivedArguments["firstName"]);
    }

    [Fact]
    public void BaseFunction_WithoutWrapper_ShouldHaveArgumentsParameter()
    {
        // Arrange - This test verifies the problem we're solving
        var baseFunction = AIFunctionFactory.Create(
            (IDictionary<string, object?>? arguments) => "OK",
            new AIFunctionFactoryOptions
            {
                Name = "testTool",
                Description = "Test tool"
            });

        // Assert - Without wrapper, schema has "arguments" parameter
        var schema = baseFunction.JsonSchema;
        Assert.True(schema.TryGetProperty("properties", out var properties));
        Assert.True(properties.TryGetProperty("arguments", out _)); // Has "arguments" wrapper
        
        Assert.True(schema.TryGetProperty("required", out var required));
        var requiredArray = required.EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("arguments", requiredArray);
    }
}
