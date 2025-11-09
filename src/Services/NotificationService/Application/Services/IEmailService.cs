namespace NotificationService.Application.Services;

/// <summary>
/// メール送信サービスインターフェース
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string recipientEmail, string recipientName, string subject, string message, CancellationToken cancellationToken = default);
}
