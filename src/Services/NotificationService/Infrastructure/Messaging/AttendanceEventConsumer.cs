using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;
using Shared.Contracts.Events;
using StackExchange.Redis;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// 勤怠イベントコンシューマー
/// </summary>
public class AttendanceEventConsumer : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AttendanceEventConsumer> _logger;

    public AttendanceEventConsumer(
        IConnectionMultiplexer redis,
        IServiceProvider serviceProvider,
        ILogger<AttendanceEventConsumer> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("勤怠イベントコンシューマーを開始しました");

        var subscriber = _redis.GetSubscriber();

        // 遅刻検知イベント
        await subscriber.SubscribeAsync(RedisChannel.Literal("attendance:late-arrival"), async (channel, message) =>
        {
            await HandleLateArrivalAsync(message!, stoppingToken);
        });

        // 早退検知イベント
        await subscriber.SubscribeAsync(RedisChannel.Literal("attendance:early-leaving"), async (channel, message) =>
        {
            await HandleEarlyLeavingAsync(message!, stoppingToken);
        });

        // 長時間労働検知イベント
        await subscriber.SubscribeAsync(RedisChannel.Literal("attendance:overtime"), async (channel, message) =>
        {
            await HandleOvertimeAsync(message!, stoppingToken);
        });

        // 休暇申請作成イベント
        await subscriber.SubscribeAsync(RedisChannel.Literal("leaverequest:created"), async (channel, message) =>
        {
            await HandleLeaveRequestCreatedAsync(message!, stoppingToken);
        });

        // 休暇申請承認イベント
        await subscriber.SubscribeAsync(RedisChannel.Literal("leaverequest:approved"), async (channel, message) =>
        {
            await HandleLeaveRequestApprovedAsync(message!, stoppingToken);
        });

        // 休暇申請却下イベント
        await subscriber.SubscribeAsync(RedisChannel.Literal("leaverequest:rejected"), async (channel, message) =>
        {
            await HandleLeaveRequestRejectedAsync(message!, stoppingToken);
        });

        _logger.LogInformation("勤怠イベントチャネルに登録しました");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleLateArrivalAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var eventData = JsonSerializer.Deserialize<LateArrivalDetectedEvent>(message);
            if (eventData == null) return;

            _logger.LogInformation("遅刻検知イベントを受信しました: EmployeeId={EmployeeId}, LateMinutes={LateMinutes}",
                eventData.EmployeeId, eventData.LateMinutes);

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            // TODO: EmployeeServiceから従業員情報を取得して名前とメールアドレスを設定
            // 現在は仮の値を使用
            var notification = new Notification(
                "employee@example.com", // 実際にはEmployeeServiceから取得
                "従業員", // 実際にはEmployeeServiceから取得
                NotificationType.LateArrival,
                "【重要】遅刻が記録されました",
                $"""
                従業員 様
                
                {eventData.WorkDate:yyyy年MM月dd日}の勤務において、遅刻が記録されました。
                
                【詳細】
                - 出勤時刻: {eventData.CheckInTime:HH:mm}
                - 遅刻時間: {eventData.LateMinutes}分
                
                頻繁な遅刻は勤務評価に影響する可能性があります。
                今後は定刻での出勤をお願いいたします。
                
                やむを得ない事情がある場合は、上長にご相談ください。
                
                従業員管理システム
                """
            );

            await repository.AddAsync(notification, cancellationToken);
            _logger.LogInformation("遅刻通知を作成しました: NotificationId={NotificationId}", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "遅刻検知イベントの処理中にエラーが発生しました");
        }
    }

    private async Task HandleEarlyLeavingAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var eventData = JsonSerializer.Deserialize<EarlyLeavingDetectedEvent>(message);
            if (eventData == null) return;

            _logger.LogInformation("早退検知イベントを受信しました: EmployeeId={EmployeeId}, WorkHours={WorkHours}",
                eventData.EmployeeId, eventData.WorkHours);

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var notification = new Notification(
                "employee@example.com",
                "従業員",
                NotificationType.EarlyLeaving,
                "【重要】早退が記録されました",
                $"""
                従業員 様
                
                {eventData.WorkDate:yyyy年MM月dd日}の勤務において、早退が記録されました。
                
                【詳細】
                - 出勤時刻: {eventData.CheckInTime:HH:mm}
                - 退勤時刻: {eventData.CheckOutTime:HH:mm}
                - 勤務時間: {eventData.WorkHours:F1}時間
                
                体調不良などやむを得ない事情がある場合は、
                速やかに上長に報告してください。
                
                従業員管理システム
                """
            );

            await repository.AddAsync(notification, cancellationToken);
            _logger.LogInformation("早退通知を作成しました: NotificationId={NotificationId}", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "早退検知イベントの処理中にエラーが発生しました");
        }
    }

    private async Task HandleOvertimeAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var eventData = JsonSerializer.Deserialize<OvertimeDetectedEvent>(message);
            if (eventData == null) return;

            _logger.LogInformation("長時間労働検知イベントを受信しました: EmployeeId={EmployeeId}, WorkHours={WorkHours}, OvertimeHours={OvertimeHours}",
                eventData.EmployeeId, eventData.WorkHours, eventData.OvertimeHours);

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var notification = new Notification(
                "employee@example.com",
                "従業員",
                NotificationType.Overtime,
                "【注意】長時間労働が検知されました",
                $"""
                従業員 様
                
                {eventData.WorkDate:yyyy年MM月dd日}の勤務において、長時間労働が検知されました。
                
                【詳細】
                - 出勤時刻: {eventData.CheckInTime:HH:mm}
                - 退勤時刻: {eventData.CheckOutTime:HH:mm}
                - 総勤務時間: {eventData.WorkHours:F1}時間
                - 超過時間: {eventData.OvertimeHours:F1}時間
                
                長時間労働は健康に悪影響を及ぼす可能性があります。
                適切な休憩と十分な休息を取るようお願いいたします。
                
                業務量が過多の場合は、上長に相談してください。
                
                従業員管理システム
                """
            );

            await repository.AddAsync(notification, cancellationToken);
            _logger.LogInformation("長時間労働通知を作成しました: NotificationId={NotificationId}", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "長時間労働検知イベントの処理中にエラーが発生しました");
        }
    }

    private async Task HandleLeaveRequestCreatedAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var eventData = JsonSerializer.Deserialize<LeaveRequestCreatedEvent>(message);
            if (eventData == null) return;

            _logger.LogInformation("休暇申請作成イベントを受信しました: LeaveRequestId={LeaveRequestId}",
                eventData.LeaveRequestId);

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var notification = new Notification(
                "employee@example.com",
                "従業員",
                NotificationType.LeaveRequestCreated,
                "休暇申請を受け付けました",
                $"""
                従業員 様
                
                休暇申請を受け付けました。
                承認をお待ちください。
                
                【申請内容】
                - 休暇種別: {eventData.Type}
                - 期間: {eventData.StartDate:yyyy年MM月dd日} ～ {eventData.EndDate:yyyy年MM月dd日}
                - 理由: {eventData.Reason}
                - 申請日時: {eventData.CreatedAt:yyyy年MM月dd日 HH:mm}
                
                承認結果は改めて通知いたします。
                
                従業員管理システム
                """
            );

            await repository.AddAsync(notification, cancellationToken);
            _logger.LogInformation("休暇申請作成通知を作成しました: NotificationId={NotificationId}", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "休暇申請作成イベントの処理中にエラーが発生しました");
        }
    }

    private async Task HandleLeaveRequestApprovedAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var eventData = JsonSerializer.Deserialize<LeaveRequestApprovedEvent>(message);
            if (eventData == null) return;

            _logger.LogInformation("休暇申請承認イベントを受信しました: LeaveRequestId={LeaveRequestId}",
                eventData.LeaveRequestId);

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var notification = new Notification(
                "employee@example.com",
                "従業員",
                NotificationType.LeaveRequestApproved,
                "休暇申請が承認されました",
                $"""
                従業員 様
                
                休暇申請が承認されました。
                
                【承認内容】
                - 休暇種別: {eventData.Type}
                - 期間: {eventData.StartDate:yyyy年MM月dd日} ～ {eventData.EndDate:yyyy年MM月dd日}
                - 承認日時: {eventData.ApprovedAt:yyyy年MM月dd日 HH:mm}
                {(string.IsNullOrEmpty(eventData.ApproverComment) ? "" : $"- 承認者コメント: {eventData.ApproverComment}")}
                
                良い休暇をお過ごしください。
                
                従業員管理システム
                """
            );

            await repository.AddAsync(notification, cancellationToken);
            _logger.LogInformation("休暇申請承認通知を作成しました: NotificationId={NotificationId}", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "休暇申請承認イベントの処理中にエラーが発生しました");
        }
    }

    private async Task HandleLeaveRequestRejectedAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var eventData = JsonSerializer.Deserialize<LeaveRequestRejectedEvent>(message);
            if (eventData == null) return;

            _logger.LogInformation("休暇申請却下イベントを受信しました: LeaveRequestId={LeaveRequestId}",
                eventData.LeaveRequestId);

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var notification = new Notification(
                "employee@example.com",
                "従業員",
                NotificationType.LeaveRequestRejected,
                "休暇申請が却下されました",
                $"""
                従業員 様
                
                休暇申請が却下されました。
                
                【却下内容】
                - 休暇種別: {eventData.Type}
                - 期間: {eventData.StartDate:yyyy年MM月dd日} ～ {eventData.EndDate:yyyy年MM月dd日}
                - 却下日時: {eventData.RejectedAt:yyyy年MM月dd日 HH:mm}
                {(string.IsNullOrEmpty(eventData.ApproverComment) ? "" : $"- 却下理由: {eventData.ApproverComment}")}
                
                詳細については上長にご確認ください。
                
                従業員管理システム
                """
            );

            await repository.AddAsync(notification, cancellationToken);
            _logger.LogInformation("休暇申請却下通知を作成しました: NotificationId={NotificationId}", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "休暇申請却下イベントの処理中にエラーが発生しました");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("勤怠イベントコンシューマーを停止しています");
        await base.StopAsync(cancellationToken);
    }
}
