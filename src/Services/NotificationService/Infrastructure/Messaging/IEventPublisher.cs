namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// イベント発行インターフェース
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(string channel, T eventData, CancellationToken cancellationToken = default) where T : class;
}
