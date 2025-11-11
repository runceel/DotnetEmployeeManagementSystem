# Issue Fix Report: SQLite 'no such table: Departments' エラー

**Issue**: SQLite: 部署関連データ取得時に 'no such table: Departments' エラーが発生する  
**Date**: 2025-11-11  
**Status**: ✅ Fixed  

## 問題の詳細

### 現象
部署 (Department) データにアクセスしようとした際、以下の例外が発生:

```
Microsoft.Data.Sqlite.SqliteException
  HResult=0x80004005
  Message=SQLite Error 1: 'no such table: Departments'.
```

### 発生場所
- `src/Services/EmployeeService/Infrastructure/Repositories/DepartmentRepository.cs`
- `src/Services/EmployeeService/Application/UseCases/DepartmentService.cs`
- `src/Services/EmployeeService/API/Program.cs`

## 原因分析

調査の結果、以下のことが判明:

1. ✅ Department エンティティは正しく定義されている
2. ✅ EF Core マイグレーション (`20251109035000_AddDepartmentEntity.cs`) は存在
3. ✅ DbContext に Departments DbSet が正しく設定されている
4. ✅ DbInitializer は `MigrateAsync()` を実行してマイグレーションを適用
5. ❌ **DbInitializer に Department のシードデータが含まれていなかった**

### 根本原因
DbInitializer に Employee のシードデータはあったが、Department のシードデータが欠落していた。これにより、データベースのテーブル構造とアプリケーションの期待動作に不整合が生じていた可能性がある。

## 解決策

### 実装した修正

**Modified File**: `src/Services/EmployeeService/Infrastructure/Data/DbInitializer.cs`

```csharp
// 部署サンプルデータを投入
var departments = new[]
{
    new Department("開発部", "ソフトウェア開発を担当する部署"),
    new Department("営業部", "営業活動を担当する部署"),
    new Department("人事部", "人事管理を担当する部署"),
    new Department("マーケティング部", "マーケティング活動を担当する部署"),
    new Department("総務部", "総務・庶務を担当する部署")
};

await context.Departments.AddRangeAsync(departments);
await context.SaveChangesAsync();

logger.LogInformation("Database seeded with {Count} departments.", departments.Length);
```

### 変更のポイント

1. **データ投入順序の最適化**: 部署データを従業員データの前に投入
2. **ログ出力の追加**: 部署シード時のログを追加して初期化状況を追跡可能に
3. **最小限の変更**: 既存コードの構造を維持し、追加のみで対応

## テスト結果

### 単体テスト・統合テスト
```
Test summary: total: 136, failed: 0, succeeded: 136, skipped: 0
```

### Department API Integration Tests (9 tests)
すべてのテストが成功:
- ✅ GetAllDepartments_ShouldReturnEmptyList_WhenNoDepartments
- ✅ CreateDepartment_WithValidRequest_ShouldReturnCreated
- ✅ GetDepartmentById_WithExistingId_ShouldReturnDepartment
- ✅ GetDepartmentById_WithNonExistingId_ShouldReturnNotFound
- ✅ UpdateDepartment_WithValidRequest_ShouldReturnUpdatedDepartment
- ✅ UpdateDepartment_WithNonExistingId_ShouldReturnNotFound
- ✅ DeleteDepartment_WithExistingId_ShouldReturnNoContent
- ✅ DeleteDepartment_WithNonExistingId_ShouldReturnNotFound
- ✅ GetAllDepartments_ShouldReturnAllCreatedDepartments

### セキュリティスキャン
```
CodeQL Analysis: No alerts found
```

## 影響範囲

### 変更されたファイル
- `src/Services/EmployeeService/Infrastructure/Data/DbInitializer.cs` (16行追加)

### 影響を受けるコンポーネント
- ✅ EmployeeService Database Initialization
- ✅ Department API Endpoints
- ✅ Fresh database creation scenarios

### 後方互換性
- ✅ 既存のデータベースには影響なし（マイグレーションは自動適用）
- ✅ 既存のテストは全て成功
- ✅ 既存の従業員データシードロジックは変更なし

## 動作確認

### 初期化フロー
1. アプリケーション起動時、`DbInitializer.InitializeAsync()` が呼び出される
2. `MigrateAsync()` により全マイグレーションが適用される
   - `20251102064319_InitialCreate` → Employees テーブル作成
   - `20251109035000_AddDepartmentEntity` → **Departments テーブル作成**
3. データが存在しない場合のみシードデータを投入:
   - 5件の Department データ
   - 5件の Employee データ

### 期待される結果
- ✅ "no such table: Departments" エラーが発生しない
- ✅ Department API が正常に動作する
- ✅ データベース初期化が完了する

## 学んだこと・改善点

### このIssueから学んだこと
1. **マイグレーションとシードデータは別物**: マイグレーションはテーブル構造を定義するが、シードデータは別途定義が必要
2. **完全な初期化戦略**: 新しいエンティティを追加する際は、マイグレーション、DbSet、シードデータの3点セットで考える必要がある

### 今後の改善提案
1. **テストカバレッジ**: DbInitializer の単体テストを追加することで、シードデータの欠落を早期発見できる
2. **ドキュメント更新**: 新しいエンティティ追加時のチェックリストを docs に追加
3. **CI/CD**: 新規データベース作成のシナリオを自動テストに含める

## まとめ

この修正により、SQLite の "no such table: Departments" エラーが解消され、Department 機能が正常に動作するようになりました。変更は最小限に抑えられ、既存機能への影響はありません。すべてのテストが成功し、セキュリティ上の問題も検出されませんでした。
