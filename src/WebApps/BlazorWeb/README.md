# Blazor Web App

従業員管理システムのフロントエンドアプリケーションです。

## 技術スタック

- Blazor Web App (.NET 9)
- MudBlazor - Material Design UIコンポーネントライブラリ
- .NET Aspire Service Defaults

## MudBlazorについて

MudBlazorは、Material Designに基づいた包括的なBlazor UIコンポーネントライブラリです。以下の機能を提供します：

- リッチなUIコンポーネント（テーブル、フォーム、ダイアログなど）
- レスポンシブデザイン
- テーマカスタマイズ
- アクセシビリティ対応

## 開発

```bash
# Aspire経由で実行（推奨）
dotnet run --project ../../AppHost

# または直接実行
dotnet run
```

## 構成

- `/Components` - Blazorコンポーネント
- `/Pages` - ページコンポーネント
- `/Services` - クライアントサービス（API通信など）

## API連携

### EmployeeService APIクライアント

BlazorWebからEmployeeServiceのAPIを呼び出すための統合実装です。

#### 主要機能

**サービス登録** (`Program.cs`):
```csharp
builder.Services.AddHttpClient<IEmployeeApiClient, EmployeeApiClient>("employeeservice-api", client =>
{
    client.BaseAddress = new Uri("http://employeeservice-api");
});
```

**リトライ・レジリエンスポリシー**:
- `AddStandardResilienceHandler()`による自動リトライ機能
- Aspireのサービスディスカバリーによる動的エンドポイント解決
- 以下のポリシーが自動適用されます（ServiceDefaultsから継承）：
  - **Retry**: 一時的なエラー時の再試行（指数バックオフ）
  - **Circuit Breaker**: サービスの過負荷防止
  - **Timeout**: リクエストタイムアウト管理

#### エラーハンドリング

APIクライアント (`EmployeeApiClient.cs`) は包括的なエラーハンドリングを実装しています：

1. **HTTPエラーの適切な処理**
   - 404 Not Found: nullを返却（存在しないリソース）
   - 400 Bad Request: エラーメッセージを含む例外
   - その他のエラー: 適切な例外メッセージ

2. **構造化ロギング**
   - 全API呼び出しのログ記録
   - エラー時の詳細情報記録
   - パフォーマンス監視用のログ

3. **ユーザーフレンドリーなエラーメッセージ**
   - 日本語でのエラーメッセージ
   - UIでの適切なエラー表示（Snackbar通知）

#### リトライポリシーの詳細

Microsoft.Extensions.Http.Resilienceの`AddStandardResilienceHandler()`は以下を提供します：

- **Hedge Strategy**: 並列リクエストでレイテンシを削減
- **Retry Pattern**: 
  - 最大3回のリトライ
  - 指数バックオフ（2秒、4秒、8秒）
  - HTTP 5xx、408、429エラー時に実行
- **Circuit Breaker**:
  - 連続した失敗を検出
  - 一定期間サービスを保護
  - 自動復旧を試行
- **Timeout**:
  - リクエスト全体のタイムアウト設定
  - デッドロック防止

#### カスタムリトライポリシーの設定例

必要に応じてカスタムポリシーを設定できます：

```csharp
builder.Services.AddHttpClient<IEmployeeApiClient, EmployeeApiClient>("employeeservice-api", client =>
{
    client.BaseAddress = new Uri("http://employeeservice-api");
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 5;
    options.Retry.Delay = TimeSpan.FromSeconds(1);
    options.CircuitBreaker.FailureRatio = 0.5;
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
});
```

#### 監視と可観測性

OpenTelemetryによる完全な可観測性：
- HTTPクライアントの自動トレーシング
- メトリクスの収集（リクエスト数、エラー率、レイテンシ）
- Aspireダッシュボードでのリアルタイム監視

詳細は [Aspireダッシュボードガイド](../../../docs/aspire-dashboard.md) を参照してください。

## 使用可能なページ

- `/` - ホーム画面
- `/dashboard` - ダッシュボード
- `/employees` - 従業員一覧（EmployeeService API連携）
- `/employees/{id}` - 従業員詳細（EmployeeService API連携）
