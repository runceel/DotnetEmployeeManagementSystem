using NotificationService.Domain.Entities;
using Shared.Contracts.NotificationService;

namespace NotificationService.Application.Mappings;

public static class NotificationMappings
{
    public static NotificationDto ToDto(this Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            RecipientEmail = notification.RecipientEmail,
            RecipientName = notification.RecipientName,
            NotificationType = notification.NotificationType,
            Subject = notification.Subject,
            Message = notification.Message,
            Status = notification.Status,
            ErrorMessage = notification.ErrorMessage,
            RetryCount = notification.RetryCount,
            CreatedAt = notification.CreatedAt,
            SentAt = notification.SentAt
        };
    }

    public static Notification ToEntity(this CreateNotificationRequest request, string notificationType = "Manual")
    {
        return new Notification(
            request.RecipientEmail,
            request.RecipientName,
            notificationType,
            request.Subject,
            request.Message
        );
    }
}
