using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;
using Shared.Contracts.Events;
using StackExchange.Redis;

namespace NotificationService.Infrastructure.Messaging;

public class EmployeeEventConsumer : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmployeeEventConsumer> _logger;

    public EmployeeEventConsumer(
        IConnectionMultiplexer redis,
        IServiceProvider serviceProvider,
        ILogger<EmployeeEventConsumer> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("従業員イベントコンシューマーを開始しました");

        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(RedisChannel.Literal("employee.created"), async (channel, message) =>
        {
            await HandleEmployeeCreatedAsync(message!, stoppingToken);
        });

        await subscriber.SubscribeAsync(RedisChannel.Literal("employee.updated"), async (channel, message) =>
        {
            await HandleEmployeeUpdatedAsync(message!, stoppingToken);
        });

        await subscriber.SubscribeAsync(RedisChannel.Literal("employee.deleted"), async (channel, message) =>
        {
            await HandleEmployeeDeletedAsync(message!, stoppingToken);
        });

        _logger.LogInformation("イベントチャネルに登録しました");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleEmployeeCreatedAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var eventData = JsonSerializer.Deserialize<EmployeeCreatedEvent>(message);
            if (eventData == null) return;

            _logger.LogInformation("従業員作成イベントを受信しました: {EmployeeId}", eventData.EmployeeId);

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var notification = new Domain.Entities.Notification(
                eventData.Email,
                $"{eventData.LastName} {eventData.FirstName}",
                NotificationType.EmployeeCreated,
                "ようこそ！従業員登録が完了しました",
                $"""
                {eventData.LastName} {eventData.FirstName} 様

                従業員管理システムへようこそ！
                あなたの従業員情報が正常に登録されました。

                【登録情報】
                - 氏名: {eventData.LastName} {eventData.FirstName}
                - メールアドレス: {eventData.Email}
                - 部署: {eventData.Department}
                - 役職: {eventData.Position}
                - 登録日: {eventData.CreatedAt:yyyy年MM月dd日 HH:mm}

                今後ともよろしくお願いいたします。

                従業員管理システム
                """
            );

            await repository.AddAsync(notification, cancellationToken);
            _logger.LogInformation("従業員作成通知を作成しました: {NotificationId}", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "従業員作成イベントの処理中にエラーが発生しました");
        }
    }

    private async Task HandleEmployeeUpdatedAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var eventData = JsonSerializer.Deserialize<EmployeeUpdatedEvent>(message);
            if (eventData == null) return;

            _logger.LogInformation("従業員更新イベントを受信しました: {EmployeeId}", eventData.EmployeeId);

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var notification = new Domain.Entities.Notification(
                eventData.Email,
                $"{eventData.LastName} {eventData.FirstName}",
                NotificationType.EmployeeUpdated,
                "従業員情報が更新されました",
                $"""
                {eventData.LastName} {eventData.FirstName} 様

                あなたの従業員情報が更新されました。

                【現在の情報】
                - 氏名: {eventData.LastName} {eventData.FirstName}
                - メールアドレス: {eventData.Email}
                - 部署: {eventData.Department}
                - 役職: {eventData.Position}
                - 更新日時: {eventData.UpdatedAt:yyyy年MM月dd日 HH:mm}

                内容に誤りがある場合は、システム管理者までお問い合わせください。

                従業員管理システム
                """
            );

            await repository.AddAsync(notification, cancellationToken);
            _logger.LogInformation("従業員更新通知を作成しました: {NotificationId}", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "従業員更新イベントの処理中にエラーが発生しました");
        }
    }

    private async Task HandleEmployeeDeletedAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var eventData = JsonSerializer.Deserialize<EmployeeDeletedEvent>(message);
            if (eventData == null) return;

            _logger.LogInformation("従業員削除イベントを受信しました: {EmployeeId}", eventData.EmployeeId);

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var notification = new Domain.Entities.Notification(
                eventData.Email,
                $"{eventData.LastName} {eventData.FirstName}",
                NotificationType.EmployeeDeleted,
                "従業員情報が削除されました",
                $"""
                {eventData.LastName} {eventData.FirstName} 様

                あなたの従業員情報がシステムから削除されました。

                削除日時: {eventData.DeletedAt:yyyy年MM月dd日 HH:mm}

                ご利用ありがとうございました。

                従業員管理システム
                """
            );

            await repository.AddAsync(notification, cancellationToken);
            _logger.LogInformation("従業員削除通知を作成しました: {NotificationId}", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "従業員削除イベントの処理中にエラーが発生しました");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("従業員イベントコンシューマーを停止しています");
        await base.StopAsync(cancellationToken);
    }
}
