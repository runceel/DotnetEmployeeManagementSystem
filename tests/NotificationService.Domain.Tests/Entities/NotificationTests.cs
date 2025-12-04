using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Tests.Entities;

public class NotificationTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateNotification()
    {
        // Arrange
        var recipientEmail = "test@example.com";
        var recipientName = "山田 太郎";
        var notificationType = NotificationType.EmployeeCreated;
        var subject = "新規従業員登録通知";
        var message = "従業員が登録されました";

        // Act
        var notification = new Notification(recipientEmail, recipientName, notificationType, subject, message);

        // Assert
        Assert.NotEqual(Guid.Empty, notification.Id);
        Assert.Equal(recipientEmail, notification.RecipientEmail);
        Assert.Equal(recipientName, notification.RecipientName);
        Assert.Equal(notificationType, notification.NotificationType);
        Assert.Equal(subject, notification.Subject);
        Assert.Equal(message, notification.Message);
        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.Equal(0, notification.RetryCount);
        Assert.Null(notification.ErrorMessage);
        Assert.Null(notification.SentAt);
        Assert.True(notification.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithNullRecipientEmail_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Notification(null!, "山田 太郎", NotificationType.EmployeeCreated, "件名", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithNullRecipientName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Notification("test@example.com", null!, NotificationType.EmployeeCreated, "件名", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithNullNotificationType_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Notification("test@example.com", "山田 太郎", null!, "件名", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithNullSubject_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeCreated, null!, "メッセージ"));
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeCreated, "件名", null!));
    }

    [Fact]
    public void Constructor_WithEmptyRecipientEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Notification("", "山田 太郎", NotificationType.EmployeeCreated, "件名", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithWhitespaceRecipientEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Notification("   ", "山田 太郎", NotificationType.EmployeeCreated, "件名", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Notification("invalid-email", "山田 太郎", NotificationType.EmployeeCreated, "件名", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithEmptyRecipientName_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Notification("test@example.com", "", NotificationType.EmployeeCreated, "件名", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithWhitespaceRecipientName_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Notification("test@example.com", "   ", NotificationType.EmployeeCreated, "件名", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithEmptyNotificationType_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Notification("test@example.com", "山田 太郎", "", "件名", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithWhitespaceNotificationType_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Notification("test@example.com", "山田 太郎", "   ", "件名", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithEmptySubject_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeCreated, "", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithWhitespaceSubject_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeCreated, "   ", "メッセージ"));
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeCreated, "件名", ""));
    }

    [Fact]
    public void Constructor_WithWhitespaceMessage_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeCreated, "件名", "   "));
    }

    [Fact]
    public void MarkAsSent_ShouldUpdateStatusAndSentAt()
    {
        // Arrange
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeCreated, "件名", "メッセージ");

        // Act
        notification.MarkAsSent();

        // Assert
        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.NotNull(notification.SentAt);
        Assert.True(notification.SentAt <= DateTime.UtcNow);
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatusAndErrorMessage()
    {
        // Arrange
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeCreated, "件名", "メッセージ");
        var errorMessage = "メール送信に失敗しました";

        // Act
        notification.MarkAsFailed(errorMessage);

        // Assert
        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal(errorMessage, notification.ErrorMessage);
        Assert.Equal(1, notification.RetryCount);
    }

    [Fact]
    public void MarkAsFailed_CalledMultipleTimes_ShouldIncrementRetryCount()
    {
        // Arrange
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeCreated, "件名", "メッセージ");

        // Act
        notification.MarkAsFailed("エラー1");
        notification.MarkAsFailed("エラー2");
        notification.MarkAsFailed("エラー3");

        // Assert
        Assert.Equal(3, notification.RetryCount);
        Assert.Equal("エラー3", notification.ErrorMessage);
    }

    [Fact]
    public void ResetForRetry_ShouldResetStatusAndErrorMessage()
    {
        // Arrange
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeCreated, "件名", "メッセージ");
        notification.MarkAsFailed("エラー");

        // Act
        notification.ResetForRetry();

        // Assert
        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.Null(notification.ErrorMessage);
        Assert.Equal(1, notification.RetryCount); // RetryCount is preserved
    }

    [Fact]
    public void Constructor_WithEmployeeUpdatedType_ShouldCreateNotification()
    {
        // Arrange & Act
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeUpdated, "件名", "メッセージ");

        // Assert
        Assert.Equal(NotificationType.EmployeeUpdated, notification.NotificationType);
    }

    [Fact]
    public void Constructor_WithEmployeeDeletedType_ShouldCreateNotification()
    {
        // Arrange & Act
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.EmployeeDeleted, "件名", "メッセージ");

        // Assert
        Assert.Equal(NotificationType.EmployeeDeleted, notification.NotificationType);
    }

    [Fact]
    public void Constructor_WithManualType_ShouldCreateNotification()
    {
        // Arrange & Act
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.Manual, "件名", "メッセージ");

        // Assert
        Assert.Equal(NotificationType.Manual, notification.NotificationType);
    }

    [Fact]
    public void Constructor_WithLateArrivalType_ShouldCreateNotification()
    {
        // Arrange & Act
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.LateArrival, "遅刻通知", "遅刻が検出されました");

        // Assert
        Assert.Equal(NotificationType.LateArrival, notification.NotificationType);
    }

    [Fact]
    public void Constructor_WithEarlyLeavingType_ShouldCreateNotification()
    {
        // Arrange & Act
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.EarlyLeaving, "早退通知", "早退が検出されました");

        // Assert
        Assert.Equal(NotificationType.EarlyLeaving, notification.NotificationType);
    }

    [Fact]
    public void Constructor_WithOvertimeType_ShouldCreateNotification()
    {
        // Arrange & Act
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.Overtime, "残業通知", "残業が検出されました");

        // Assert
        Assert.Equal(NotificationType.Overtime, notification.NotificationType);
    }

    [Fact]
    public void Constructor_WithLeaveRequestCreatedType_ShouldCreateNotification()
    {
        // Arrange & Act
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.LeaveRequestCreated, "休暇申請通知", "休暇申請が作成されました");

        // Assert
        Assert.Equal(NotificationType.LeaveRequestCreated, notification.NotificationType);
    }

    [Fact]
    public void Constructor_WithLeaveRequestApprovedType_ShouldCreateNotification()
    {
        // Arrange & Act
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.LeaveRequestApproved, "休暇承認通知", "休暇申請が承認されました");

        // Assert
        Assert.Equal(NotificationType.LeaveRequestApproved, notification.NotificationType);
    }

    [Fact]
    public void Constructor_WithLeaveRequestRejectedType_ShouldCreateNotification()
    {
        // Arrange & Act
        var notification = new Notification("test@example.com", "山田 太郎", NotificationType.LeaveRequestRejected, "休暇却下通知", "休暇申請が却下されました");

        // Assert
        Assert.Equal(NotificationType.LeaveRequestRejected, notification.NotificationType);
    }
}
