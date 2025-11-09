# 通知サービス実装ガイド

## 📖 目次

1. [概要](#概要)
2. [アーキテクチャ](#アーキテクチャ)
3. [セットアップ](#セットアップ)
4. [使用方法](#使用方法)
5. [実装詳細](#実装詳細)
6. [カスタマイズ](#カスタマイズ)
7. [トラブルシューティング](#トラブルシューティング)

## 概要

通知サービス（NotificationService）は、従業員管理システムにリアルタイム通知機能を追加するマイクロサービスです。従業員の作成、更新、削除などのイベントに応じて、自動的にメール通知を送信します。

### 主な特徴

✅ **イベント駆動アーキテクチャ**: Redisを使用した軽量なPub/Subメッセージング
✅ **クリーンアーキテクチャ**: Domain駆動設計による保守性の高い実装
✅ **Aspire統合**: .NET Aspireによる簡単なローカル開発環境
✅ **通知履歴管理**: すべての通知を追跡可能
✅ **開発者フレンドリー**: コンソール出力による簡単なデバッグ

## アーキテクチャ

### システム構成図

```
┌─────────────────┐      ┌──────────────────┐      ┌─────────────────────┐
│  BlazorWeb      │─────▶│  EmployeeService │─────▶│  NotificationService│
│  (UI)           │      │  (API)           │      │  (API)              │
└─────────────────┘      └──────────────────┘      └─────────────────────┘
         │                        │                          │
         │                        │                          │
         │                        ▼                          ▼
         │                   ┌────────┐              ┌──────────────┐
         │                   │  Redis │◀─────────────│  Workers     │
         │                   │ Pub/Sub│              │  - Consumer  │
         │                   └────────┘              │  - Processor │
         │                                           └──────────────┘
         │                                                   │
         ▼                                                   ▼
┌──────────────────┐                              ┌──────────────────┐
│  NotificationAPI │                              │  SQLite DB       │
│  (Direct Call)   │                              │  (notifications) │
└──────────────────┘                              └──────────────────┘
```

### イベントフロー

#### 1. 従業員作成時の通知フロー

```
[EmployeeService]
    ├─ CreateAsync() が呼ばれる
    ├─ 従業員をDBに保存
    ├─ EmployeeCreatedEvent を作成
    └─ Redis の "employee.created" チャネルに発行
        ↓
[Redis Pub/Sub]
    └─ "employee.created" チャネルにメッセージ配信
        ↓
[NotificationService - EmployeeEventConsumer]
    ├─ イベントを受信
    ├─ Notification エンティティを作成
    │   - Type: EmployeeCreated
    │   - Status: Pending
    │   - Subject: "ようこそ！従業員登録が完了しました"
    └─ DBに保存
        ↓
[NotificationService - NotificationProcessorWorker]
    ├─ 10秒ごとに Pending 通知をチェック
    ├─ IEmailService.SendEmailAsync() を呼び出し
    ├─ 成功時: Status を Sent に更新
    └─ 失敗時: Status を Failed に更新、RetryCount をインクリメント
```

### データフロー

```
┌─────────────────────┐
│  Employee CRUD      │
│  - Create           │
│  - Update           │
│  - Delete           │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  Event Publishing   │
│  - EmployeeCreated  │
│  - EmployeeUpdated  │
│  - EmployeeDeleted  │
└──────────┬──────────┘
           │
           ▼
    ┌──────────┐
    │  Redis   │
    │  Pub/Sub │
    └─────┬────┘
          │
          ▼
┌─────────────────────┐
│  Event Consumer     │
│  - Subscribe        │
│  - Deserialize      │
│  - Create Notif.    │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  Notification DB    │
│  - Pending Queue    │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  Processor Worker   │
│  - Fetch Pending    │
│  - Send Email       │
│  - Update Status    │
└─────────────────────┘
```

## セットアップ

### 前提条件

- .NET 9 SDK
- .NET Aspire Workload

### プロジェクト構成

システムには以下のプロジェクトが追加されました：

```
src/Services/NotificationService/
├── Domain/                  # ドメインモデル
│   ├── Entities/
│   │   └── Notification.cs  # 通知エンティティ
│   └── Repositories/
│       └── INotificationRepository.cs
├── Application/             # ビジネスロジック
│   ├── UseCases/
│   │   ├── INotificationService.cs
│   │   └── NotificationService.cs
│   ├── Services/
│   │   └── IEmailService.cs
│   └── Mappings/
│       └── NotificationMappings.cs
├── Infrastructure/          # インフラストラクチャ
│   ├── Data/
│   │   └── NotificationDbContext.cs
│   ├── Repositories/
│   │   └── NotificationRepository.cs
│   ├── Services/
│   │   └── ConsoleEmailService.cs
│   ├── Messaging/
│   │   ├── IEventPublisher.cs
│   │   ├── RedisEventPublisher.cs
│   │   └── EmployeeEventConsumer.cs
│   └── Workers/
│       └── NotificationProcessorWorker.cs
└── API/                     # Web API
    ├── Program.cs
    └── Endpoints/
        └── NotificationEndpoints.cs
```

### Aspire構成の変更

`src/AppHost/AppHost.cs` に以下が追加されました：

```csharp
// Redis for messaging
var redis = builder.AddRedis("redis");

// NotificationService database
var notificationDb = builder.AddSqlite("notificationdb");

// NotificationService API
var notificationServiceApi = builder.AddProject<Projects.NotificationService_API>("notificationservice-api")
    .WithReference(notificationDb)
    .WithReference(redis);
```

### 起動方法

```bash
# リポジトリのルートディレクトリで実行
cd DotnetEmployeeManagementSystem

# Aspire AppHost を起動（すべてのサービスを含む）
dotnet run --project src/AppHost

# ブラウザで自動的に Aspire Dashboard が開きます
# http://localhost:15003 (ポートは実行時に表示されます)
```

## 使用方法

### 1. Aspireダッシュボードでサービス確認

起動後、Aspireダッシュボードで以下のサービスが表示されます：

- **employeeservice-api**: 従業員管理API
- **notificationservice-api**: 通知サービスAPI
- **authservice-api**: 認証サービスAPI
- **blazorweb**: BlazorWebアプリ
- **redis**: Redisサーバー
- **employeedb / notificationdb / authdb**: SQLiteデータベース

### 2. BlazorWebで通知機能を使用

#### アクセス方法

1. ブラウザで BlazorWeb を開く（Aspireダッシュボードのリンクをクリック）
2. ナビゲーションメニューから「通知管理」をクリック

#### 通知履歴の確認

「通知履歴」タブで以下の情報を確認できます：

- 作成日時
- 受信者（名前とメールアドレス）
- 通知タイプ（従業員作成/更新/削除/手動）
- 件名
- 状態（送信待ち/送信済み/失敗）
- 送信日時

#### 手動通知の送信

「テスト通知送信」タブで：

1. ドロップダウンから従業員を選択
2. 件名を入力
3. メッセージを入力
4. 「通知を送信」ボタンをクリック

### 3. 自動通知のテスト

#### 従業員作成通知

1. 「従業員管理」ページで新しい従業員を作成
2. 自動的に「ようこそメール」通知が作成される
3. 10秒以内に通知が送信される（コンソールに出力）
4. 「通知管理」ページで送信履歴を確認

#### 従業員更新通知

1. 既存の従業員情報を編集
2. 「更新通知」が自動作成される
3. 通知履歴で確認

#### 従業員削除通知

1. 従業員を削除
2. 「削除通知」が自動作成される
3. 通知履歴で確認

### 4. コンソールでメール内容を確認

NotificationServiceのコンソールログに、実際に送信されるメール内容が表示されます：

```
========== メール送信 (コンソール出力モード) ==========
宛先: 山田 太郎 <yamada.taro@example.com>
件名: ようこそ！従業員登録が完了しました
---
山田 太郎 様

従業員管理システムへようこそ！
あなたの従業員情報が正常に登録されました。

【登録情報】
- 氏名: 山田 太郎
- メールアドレス: yamada.taro@example.com
- 部署: 開発部
- 役職: エンジニア
- 登録日: 2025年11月09日 14:30

今後ともよろしくお願いいたします。

従業員管理システム
======================================================
```

## 実装詳細

### イベント契約

`src/Shared/Contracts/Events/EmployeeEvents.cs`:

```csharp
public record EmployeeCreatedEvent
{
    public Guid EmployeeId { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string Department { get; init; }
    public string Position { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

### イベント発行（EmployeeService）

`src/Services/EmployeeService/Application/UseCases/EmployeeService.cs`:

```csharp
public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, ...)
{
    // ... 従業員作成処理 ...
    
    // イベントを発行
    if (_eventPublisher != null)
    {
        var eventData = new EmployeeCreatedEvent { ... };
        await _eventPublisher.PublishAsync("employee.created", eventData, ...);
    }
    
    return created.ToDto();
}
```

### イベント購読（NotificationService）

`src/Services/NotificationService/Infrastructure/Messaging/EmployeeEventConsumer.cs`:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    var subscriber = _redis.GetSubscriber();
    
    await subscriber.SubscribeAsync(
        RedisChannel.Literal("employee.created"),
        async (channel, message) => {
            await HandleEmployeeCreatedAsync(message!, stoppingToken);
        });
    
    // ... 他のイベントチャネルも購読 ...
}
```

### 通知処理

`src/Services/NotificationService/Infrastructure/Workers/NotificationProcessorWorker.cs`:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        // Pending 通知を処理
        await notificationService.ProcessPendingNotificationsAsync(stoppingToken);
        
        // 10秒待機
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
    }
}
```

## カスタマイズ

### メール送信サービスの置き換え

開発環境では `ConsoleEmailService` を使用していますが、本番環境では実際のメール送信サービスに置き換えます。

#### SMTP実装例

```csharp
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    
    public async Task SendEmailAsync(string recipientEmail, ...)
    {
        using var client = new SmtpClient(_configuration["Smtp:Host"])
        {
            Port = int.Parse(_configuration["Smtp:Port"]),
            Credentials = new NetworkCredential(
                _configuration["Smtp:Username"],
                _configuration["Smtp:Password"]
            ),
            EnableSsl = true
        };
        
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["Smtp:FromAddress"]),
            Subject = subject,
            Body = message,
            IsBodyHtml = false
        };
        mailMessage.To.Add(recipientEmail);
        
        await client.SendMailAsync(mailMessage);
    }
}
```

#### Program.csで登録

```csharp
// 開発環境: コンソール出力
if (app.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, ConsoleEmailService>();
}
// 本番環境: 実際のメール送信
else
{
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
}
```

### 通知テンプレートのカスタマイズ

`EmployeeEventConsumer.cs` の各ハンドラーメソッドで通知内容をカスタマイズできます：

```csharp
private async Task HandleEmployeeCreatedAsync(string message, ...)
{
    var eventData = JsonSerializer.Deserialize<EmployeeCreatedEvent>(message);
    
    // カスタムメッセージ
    var customMessage = $"""
        {eventData.LastName} {eventData.FirstName} 様
        
        🎉 従業員管理システムへようこそ！
        
        あなたのアカウントが作成されました。
        詳細は以下の通りです：
        
        📧 メール: {eventData.Email}
        🏢 部署: {eventData.Department}
        💼 役職: {eventData.Position}
        📅 登録日: {eventData.CreatedAt:yyyy年MM月dd日}
        
        ご不明な点がございましたら、システム管理者までお問い合わせください。
        """;
    
    var notification = new Domain.Entities.Notification(
        eventData.Email,
        $"{eventData.LastName} {eventData.FirstName}",
        NotificationType.EmployeeCreated,
        "ようこそ！従業員管理システムへ",
        customMessage
    );
    
    await repository.AddAsync(notification, cancellationToken);
}
```

### 処理間隔の調整

`NotificationProcessorWorker.cs` の処理間隔を変更：

```csharp
private readonly TimeSpan _interval = TimeSpan.FromSeconds(5); // 5秒に短縮

// または appsettings.json から設定
private readonly TimeSpan _interval = TimeSpan.FromSeconds(
    _configuration.GetValue<int>("NotificationProcessor:IntervalSeconds", 10)
);
```

### 再試行ロジックの追加

```csharp
public void MarkAsFailed(string errorMessage)
{
    Status = NotificationStatus.Failed;
    ErrorMessage = errorMessage;
    RetryCount++;
    
    // 再試行上限を超えたら永久失敗
    if (RetryCount >= 3)
    {
        Status = "PermanentlyFailed";
    }
}
```

## トラブルシューティング

### 問題: 通知が作成されない

**原因**: EmployeeServiceがRedisに接続できていない

**解決方法**:
1. Aspireダッシュボードでredisの状態を確認
2. EmployeeServiceのログで "イベントを発行しました" を確認
3. Redis接続文字列を確認

```bash
# Redisのログを確認
# Aspireダッシュボード > redis > Logs
```

### 問題: 通知がPendingのままで送信されない

**原因**: NotificationProcessorWorkerが動作していない

**解決方法**:
1. NotificationServiceのログで "通知処理ワーカーを開始しました" を確認
2. Aspireダッシュボードで NotificationService の状態を確認
3. エラーログを確認

```bash
# NotificationServiceのログを確認
# Aspireダッシュボード > notificationservice-api > Logs
```

### 問題: 通知が Failed 状態になる

**原因**: メール送信処理でエラーが発生

**解決方法**:
1. 通知の `ErrorMessage` を確認
2. ConsoleEmailService のログを確認
3. メールアドレスの形式を確認

```sql
-- SQLiteでエラーメッセージを確認
SELECT Id, RecipientEmail, Status, ErrorMessage, RetryCount
FROM Notifications
WHERE Status = 'Failed'
ORDER BY CreatedAt DESC;
```

### 問題: BlazorWebで通知履歴が表示されない

**原因**: NotificationServiceのAPIエンドポイントに接続できない

**解決方法**:
1. NotificationServiceが起動しているか確認
2. Aspireのサービスディスカバリーが正常に動作しているか確認
3. ブラウザの開発者ツールでネットワークエラーを確認

### デバッグ用SQLクエリ

```sql
-- すべての通知を確認
SELECT * FROM Notifications ORDER BY CreatedAt DESC;

-- 状態別の通知数
SELECT Status, COUNT(*) as Count
FROM Notifications
GROUP BY Status;

-- 最近の失敗した通知
SELECT RecipientEmail, Subject, ErrorMessage, CreatedAt
FROM Notifications
WHERE Status = 'Failed'
ORDER BY CreatedAt DESC
LIMIT 10;

-- 再試行回数が多い通知
SELECT RecipientEmail, Subject, RetryCount, ErrorMessage
FROM Notifications
WHERE RetryCount > 0
ORDER BY RetryCount DESC;
```

## パフォーマンス最適化

### データベースインデックス

```csharp
// NotificationDbContext.cs に追加
entity.HasIndex(e => e.Status);
entity.HasIndex(e => e.CreatedAt);
entity.HasIndex(e => new { e.Status, e.CreatedAt });
```

### バッチ処理

```csharp
// 一度に複数の通知を処理
var pendingNotifications = await _repository.GetPendingAsync(cancellationToken);
var batch = pendingNotifications.Take(10); // 一度に10件まで

var tasks = batch.Select(async notification => {
    // 並列処理
    await ProcessNotificationAsync(notification, cancellationToken);
});

await Task.WhenAll(tasks);
```

### キャッシング

```csharp
// 通知テンプレートをキャッシュ
private readonly IMemoryCache _cache;

private string GetNotificationTemplate(string type)
{
    return _cache.GetOrCreate($"template_{type}", entry =>
    {
        entry.SlidingExpiration = TimeSpan.FromHours(1);
        return LoadTemplate(type);
    });
}
```

## まとめ

通知サービスは、従業員管理システムに重要な通知機能を追加するマイクロサービスです。イベント駆動アーキテクチャとクリーンアーキテクチャの原則に従い、保守性とスケーラビリティを兼ね備えています。

このドキュメントを参考に、システムのカスタマイズや拡張を行ってください。質問や問題がある場合は、GitHubのIssueで報告してください。
