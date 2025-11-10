namespace AttendanceService.Application.Services;

/// <summary>
/// イベント発行インターフェース
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// イベントを発行
    /// </summary>
    Task PublishAsync<T>(string channel, T @event, CancellationToken cancellationToken = default) where T : class;
}
