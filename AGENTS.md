# AGENTS.md - AI エージェント・開発者向けガイドライン

このドキュメントは、AIエージェント（GitHub Copilot等）や新規参加開発者が、このプロジェクトで効果的に貢献するための必須情報をまとめています。

## 📋 プロジェクト概要

**従業員管理システム** - .NET 10 + .NET Aspire を使用したマイクロサービスアーキテクチャの従業員管理システム

### 主要技術スタック
- **.NET 10** - 最新のフレームワーク
- **.NET Aspire 13.0.0** - マイクロサービスオーケストレーション
- **Blazor Web App + MudBlazor** - フロントエンド
- **Entity Framework Core 10** - ORM
- **SQLite** - 開発環境のデータベース
- **Redis** - Pub/Sub メッセージング
- **OpenTelemetry** - 可観測性

## 🎯 必読ドキュメント（優先順位順）

### 1. セットアップ・実行（最優先）
- **[Getting Started](docs/getting-started.md)** - 開発環境のセットアップと実行方法

### 2. アーキテクチャ理解
- **[詳細アーキテクチャ設計書](docs/architecture-detailed.md)** - 技術的な詳細設計（最も包括的）
- **[アーキテクチャ概要](docs/architecture.md)** - 高レベルの設計パターン

### 3. 開発作業
- **[開発ガイド](docs/development-guide.md)** - コーディング規約、新機能追加方法、テスト戦略

### 4. UI/UXデザイン（**UI画面追加時は必読**）
- **[デザインカタログ](docs/design-catalog/README.md)** - MudBlazor UIパターン、コンポーネント使用ガイド（インデックス）
- **画面パターン**（必要なパターンのみ参照してトークン効率化）
  - **[一覧画面](docs/design-catalog/patterns/list-page.md)** - データ一覧、CRUD操作
  - **[詳細画面](docs/design-catalog/patterns/detail-page.md)** - 単一データ詳細表示
  - **[編集画面](docs/design-catalog/patterns/edit-dialog.md)** - ダイアログフォーム
  - **[ダッシュボード](docs/design-catalog/patterns/dashboard.md)** - 統計、アクティビティ表示
- **[推奨・非推奨ルール](docs/design-catalog/dos-and-donts.md)** - UIベストプラクティス
- **[デザイントークン](docs/design-catalog/tokens.md)** - カラー、タイポグラフィ、スペーシング定義

### 5. 特定領域
- **[データベース管理](docs/database.md)** - マイグレーション、シードデータ、クエリのベストプラクティス
- **[通知サービス](docs/notification-service.md)** - イベント駆動通知システムの詳細
- **[Aspireダッシュボード](docs/aspire-dashboard.md)** - 監視、デバッグ、トレース方法

## 🏗️ プロジェクト構造

```
DotnetEmployeeManagementSystem/
├── src/
│   ├── AppHost/                      # .NET Aspire オーケストレーション
│   ├── ServiceDefaults/              # 共通設定（OpenTelemetry, Health Checks）
│   ├── Services/
│   │   ├── EmployeeService/          # 従業員管理マイクロサービス
│   │   │   ├── Domain/              # ドメインモデル（エンティティ、リポジトリIF）
│   │   │   ├── Application/         # ビジネスロジック（ユースケース）
│   │   │   ├── Infrastructure/      # データアクセス（EF Core）
│   │   │   └── API/                 # Web API（コントローラー、エンドポイント）
│   │   ├── AuthService/             # 認証サービス
│   │   │   ├── Domain/
│   │   │   ├── Application/
│   │   │   ├── Infrastructure/
│   │   │   └── API/
│   │   └── NotificationService/     # 通知サービス（Redis Pub/Sub）
│   │       ├── Domain/
│   │       ├── Application/
│   │       ├── Infrastructure/
│   │       └── API/
│   ├── WebApps/
│   │   └── BlazorWeb/               # Blazor Web App（UI）
│   └── Shared/
│       └── Contracts/               # 共有DTO
├── tests/                           # ユニットテスト・統合テスト
├── docs/                            # 恒久的ドキュメント（設計・仕様）
├── .github/
│   ├── agents/                      # カスタムAIエージェント定義
│   └── issue-reports/               # 一時的なIssue作業報告
└── data/                            # SQLiteデータベースファイル
```

## 📚 ドキュメント管理ルール

