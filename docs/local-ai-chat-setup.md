# Local AI Chat with Ollama - Setup Guide

This guide explains how to set up and test the MCP Chat feature locally using Ollama with SLM (Small Language Models) for AI-powered interactions.

## Overview

The MCP Chat feature is enhanced with local AI capabilities using:
- **Aspire Ollama Integration**: Automatically starts Ollama when running AppHost
- **Open WebUI**: Browser-based interface for Ollama included
- **phi3 Model**: Pre-configured small language model for quick testing
- **OllamaSharp Client**: .NET integration for AI-powered features

## Prerequisites

### 1. Docker Required

Ollama runs as a Docker container in Aspire. Ensure Docker is installed and running:

**Check Docker:**
```bash
docker --version
docker info
```

**Install Docker (if needed):**
- **Windows**: [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- **macOS**: [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- **Linux**: [Docker Engine](https://docs.docker.com/engine/install/)

## Quick Start with Aspire

### 1. Start Aspire AppHost

Simply start the Aspire application - Ollama will start automatically:

```bash
cd /path/to/DotnetEmployeeManagementSystem
dotnet run --project src/AppHost
```

### 2. What Gets Started Automatically

When you run `dotnet run --project src/AppHost`, the following happens:

| Service | Description | Access |
|---------|-------------|--------|
| **Ollama** | Local LLM server (Docker container) | http://localhost:11434 |
| **Open WebUI** | Browser-based chat interface | http://localhost:8080 |
| **phi3 Model** | Pre-downloaded SLM (3.8B params) | Automatically available |
| **BlazorWeb** | Main application with AI integration | See Aspire dashboard |

### 3. Access Aspire Dashboard

Open the Aspire dashboard URL shown in the console (e.g., `http://localhost:15000`) to:
- View all running services
- Access Ollama and Open WebUI endpoints
- Monitor health and logs

### 4. Test with Open WebUI

1. Find "ollama-openwebui" in the Aspire dashboard
2. Click on the endpoint URL
3. Create an account (local only)
4. Start chatting with phi3 model

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                       Aspire AppHost                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────────┐ │
│  │   Ollama    │    │ Open WebUI  │    │      BlazorWeb      │ │
│  │  (Docker)   │◄───│  (Docker)   │    │  (with AiChatService)│ │
│  │             │    │             │    │                     │ │
│  │  phi3 model │    │ Chat UI     │    │ MCP Chat + AI       │ │
│  └─────────────┘    └─────────────┘    └─────────────────────┘ │
│         │                                        │              │
│         └────────────────────────────────────────┘              │
│                     OllamaSharp Client                          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## BlazorWeb AI Integration

The BlazorWeb application includes an `AiChatService` that uses Ollama for:

### Available Features

1. **AI-Assisted MCP Tool Arguments**
   - Generate JSON arguments for MCP tools
   - Natural language to structured data conversion

2. **Model Availability Check**
   - Verify Ollama connection
   - Check if required models are ready

3. **Text Generation**
   - Generate responses from prompts
   - Stream responses in real-time

4. **Model Listing**
   - List available local models
   - Check model capabilities

### Usage in Code

```csharp
// Inject AiChatService
@inject AiChatService AiChat

// Check if Ollama is available
var isAvailable = await AiChat.IsAvailableAsync();

// Generate MCP tool arguments
var arguments = await AiChat.GenerateMcpToolArgumentsAsync(
    "CreateEmployeeAsync",
    "Create a new employee with the specified details",
    "Create an employee named John Doe, email john@example.com"
);
// Returns: {"firstName":"John","lastName":"Doe","email":"john@example.com",...}

// Generate text
var response = await AiChat.GenerateAsync("Explain what MCP protocol is");

// Stream response
await foreach (var chunk in AiChat.GenerateStreamAsync("Tell me about .NET Aspire"))
{
    Console.Write(chunk);
}

// List models
var models = await AiChat.ListModelsAsync();
```

## Configuration Details

### AppHost Configuration

The Ollama integration is configured in `src/AppHost/AppHost.cs`:

```csharp
// Add Ollama with phi3 model for local AI chat
var ollama = builder.AddOllama("ollama")
    .WithDataVolume()        // Persist model data
    .WithOpenWebUI()         // Include browser UI
    .AddModel("phi3");       // Download phi3 model

// Reference Ollama in BlazorWeb
builder.AddProject<Projects.BlazorWeb>("blazorweb")
    .WithReference(ollama);  // Auto-configure connection
```

### Service Configuration

BlazorWeb automatically configures the Ollama client via Aspire service discovery:

```csharp
// Program.cs
builder.AddOllamaApiClient("ollama");  // Auto-configured via Aspire
builder.Services.AddScoped<AiChatService>();
```

## Available Models

### Default: phi3 (Recommended)

| Property | Value |
|----------|-------|
| **Name** | phi3 |
| **Parameters** | 3.8B |
| **Size** | ~2.3GB |
| **RAM Required** | 8GB |
| **Speed** | Fast |
| **Use Case** | JSON generation, code assistance |

### Adding More Models

To add additional models, modify `AppHost.cs`:

```csharp
var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithOpenWebUI()
    .AddModel("phi3")
    .AddModel("llama3.2:3b")    // Add Llama 3.2 (3B)
    .AddModel("gemma2:9b");     // Add Gemma 2 (9B)
```

### Model Recommendations

| Model | Size | RAM | Speed | Best For |
|-------|------|-----|-------|----------|
| **phi3** | 3.8B | 8GB | ⚡⚡⚡ | Quick testing, JSON |
| **llama3.2:3b** | 3B | 8GB | ⚡⚡⚡ | General use |
| **llama3.2** | 8B | 16GB | ⚡⚡ | Complex queries |
| **gemma2:9b** | 9B | 16GB | ⚡⚡ | High accuracy |

## Using AI with MCP Chat

### Scenario 1: Generate Tool Arguments

When in MCP Chat, use the AI to generate tool arguments:

**User Request**: "Create an employee named Jane Smith, email jane@company.com, position Senior Developer"

**AI Generated JSON**:
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@company.com",
  "position": "Senior Developer",
  "departmentId": "00000000-0000-0000-0000-000000000000",
  "hireDate": "2024-11-25"
}
```

### Scenario 2: Understand Tool Descriptions

Ask the AI to explain what an MCP tool does:

**Prompt**: "What parameters does GetEmployeeAsync need?"

**AI Response**: "The GetEmployeeAsync tool requires an 'employeeId' parameter as a GUID string to retrieve a specific employee's information."

### Scenario 3: Validate JSON Format

Ask the AI to check your JSON:

**Prompt**: "Is this valid JSON? {firstName: John}"

**AI Response**: "No, JSON requires double quotes around property names. The correct format is: {\"firstName\": \"John\"}"

## Open WebUI Features

The included Open WebUI provides:

- 📝 **Chat Interface**: Direct conversation with Ollama models
- 📚 **Model Management**: Download/delete models
- ⚙️ **Settings**: Adjust temperature, context size
- 📊 **History**: Save and review conversations
- 🔄 **Multi-Model**: Switch between models easily

### Access Open WebUI

1. Start Aspire AppHost
2. Find "ollama-openwebui" in dashboard
3. Click endpoint URL
4. First time: Create local account
5. Select phi3 model and start chatting

## Troubleshooting

### Docker Not Running

```bash
# Check Docker status
docker info

# Start Docker
# Windows/macOS: Open Docker Desktop
# Linux: sudo systemctl start docker
```

### Model Download Slow

Models are downloaded on first run. The phi3 model (~2.3GB) may take a few minutes:

```bash
# Check download progress in Aspire dashboard logs
# Or use docker logs:
docker logs <ollama-container-id>
```

### Memory Issues

If you experience slowness or crashes:

```bash
# Check available memory
free -h  # Linux
# or check Docker Desktop resources

# Use smaller models if needed
# Minimum 8GB RAM recommended for phi3
```

### Connection Refused

If BlazorWeb can't connect to Ollama:

1. Check Ollama container is running in Aspire dashboard
2. Verify Ollama endpoint in dashboard resources
3. Check container logs for errors

### Model Not Found

If "phi3" model isn't available:

```bash
# The model should auto-download, but you can manually pull:
docker exec <ollama-container-id> ollama pull phi3
```

## Performance Tips

### 1. Use Data Volume

The `WithDataVolume()` option persists model data between runs:

```csharp
builder.AddOllama("ollama")
    .WithDataVolume()  // Models persist, no re-download
```

### 2. GPU Acceleration

If you have a GPU, Ollama will automatically use it for faster inference.

### 3. Adjust Context Size

For longer conversations, increase context in Open WebUI settings.

### 4. Use Streaming

For responsive UIs, use streaming responses:

```csharp
await foreach (var chunk in AiChat.GenerateStreamAsync(prompt))
{
    // Update UI incrementally
}
```

## Security Considerations

### Local Execution

- ✅ All AI processing happens locally
- ✅ No data sent to external APIs
- ✅ Complete privacy control
- ✅ Works offline after model download

### Docker Isolation

- ✅ Ollama runs in isolated container
- ✅ No host system access by default
- ✅ Network isolated to Aspire network

### Production Notes

- ⚠️ Disable Open WebUI in production
- ⚠️ Consider authentication for Ollama
- ⚠️ Monitor resource usage

## Resources

### Official Documentation

- **Aspire Ollama Extension**: [CommunityToolkit.Aspire](https://github.com/CommunityToolkit/Aspire)
- **Ollama**: [ollama.com](https://ollama.com/)
- **OllamaSharp**: [GitHub](https://github.com/awaescher/OllamaSharp)
- **Open WebUI**: [GitHub](https://github.com/open-webui/open-webui)

### Model Information

- [phi3 Technical Report](https://arxiv.org/abs/2404.14219)
- [Ollama Model Library](https://ollama.com/library)

## Summary

With Aspire's Ollama integration:

1. ✅ **Zero Setup**: Just run `dotnet run --project src/AppHost`
2. ✅ **Auto-Start**: Ollama + Open WebUI start automatically
3. ✅ **Pre-Configured**: phi3 model ready to use
4. ✅ **Integrated**: BlazorWeb has AiChatService for AI features
5. ✅ **Privacy**: All processing is local

---

**Created**: 2024-11-24  
**Last Updated**: 2024-11-25  
**Version**: 2.0 (Aspire Integration)  
**Maintainer**: Development Team
