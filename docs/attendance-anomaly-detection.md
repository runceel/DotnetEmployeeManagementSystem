# 勤怠異常検知・通知連携機能

## 概要

勤怠異常検知機能は、従業員の勤怠記録に基づいて、遅刻・早退・長時間労働などの異常を自動的に検知し、NotificationServiceを通じて関係者に通知を送信する機能です。

## 主な機能

### 1. 遅刻検知
- **検知条件**: 出勤時刻が9:00を超過している場合
- **検知タイミング**: 出勤打刻時（CheckIn）
- **通知内容**: 遅刻時刻、遅刻時間（分単位）
- **通知対象**: 該当従業員（将来的には上長も対象に）

### 2. 早退検知
- **検知条件**: 
  - 退勤時刻が17:00より前
  - かつ、勤務時間が4時間以上
- **検知タイミング**: 退勤打刻時（CheckOut）
- **通知内容**: 出勤時刻、退勤時刻、勤務時間
- **通知対象**: 該当従業員

### 3. 長時間労働検知
- **検知条件**: 勤務時間が10時間以上
- **検知タイミング**: 退勤打刻時（CheckOut）
- **通知内容**: 出勤時刻、退勤時刻、総勤務時間、超過時間
- **通知対象**: 該当従業員（将来的には人事部門も対象に）

### 4. 休暇申請通知
- **作成通知**: 休暇申請が作成されたとき
- **承認通知**: 休暇申請が承認されたとき
- **却下通知**: 休暇申請が却下されたとき

## アーキテクチャ

### システム構成図

```
[AttendanceService]
    ├─ CheckIn/CheckOut API
    ├─ AttendanceService (Application)
    │   └─ AttendanceAnomalyDetector (Domain)
    └─ RedisEventPublisher (Infrastructure)
          ↓ (Redis Pub/Sub)
[Redis]
    ├─ attendance:late-arrival
    ├─ attendance:early-leaving
    ├─ attendance:overtime
    ├─ leaverequest:created
    ├─ leaverequest:approved
    └─ leaverequest:rejected
          ↓ (Subscribe)
[NotificationService]
    ├─ AttendanceEventConsumer
    └─ Notification DB
```

### イベントフロー

#### 遅刻検知フロー
1. 従業員が9:30に出勤打刻
2. AttendanceServiceがCheckInAsync()を実行
3. AttendanceAnomalyDetectorが遅刻を検知（30分遅刻）
4. LateArrivalDetectedEventをRedisに発行
5. NotificationServiceのAttendanceEventConsumerがイベント受信
6. 遅刻通知をNotificationDBに作成（Status: Pending）
7. NotificationProcessorWorkerが通知を処理・送信（Status: Sent）

#### 長時間労働検知フロー
1. 従業員が9:00に出勤、21:00に退勤打刻（12時間勤務）
2. AttendanceServiceがCheckOutAsync()を実行
3. AttendanceAnomalyDetectorが長時間労働を検知（超過4時間）
4. OvertimeDetectedEventをRedisに発行
5. NotificationServiceが長時間労働通知を作成・送信

## 実装詳細

### 1. Domain層: 異常検知ロジック

#### AttendanceAnomalyDetector
```csharp
public class AttendanceAnomalyDetector : IAttendanceAnomalyDetector
{
    // 標準開始時刻: 9:00
    private static readonly TimeSpan StandardStartTime = new(9, 0, 0);
    
    // 標準終了時刻: 17:00
    private static readonly TimeSpan StandardEndTime = new(17, 0, 0);
    
    // 最小勤務時間（早退判定用）: 4時間
    private const double MinimumWorkHours = 4.0;
    
    // 標準勤務時間: 8時間
    private const double StandardWorkHours = 8.0;
    
    // 長時間労働の閾値: 10時間
    private const double OvertimeThreshold = 10.0;
    
    // 遅刻判定、早退判定、長時間労働判定メソッド
}
```

