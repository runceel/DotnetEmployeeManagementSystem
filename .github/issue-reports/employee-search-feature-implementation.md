# 従業員検索機能の実装

## 1. 概要

### 目的
従業員一覧画面に検索・フィルタリング機能を追加し、大量の従業員データから目的の従業員を素早く見つけられるようにする。

### 背景
現在の従業員一覧画面（`/employees`）は全従業員を一括取得して表示しているため、以下の課題がある：
- 従業員数が増えると目的の従業員を見つけにくい
- パフォーマンスの問題（全件取得）
- ユーザビリティの低下

### 対象画面
- [Employees.razor](../../src/WebApps/BlazorWeb/Components/Pages/Employees.razor)

---

## 2. 機能要件

### 2.1 検索条件

| 検索項目 | 検索方式 | 必須/任意 |
|----------|----------|-----------|
| 氏名（FirstName/LastName） | 部分一致 | 任意 |
| メールアドレス | 部分一致 | 任意 |
| 部署 | 完全一致（ドロップダウン） | 任意 |
| 役職 | 部分一致 | 任意 |
| 入社日（開始） | 以降 | 任意 |
| 入社日（終了） | 以前 | 任意 |

### 2.2 ソート機能

| ソート項目 | デフォルト順 |
|------------|--------------|
| 氏名 | 昇順 |
| 部署 | 昇順 |
| 役職 | 昇順 |
| 入社日 | 降順 |
| 作成日時 | 降順 |

### 2.3 ページネーション

| 項目 | 値 |
|------|-----|
| デフォルト表示件数 | 10件 |
| 選択可能件数 | 10, 25, 50, 100 |

---

## 3. API設計

### 3.1 新規エンドポイント

```
GET /api/employees/search
```

### 3.2 クエリパラメータ

| パラメータ | 型 | 説明 | 例 |
|------------|-----|------|-----|
| `name` | string | 氏名（部分一致） | `田中` |
| `email` | string | メールアドレス（部分一致） | `@example.com` |
| `department` | string | 部署名（完全一致） | `開発部` |
| `position` | string | 役職（部分一致） | `マネージャー` |
| `hireDateFrom` | DateTime | 入社日（開始） | `2024-01-01` |
| `hireDateTo` | DateTime | 入社日（終了） | `2024-12-31` |
| `sortBy` | string | ソート項目 | `name`, `department`, `hireDate` |
| `sortOrder` | string | ソート順 | `asc`, `desc` |
| `page` | int | ページ番号（1始まり） | `1` |
| `pageSize` | int | 1ページあたりの件数 | `10` |

### 3.3 リクエストDTO

```csharp
// src/Shared/Contracts/EmployeeService/SearchEmployeesRequest.cs
namespace Shared.Contracts.EmployeeService;

public record SearchEmployeesRequest
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Department { get; init; }
    public string? Position { get; init; }
    public DateTime? HireDateFrom { get; init; }
    public DateTime? HireDateTo { get; init; }
    public string SortBy { get; init; } = "name";
    public string SortOrder { get; init; } = "asc";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
```

### 3.4 レスポンスDTO

```csharp
// src/Shared/Contracts/EmployeeService/SearchEmployeesResponse.cs
namespace Shared.Contracts.EmployeeService;

public record SearchEmployeesResponse
{
    public IEnumerable<EmployeeDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
```

---

## 4. 実装範囲

### 4.1 バックエンド

| レイヤー | ファイル | 変更内容 |
|----------|----------|----------|
| Domain | `IEmployeeRepository.cs` | `SearchAsync` メソッド追加 |
| Application | `IEmployeeService.cs` | `SearchAsync` メソッド追加 |
| Application | `EmployeeService.cs` | 検索ロジック実装 |
| Infrastructure | `EmployeeRepository.cs` | EF Core クエリ実装 |
| API | `EmployeeEndpoints.cs` | `/search` エンドポイント追加 |
| Shared | `SearchEmployeesRequest.cs` | 新規作成 |
| Shared | `SearchEmployeesResponse.cs` | 新規作成 |

### 4.2 フロントエンド

| ファイル | 変更内容 |
|----------|----------|
| `IEmployeeApiClient.cs` | `SearchEmployeesAsync` メソッド追加 |
| `EmployeeApiClient.cs` | API呼び出し実装 |
| `Employees.razor` | 検索フォーム・ページネーション追加 |

---

## 5. 実装手順

### Step 1: 共有DTOの作成

1. `src/Shared/Contracts/EmployeeService/SearchEmployeesRequest.cs` を作成
2. `src/Shared/Contracts/EmployeeService/SearchEmployeesResponse.cs` を作成

### Step 2: Domain層の変更

1. `IEmployeeRepository.cs` に `SearchAsync` メソッドを追加

