using Microsoft.Extensions.Logging;
using NotificationService.Application.Mappings;
using NotificationService.Application.Services;
using NotificationService.Domain.Repositories;
using Shared.Contracts.NotificationService;

namespace NotificationService.Application.UseCases;

/// <summary>
/// 通知サービス実装
/// </summary>
public class NotificationService(
    INotificationRepository repository,
    IEmailService emailService,
    ILogger<NotificationService> logger) : INotificationService
{
    private readonly INotificationRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    private readonly ILogger<NotificationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<NotificationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetByIdAsync(id, cancellationToken);
        return notification?.ToDto();
    }

    public async Task<IEnumerable<NotificationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetAllAsync(cancellationToken);
        return notifications.Select(n => n.ToDto());
    }

    public async Task<IEnumerable<NotificationDto>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetRecentAsync(count, cancellationToken);
        return notifications.Select(n => n.ToDto());
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var notification = request.ToEntity();
        var created = await _repository.AddAsync(notification, cancellationToken);
        
        _logger.LogInformation("通知を作成しました: {NotificationId}, 宛先: {RecipientEmail}", created.Id, created.RecipientEmail);
        
        return created.ToDto();
    }

    public async Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var pendingNotifications = await _repository.GetPendingAsync(cancellationToken);

        foreach (var notification in pendingNotifications)
        {
            try
            {
                await _emailService.SendEmailAsync(
                    notification.RecipientEmail,
                    notification.RecipientName,
                    notification.Subject,
                    notification.Message,
                    cancellationToken);

                notification.MarkAsSent();
                await _repository.UpdateAsync(notification, cancellationToken);

                _logger.LogInformation("通知を送信しました: {NotificationId}, 宛先: {RecipientEmail}", 
                    notification.Id, notification.RecipientEmail);
            }
            catch (Exception ex)
            {
                notification.MarkAsFailed(ex.Message);
                await _repository.UpdateAsync(notification, cancellationToken);

                _logger.LogError(ex, "通知の送信に失敗しました: {NotificationId}, 宛先: {RecipientEmail}", 
                    notification.Id, notification.RecipientEmail);
            }
        }
    }
}
