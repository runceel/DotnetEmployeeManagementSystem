using BlazorWeb.Components;
using BlazorWeb.Models;
using BlazorWeb.Services;
using Microsoft.Extensions.AI;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add OpenAI client with Aspire service discovery.
// AddOpenAIClient returns a builder, and AddChatClient chains 
// the IChatClient registration for use with Microsoft.Extensions.AI.
// Note: The connection name "chat" matches the OpenAI resource name from AppHost.cs
builder.AddOpenAIClient("chat").AddChatClient();

// Configure MCP options
builder.Services.Configure<McpOptions>(builder.Configuration.GetSection(McpOptions.SectionName));

// Add MudBlazor services
builder.Services.AddMudServices();

// Add HttpClient for EmployeeService with Aspire service discovery
builder.Services.AddHttpClient<IEmployeeApiClient, EmployeeApiClient>("employeeservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("http://employeeservice-api");
});

// Add HttpClient for DepartmentService with Aspire service discovery
builder.Services.AddHttpClient<IDepartmentApiClient, DepartmentApiClient>("employeeservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint (same as EmployeeService)
    client.BaseAddress = new Uri("http://employeeservice-api");
});

// Add HttpClient for AuthService with Aspire service discovery
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>("authservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("http://authservice-api");
});

// Add HttpClient for NotificationService with Aspire service discovery
builder.Services.AddHttpClient<INotificationApiClient, NotificationApiClient>("notificationservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("http://notificationservice-api");
});

// Add HttpClient for AttendanceService with Aspire service discovery
builder.Services.AddHttpClient<IAttendanceApiClient, AttendanceApiClient>("attendanceservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("http://attendanceservice-api");
});

// Add Named HttpClients for MCP servers with Aspire service discovery
builder.Services.AddHttpClient("mcp-employeeservice", client =>
{
    client.BaseAddress = new Uri("http://employeeservice-api");
});
builder.Services.AddHttpClient("mcp-authservice", client =>
{
    client.BaseAddress = new Uri("http://authservice-api");
});
builder.Services.AddHttpClient("mcp-notificationservice", client =>
{
    client.BaseAddress = new Uri("http://notificationservice-api");
});
builder.Services.AddHttpClient("mcp-attendanceservice", client =>
{
    client.BaseAddress = new Uri("http://attendanceservice-api");
});

// Add authentication state management service
builder.Services.AddScoped<AuthStateService>();

// Add MCP connection helper (shared by McpChatService and McpAiAgentService)
builder.Services.AddScoped<McpConnectionHelper>();

// Add MCP chat service
builder.Services.AddScoped<McpChatService>();

// Add AI chat service (uses Ollama via Aspire)
builder.Services.AddScoped<AiChatService>();

// Add MCP AI Agent service (uses IChatClient with automatic MCP tool calling)
builder.Services.AddScoped<McpAiAgentService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
