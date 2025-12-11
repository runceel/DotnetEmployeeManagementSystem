# 従業員管理システム - AI エージェント向けガイド

このドキュメントは、AIエージェントがこのプロジェクトで効果的にコード生成・修正を行うための必須情報です。

## コード生成前の必須確認
1. **既存の類似実装を必ず検索・参照**してから新規コードを生成すること
2. **クリーンアーキテクチャの層分離**を厳守すること（Domain → Application → Infrastructure → API）
3. **日本語**でコメント・UI文言・XMLドキュメントを記述すること
4. **MudBlazor コンポーネント**を使用し、生の HTML/CSS は最小限にすること

### ファイル配置ルール

| 作成物 | 配置先 | 参照例 |
|--------|--------|--------|
| エンティティ | `src/Services/{ServiceName}/Domain/Entities/` | `Employee.cs` |
| リポジトリIF | `src/Services/{ServiceName}/Domain/Repositories/` | `IEmployeeRepository.cs` |
| ユースケース/サービスIF | `src/Services/{ServiceName}/Application/UseCases/` | `IEmployeeService.cs` |
| リポジトリ実装 | `src/Services/{ServiceName}/Infrastructure/Repositories/` | `EmployeeRepository.cs` |
| DbContext | `src/Services/{ServiceName}/Infrastructure/Data/` | `EmployeeDbContext.cs` |
| API エンドポイント | `src/Services/{ServiceName}/API/Endpoints/` | `EmployeeEndpoints.cs` |
| 共有 DTO | `src/Shared/Contracts/{ServiceName}/` | `EmployeeDto.cs` |
| イベント | `src/Shared/Contracts/Events/` | `EmployeeCreatedEvent.cs` |
| Blazor ページ | `src/WebApps/BlazorWeb/Components/Pages/` | `Employees.razor` |
| Blazor ダイアログ | `src/WebApps/BlazorWeb/Components/Dialogs/` | `EmployeeEditDialog.razor` |

### DI 登録の場所

| サービス種別 | 登録ファイル |
|-------------|-------------|
| Infrastructure層（リポジトリ等） | `Infrastructure/DependencyInjection.cs` の `AddInfrastructure()` |
| Application層（ユースケース等） | `API/Program.cs` に直接 `AddScoped<>()` |
| API クライアント（BlazorWeb用） | `BlazorWeb/Program.cs` に `AddHttpClient<>()` |

### 既存コード参照パターン

| タスク | 参照すべき既存実装 |
|--------|-------------------|
| 新しいエンティティ追加 | `EmployeeService/Domain/Entities/Employee.cs` |
| 新しい API エンドポイント | `EmployeeService/API/Endpoints/EmployeeEndpoints.cs` |
| 新しい Blazor 一覧画面 | `BlazorWeb/Components/Pages/Employees.razor` |
| 新しい Blazor 詳細画面 | `BlazorWeb/Components/Pages/EmployeeDetail.razor` |
| 新しい編集ダイアログ | `BlazorWeb/Components/Dialogs/EmployeeEditDialog.razor` |
| Redis イベント発行 | `EmployeeService/Application/UseCases/EmployeeService.cs` |
| Redis イベント購読 | `NotificationService/Infrastructure/Workers/EmployeeEventConsumer.cs` |

---

## プロジェクト概要

**従業員管理システム** - .NET 10 + .NET Aspire 13.0.0 のマイクロサービスアーキテクチャ

### 技術スタック
- **.NET 10** / **.NET Aspire 13.0.0** - オーケストレーション
- **Blazor Web App + MudBlazor** - フロントエンド
- **Entity Framework Core 10** - ORM（SQLite開発環境）
- **Redis** - Pub/Sub メッセージング
- **OpenTelemetry** - 可観測性

### マイクロサービス構成（`src/AppHost/AppHost.cs`）
- **EmployeeService** - 従業員・部署管理
- **AuthService** - 認証（ASP.NET Core Identity）
- **AttendanceService** - 勤怠管理
- **NotificationService** - イベント駆動通知
- **BlazorWeb** - UI フロントエンド

## クリーンアーキテクチャ

```
API (Presentation)
  ↓ 依存
Application (Business Logic)
  ↓ 依存
Domain (Core Business Rules) ← 依存なし
  ↑ 実装
Infrastructure (Data Access)
```

**原則**：
- Domain層は他の層に依存しない
- Infrastructure層がDomain層のインターフェースを実装
- 外部依存（EF Core, Redis等）はInfrastructure層のみ

## 開発コマンド

```bash
# 全サービス起動
dotnet run --project src/AppHost

# テスト実行
dotnet test

# EFマイグレーション（例: EmployeeService）
cd src/Services/EmployeeService/API
dotnet ef migrations add <Name> --project ../Infrastructure
dotnet ef database update --project ../Infrastructure
```

## コーディング規約

### 必須ルール
- **命名**: PascalCase（クラス、メソッド）、camelCase（変数、パラメータ）
- **非同期メソッド**: `Async` サフィックス必須
- **Null許容参照型**: 有効（`<Nullable>enable</Nullable>`）
- **1ファイル1クラス**、ファイル名 = クラス名

### テスト命名規則
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
```

## Blazor UI 開発

### 必須パターン
1. **ローディング状態**: `MudProgressCircular` または `MudSkeleton`
2. **エラー状態**: `MudAlert` + 再試行ボタン
3. **空状態**: データなし時の `MudAlert`
4. **認証チェック**: `AuthStateService.IsAuthenticated` / `IsAdmin`

### 状態管理テンプレート
```csharp
private bool _loading = true;
private bool _error = false;
private string _errorMessage = string.Empty;
```

### UI デザインカタログ（必読）
- **[一覧画面](docs/design-catalog/patterns/list-page.md)**
- **[詳細画面](docs/design-catalog/patterns/detail-page.md)**
- **[編集画面](docs/design-catalog/patterns/edit-dialog.md)**
- **[ダッシュボード](docs/design-catalog/patterns/dashboard.md)**
- **[推奨・非推奨ルール](docs/design-catalog/dos-and-donts.md)**

## 詳細ドキュメント

| 目的 | ドキュメント |
|------|-------------|
| 新機能追加手順 | [development-guide.md](docs/development-guide.md) |
| アーキテクチャ詳細 | [architecture-detailed.md](docs/architecture-detailed.md) |
| DB マイグレーション | [database.md](docs/database.md) |
| 通知サービス実装 | [notification-service.md](docs/notification-service.md) |
| MCP サーバー実装 | [mcp-implementation-guide.md](docs/mcp-implementation-guide.md) |
| 認証実装 | [authorization-implementation.md](docs/authorization-implementation.md) |
