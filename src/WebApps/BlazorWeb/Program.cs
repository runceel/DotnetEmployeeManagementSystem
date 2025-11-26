using BlazorWeb.Components;
using BlazorWeb.Models;
using BlazorWeb.Services;
using Microsoft.Extensions.AI;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Ollama client with Aspire service discovery as IChatClient
builder.AddOllamaApiClient("ollama");

// Add IChatClient from OllamaSharp (OllamaApiClient implements IChatClient)
builder.Services.AddScoped<IChatClient>(sp =>
{
    var ollamaClient = sp.GetRequiredService<OllamaSharp.IOllamaApiClient>();
    // OllamaApiClient implements IChatClient, cast to get it
    if (ollamaClient is IChatClient chatClient)
    {
        return chatClient;
    }
    // Ollama client should always implement IChatClient
    throw new InvalidOperationException(
        "Ollama client does not implement IChatClient. Ensure OllamaSharp version supports Microsoft.Extensions.AI.");
});

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

// Add authentication state management service
builder.Services.AddScoped<AuthStateService>();

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
