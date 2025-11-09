using Shared.Contracts.NotificationService;

namespace BlazorWeb.Services;

public interface INotificationApiClient
{
    Task<IEnumerable<NotificationDto>> GetAllAsync();
    Task<IEnumerable<NotificationDto>> GetRecentAsync(int count = 50);
    Task<NotificationDto?> GetByIdAsync(Guid id);
    Task<NotificationDto> CreateAsync(CreateNotificationRequest request);
}
