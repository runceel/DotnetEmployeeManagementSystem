using BlazorWeb.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add HttpClient for EmployeeService with Aspire service discovery
builder.Services.AddHttpClient("employeeservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("http://employeeservice-api");
});

// Add HttpClient for AuthService with Aspire service discovery
builder.Services.AddHttpClient("authservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("http://authservice-api");
});

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
