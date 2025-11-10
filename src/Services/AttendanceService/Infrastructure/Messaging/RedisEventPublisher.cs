using System.Text.Json;
using AttendanceService.Application.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AttendanceService.Infrastructure.Messaging;

/// <summary>
/// Redis イベント発行実装
/// </summary>
public class RedisEventPublisher : IEventPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisEventPublisher> _logger;

    public RedisEventPublisher(IConnectionMultiplexer redis, ILogger<RedisEventPublisher> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync<T>(string channel, T eventData, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(eventData);

        var json = JsonSerializer.Serialize(eventData);
        var subscriber = _redis.GetSubscriber();

        await subscriber.PublishAsync(RedisChannel.Literal(channel), json);

        _logger.LogInformation("イベントを発行しました: チャネル={Channel}, タイプ={EventType}", channel, typeof(T).Name);
    }
}
