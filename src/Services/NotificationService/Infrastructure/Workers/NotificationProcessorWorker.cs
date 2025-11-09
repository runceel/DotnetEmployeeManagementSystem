using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.UseCases;

namespace NotificationService.Infrastructure.Workers;

public class NotificationProcessorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationProcessorWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public NotificationProcessorWorker(
        IServiceProvider serviceProvider,
        ILogger<NotificationProcessorWorker> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("通知処理ワーカーを開始しました");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                await notificationService.ProcessPendingNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通知処理中にエラーが発生しました");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("通知処理ワーカーを停止しました");
    }
}
