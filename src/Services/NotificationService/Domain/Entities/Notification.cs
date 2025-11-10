namespace NotificationService.Domain.Entities;

/// <summary>
/// 通知エンティティ
/// </summary>
public class Notification
{
    /// <summary>
    /// 通知ID
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// 受信者のメールアドレス
    /// </summary>
    public string RecipientEmail { get; private set; }

    /// <summary>
    /// 受信者の名前
    /// </summary>
    public string RecipientName { get; private set; }

    /// <summary>
    /// 通知タイプ (EmployeeCreated, EmployeeUpdated, EmployeeDeleted, Manual)
    /// </summary>
    public string NotificationType { get; private set; }

    /// <summary>
    /// 件名
    /// </summary>
    public string Subject { get; private set; }

    /// <summary>
    /// メッセージ本文
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// 送信状態 (Pending, Sent, Failed)
    /// </summary>
    public string Status { get; private set; }

    /// <summary>
    /// エラーメッセージ（送信失敗時）
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// 送信試行回数
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 送信日時
    /// </summary>
    public DateTime? SentAt { get; private set; }

    private Notification()
    {
        RecipientEmail = string.Empty;
        RecipientName = string.Empty;
        NotificationType = string.Empty;
        Subject = string.Empty;
        Message = string.Empty;
        Status = string.Empty;
    }

    public Notification(string recipientEmail, string recipientName, string notificationType, string subject, string message)
    {
        ArgumentNullException.ThrowIfNull(recipientEmail);
        ArgumentNullException.ThrowIfNull(recipientName);
        ArgumentNullException.ThrowIfNull(notificationType);
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(message);

        Id = Guid.NewGuid();
        RecipientEmail = recipientEmail;
        RecipientName = recipientName;
        NotificationType = notificationType;
        Subject = subject;
        Message = message;
        Status = NotificationStatus.Pending;
        RetryCount = 0;
        CreatedAt = DateTime.UtcNow;

        ValidateNotification();
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
        RetryCount++;
    }

    public void ResetForRetry()
    {
        Status = NotificationStatus.Pending;
        ErrorMessage = null;
    }

    private void ValidateNotification()
    {
        if (string.IsNullOrWhiteSpace(RecipientEmail))
            throw new ArgumentException("受信者のメールアドレスを入力してください。", nameof(RecipientEmail));

        if (!IsValidEmail(RecipientEmail))
            throw new ArgumentException("有効なメールアドレスを入力してください。", nameof(RecipientEmail));

        if (string.IsNullOrWhiteSpace(RecipientName))
            throw new ArgumentException("受信者の名前を入力してください。", nameof(RecipientName));

        if (string.IsNullOrWhiteSpace(NotificationType))
            throw new ArgumentException("通知タイプを入力してください。", nameof(NotificationType));

        if (string.IsNullOrWhiteSpace(Subject))
            throw new ArgumentException("件名を入力してください。", nameof(Subject));

        if (string.IsNullOrWhiteSpace(Message))
            throw new ArgumentException("メッセージを入力してください。", nameof(Message));
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

/// <summary>
/// 通知状態の定数
/// </summary>
public static class NotificationStatus
{
    public const string Pending = "Pending";
    public const string Sent = "Sent";
    public const string Failed = "Failed";
}

/// <summary>
/// 通知タイプの定数
/// </summary>
public static class NotificationType
{
    public const string EmployeeCreated = "EmployeeCreated";
    public const string EmployeeUpdated = "EmployeeUpdated";
    public const string EmployeeDeleted = "EmployeeDeleted";
    public const string Manual = "Manual";
    
    // 勤怠異常通知
    public const string LateArrival = "LateArrival";
    public const string EarlyLeaving = "EarlyLeaving";
    public const string Overtime = "Overtime";
    
    // 休暇申請通知
    public const string LeaveRequestCreated = "LeaveRequestCreated";
    public const string LeaveRequestApproved = "LeaveRequestApproved";
    public const string LeaveRequestRejected = "LeaveRequestRejected";
}
