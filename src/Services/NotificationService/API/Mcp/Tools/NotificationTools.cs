using ModelContextProtocol.Server;
using NotificationService.Application.UseCases;
using Shared.Contracts.NotificationService;

namespace NotificationService.API.Mcp.Tools;

/// <summary>
/// 通知管理に関するMCPツール
/// </summary>
[McpServerToolType]
public class NotificationTools
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationTools> _logger;

    public NotificationTools(
        INotificationService notificationService,
        ILogger<NotificationTools> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// 通知一覧を取得します。全通知の一覧を取得し、送信状態、作成日時、受信者などの情報が含まれます。
    /// </summary>
    /// <param name="limit">取得する通知の最大件数（デフォルト: 50）</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    [McpServerTool]
    public async Task<NotificationListResponse> GetNotificationsAsync(
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MCP Tool: GetNotifications - Limit: {Limit}", limit);

        var notifications = await _notificationService.GetRecentAsync(limit, cancellationToken);
        var notificationList = notifications.ToList();

        return new NotificationListResponse(
            Notifications: notificationList,
            TotalCount: notificationList.Count
        );
    }

    /// <summary>
    /// 指定されたIDの通知を取得します。通知IDを指定して、特定の通知の詳細情報を取得します。
    /// </summary>
    /// <param name="notificationId">取得する通知のID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    [McpServerTool]
    public async Task<NotificationDetailResponse> GetNotificationByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MCP Tool: GetNotificationById - NotificationId: {NotificationId}", notificationId);

        var notification = await _notificationService.GetByIdAsync(notificationId, cancellationToken);
        
        if (notification == null)
        {
            throw new InvalidOperationException($"通知ID {notificationId} が見つかりませんでした。");
        }

        return new NotificationDetailResponse(
            Id: notification.Id,
            RecipientEmail: notification.RecipientEmail,
            RecipientName: notification.RecipientName,
            NotificationType: notification.NotificationType,
            Subject: notification.Subject,
            Message: notification.Message,
            Status: notification.Status,
            ErrorMessage: notification.ErrorMessage,
            RetryCount: notification.RetryCount,
            CreatedAt: notification.CreatedAt,
            SentAt: notification.SentAt
        );
    }

    /// <summary>
    /// 状態別の通知数を取得します。送信待ち、送信済み、失敗などの状態別に通知数を集計します。
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    [McpServerTool]
    public async Task<NotificationStatsResponse> GetNotificationStatsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MCP Tool: GetNotificationStats");

        var allNotifications = await _notificationService.GetAllAsync(cancellationToken);
        var notificationList = allNotifications.ToList();

        var stats = new NotificationStatsResponse(
            Total: notificationList.Count,
            Pending: notificationList.Count(n => n.Status == "Pending"),
            Sent: notificationList.Count(n => n.Status == "Sent"),
            Failed: notificationList.Count(n => n.Status == "Failed")
        );

        return stats;
    }

    /// <summary>
    /// 新しい通知を作成します。新しい通知を作成して送信キューに追加します。通知は自動的に処理されます。
    /// </summary>
    /// <param name="recipientEmail">受信者のメールアドレス</param>
    /// <param name="recipientName">受信者の名前</param>
    /// <param name="subject">通知の件名</param>
    /// <param name="message">通知メッセージの本文</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    [McpServerTool]
    public async Task<NotificationDetailResponse> CreateNotificationAsync(
        string recipientEmail,
        string recipientName,
        string subject,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "MCP Tool: CreateNotification - RecipientEmail: {Email}, Subject: {Subject}",
            recipientEmail, subject);

        // 入力検証
        ValidateNotificationInput(recipientEmail, recipientName, subject, message);

        var request = new CreateNotificationRequest
        {
            RecipientEmail = recipientEmail.Trim(),
            RecipientName = recipientName.Trim(),
            Subject = subject.Trim(),
            Message = message.Trim()
        };

        var notification = await _notificationService.CreateAsync(request, cancellationToken);

        _logger.LogInformation(
            "MCP Tool: CreateNotification - Successfully created notification {NotificationId}",
            notification.Id);

        return new NotificationDetailResponse(
            Id: notification.Id,
            RecipientEmail: notification.RecipientEmail,
            RecipientName: notification.RecipientName,
            NotificationType: notification.NotificationType,
            Subject: notification.Subject,
            Message: notification.Message,
            Status: notification.Status,
            ErrorMessage: notification.ErrorMessage,
            RetryCount: notification.RetryCount,
            CreatedAt: notification.CreatedAt,
            SentAt: notification.SentAt
        );
    }

    /// <summary>
    /// 受信者別の通知一覧を取得します。指定したメールアドレスの受信者に送信された通知の一覧を取得します。
    /// </summary>
    /// <param name="recipientEmail">受信者のメールアドレス</param>
    /// <param name="limit">取得する通知の最大件数（デフォルト: 50）</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    [McpServerTool]
    public async Task<NotificationListResponse> GetNotificationsByRecipientAsync(
        string recipientEmail,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "MCP Tool: GetNotificationsByRecipient - Email: {Email}, Limit: {Limit}",
            recipientEmail, limit);

        ArgumentNullException.ThrowIfNull(recipientEmail);

        var allNotifications = await _notificationService.GetAllAsync(cancellationToken);
        var filteredNotifications = allNotifications
            .Where(n => n.RecipientEmail.Equals(recipientEmail, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToList();

        return new NotificationListResponse(
            Notifications: filteredNotifications,
            TotalCount: filteredNotifications.Count
        );
    }

    /// <summary>
    /// 通知タイプ別の通知一覧を取得します。指定したタイプ（EmployeeCreated、EmployeeUpdated等）の通知一覧を取得します。
    /// </summary>
    /// <param name="notificationType">通知タイプ（例: EmployeeCreated, EmployeeUpdated, EmployeeDeleted, Manual）</param>
    /// <param name="limit">取得する通知の最大件数（デフォルト: 50）</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    [McpServerTool]
    public async Task<NotificationListResponse> GetNotificationsByTypeAsync(
        string notificationType,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "MCP Tool: GetNotificationsByType - Type: {Type}, Limit: {Limit}",
            notificationType, limit);

        ArgumentNullException.ThrowIfNull(notificationType);

        var allNotifications = await _notificationService.GetAllAsync(cancellationToken);
        var filteredNotifications = allNotifications
            .Where(n => n.NotificationType.Equals(notificationType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToList();

        return new NotificationListResponse(
            Notifications: filteredNotifications,
            TotalCount: filteredNotifications.Count
        );
    }

    /// <summary>
    /// 失敗した通知の一覧を取得します。送信に失敗した通知の一覧を取得します。エラーメッセージと再試行回数が含まれます。
    /// </summary>
    /// <param name="limit">取得する通知の最大件数（デフォルト: 50）</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    [McpServerTool]
    public async Task<NotificationListResponse> GetFailedNotificationsAsync(
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MCP Tool: GetFailedNotifications - Limit: {Limit}", limit);

        var allNotifications = await _notificationService.GetAllAsync(cancellationToken);
        var failedNotifications = allNotifications
            .Where(n => n.Status == "Failed")
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToList();

        return new NotificationListResponse(
            Notifications: failedNotifications,
            TotalCount: failedNotifications.Count
        );
    }

    // プライベートヘルパーメソッド

    private static void ValidateNotificationInput(
        string recipientEmail,
        string recipientName,
        string subject,
        string message)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            throw new ArgumentException("受信者のメールアドレスを入力してください。", nameof(recipientEmail));
        }

        if (!IsValidEmail(recipientEmail))
        {
            throw new ArgumentException("有効なメールアドレスを入力してください。", nameof(recipientEmail));
        }

        if (string.IsNullOrWhiteSpace(recipientName))
        {
            throw new ArgumentException("受信者の名前を入力してください。", nameof(recipientName));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("件名を入力してください。", nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("メッセージを入力してください。", nameof(message));
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

// レスポンスDTO定義

/// <summary>
/// 通知一覧レスポンス
/// </summary>
public record NotificationListResponse(
    IReadOnlyList<NotificationDto> Notifications,
    int TotalCount
);

/// <summary>
/// 通知詳細レスポンス
/// </summary>
public record NotificationDetailResponse(
    Guid Id,
    string RecipientEmail,
    string RecipientName,
    string NotificationType,
    string Subject,
    string Message,
    string Status,
    string? ErrorMessage,
    int RetryCount,
    DateTime CreatedAt,
    DateTime? SentAt
);

/// <summary>
/// 通知統計レスポンス
/// </summary>
public record NotificationStatsResponse(
    int Total,
    int Pending,
    int Sent,
    int Failed
);
