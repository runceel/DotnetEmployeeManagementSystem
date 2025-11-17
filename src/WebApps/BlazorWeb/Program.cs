using BlazorWeb.Components;
using BlazorWeb.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add HttpClient for EmployeeService with Aspire service discovery
builder.Services.AddHttpClient<IEmployeeApiClient, EmployeeApiClient>("employeeservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("http://employeeservice");
});

// Add HttpClient for DepartmentService with Aspire service discovery
builder.Services.AddHttpClient<IDepartmentApiClient, DepartmentApiClient>("employeeservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint (same as EmployeeService)
    client.BaseAddress = new Uri("http://employeeservice");
});

// Add HttpClient for AuthService with Aspire service discovery
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>("authservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("http://authservice");
});

// Add HttpClient for NotificationService with Aspire service discovery
builder.Services.AddHttpClient<INotificationApiClient, NotificationApiClient>("notificationservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("http://notificationservice");
});

// Add HttpClient for AttendanceService with Aspire service discovery
builder.Services.AddHttpClient<IAttendanceApiClient, AttendanceApiClient>("attendanceservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("http://attendanceservice");
});

// Add authentication state management service
builder.Services.AddScoped<AuthStateService>();

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
