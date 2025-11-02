using EmployeeService.Application.UseCases;
using EmployeeService.Infrastructure;
using EmployeeService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.EmployeeService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// データベース接続文字列とInfrastructure層の初期化 (Test環境ではスキップ)
if (!builder.Environment.IsEnvironment("Test"))
{
    var connectionString = builder.Configuration.GetConnectionString("EmployeeDb") 
        ?? "Data Source=employees.db";
    
    // Infrastructure層のサービスを追加
    builder.Services.AddInfrastructure(connectionString);
}

// Application層のサービスを追加
builder.Services.AddScoped<IEmployeeService, EmployeeService.Application.UseCases.EmployeeService>();

var app = builder.Build();

// データベース初期化 (Test環境では実行しない)
if (!app.Environment.IsEnvironment("Test"))
{
    await DbInitializer.InitializeAsync(app.Services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// Employee API endpoints
var employees = app.MapGroup("/api/employees")
    .WithTags("Employees")
    .WithOpenApi();

// 全従業員を取得
employees.MapGet("/", async (IEmployeeService employeeService) =>
{
    var result = await employeeService.GetAllAsync();
    return Results.Ok(result);
})
.WithName("GetAllEmployees")
.Produces<IEnumerable<EmployeeDto>>();

// IDで従業員を取得
employees.MapGet("/{id:guid}", async (Guid id, IEmployeeService employeeService) =>
{
    var result = await employeeService.GetByIdAsync(id);
    return result is not null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetEmployeeById")
.Produces<EmployeeDto>()
.Produces(StatusCodes.Status404NotFound);

// 従業員を作成
employees.MapPost("/", async ([FromBody] CreateEmployeeRequest request, IEmployeeService employeeService) =>
{
    try
    {
        var result = await employeeService.CreateAsync(request);
        return Results.Created($"/api/employees/{result.Id}", result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateEmployee")
.Produces<EmployeeDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

// 従業員を更新
employees.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateEmployeeRequest request, IEmployeeService employeeService) =>
{
    try
    {
        var result = await employeeService.UpdateAsync(id, request);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("UpdateEmployee")
.Produces<EmployeeDto>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest);

// 従業員を削除
employees.MapDelete("/{id:guid}", async (Guid id, IEmployeeService employeeService) =>
{
    var result = await employeeService.DeleteAsync(id);
    return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteEmployee")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
