using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Repositories;

/// <summary>
/// 通知リポジトリインターフェース
/// </summary>
public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
}
