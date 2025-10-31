# アーキテクチャ概要

従業員管理システムのアーキテクチャと設計原則について説明します。

## 全体アーキテクチャ

このシステムは**.NET Aspire**を使用したマイクロサービスアーキテクチャで構成されています。

```
┌─────────────────────────────────────────────────────────────┐
│                    Aspire AppHost                            │
│              (オーケストレーション層)                          │
└─────────────────────────────────────────────────────────────┘
           │              │                │
           ▼              ▼                ▼
    ┌──────────┐   ┌──────────┐    ┌──────────┐
    │ BlazorWeb│   │ Employee │    │   Auth   │
    │          │   │ Service  │    │ Service  │
    │  (UI)    │   │  (API)   │    │  (API)   │
    └──────────┘   └──────────┘    └──────────┘
           │              │                │
           └──────────────┴────────────────┘
                         │
                  ┌──────▼──────┐
                  │ Service     │
                  │ Defaults    │
                  │ (共通設定)  │
                  └─────────────┘
```

## コンポーネント構成

### 1. Aspire AppHost (`src/AppHost`)

**役割**: すべてのサービスとアプリケーションのオーケストレーション

- サービスの起動と停止の管理
- サービスディスカバリーの提供
- 環境変数と構成の注入
- OpenTelemetryによる分散トレーシング

**主要機能**:
```csharp
// サービスの登録と相互参照
var employeeServiceApi = builder.AddProject<Projects.EmployeeService_API>("employeeservice-api");
var authServiceApi = builder.AddProject<Projects.AuthService_API>("authservice-api");

builder.AddProject<Projects.BlazorWeb>("blazorweb")
    .WithExternalHttpEndpoints()
    .WithReference(employeeServiceApi)
    .WithReference(authServiceApi);
```

### 2. ServiceDefaults (`src/ServiceDefaults`)

**役割**: すべてのサービスで共有される横断的関心事の実装

**提供機能**:
- **OpenTelemetry**: 分散トレーシング、メトリクス、ログ
- **ヘルスチェック**: `/health` および `/alive` エンドポイント
- **サービスディスカバリー**: Aspire経由での他サービスの検出
- **Resilience**: HTTP通信のリトライ、サーキットブレーカー

**使用方法**:
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults(); // OpenTelemetry、ヘルスチェック等を追加

var app = builder.Build();
app.MapDefaultEndpoints(); // /health, /alive エンドポイントを公開
```

### 3. EmployeeService

**役割**: 従業員データの管理

**クリーンアーキテクチャ**による層構成:

#### Domain層 (`Domain/`)
- **責務**: ビジネスロジックとドメインルール
- **依存**: なし（他の層に依存しない）
- **内容**:
  - エンティティ（Entity）
  - 値オブジェクト（Value Objects）
  - ドメインイベント
  - ドメインサービス

#### Application層 (`Application/`)
- **責務**: ユースケースとアプリケーションロジック
- **依存**: Domain層のみ
- **内容**:
  - コマンド/クエリハンドラー（CQRS）
  - DTO（Data Transfer Objects）
  - リポジトリインターフェース
  - アプリケーションサービス

#### Infrastructure層 (`Infrastructure/`)
- **責務**: 外部システムとの統合
- **依存**: Domain層、Application層
- **内容**:
  - Entity Framework Core DbContext
  - リポジトリ実装
  - 外部APIクライアント
  - ファイルシステムアクセス

#### API層 (`API/`)
- **責務**: HTTPエンドポイントの公開
- **依存**: Application層、Infrastructure層
- **内容**:
  - コントローラー/Minimal API
  - リクエスト/レスポンスモデル
  - 認証/認可
  - OpenAPI/Swagger設定

**依存関係の方向**:
```
API ─→ Infrastructure ─→ Application ─→ Domain
                              ↓
                          Contracts
```

### 4. AuthService (`src/Services/AuthService/API`)

**役割**: ユーザー認証と認可

**主要機能**:
- ASP.NET Core Identityによるユーザー管理
- JWT トークンベース認証（将来実装予定）
- ロールベースアクセス制御
- パスワードポリシーの適用

**将来の拡張**:
- Microsoft Entra ID（旧Azure AD）統合
- OAuth 2.0 / OpenID Connect対応
- 多要素認証（MFA）

### 5. BlazorWeb (`src/WebApps/BlazorWeb`)

**役割**: Webフロントエンド

**技術スタック**:
- Blazor Web App（.NET 9）
- MudBlazor UIコンポーネントライブラリ
- Interactive Server レンダリング

**機能**:
- 従業員情報の表示と編集
- ダッシュボード
- レスポンシブデザイン

### 6. Shared.Contracts (`src/Shared/Contracts`)

**役割**: サービス間で共有されるDTO

**内容**:
- API契約（リクエスト/レスポンスDTO）
- 共有エンティティモデル
- 定数とEnum

## データフロー

### 典型的なリクエストフロー

```
1. ユーザー操作
   ↓
2. BlazorWeb (UI)
   ↓ HTTP Request
3. EmployeeService API
   ↓ コントローラー
4. Application層（ハンドラー）
   ↓ ビジネスロジック
5. Domain層（エンティティ）
   ↓ 永続化
6. Infrastructure層（リポジトリ）
   ↓ EF Core
7. SQLite データベース
```

## セキュリティ

### 現在の実装
- HTTPS通信
- ServiceDefaults経由でのヘルスチェック認証

### 今後の実装予定
- JWT認証
- APIキー認証
- ロールベースアクセス制御（RBAC）
- 監査ログ

## 可観測性（Observability）

Aspire ServiceDefaultsにより、すべてのサービスで以下が自動的に有効化されます：

### 1. 分散トレーシング
- OpenTelemetryによる分散トレース
- サービス間のリクエストフローの可視化

### 2. メトリクス
- HTTPリクエスト数
- レスポンス時間
- エラー率
- カスタムメトリクス

### 3. ログ
- 構造化ログ
- 相関ID
- ログレベルの動的変更

### 4. ヘルスチェック
- `/health`: 詳細なヘルスチェック結果
- `/alive`: 単純な生存確認

## データベース戦略

### 開発環境
- **SQLite**: ローカル開発用
- ファイル配置: `data/` ディレクトリ

### 本番環境（将来）
- **Azure SQL Database** または **PostgreSQL**
- マイグレーション戦略の策定

## スケーラビリティ

### 水平スケーリング
各マイクロサービスは独立してスケール可能：
- コンテナ化（Docker）
- Kubernetes対応
- Aspire Deploymentによる自動デプロイ

### データベース分離
将来的には各サービスが独自のデータベースを持つ予定：
- EmployeeService → Employee DB
- AuthService → Auth DB

## 関連ドキュメント

- [開発ガイド](development-guide.md)
- [Aspireダッシュボード](aspire-dashboard.md)
- [データベース管理](database.md)
