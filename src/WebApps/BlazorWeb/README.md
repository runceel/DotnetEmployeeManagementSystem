# Blazor Web App

従業員管理システムのフロントエンドアプリケーションです。

## 技術スタック

- Blazor Web App (.NET 10)
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

### AuthService APIクライアント

BlazorWebからAuthServiceの認証APIを呼び出すための統合実装です。

#### 主要機能

**サービス登録** (`Program.cs`):
```csharp
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>("authservice-api", client =>
{
    client.BaseAddress = new Uri("http://authservice-api");
});

// 認証状態管理サービス
builder.Services.AddScoped<AuthStateService>();
```

**認証機能**:
- ユーザーログイン（POST /api/auth/login）
- ユーザー登録（POST /api/auth/register）
- 認証状態管理（セッション内）
- ログイン/ログアウト機能

#### ログイン画面

`/login` ページでは、MudBlazorコンポーネントを使用した洗練されたログイン画面を提供：

- ユーザー名/メールアドレス入力
- パスワード入力
- ローディング状態表示
- エラーメッセージ表示
- テストユーザー情報の表示

**テストユーザー**:
- `testuser` / `Password123!`
- `admin` / `Admin123!`

#### 認証状態管理

`AuthStateService` は以下を提供します：

```csharp
public class AuthStateService
{
    public AuthResponse? CurrentUser { get; }
    public bool IsAuthenticated { get; }
    public event Action? OnAuthStateChanged;
    
    public void Login(AuthResponse user);
    public void Logout();
}
```

- リアクティブな認証状態変更通知
- コンポーネント間での認証情報共有
- セッション内での状態保持

#### エラーハンドリング

APIクライアント (`AuthApiClient.cs`) は包括的なエラーハンドリングを実装：

1. **HTTPエラーの適切な処理**
   - 401 Unauthorized: null返却（認証失敗）
   - 400 Bad Request: null返却（登録失敗）
   - その他のエラー: 適切な例外メッセージ

2. **構造化ロギング**
   - すべてのAPI呼び出しをログ記録
   - エラー時の詳細情報記録

3. **ユーザーフレンドリーなエラーメッセージ**
   - 日本語でのエラーメッセージ
   - UIでの適切なエラー表示（Snackbar通知）

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

- `/` - ホーム画面（認証状態表示）
- `/dashboard` - ダッシュボード
- `/login` - ログイン画面（AuthService API連携）
- `/employees` - 従業員一覧（EmployeeService API連携、認証推奨）
- `/employees/{id}` - 従業員詳細（EmployeeService API連携）

## 認証機能

### 概要

BlazorWebは、AuthServiceと連携してユーザー認証機能を提供します。現在はダミー認証を使用しており、将来的にMicrosoft Entra IDに移行する設計となっています。

### 認証フロー

1. **ログイン**
   - ユーザーが `/login` にアクセス
   - ユーザー名/メールアドレスとパスワードを入力
   - AuthService APIで認証
   - 成功時は認証トークンを取得し、セッションに保存

2. **認証状態の表示**
   - ナビゲーションメニューにユーザー名表示
   - ホーム画面に認証状態バナー表示
   - 従業員管理画面に認証チップ表示

3. **ログアウト**
   - ナビゲーションメニューまたはホーム画面からログアウト
   - セッションから認証情報をクリア
   - ホーム画面にリダイレクト

### セキュリティ

**現在の実装（ダミー認証）**:
- ASP.NET Core Identity + SQLite
- Base64エンコードされたダミートークン
- セッション内での認証状態管理
- 開発環境専用（本番環境非対応）

**将来の実装（Entra ID）**:
- Microsoft Entra ID統合
- JWT（JSON Web Token）
- OAuth 2.0 / OpenID Connect
- 多要素認証（MFA）対応

詳細は [Entra ID統合設計ドキュメント](../../../docs/entra-id-integration-design.md) を参照してください。