**設計ポイント**:
- ハードコードされた設定値（将来的には設定ファイルやDBから取得可能に）
- 純粋なドメインロジック（外部依存なし）
- テスタビリティ重視

### 2. Application層: 異常検知の統合

#### AttendanceService
```csharp
public async Task<Attendance> CheckInAsync(Guid employeeId, DateTime checkInTime, ...)
{
    // 出勤記録
    // ... 

    // イベント発行
    await _eventPublisher.PublishAsync("attendance:checkin", new CheckInRecordedEvent { ... });

    // 遅刻検知
    if (_anomalyDetector.IsLateArrival(checkInTime))
    {
        var lateMinutes = _anomalyDetector.CalculateLateMinutes(checkInTime);
        await _eventPublisher.PublishAsync("attendance:late-arrival", new LateArrivalDetectedEvent
        {
            AttendanceId = result.Id,
            EmployeeId = result.EmployeeId,
            CheckInTime = checkInTime,
            WorkDate = workDate,
            LateMinutes = lateMinutes
        });
    }

    return result;
}
```

### 3. Infrastructure層: イベント発行

#### RedisEventPublisher
既存の実装を使用し、Redisチャネルにイベントを発行します。

### 4. NotificationService: イベント購読と通知作成

#### AttendanceEventConsumer
```csharp
public class AttendanceEventConsumer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();

        // 遅刻検知イベント購読
        await subscriber.SubscribeAsync(
            RedisChannel.Literal("attendance:late-arrival"), 
            async (channel, message) => {
                await HandleLateArrivalAsync(message!, stoppingToken);
            });

        // 早退・長時間労働・休暇申請イベント購読
        // ...
    }

    private async Task HandleLateArrivalAsync(string message, CancellationToken ct)
    {
        var eventData = JsonSerializer.Deserialize<LateArrivalDetectedEvent>(message);
        
        var notification = new Notification(
            "employee@example.com", // TODO: EmployeeServiceから取得
            "従業員",
            NotificationType.LateArrival,
            "【重要】遅刻が記録されました",
            $"""
            {eventData.WorkDate:yyyy年MM月dd日}の勤務において、遅刻が記録されました。
            
            【詳細】
            - 出勤時刻: {eventData.CheckInTime:HH:mm}
            - 遅刻時間: {eventData.LateMinutes}分
            
            頻繁な遅刻は勤務評価に影響する可能性があります。
            今後は定刻での出勤をお願いいたします。
            """
        );

        await _repository.AddAsync(notification, ct);
    }
}
```

## イベント契約

### 1. LateArrivalDetectedEvent（遅刻検知）
```csharp
public record LateArrivalDetectedEvent
{
    public Guid AttendanceId { get; init; }
    public Guid EmployeeId { get; init; }
    public DateTime CheckInTime { get; init; }
    public DateTime WorkDate { get; init; }
    public int LateMinutes { get; init; }
}
```

### 2. EarlyLeavingDetectedEvent（早退検知）
```csharp
public record EarlyLeavingDetectedEvent
{
    public Guid AttendanceId { get; init; }
    public Guid EmployeeId { get; init; }
    public DateTime CheckInTime { get; init; }
    public DateTime CheckOutTime { get; init; }
    public DateTime WorkDate { get; init; }
    public double WorkHours { get; init; }
}
```

### 3. OvertimeDetectedEvent（長時間労働検知）
```csharp
public record OvertimeDetectedEvent
{
    public Guid AttendanceId { get; init; }
    public Guid EmployeeId { get; init; }
    public DateTime CheckInTime { get; init; }
    public DateTime CheckOutTime { get; init; }
    public DateTime WorkDate { get; init; }
    public double WorkHours { get; init; }
    public double OvertimeHours { get; init; }
}
```

## テスト戦略

### 1. Domain層のユニットテスト
**テスト対象**: AttendanceAnomalyDetector

**テストカバレッジ**: 22テストケース
- 遅刻判定（4ケース）
- 早退判定（6ケース）
- 長時間労働判定（4ケース）
- 遅刻時間計算（4ケース）
- 超過労働時間計算（4ケース）