```csharp
Task<(IEnumerable<Employee> Items, int TotalCount)> SearchAsync(
    string? name,
    string? email,
    string? department,
    string? position,
    DateTime? hireDateFrom,
    DateTime? hireDateTo,
    string sortBy,
    string sortOrder,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);
```

### Step 3: Infrastructure層の実装

1. `EmployeeRepository.cs` に検索クエリを実装
2. EF Core の `Where`, `OrderBy`, `Skip`, `Take` を使用

### Step 4: Application層の実装

1. `IEmployeeService.cs` に `SearchAsync` メソッドを追加
2. `EmployeeService.cs` でDTOへの変換とビジネスロジックを実装

### Step 5: API層の実装

1. `EmployeeEndpoints.cs` に `/api/employees/search` エンドポイントを追加

```csharp
employees.MapGet("/search", SearchEmployees)
    .WithName("SearchEmployees")
    .Produces<SearchEmployeesResponse>();
```

### Step 6: BlazorWeb APIクライアントの実装

1. `IEmployeeApiClient.cs` にメソッド追加
2. `EmployeeApiClient.cs` に実装

### Step 7: Blazor UIの実装

1. `Employees.razor` に検索フォームを追加
2. MudBlazor の `MudTextField`, `MudSelect`, `MudDatePicker` を使用
3. `MudTable` のページネーション機能を有効化

---

## 6. UI設計

### 6.1 検索フォームのレイアウト

```
┌─────────────────────────────────────────────────────────────┐
│ 従業員検索                                                    │
├─────────────────────────────────────────────────────────────┤
│ ┌─────────────┐ ┌─────────────┐ ┌─────────────────────────┐ │
│ │ 氏名        │ │ 部署 ▼     │ │ 役職                    │ │
│ └─────────────┘ └─────────────┘ └─────────────────────────┘ │
│ ┌─────────────┐ ┌─────────────┐ ┌───────┐ ┌───────────────┐ │
│ │ 入社日(From)│ │ 入社日(To) │ │ 検索  │ │ クリア        │ │
│ └─────────────┘ └─────────────┘ └───────┘ └───────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### 6.2 MudBlazor コンポーネント

- 検索フォーム: `MudPaper` + `MudGrid`
- テキスト入力: `MudTextField`
- 部署選択: `MudSelect`
- 日付選択: `MudDatePicker`
- ボタン: `MudButton`
- テーブル: `MudTable` with `ServerData`
- ページネーション: `MudTablePager`

---

## 7. テスト計画

### 7.1 ユニットテスト

| テスト対象 | テスト内容 |
|------------|------------|
| `EmployeeService.SearchAsync` | 各検索条件の動作確認 |
| `EmployeeRepository.SearchAsync` | クエリ生成の確認 |

### 7.2 統合テスト

| テスト対象 | テスト内容 |
|------------|------------|
| `/api/employees/search` | エンドポイントの正常動作 |
| ページネーション | ページ切り替えの動作 |
| ソート | 各項目でのソート動作 |

### 7.3 E2Eテスト

| テスト対象 | テスト内容 |
|------------|------------|
| 検索フォーム | 入力・検索・クリア動作 |
| 検索結果表示 | 正しい結果の表示 |
| ページネーション | ページ切り替えUI |

---

## 8. 実装時の注意点

### 8.1 パフォーマンス

- 検索クエリにはインデックスを考慮
- 大量データの場合は非同期処理を適切に使用
- フロントエンドでのデバウンス処理（入力中の連続API呼び出し防止）

### 8.2 セキュリティ

- SQLインジェクション対策（EF Coreのパラメータ化クエリを使用）
- 入力値のバリデーション

### 8.3 ユーザビリティ

- 検索中のローディング表示
- 検索結果が0件の場合の適切なメッセージ
- 検索条件のクリアボタン

---

## 9. 関連ドキュメント

- [開発ガイド](../../docs/development-guide.md)
- [アーキテクチャ詳細](../../docs/architecture-detailed.md)
- [UI デザインカタログ - 一覧画面パターン](../../docs/design-catalog/patterns/list-page.md)
- [従業員管理マニュアル](../../docs/manual/05-employees.md)

---

## 10. 実装チェックリスト

- [ ] `SearchEmployeesRequest.cs` 作成
- [ ] `SearchEmployeesResponse.cs` 作成
- [ ] `IEmployeeRepository.SearchAsync` 追加
- [ ] `EmployeeRepository.SearchAsync` 実装
- [ ] `IEmployeeService.SearchAsync` 追加
- [ ] `EmployeeService.SearchAsync` 実装
- [ ] `EmployeeEndpoints.cs` に `/search` 追加
- [ ] `IEmployeeApiClient.SearchEmployeesAsync` 追加
- [ ] `EmployeeApiClient.SearchEmployeesAsync` 実装
- [ ] `Employees.razor` 検索フォーム追加
- [ ] `Employees.razor` ページネーション追加
- [ ] ユニットテスト作成
- [ ] 統合テスト作成
- [ ] マニュアル更新
