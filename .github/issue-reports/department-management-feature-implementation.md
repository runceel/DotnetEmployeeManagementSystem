# 部署管理機能実装完了報告

## 実装日時
2025-11-11

## 実装概要
Blazor Webアプリケーションに部署マスタのCRUD管理機能を実装しました。従業員登録・編集時の部署入力欄をAPI連携での選択式（ドロップダウン）に変更し、管理者のみが部署マスタの編集を実行できるよう認可ガードを追加しました。

## 実装内容

### 1. バックエンド実装

#### 1.1 部署削除時の従業員チェック機能
**ファイル**: `src/Services/EmployeeService/Domain/Repositories/IDepartmentRepository.cs`
- 新しいメソッド `HasEmployeesAsync` を追加
- 部署に所属する従業員が存在するか確認する機能

**ファイル**: `src/Services/EmployeeService/Infrastructure/Repositories/DepartmentRepository.cs`
- `HasEmployeesAsync` の実装を追加
- Entity Framework Core の `AnyAsync` を使用して効率的にチェック

#### 1.2 部署削除ビジネスロジックの更新
**ファイル**: `src/Services/EmployeeService/Application/UseCases/DepartmentService.cs`
- `DeleteAsync` メソッドを更新
- 従業員が所属している部署は削除できないように検証を追加
- 従業員が所属している場合は `InvalidOperationException` をスロー

```csharp
public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
{
    // 部署を取得
    var department = await _repository.GetByIdAsync(id, cancellationToken);
    if (department is null)
        return false;

    // 従業員が所属している部署は削除できない
    if (await _repository.HasEmployeesAsync(department.Name, cancellationToken))
    {
        throw new InvalidOperationException("従業員が所属している部署は削除できません。");
    }

    return await _repository.DeleteAsync(id, cancellationToken);
}
```

#### 1.3 API エンドポイントのエラーハンドリング
**ファイル**: `src/Services/EmployeeService/API/Program.cs`
- 部署削除エンドポイントに `try-catch` を追加
- `InvalidOperationException` を `BadRequest (400)` として返却
- エラーメッセージをクライアントに適切に伝達

### 2. フロントエンド実装

#### 2.1 API クライアントの作成
**ファイル**: 
- `src/WebApps/BlazorWeb/Services/IDepartmentApiClient.cs` (新規作成)
- `src/WebApps/BlazorWeb/Services/DepartmentApiClient.cs` (新規作成)

機能:
- 部署の全件取得 (`GetAllDepartmentsAsync`)
- 部署の個別取得 (`GetDepartmentByIdAsync`)
- 部署の新規作成 (`CreateDepartmentAsync`)
- 部署の更新 (`UpdateDepartmentAsync`)
- 部署の削除 (`DeleteDepartmentAsync`)
- 認証ヘッダーの自動付与
- エラーハンドリングとログ出力

#### 2.2 依存性注入の登録
**ファイル**: `src/WebApps/BlazorWeb/Program.cs`
```csharp
builder.Services.AddHttpClient<IDepartmentApiClient, DepartmentApiClient>("employeeservice-api", client =>
{
    client.BaseAddress = new Uri("http://employeeservice-api");
});
```

#### 2.3 部署フォームモデルの作成
**ファイル**: `src/WebApps/BlazorWeb/Models/DepartmentFormModel.cs` (新規作成)
- 部署名 (`Name`)
- 部署説明 (`Description`)

#### 2.4 部署フォームダイアログの作成
**ファイル**: `src/WebApps/BlazorWeb/Components/Dialogs/DepartmentFormDialog.razor` (新規作成)

機能:
- MudBlazor の `MudDialog` を使用
- 部署名と部署説明の入力フィールド
- バリデーション機能（必須チェック）
- キャンセルと保存ボタン

#### 2.5 部署管理ページの作成
**ファイル**: `src/WebApps/BlazorWeb/Components/Pages/Departments.razor` (新規作成)

機能:
- 部署一覧の表示 (MudTable)
- 部署の新規追加（管理者のみ）
- 部署の編集（管理者のみ）
- 部署の削除（管理者のみ）
- 削除時の確認ダイアログ
- 従業員が所属している部署の削除時のエラー表示
- ローディング表示
- エラーハンドリングとメッセージ表示