**テスト例**:
```csharp
[Fact]
public void IsLateArrival_WhenCheckInAt0901_ReturnsTrue()
{
    // Arrange
    var checkInTime = new DateTime(2025, 1, 15, 9, 1, 0);
    var detector = new AttendanceAnomalyDetector();

    // Act
    var result = detector.IsLateArrival(checkInTime);

    // Assert
    Assert.True(result);
}
```

### 2. Application層の統合テスト
**テスト対象**: AttendanceService + AttendanceAnomalyDetector

**テストカバレッジ**: 11テストケース
- 遅刻時のイベント発行（2ケース）
- 早退時のイベント発行（2ケース）
- 長時間労働時のイベント発行（2ケース）
- 複数異常の組み合わせ（5ケース）

**テスト例**:
```csharp
[Fact]
public async Task CheckInAsync_WhenLateArrival_ShouldPublishLateArrivalEvent()
{
    // Arrange
    var employeeId = Guid.NewGuid();
    var checkInTime = new DateTime(2024, 1, 15, 9, 30, 0);
    
    _mockAnomalyDetector.Setup(d => d.IsLateArrival(checkInTime)).Returns(true);
    _mockAnomalyDetector.Setup(d => d.CalculateLateMinutes(checkInTime)).Returns(30);

    // Act
    await _service.CheckInAsync(employeeId, checkInTime);

    // Assert
    _mockEventPublisher.Verify(
        e => e.PublishAsync("attendance:late-arrival", 
            It.Is<LateArrivalDetectedEvent>(evt => evt.LateMinutes == 30),
            It.IsAny<CancellationToken>()),
        Times.Once);
}
```

## 使用方法

### 開発環境での起動

```bash
# Aspire AppHost経由で全サービス起動
dotnet run --project src/AppHost

# Aspireダッシュボードが自動的に開きます
# - AttendanceService API
# - NotificationService API
# - Redis
# - BlazorWeb UI
```

### 動作確認手順

1. **出勤打刻（遅刻）のテスト**
   ```bash
   # 9:30に出勤打刻
   curl -X POST http://localhost:5003/api/attendances/checkin \
     -H "Content-Type: application/json" \
     -d '{
       "employeeId": "your-employee-id",
       "checkInTime": "2025-01-15T09:30:00Z"
     }'
   ```

2. **NotificationServiceのログ確認**
   - Aspireダッシュボードで NotificationService のログを表示
   - 遅刻検知イベントの受信ログを確認
   - 通知作成のログを確認

3. **通知の確認**
   ```bash
   # 通知一覧を取得
   curl http://localhost:5004/api/notifications
   ```

4. **退勤打刻（長時間労働）のテスト**
   ```bash
   # 21:00に退勤打刻（12時間勤務）
   curl -X POST http://localhost:5003/api/attendances/checkout \
     -H "Content-Type: application/json" \
     -d '{
       "employeeId": "your-employee-id",
       "checkOutTime": "2025-01-15T21:00:00Z"
     }'
   ```

### Redisイベントの確認

```bash
# Redisに接続
docker exec -it redis redis-cli

# チャネル購読
SUBSCRIBE attendance:late-arrival
SUBSCRIBE attendance:early-leaving
SUBSCRIBE attendance:overtime
```

## 設定のカスタマイズ

### 異常検知基準の変更

現在はハードコードされていますが、将来的には以下のように設定可能にする予定：

```json
// appsettings.json
{
  "AttendanceAnomalyDetection": {
    "StandardStartTime": "09:00:00",
    "StandardEndTime": "17:00:00",
    "StandardWorkHours": 8.0,
    "OvertimeThreshold": 10.0,
    "MinimumWorkHours": 4.0
  }
}
```

```csharp
// 設定から読み込む例
public class AttendanceAnomalyDetector : IAttendanceAnomalyDetector
{
    private readonly AttendanceAnomalySettings _settings;

    public AttendanceAnomalyDetector(IOptions<AttendanceAnomalySettings> settings)
    {
        _settings = settings.Value;
    }
}
```

