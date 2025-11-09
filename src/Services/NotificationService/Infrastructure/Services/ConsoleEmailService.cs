using Microsoft.Extensions.Logging;
using NotificationService.Application.Services;

namespace NotificationService.Infrastructure.Services;

/// <summary>
/// コンソール出力によるメール送信サービス（開発用）
/// </summary>
public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task SendEmailAsync(string recipientEmail, string recipientName, string subject, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("""
            
            ========== メール送信 (コンソール出力モード) ==========
            宛先: {RecipientName} <{RecipientEmail}>
            件名: {Subject}
            ---
            {Message}
            ======================================================
            
            """, recipientName, recipientEmail, subject, message);

        return Task.CompletedTask;
    }
}