UI要素:
- ページタイトル
- 認証状態の表示
- 管理者権限の確認とガイダンス
- データテーブル（部署名、部署説明、作成日時、更新日時、アクション）
- アクションボタン（編集、削除）

#### 2.6 ナビゲーションメニューの更新
**ファイル**: `src/WebApps/BlazorWeb/Components/Layout/NavMenu.razor`
- 「部署管理」リンクを追加
- アイコン: `Icons.Material.Filled.Business`
- パス: `/departments`

#### 2.7 従業員フォームダイアログの更新
**ファイル**: `src/WebApps/BlazorWeb/Components/Dialogs/EmployeeFormDialog.razor`

変更内容:
- 部署入力欄をテキストフィールドからドロップダウン (`MudSelect`) に変更
- `IDepartmentApiClient` を注入
- ダイアログ初期化時に部署一覧を取得
- 取得した部署リストから選択可能

```razor
<MudSelect @bind-Value="_model.Department" 
           Label="部署" 
           Required="true" 
           RequiredError="部署を選択してください。"
           Variant="Variant.Outlined"
           Class="mb-3">
    @if (_departments is not null)
    {
        @foreach (var dept in _departments)
        {
            <MudSelectItem Value="@dept.Name">@dept.Name</MudSelectItem>
        }
    }
</MudSelect>
```

### 3. テストの追加

#### 3.1 ユニットテストの更新
**ファイル**: `tests/EmployeeService.Application.Tests/UseCases/DepartmentServiceTests.cs`

追加したテスト:
1. `DeleteAsync_WithExistingIdAndNoEmployees_ShouldReturnTrue`
   - 従業員がいない部署の削除が成功することを確認

2. `DeleteAsync_WithNonExistingId_ShouldReturnFalse`
   - 存在しない部署の削除が false を返すことを確認

3. `DeleteAsync_WithDepartmentThatHasEmployees_ShouldThrowInvalidOperationException`
   - 従業員が所属している部署の削除が `InvalidOperationException` をスローすることを確認
   - エラーメッセージが正しいことを確認

### 4. テスト結果

#### 4.1 ユニットテスト
```
Total tests: 136
     Passed: 136
     Failed: 0
```

全てのユニットテストが成功しています。

#### 4.2 統合テスト
```
Total tests: 45
     Passed: 45
     Failed: 0
```

全ての統合テストが成功しています。部署管理APIの動作が正常に確認されました。

## セキュリティ考慮事項

### 認可制御
- 部署の作成、更新、削除は管理者 (Admin) のみが実行可能
- API レベルで `RequireAuthorization("AdminPolicy")` を適用
- UI レベルでも管理者権限チェックを実施
- 一般ユーザーには閲覧のみ許可

### データ整合性
- 従業員が所属している部署は削除不可
- データベース制約違反を事前にチェック
- エラーメッセージを適切に表示

### エラーハンドリング
- API エラーをキャッチしてユーザーフレンドリーなメッセージに変換
- ログ出力により問題のトレーシングが可能
- 不正なリクエストは適切な HTTP ステータスコードで拒否

## アーキテクチャパターンの適用

### クリーンアーキテクチャ
- Domain層: エンティティとリポジトリインターフェース
- Application層: ビジネスロジック (DepartmentService)
- Infrastructure層: データアクセス実装 (DepartmentRepository)
- API層: エンドポイント定義
- UI層: Blazor コンポーネント

### 依存性の原則
- 全ての依存関係が内側（Domain層）に向かっている
- インターフェースを使用して疎結合を実現
- 依存性注入により実装を切り替え可能

### SOLID原則の適用
- **S (Single Responsibility)**: 各クラスは単一の責任を持つ
- **O (Open/Closed)**: インターフェースにより拡張可能
- **L (Liskov Substitution)**: インターフェースの実装が置き換え可能
- **I (Interface Segregation)**: 必要なメソッドのみを定義
- **D (Dependency Inversion)**: 抽象に依存、具象に依存しない

## コーディング規約の遵守

