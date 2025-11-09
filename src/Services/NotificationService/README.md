# Notification Service

通知サービス（Notification Service）は、従業員管理システムのイベントを受けて、従業員にメール通知を送信するマイクロサービスです。

## 📋 概要

このサービスは、従業員の作成・更新・削除などのイベントをリアルタイムで監視し、対応する通知を自動的に送信します。また、手動での通知送信もサポートしています。

## 🏗️ アーキテクチャ

### レイヤー構造

```
NotificationService/
├── Domain/              # ドメイン層
│   ├── Entities/       # エンティティ（Notification）
│   └── Repositories/   # リポジトリインターフェース
├── Application/        # アプリケーション層
│   ├── UseCases/      # ユースケース（NotificationService）
│   ├── Services/      # サービスインターフェース（IEmailService）
│   └── Mappings/      # DTOマッピング
├── Infrastructure/     # インフラストラクチャ層
│   ├── Data/          # データベースコンテキスト
│   ├── Repositories/  # リポジトリ実装
│   ├── Services/      # サービス実装（ConsoleEmailService）
│   ├── Messaging/     # イベント処理（Redis）
│   └── Workers/       # バックグラウンドワーカー
└── API/               # API層
    └── Endpoints/     # エンドポイント定義
```

### クリーンアーキテクチャ

- **Domain層**: ビジネスロジックとエンティティを含む中核層
- **Application層**: ユースケースとビジネスルールの調整
- **Infrastructure層**: 外部システムとの統合（DB、メッセージング）
- **API層**: HTTPエンドポイントの公開

## ✨ 主な機能

### 1. イベント駆動通知

従業員サービスから発行されるイベントを自動的に監視し、通知を作成します：

- **EmployeeCreated**: 新規従業員登録時の「ようこそメール」
- **EmployeeUpdated**: 従業員情報更新時の「更新通知」
- **EmployeeDeleted**: 従業員削除時の「削除通知」

### 2. 手動通知送信

BlazorWebアプリから手動で通知を送信できます：

- 特定の従業員を選択
- カスタムな件名とメッセージを作成
- 即座に通知キューに追加

### 3. 通知履歴管理

すべての通知の履歴を保存し、以下の情報を追跡します：

- 送信状態（Pending/Sent/Failed）
- 送信日時
- 再試行回数
- エラーメッセージ（失敗時）

## 🔧 技術スタック

| コンポーネント | 技術 |
|------------|------|
| データベース | SQLite |
| メッセージング | Redis (Pub/Sub) |
| ORM | Entity Framework Core 9 |
| メール送信 | Console（開発用）|
| バックグラウンド処理 | IHostedService |

## 📝 通知エンティティ

```csharp
public class Notification
{
    public Guid Id { get; private set; }
    public string RecipientEmail { get; private set; }
    public string RecipientName { get; private set; }
    public string NotificationType { get; private set; }
    public string Subject { get; private set; }
    public string Message { get; private set; }
    public string Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }
}
```

### 通知状態

- **Pending**: 送信待ち
- **Sent**: 送信完了
- **Failed**: 送信失敗

### 通知タイプ

- **EmployeeCreated**: 従業員作成イベント
- **EmployeeUpdated**: 従業員更新イベント
- **EmployeeDeleted**: 従業員削除イベント
- **Manual**: 手動送信

## 🚀 API エンドポイント

### 通知一覧取得
```
GET /api/notifications
```

### 最近の通知取得
```
GET /api/notifications/recent?count=50
```

### 通知詳細取得
```
GET /api/notifications/{id}
```

### 手動通知作成
```
POST /api/notifications
Content-Type: application/json

{
  "recipientEmail": "employee@example.com",
  "recipientName": "山田 太郎",
  "subject": "テスト通知",
  "message": "これはテストメッセージです"
}
```

## 🔄 イベント処理フロー

### 従業員作成時

```
1. EmployeeService: 従業員を作成
2. EmployeeService: Redis に EmployeeCreatedEvent を発行
3. NotificationService: イベントを受信
4. NotificationService: 通知エンティティを作成（Status: Pending）
5. NotificationProcessorWorker: 10秒ごとにPending通知を処理
6. ConsoleEmailService: メール送信（コンソールに出力）
7. Notification: 状態を Sent に更新
```

### バックグラウンド処理

**EmployeeEventConsumer**
- Redisイベントチャネルを監視
- employee.created, employee.updated, employee.deleted を購読
- イベントを受信して通知エンティティを作成

**NotificationProcessorWorker**
- 10秒間隔で実行
- Pending状態の通知を取得
- IEmailServiceを使用してメール送信
- 成功/失敗に応じて状態を更新

## 🛠️ 開発・デバッグ

### ローカル実行

```bash
# Aspire AppHostを起動（すべてのサービスを含む）
dotnet run --project src/AppHost

# または NotificationService APIを単独で起動
dotnet run --project src/Services/NotificationService/API
```

### コンソールログでメール確認

開発環境では、実際のメール送信の代わりにコンソールにメッセージを出力します：

```
========== メール送信 (コンソール出力モード) ==========
宛先: 山田 太郎 <yamada@example.com>
件名: ようこそ！従業員登録が完了しました
---
山田 太郎 様

従業員管理システムへようこそ！
...
======================================================
```

### データベース確認

SQLiteデータベースファイル: `notifications.db`

```bash
# データベース内容を確認
sqlite3 notifications.db "SELECT * FROM Notifications ORDER BY CreatedAt DESC LIMIT 10;"
```

## 🔍 トラブルシューティング

### 通知が送信されない

1. **Redisが起動しているか確認**
   ```bash
   # Aspireダッシュボードで Redis の状態を確認
   ```

2. **EmployeeServiceがRedisに接続できているか確認**
   - ログに "イベントを発行しました" が表示されているか

3. **NotificationServiceのワーカーが起動しているか確認**
   - ログに "通知処理ワーカーを開始しました" が表示されているか

### 通知が Failed 状態のまま

- エラーメッセージを確認
- `NotificationRepository` で通知の `ErrorMessage` を取得
- メール送信サービスのログを確認

## 🔒 セキュリティ考慮事項

- メールアドレスの検証（エンティティレベル）
- イベントペイロードの検証
- 再試行回数の制限（無限ループ防止）
- 個人情報の適切な取り扱い

## 🚀 本番環境への展開

### メール送信の実装

開発環境では `ConsoleEmailService` を使用していますが、本番環境では以下のような実装に置き換えることを推奨します：

- SMTP経由でのメール送信
- SendGrid / Azure Communication Services などのサービス利用
- メール送信の非同期処理と再試行ロジック

### スケーラビリティ

- NotificationProcessorWorker の複数インスタンス対応
- Redis のクラスタリング
- データベースの最適化（インデックス追加）

## 📚 関連ドキュメント

- [アーキテクチャ設計書](../../../docs/architecture-detailed.md)
- [開発ガイド](../../../docs/development-guide.md)
- [Aspireダッシュボード](../../../docs/aspire-dashboard.md)
