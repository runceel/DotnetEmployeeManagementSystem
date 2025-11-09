using Shared.Contracts.NotificationService;

namespace NotificationService.Application.UseCases;

/// <summary>
/// 通知サービスインターフェース
/// </summary>
public interface INotificationService
{
    Task<NotificationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationDto>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default);
    Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default);
}
