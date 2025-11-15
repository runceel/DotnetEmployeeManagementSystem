using EmployeeService.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.DepartmentService;

namespace EmployeeService.API.Endpoints;

public static class DepartmentEndpoints
{
    public static IEndpointRouteBuilder MapDepartmentEndpoints(this IEndpointRouteBuilder app)
    {
        var departments = app.MapGroup("/api/departments")
            .WithTags("Departments");

        departments.MapGet("/", GetAllDepartments)
            .WithName("GetAllDepartments")
            .Produces<IEnumerable<DepartmentDto>>();

        departments.MapGet("/{id:guid}", GetDepartmentById)
            .WithName("GetDepartmentById")
            .Produces<DepartmentDto>()
            .Produces(StatusCodes.Status404NotFound);

        departments.MapPost("/", CreateDepartment)
            .WithName("CreateDepartment")
            .Produces<DepartmentDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("AdminPolicy");

        departments.MapPut("/{id:guid}", UpdateDepartment)
            .WithName("UpdateDepartment")
            .Produces<DepartmentDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("AdminPolicy");

        departments.MapDelete("/{id:guid}", DeleteDepartment)
            .WithName("DeleteDepartment")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("AdminPolicy");

        return app;
    }

    private static async Task<IResult> GetAllDepartments(
        [FromServices] IDepartmentService departmentService)
    {
        var result = await departmentService.GetAllAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> GetDepartmentById(
        Guid id,
        [FromServices] IDepartmentService departmentService)
    {
        var result = await departmentService.GetByIdAsync(id);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> CreateDepartment(
        [FromBody] CreateDepartmentRequest request,
        [FromServices] IDepartmentService departmentService)
    {
        try
        {
            var result = await departmentService.CreateAsync(request);
            return Results.Created($"/api/departments/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateDepartment(
        Guid id,
        [FromBody] UpdateDepartmentRequest request,
        [FromServices] IDepartmentService departmentService)
    {
        try
        {
            var result = await departmentService.UpdateAsync(id, request);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteDepartment(
        Guid id,
        [FromServices] IDepartmentService departmentService)
    {
        try
        {
            var result = await departmentService.DeleteAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
