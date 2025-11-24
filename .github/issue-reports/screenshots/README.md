# MCP Chat Screenshots

This directory contains screenshots captured from the MCP Chat feature.

## Automated Capture

Use the Playwright script to automatically capture screenshots:

```bash
# 1. Start Aspire AppHost
dotnet run --project src/AppHost

# 2. Get BlazorWeb URL from Aspire dashboard (e.g., http://localhost:5001)

# 3. Run the screenshot script
python3 .github/scripts/capture-mcp-screenshots.py http://localhost:5001
```

## Expected Screenshots

- `01-home-page.png` - Initial landing page or login
- `02-mcp-chat-initial.png` - MCP Chat page loaded
- `03-before-connection.png` - Connection screen with service cards
- `04-connecting.png` - Connection in progress (spinner visible)
- `05-connected.png` - All services connected (green checkmarks)
- `06-employee-service-selected.png` - Employee service selected
- `07-tool-list.png` - Tool list displayed
- `08-tool-execution.png` - Tool being executed
- `09-chat-results.png` - Execution results in chat history
- `10-help-guide.png` - Help guide expanded

## Manual Capture

If automated capture doesn't work:

1. Set browser viewport to 1920x1080
2. Navigate through the MCP Chat workflow
3. Use browser DevTools (F12) → ... → Capture screenshot
4. Save with the naming convention above

## Viewing Screenshots

Screenshots are PNG format and can be viewed with any image viewer.

For PR reviews, upload screenshots to the PR comments.

---

**Note**: Screenshots are not committed to the repository by default.
Add them manually to PR comments or documentation as needed.
