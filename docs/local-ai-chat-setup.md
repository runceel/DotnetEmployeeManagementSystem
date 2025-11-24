# Local AI Chat with Ollama - Setup Guide

This guide explains how to set up and test the MCP Chat feature locally using Ollama with SLM (Small Language Models) for AI-powered interactions.

## Overview

The MCP Chat feature can be enhanced with local AI capabilities using:
- **Ollama**: Local LLM runtime
- **SLM Models**: Small Language Models (e.g., Phi-3, Llama3.2)
- **MCP Protocol**: Model Context Protocol for tool integration

## Prerequisites

### 1. Install Ollama

**macOS/Linux:**
```bash
curl -fsSL https://ollama.com/install.sh | sh
```

**Windows:**
Download from [ollama.com](https://ollama.com/download)

**Verify installation:**
```bash
ollama --version
```

### 2. Pull a Small Language Model

Choose a model based on your hardware:

**For 8GB RAM (Recommended for testing):**
```bash
# Phi-3 Mini (3.8B parameters, ~2.3GB)
ollama pull phi3

# or Llama 3.2 (3B parameters)
ollama pull llama3.2:3b
```

**For 16GB+ RAM:**
```bash
# Llama 3.2 (8B parameters)
ollama pull llama3.2

# or Gemma 2 (9B parameters)
ollama pull gemma2:9b
```

**List installed models:**
```bash
ollama list
```

### 3. Install Playwright (for screenshots)

```bash
# Install Playwright
pip install playwright

# Install browsers
playwright install chromium
```

## Setup Steps

### 1. Start Ollama Server

```bash
# Start Ollama (runs on http://localhost:11434 by default)
ollama serve
```

**Verify Ollama is running:**
```bash
curl http://localhost:11434/api/tags
```

### 2. Test Ollama with a Simple Query

```bash
ollama run phi3 "What is the MCP protocol?"
```

### 3. Start the Employee Management System

```bash
cd /path/to/DotnetEmployeeManagementSystem

# Start Aspire AppHost
dotnet run --project src/AppHost
```

**Note the URLs from Aspire Dashboard:**
- Dashboard: `http://localhost:15XXX`
- BlazorWeb: `http://localhost:5XXX`

### 4. Access MCP Chat

1. Open BlazorWeb URL in browser
2. Navigate to **MCPチャット** (MCP Chat)
3. Click **全サービスに接続** (Connect to All Services)

## Using AI Chat with MCP

### Scenario 1: AI-Assisted Tool Discovery

**Ask Ollama to help understand MCP tools:**

```bash
ollama run phi3 "Based on these MCP tools: ListEmployeesAsync, GetEmployeeAsync, CreateEmployeeAsync, UpdateEmployeeAsync, DeleteEmployeeAsync. Generate a JSON query to get employee with ID 123e4567-e89b-12d3-a456-426614174000"
```

**Expected Response:**
```json
{
  "employeeId": "123e4567-e89b-12d3-a456-426614174000"
}
```

### Scenario 2: AI-Generated Test Data

**Ask Ollama to generate test employee data:**

```bash
ollama run phi3 "Generate a JSON object for creating a new employee with fields: firstName, lastName, email, departmentId (use a UUID), position, and hireDate (use 2024-01-15)"
```

**Expected Response:**
```json
{
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@example.com",
  "departmentId": "456e7890-e89b-12d3-a456-426614174010",
  "position": "Software Engineer",
  "hireDate": "2024-01-15"
}
```

### Scenario 3: AI Query Translation

**Convert natural language to MCP tool calls:**

```bash
ollama run phi3 "Convert this request to an MCP tool call: 'Get all employees from the engineering department'. Available tools: ListEmployeesAsync (no args), SearchEmployeeByEmailAsync (email), GetEmployeeAsync (employeeId)"
```

**Expected Response:**
```
Tool: ListEmployeesAsync
Arguments: {}
Note: Then filter by department='Engineering' in the client
```

## Automated Screenshot Capture

### Using the Playwright Script

```bash
# Capture screenshots automatically
python3 .github/scripts/capture-mcp-screenshots.py http://localhost:5001
```

**Output Location:**
`.github/issue-reports/screenshots/`

**Generated Screenshots:**
- `01-home-page.png` - Initial landing page
- `02-mcp-chat-initial.png` - MCP Chat page
- `03-before-connection.png` - Pre-connection state
- `04-connecting.png` - Connection in progress
- `05-connected.png` - All services connected
- `06-employee-service-selected.png` - Employee service selected
- `07-tool-list.png` - Available tools displayed
- `08-tool-execution.png` - Tool being executed
- `09-chat-results.png` - Execution results in chat
- `10-help-guide.png` - Help guide expanded

### Manual Screenshot Workflow

If automated capture doesn't work:

1. Open browser DevTools (F12)
2. Set viewport to 1920x1080
3. Navigate through the MCP Chat workflow
4. Use browser screenshot: Ctrl+Shift+I → ... → Capture screenshot

## AI-Enhanced Testing Workflow

### Complete Test Flow with AI Assistance

```bash
# 1. Start services
dotnet run --project src/AppHost &
ASPIRE_PID=$!

# 2. Wait for services to be ready
sleep 30

# 3. Get BlazorWeb URL from Aspire dashboard (manual step)
BLAZOR_URL="http://localhost:5001"

# 4. Use AI to generate test scenarios
ollama run phi3 "Generate 5 test scenarios for an employee management MCP API with tools: ListEmployeesAsync, GetEmployeeAsync, CreateEmployeeAsync"

# 5. Capture screenshots
python3 .github/scripts/capture-mcp-screenshots.py $BLAZOR_URL

# 6. Clean up
kill $ASPIRE_PID
```

## Advanced: Building an AI Chat Bot for MCP

### Concept: Local AI Assistant for MCP Tools

Create a simple Python script that uses Ollama to interact with MCP:

```python
#!/usr/bin/env python3
"""Simple AI assistant for MCP Chat"""

import requests
import json

def ask_ollama(prompt, model="phi3"):
    """Query Ollama for assistance."""
    response = requests.post(
        "http://localhost:11434/api/generate",
        json={
            "model": model,
            "prompt": prompt,
            "stream": False
        }
    )
    return response.json()["response"]

# Example: Generate tool arguments
prompt = """
Given this MCP tool:
Tool: CreateEmployeeAsync
Parameters: firstName, lastName, email, departmentId (GUID), position, hireDate (ISO 8601)

Generate valid JSON arguments to create an employee named "Jane Doe", 
email "jane.doe@example.com", position "Senior Engineer", hired on 2024-06-01.
Use a random GUID for departmentId.

Return only the JSON object, no explanation.
"""

result = ask_ollama(prompt)
print("AI Generated Arguments:")
print(result)
```

**Save as:** `.github/scripts/ai-mcp-assistant.py`

**Usage:**
```bash
python3 .github/scripts/ai-mcp-assistant.py
```

## Ollama Model Recommendations

### Best Models for MCP Testing

| Model | Size | RAM | Speed | Accuracy | Use Case |
|-------|------|-----|-------|----------|----------|
| **phi3** | 3.8B | 8GB | ⚡⚡⚡ | ⭐⭐⭐ | Quick testing, JSON generation |
| **llama3.2:3b** | 3B | 8GB | ⚡⚡⚡ | ⭐⭐⭐⭐ | Balanced performance |
| **llama3.2** | 8B | 16GB | ⚡⚡ | ⭐⭐⭐⭐⭐ | Complex queries |
| **gemma2:9b** | 9B | 16GB | ⚡⚡ | ⭐⭐⭐⭐⭐ | High accuracy needed |

### Testing Model Performance

```bash
# Test JSON generation speed
time ollama run phi3 "Generate a JSON object with fields: id (UUID), name, email, active (boolean)"

# Compare models
for model in phi3 llama3.2:3b llama3.2; do
  echo "Testing $model..."
  time ollama run $model "What is MCP protocol?" > /dev/null
done
```

## Troubleshooting

### Ollama Not Running

```bash
# Check if Ollama is running
ps aux | grep ollama

# Restart Ollama
pkill ollama
ollama serve
```

### Model Not Found

```bash
# List available models
ollama list

# Pull missing model
ollama pull phi3
```

### Playwright Screenshot Fails

```bash
# Reinstall Playwright browsers
playwright install --force chromium

# Check if BlazorWeb is accessible
curl -I http://localhost:5001
```

### Aspire Services Not Starting

```bash
# Check logs
dotnet run --project src/AppHost

# Verify all services are running in Aspire dashboard
# Look for green status indicators
```

## Performance Optimization

### 1. Use Smaller Models for Faster Responses

```bash
# Phi-3 is fastest for simple JSON tasks
ollama pull phi3

# Use quantized models for lower memory
ollama pull llama3.2:3b-q4_0
```

### 2. Enable GPU Acceleration (if available)

Ollama automatically uses GPU if available (CUDA, Metal, ROCm).

**Check GPU usage:**
```bash
# macOS
sudo powermetrics --samplers gpu_power

# Linux with NVIDIA
nvidia-smi

# Windows
Task Manager > Performance > GPU
```

### 3. Adjust Context Window

```bash
ollama run phi3 --context-size 2048 "Your prompt here"
```

## Security Considerations

### Running Ollama Locally

- ✅ All data stays on your machine
- ✅ No external API calls
- ✅ Full privacy and control
- ✅ Works offline

### MCP Chat Testing

- ⚠️ Use test data only
- ⚠️ Don't expose production endpoints
- ⚠️ Review generated JSON before execution
- ⚠️ Validate AI-generated GUIDs

## Resources

### Official Documentation

- **Ollama**: https://ollama.com/
- **Model Context Protocol**: https://modelcontextprotocol.io/
- **Playwright**: https://playwright.dev/python/

### Recommended Reading

- [Ollama Model Library](https://ollama.com/library)
- [MCP Specification](https://spec.modelcontextprotocol.io/)
- [Phi-3 Technical Report](https://arxiv.org/abs/2404.14219)

### Community

- [Ollama Discord](https://discord.gg/ollama)
- [MCP GitHub](https://github.com/modelcontextprotocol)

## Next Steps

1. ✅ Install Ollama and pull a model
2. ✅ Start the Employee Management System
3. ✅ Test MCP Chat manually
4. ✅ Run Playwright screenshot script
5. ✅ Experiment with AI-assisted tool discovery
6. ⏭️ Build custom AI assistant for MCP
7. ⏭️ Integrate AI suggestions into BlazorWeb UI (future enhancement)

---

**Created**: 2024-11-24  
**Last Updated**: 2024-11-24  
**Maintainer**: Development Team