### 恒久的ドキュメント → `docs/`
以下は `docs/` ディレクトリに配置：
- アーキテクチャ設計書
- 開発ガイド・コーディング規約
- セットアップ手順
- データベース設計
- サービス仕様書
- 運用・監視ガイド

### 一時的ドキュメント → `.github/issue-reports/`
以下は `.github/issue-reports/` に配置：
- Issue作業完了報告
- バグ修正報告
- 特定機能の実装記録
- 検証レポート
- 一時的なスクリーンショット集

詳細は [.github/issue-reports/README.md](.github/issue-reports/README.md) を参照してください。

## 🛠️ 開発ワークフロー

### 1. 環境構築
```bash
# 前提条件: .NET 10 SDK インストール済み
# 注: Aspire 13.0.0 は NuGet パッケージとして利用されるため、workload インストールは不要です

# リポジトリクローン・依存関係復元
git clone https://github.com/runceel/DotnetEmployeeManagementSystem.git
cd DotnetEmployeeManagementSystem
dotnet restore
```

### 2. アプリケーション起動
```bash
# Aspire AppHost経由で全サービスを起動（推奨）
dotnet run --project src/AppHost

# Aspireダッシュボードが自動的に開きます
# - BlazorWeb UI
# - EmployeeService API
# - AuthService API
# - NotificationService API
# - Redis
```

### 3. テスト実行
```bash
# 全テスト実行
dotnet test

# 特定テストプロジェクト
dotnet test tests/EmployeeService.Domain.Tests/
dotnet test tests/EmployeeService.Integration.Tests/
```

### 4. データベースマイグレーション
```bash
# マイグレーション作成
cd src/Services/EmployeeService/API
dotnet ef migrations add MigrationName --project ../Infrastructure

# マイグレーション適用
dotnet ef database update --project ../Infrastructure
```

## 🎨 アーキテクチャパターン

### クリーンアーキテクチャ（各マイクロサービス）
```
API (Presentation)
  ↓ 依存
Application (Business Logic)
  ↓ 依存
Domain (Core Business Rules)
  ↑ 実装
Infrastructure (Data Access)
```

**依存関係の原則**：
- 依存は常に内側（Domain）に向かう
- Domain層は他の層に依存しない
- Infrastructure層がDomain層のインターフェースを実装

### サービス間通信
- **同期通信**: HTTP/REST（Aspire Service Discovery経由）
- **非同期通信**: Redis Pub/Sub（イベント駆動）

## 🔍 よくあるタスクと参照先

