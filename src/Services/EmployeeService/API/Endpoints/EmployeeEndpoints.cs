using EmployeeService.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.EmployeeService;

namespace EmployeeService.API.Endpoints;

public static class EmployeeEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var employees = app.MapGroup("/api/employees")
            .WithTags("Employees");

        employees.MapGet("/", GetAllEmployees)
            .WithName("GetAllEmployees")
            .Produces<IEnumerable<EmployeeDto>>();

        employees.MapGet("/{id:guid}", GetEmployeeById)
            .WithName("GetEmployeeById")
            .Produces<EmployeeDto>()
            .Produces(StatusCodes.Status404NotFound);

        employees.MapPost("/", CreateEmployee)
            .WithName("CreateEmployee")
            .Produces<EmployeeDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("AdminPolicy");

        employees.MapPut("/{id:guid}", UpdateEmployee)
            .WithName("UpdateEmployee")
            .Produces<EmployeeDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("AdminPolicy");

        employees.MapDelete("/{id:guid}", DeleteEmployee)
            .WithName("DeleteEmployee")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        employees.MapGet("/dashboard/statistics", GetDashboardStatistics)
            .WithName("GetDashboardStatistics")
            .Produces<DashboardStatisticsDto>();

        employees.MapGet("/dashboard/recent-activities", GetRecentActivities)
            .WithName("GetRecentActivities")
            .Produces<IEnumerable<RecentActivityDto>>();

        return app;
    }

    private static async Task<IResult> GetAllEmployees(
        [FromServices] IEmployeeService employeeService)
    {
        var result = await employeeService.GetAllAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> GetEmployeeById(
        Guid id,
        [FromServices] IEmployeeService employeeService)
    {
        var result = await employeeService.GetByIdAsync(id);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> CreateEmployee(
        [FromBody] CreateEmployeeRequest request,
        [FromServices] IEmployeeService employeeService)
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
    }

    private static async Task<IResult> UpdateEmployee(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        [FromServices] IEmployeeService employeeService)
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
    }

    private static async Task<IResult> DeleteEmployee(
        Guid id,
        [FromServices] IEmployeeService employeeService)
    {
        var result = await employeeService.DeleteAsync(id);
        return result ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> GetDashboardStatistics(
        [FromServices] IEmployeeService employeeService)
    {
        var result = await employeeService.GetDashboardStatisticsAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> GetRecentActivities(
        [FromServices] IEmployeeService employeeService,
        [FromQuery] int count = 10)
    {
        var result = await employeeService.GetRecentActivitiesAsync(count);
        return Results.Ok(result);
    }
}