### C# コーディングスタイル
- PascalCase: クラス、メソッド名
- camelCase: 変数、パラメータ名
- 非同期メソッドに `Async` サフィックス
- XML ドキュメントコメント: 公開APIに必須
- `ArgumentNullException.ThrowIfNull` を使用

### エラーハンドリング
- 適切な例外タイプを使用 (`InvalidOperationException`)
- エラーを無視せず、ログ出力して再スロー
- ユーザーフレンドリーなエラーメッセージ

### 非同期プログラミング
- `CancellationToken` を全てのメソッドに渡す
- `ConfigureAwait(false)` は使用しない（UI コンテキストで実行）
- 非同期メソッドチェーンを適切に処理

## 実装ファイル一覧

### 新規作成ファイル
1. `src/WebApps/BlazorWeb/Services/IDepartmentApiClient.cs`
2. `src/WebApps/BlazorWeb/Services/DepartmentApiClient.cs`
3. `src/WebApps/BlazorWeb/Models/DepartmentFormModel.cs`
4. `src/WebApps/BlazorWeb/Components/Dialogs/DepartmentFormDialog.razor`
5. `src/WebApps/BlazorWeb/Components/Pages/Departments.razor`

### 変更ファイル
1. `src/Services/EmployeeService/Domain/Repositories/IDepartmentRepository.cs`
2. `src/Services/EmployeeService/Infrastructure/Repositories/DepartmentRepository.cs`
3. `src/Services/EmployeeService/Application/UseCases/DepartmentService.cs`
4. `src/Services/EmployeeService/API/Program.cs`
5. `src/WebApps/BlazorWeb/Program.cs`
6. `src/WebApps/BlazorWeb/Components/Layout/NavMenu.razor`
7. `src/WebApps/BlazorWeb/Components/Dialogs/EmployeeFormDialog.razor`
8. `tests/EmployeeService.Application.Tests/UseCases/DepartmentServiceTests.cs`

## 今後の改善案

### 機能拡張
1. 部署の階層構造サポート（親部署・子部署）
2. 部署マネージャーの割り当て機能
3. 部署の有効/無効フラグ（論理削除）
4. 部署の並び順変更機能
5. 部署の統計情報表示（所属従業員数など）

### UX改善
1. 部署の検索・フィルタリング機能
2. 部署の並び替え機能（名前順、作成日順など）
3. ページネーション（部署数が多い場合）
4. 部署削除時の詳細な警告（所属従業員数を表示）
5. 部署のエクスポート・インポート機能

### パフォーマンス
1. 部署一覧のキャッシング
2. 仮想スクロール（大量データ対応）
3. 部署取得のバッチ処理

### テスト
1. Blazor コンポーネントの単体テスト追加
2. E2Eテストの追加（Playwright等）
3. パフォーマンステストの追加

## 完了基準の確認

### 要件達成状況
- ✅ 部署一覧の取得および表示
- ✅ 部署の新規追加（管理者のみ）
- ✅ 部署の編集（管理者のみ）
- ✅ 部署の削除（管理者のみ）
- ✅ 従業員登録・編集時の部署ドロップダウン選択
- ✅ 管理者のみ部署マスタの編集が可能（認可ガード）
- ✅ バリデーション実装
- ✅ エラーハンドリング実装
- ✅ 従業員が所属している部署は削除不可
- ✅ テストの追加と実行

### 品質基準
- ✅ 全ユニットテストが成功
- ✅ 全統合テストが成功
- ✅ コーディング規約に準拠
- ✅ アーキテクチャパターンに準拠
- ✅ SOLID原則に準拠
- ✅ セキュリティ考慮事項を実装
- ✅ ログ出力を適切に実装
- ✅ エラーメッセージが日本語化

## まとめ

部署管理機能の実装が完了しました。バックエンドでは従業員が所属している部署の削除を防ぐ検証ロジックを追加し、フロントエンドでは管理者のみが操作可能な部署CRUD画面と従業員編集時の部署選択ドロップダウンを実装しました。全てのテストが成功し、コーディング規約とアーキテクチャパターンに準拠した高品質な実装となっています。

---

**実装者**: GitHub Copilot Agent
**レビュー**: 必要に応じてコードレビューを実施してください