### 通知テンプレートのカスタマイズ

AttendanceEventConsumerの各ハンドラーメソッドで通知メッセージをカスタマイズ可能：

```csharp
var notification = new Notification(
    recipientEmail,
    recipientName,
    NotificationType.LateArrival,
    "【カスタムタイトル】遅刻通知",
    $"""
    カスタムメッセージテンプレート
    - 従業員: {employeeName}
    - 遅刻時間: {lateMinutes}分
    """
);
```

## パフォーマンス考慮事項

### イベント発行の非同期処理
- イベント発行は非同期で行われるため、CheckIn/CheckOut APIのレスポンスに影響なし
- Redisは高速なメッセージングを提供

### 通知処理のバッチ化
- NotificationProcessorWorkerは10秒ごとにPending通知をバッチ処理
- 大量の通知が発生しても処理が追いつく設計

### データベース負荷
- 通知履歴はSQLiteに保存（開発環境）
- 本番環境ではPostgreSQLなどのスケーラブルなDBを推奨

## セキュリティ考慮事項

### イベントデータの検証
- イベントデシリアライズ時のnullチェック
- 不正なデータによる例外をキャッチしてログ出力

### 通知対象の制限
- 現在は仮の通知先（"employee@example.com"）を使用
- 実装時にはEmployeeServiceから実際のメールアドレスを取得
- 通知対象の権限チェックが必要

## トラブルシューティング

### 問題: 遅刻検知されない
**原因**: AttendanceAnomalyDetectorがDI登録されていない  
**解決方法**: 
```csharp
// AttendanceService.Infrastructure/DependencyInjection.cs
services.AddScoped<IAttendanceAnomalyDetector, AttendanceAnomalyDetector>();
```

### 問題: 通知が作成されない
**原因**: AttendanceEventConsumerが起動していない  
**解決方法**:
```csharp
// NotificationService.API/Program.cs
builder.Services.AddHostedService<AttendanceEventConsumer>();
```

### 問題: Redisイベントが届かない
**原因**: チャネル名の不一致  
**解決方法**: イベント発行側と購読側のチャネル名を確認
```csharp
// 発行側: "attendance:late-arrival"
// 購読側: RedisChannel.Literal("attendance:late-arrival")
```

### デバッグ用ログ
```csharp
// AttendanceEventConsumer.cs
_logger.LogInformation("遅刻検知イベントを受信しました: EmployeeId={EmployeeId}, LateMinutes={LateMinutes}",
    eventData.EmployeeId, eventData.LateMinutes);
```

## 今後の改善予定

### Phase 1: 機能拡張
- [ ] 欠勤検知機能の追加
- [ ] 通知対象の拡張（上長、人事部門）
- [ ] 異常判定基準の設定UI
- [ ] 通知テンプレートの外部化

### Phase 2: 高度な分析
- [ ] 異常パターンの統計分析
- [ ] 繰り返し異常の検知
- [ ] 予測アラート（遅刻傾向がある従業員への事前通知）

### Phase 3: スケーラビリティ向上
- [ ] イベントストリーミング（Apache Kafka等）への移行
- [ ] 通知配信のリトライ機能強化
- [ ] 通知優先度の実装

## 関連ドキュメント

- [勤怠管理サービス](./attendance-service.md) - AttendanceServiceの全体像
- [通知サービス](./notification-service.md) - NotificationServiceの実装詳細
- [アーキテクチャ概要](./architecture.md) - システム全体のアーキテクチャ
- [開発ガイド](./development-guide.md) - 開発手順とベストプラクティス
- [Aspireダッシュボード](./aspire-dashboard.md) - 監視とデバッグ

## まとめ

勤怠異常検知・通知連携機能は、イベント駆動アーキテクチャとクリーンアーキテクチャの原則に従い、保守性とテスタビリティを重視して実装されています。この機能により、勤怠管理の自動化と従業員の健康管理が促進されます。