| タスク | 参照ドキュメント |
|--------|-----------------|
| **UI画面を追加** | **[design-catalog/README.md](docs/design-catalog/README.md)（インデックス）** |
| **一覧画面テンプレート** | **[patterns/list-page.md](docs/design-catalog/patterns/list-page.md)** |
| **詳細画面テンプレート** | **[patterns/detail-page.md](docs/design-catalog/patterns/detail-page.md)** |
| **編集画面テンプレート** | **[patterns/edit-dialog.md](docs/design-catalog/patterns/edit-dialog.md)** |
| **ダッシュボードテンプレート** | **[patterns/dashboard.md](docs/design-catalog/patterns/dashboard.md)** |
| **UIコンポーネント選択・ベストプラクティス** | **[dos-and-donts.md](docs/design-catalog/dos-and-donts.md)** |
| **UIカラー・スペーシング・タイポグラフィ** | **[tokens.md](docs/design-catalog/tokens.md)** |
| 新しいエンティティを追加 | [development-guide.md#新機能の追加](docs/development-guide.md) |
| 新しいAPIエンドポイントを追加 | [development-guide.md](docs/development-guide.md) |
| データベーススキーマ変更 | [database.md#マイグレーション](docs/database.md) |
| 新しいマイクロサービスを追加 | [development-guide.md#新しいマイクロサービスの追加](docs/development-guide.md) |
| イベント駆動通知の実装 | [notification-service.md](docs/notification-service.md) |
| パフォーマンス問題の調査 | [aspire-dashboard.md#パフォーマンス問題の調査](docs/aspire-dashboard.md) |
| エラーのデバッグ | [aspire-dashboard.md#エラーのデバッグ](docs/aspire-dashboard.md) |
| 認証機能の実装 | [authorization-implementation.md](docs/authorization-implementation.md) |
| MCPサーバー・クライアントの実装 | [mcp-integration-design.md](docs/mcp-integration-design.md), [mcp-implementation-guide.md](docs/mcp-implementation-guide.md) |
| AIチャット機能の追加 | [mcp-integration-design.md#5-チャット画面mcpクライアント実装方針](docs/mcp-integration-design.md) |

## 🧪 テスト戦略

### テストの種類
1. **ユニットテスト** (`tests/**/Domain.Tests/`, `**/Application.Tests/`)
   - Domain層のビジネスロジック
   - Application層のユースケース
   - モック使用（Moq）

2. **統合テスト** (`tests/**/Integration.Tests/`)
   - API エンドポイント
   - データベース操作
   - WebApplicationFactory使用

### テスト命名規則
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    // Act
    // Assert
}
```

### カバレッジ目標
- Domain層: 90%以上
- Application層: 80%以上
- Infrastructure層: 70%以上

## 🔒 セキュリティ考慮事項

1. **認証・認可**
   - ASP.NET Core Identity使用
   - JWT トークンベース認証
   - ロールベースアクセス制御

2. **データ保護**
   - パスワードはハッシュ化保存
   - SQLインジェクション対策（EF Core使用）
   - XSS対策（Blazorの自動エスケープ）

3. **通信セキュリティ**
   - HTTPS強制（本番環境）
   - CORS設定適切に管理

## 📏 コーディング規約

### C# コーディングスタイル
- **命名**: PascalCase (クラス、メソッド)、camelCase (変数、パラメータ)
- **Null許容参照型**: 有効化済み（`<Nullable>enable</Nullable>`）
- **非同期メソッド**: `Async` サフィックス必須
- **XML ドキュメントコメント**: 公開API には必須

### ファイル構成
- 1ファイル1クラス原則
- ファイル名 = クラス名
- 名前空間はフォルダ構造に対応

### 依存性注入
- コンストラクタインジェクション使用
- インターフェースベースの設計
- サービスライフタイム適切に管理（Scoped, Transient, Singleton）

## 🎨 UI開発ガイドライン

### UI画面追加時のチェックリスト

新しいUI画面を作成・修正する際は、以下を必ず確認してください：

#### ✅ 基本設計
- [ ] **デザインカタログを参照**（[design-catalog/README.md](docs/design-catalog/README.md)）
- [ ] **画面パターンを選択**
  - 一覧画面: [list-page.md](docs/design-catalog/patterns/list-page.md)
  - 詳細画面: [detail-page.md](docs/design-catalog/patterns/detail-page.md)
  - 編集画面: [edit-dialog.md](docs/design-catalog/patterns/edit-dialog.md)
  - ダッシュボード: [dashboard.md](docs/design-catalog/patterns/dashboard.md)
- [ ] **類似の既存画面を参照**（`src/WebApps/BlazorWeb/Components/Pages/`）

#### ✅ MudBlazorコンポーネント
- [ ] MudBlazorコンポーネントを優先使用（生のHTML/CSSは最小限）
- [ ] レスポンシブデザイン（`MudGrid`と`Breakpoint`使用）
- [ ] 適切なVariant指定（フォームは`Variant.Outlined`で統一）

#### ✅ 状態管理
- [ ] **ローディング状態を実装**（`MudProgressCircular`または`MudSkeleton`）
- [ ] **エラー状態を実装**（`MudAlert` + 再試行ボタン）
- [ ] **空状態を実装**（データなし時の`MudAlert`）
- [ ] 状態フラグ（`_loading`, `_error`, `_errorMessage`）を適切に管理

#### ✅ エラーハンドリング
- [ ] try-catchで例外を捕捉
- [ ] 具体的なエラーメッセージを表示
- [ ] Snackbarで操作結果を通知（成功/失敗）
- [ ] UnauthorizedAccessExceptionなど特定例外を適切に処理

#### ✅ 認証・認可
- [ ] 認証チェックを実装（`AuthStateService.IsAuthenticated`）
- [ ] 権限チェックを実装（`AuthStateService.IsAdmin`等）
- [ ] 権限に応じたUI表示切り替え

#### ✅ バリデーション
- [ ] MudFormでバリデーション実装
- [ ] Required属性とRequiredErrorメッセージ設定
- [ ] フォーム有効性に応じた送信ボタン制御（`Disabled="!_isValid"`）

#### ✅ UI/UX
- [ ] **日本語UI文言を一貫使用**（英語混在NG）
- [ ] 日付フォーマット統一（`yyyy/MM/dd` or `yyyy年MM月dd日`）
- [ ] 適切なアイコン使用（`Icons.Material.Filled.*`）
- [ ] PageTitle設定（例: `<PageTitle>従業員一覧 - 従業員管理システム</PageTitle>`）
- [ ] Breadcrumbs実装（詳細画面等）

#### ✅ パフォーマンス
- [ ] 非同期メソッド使用（`async`/`await`）
- [ ] 大量データには仮想化検討（`Virtualize`コンポーネント）

#### ✅ 参照ドキュメント確認
- [ ] [dos-and-donts.md](docs/design-catalog/dos-and-donts.md) でベストプラクティス確認
- [ ] [tokens.md](docs/design-catalog/tokens.md) でデザイントークン参照

### UI開発クイックスタート

```razor
<!-- 1. 画面タイプを特定 -->
一覧画面? → patterns/list-page.md を使用
詳細画面? → patterns/detail-page.md を使用
編集画面? → patterns/edit-dialog.md を使用
ダッシュボード? → patterns/dashboard.md を使用

<!-- 2. テンプレートをコピー＆カスタマイズ -->
@page "/items"
@using Shared.Contracts.ItemService
@inject IItemApiClient ItemApiClient
@inject ISnackbar Snackbar

<PageTitle>項目一覧 - 従業員管理システム</PageTitle>

@if (_loading)
{
    <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
}
else if (_error)
{
    <MudAlert Severity="Severity.Error">@_errorMessage</MudAlert>
}
else
{
    <MudTable Items="@_items">...</MudTable>
}

@code {
    private bool _loading = true;
    private bool _error = false;
    private string _errorMessage = string.Empty;
    private IEnumerable<ItemDto>? _items;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadItems();
    }
    
    private async Task LoadItems()
    {
        _loading = true;
        _error = false;
        try
        {
            _items = await ItemApiClient.GetItemsAsync();
            Snackbar.Add("データを読み込みました。", Severity.Success);
        }
        catch (Exception ex)
        {
            _error = true;
            _errorMessage = ex.Message;
            Snackbar.Add($"エラー: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }
}
```

## 🚀 デプロイメント

### 開発環境
- Aspire AppHost経由でローカル起動
- SQLite データベース使用

### 本番環境（予定）
- Azure Container Apps または Kubernetes
- Azure SQL Database または PostgreSQL
- Azure Cache for Redis
- Application Insights（監視）

## 🔗 便利なコマンド集

```bash
# ソリューション全体のビルド
dotnet build

# 特定プロジェクトのみ実行
dotnet run --project src/Services/EmployeeService/API

# ホットリロード有効化
dotnet watch --project src/WebApps/BlazorWeb

# パッケージの更新確認
dotnet list package --outdated

# コードフォーマット
dotnet format
```

## 📞 トラブルシューティング

### Aspireが起動しない
→ `dotnet workload install aspire` を実行

### データベースエラー
→ `dotnet ef database update` を実行してマイグレーション適用

### ポート競合エラー
→ Aspireダッシュボードで動的ポート割り当てを確認

### 詳細なトラブルシューティング
→ [getting-started.md#トラブルシューティング](docs/getting-started.md)

## 🤝 貢献ガイドライン

1. **Issue作成**: バグ報告や機能提案はGitHub Issueで
2. **Pull Request**: 
   - 小さく、焦点を絞ったPR
   - テスト追加必須
   - コードレビュー対応
3. **コミットメッセージ**: 
   - 日本語または英語
   - 明確で具体的な説明

## 📝 更新履歴

| 日付 | バージョン | 変更内容 |
|------|-----------|---------|
| 2025-12-11 | 1.2.1 | デザインカタログをパターン別ファイルに分割（AIトークン効率化） |
| 2025-12-11 | 1.2 | UIデザインカタログ追加、UI開発ガイドライン・チェックリスト策定 |
| 2025-11-15 | 1.1 | .NET 10 / Aspire 13.0.0 への更新 |
| 2025-11-09 | 1.0 | 初版作成 - ドキュメント管理ルール策定 |

## 📄 ライセンス

このプロジェクトは [LICENSE](LICENSE) ファイルに記載されているライセンスの下で公開されています。

---

**このドキュメントは常に最新に保つことを目指しています。**  
質問や提案があれば、GitHub Issueでお知らせください。
