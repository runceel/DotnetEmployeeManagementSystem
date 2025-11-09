using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using Shared.Contracts.NotificationService;

namespace NotificationService.API.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications");

        group.MapGet("/", GetAllNotifications)
            .WithName("GetAllNotifications")
            .WithOpenApi();

        group.MapGet("/recent", GetRecentNotifications)
            .WithName("GetRecentNotifications")
            .WithOpenApi();

        group.MapGet("/{id}", GetNotificationById)
            .WithName("GetNotificationById")
            .WithOpenApi();

        group.MapPost("/", CreateNotification)
            .WithName("CreateNotification")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllNotifications(
        [FromServices] INotificationService service,
        CancellationToken cancellationToken)
    {
        var notifications = await service.GetAllAsync(cancellationToken);
        return Results.Ok(notifications);
    }

    private static async Task<IResult> GetRecentNotifications(
        [FromServices] INotificationService service,
        [FromQuery] int count = 50,
        CancellationToken cancellationToken = default)
    {
        var notifications = await service.GetRecentAsync(count, cancellationToken);
        return Results.Ok(notifications);
    }

    private static async Task<IResult> GetNotificationById(
        [FromServices] INotificationService service,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var notification = await service.GetByIdAsync(id, cancellationToken);
        
        if (notification == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(notification);
    }

    private static async Task<IResult> CreateNotification(
        [FromServices] INotificationService service,
        [FromBody] CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await service.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/notifications/{notification.Id}", notification);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
