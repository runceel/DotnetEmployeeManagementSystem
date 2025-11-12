# 勤怠管理用デモシードデータ生成機能 - 実装完了報告

**作成日**: 2025-11-12  
**関連Issue**: 勤怠管理用デモシードデータ生成機能の追加

## 概要

AttendanceServiceのシードデータ生成機能を改善し、EmployeeServiceから取得した実際の従業員IDを使用して、過去3ヶ月分の勤怠データを自動生成する機能を実装しました。

## 実装内容

### 1. 動的な従業員ID取得

**変更前:**
```csharp
// ハードコードされた従業員ID
var employeeIds = new[]
{
    Guid.Parse("00000000-0000-0000-0000-000000000001"),
    Guid.Parse("00000000-0000-0000-0000-000000000002"),
    // ...
};
```

**変更後:**
```csharp
// EmployeeService APIから動的に取得
var httpClient = httpClientFactory.CreateClient("EmployeeService");
var response = await httpClient.GetAsync("/api/employees");
var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
var employeeIds = employees.Select(e => e.Id).ToList();
await DbInitializer.InitializeAsync(app.Services, employeeIds);
```

### 2. シードデータ制御オプション

#### 環境変数による制御

`appsettings.json`または環境変数で設定可能:

```json
{
  "AttendanceService": {
    "SkipSeedData": true
  }
}
```

#### プログラム的な制御

```csharp
// 従業員IDを指定してシードデータ生成
await DbInitializer.InitializeAsync(serviceProvider, employeeIds);

// シードデータ生成をスキップ
await DbInitializer.InitializeAsync(serviceProvider, null);
```

### 3. 開発用クリーンアップエンドポイント

開発環境でのみ利用可能な削除エンドポイント:

```bash
# 全シードデータをクリア
curl -X DELETE http://localhost:5001/api/dev/clear-seed-data
```

**レスポンス例:**
```json
{
  "message": "Seed data cleared successfully"
}
```

## 生成されるデータ

### 勤怠記録 (Attendance)
- **期間**: 過去3ヶ月分
- **対象日**: 平日のみ（土日を除く）
- **出勤率**: 90%
- **勤怠種別**:
  - Normal (通常勤務): 80%
  - Remote (リモートワーク): 10%
  - BusinessTrip (出張): 5%
  - HalfDay (半日勤務): 5%
- **出勤時刻**: 8:00-9:30の間でランダム
- **退勤時刻**: 出勤時刻から7-10時間後
- **備考**: 10%の確率で追加

### 休暇申請 (LeaveRequest)
- **件数**: 従業員1人あたり2-3件
- **休暇種別**:
  - PaidLeave (有給休暇)
  - SickLeave (病気休暇)
  - SpecialLeave (特別休暇)
  - UnpaidLeave (無給休暇)
- **期間**: 現在から10-100日後の未来日
- **日数**: 1-3日間
- **ステータス**:
  - Approved (承認済み): 75%
  - Rejected (却下): 10%
  - Pending (承認待ち): 15%

## 使用方法

### 1. 通常の起動（自動シード生成）

```bash
# Aspire AppHost経由で全サービスを起動
cd src/AppHost
dotnet run
```

AttendanceServiceが起動すると、以下の順序で処理されます:

1. EmployeeServiceから従業員リストを取得
2. 従業員IDリストをDbInitializerに渡す
3. データベースをチェックし、データが存在しない場合のみシード生成
4. 過去3ヶ月分の勤怠データと休暇申請を生成

### 2. シード生成の無効化

**appsettings.Development.json**:
```json
{
  "AttendanceService": {
    "SkipSeedData": true
  }
}
```

### 3. シードデータのクリア（開発環境のみ）

```bash
# 全シードデータを削除
curl -X DELETE http://localhost:5001/api/dev/clear-seed-data

# 再度シード生成する場合はサービスを再起動
```

## テスト

### テストカバレッジ

新規追加されたテスト:

| テストケース | 説明 |
|------------|------|
| `InitializeAsync_WithEmployeeIds_ShouldGenerateSeedData` | 従業員IDを指定した場合、正常にシードデータが生成される |
| `InitializeAsync_WithoutEmployeeIds_ShouldSkipSeedData` | 従業員IDがnullの場合、シード生成がスキップされる |
| `InitializeAsync_WithSkipSeedDataConfig_ShouldSkipSeedData` | 設定で無効化した場合、シード生成がスキップされる |
| `InitializeAsync_WhenDataAlreadyExists_ShouldSkipSeedData` | データが既に存在する場合、重複生成を防ぐ |
| `ClearSeedDataAsync_ShouldRemoveAllData` | クリーンアップ機能が正常に動作する |

### テスト実行

```bash
# 全テスト実行
dotnet test

# AttendanceServiceのみ
dotnet test tests/AttendanceService.Integration.Tests/

# DbInitializerのテストのみ
dotnet test --filter "FullyQualifiedName~DbInitializerTests"
```

**結果:**
```
✓ 全テストパス: 141/141
  - AttendanceService.Integration.Tests: 31/31 (新規5テスト含む)
  - その他既存テスト: 110/110
```

## 技術的詳細

### アーキテクチャ

```
┌─────────────────────┐
│  AttendanceService  │
│    Program.cs       │
└──────────┬──────────┘
           │ 1. HTTP GET /api/employees
           ↓
┌─────────────────────┐
│  EmployeeService    │
│       API           │
└──────────┬──────────┘
           │ 2. 従業員IDリスト返却
           ↓
┌─────────────────────┐
│  DbInitializer      │
│  InitializeAsync()  │
└──────────┬──────────┘
           │ 3. シードデータ生成
           ↓
┌─────────────────────┐
│ AttendanceDbContext │
│     (SQLite)        │
└─────────────────────┘
```

### データベース互換性

- **SQLite**: 本番・開発環境
- **InMemoryDatabase**: テスト環境

DbInitializerは両方の環境に対応:

```csharp
var isInMemory = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
if (isInMemory)
{
    await context.Database.EnsureCreatedAsync();
}
else
{
    await context.Database.MigrateAsync();
}
```

### エラーハンドリング

EmployeeServiceが利用できない場合の動作:

```csharp
try
{
    var response = await httpClient.GetAsync("/api/employees");
    if (response.IsSuccessStatusCode)
    {
        // 従業員IDを取得してシード生成
    }
    else
    {
        logger.LogWarning("Failed to fetch employees. Skipping seed data.");
        await DbInitializer.InitializeAsync(app.Services);  // 空で実行
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Error fetching employees. Skipping seed data.");
    await DbInitializer.InitializeAsync(app.Services);  // 空で実行
}
```

## ベストプラクティス

### 1. 開発環境での使用

```bash
# 1. 全サービスを起動
dotnet run --project src/AppHost

# 2. Aspireダッシュボードで確認
# http://localhost:15028 (自動で開きます)

# 3. AttendanceServiceのログを確認
# "Database seeded with X attendances and Y leave requests."
```

### 2. データの確認

```bash
# SQLiteデータベースを直接確認
sqlite3 data/attendance.db

# テーブル一覧
.tables

# 勤怠レコード数
SELECT COUNT(*) FROM Attendances;

# 従業員別の勤怠レコード数
SELECT EmployeeId, COUNT(*) as Count 
FROM Attendances 
GROUP BY EmployeeId;
```

### 3. クリーンアップとリセット

```bash
# 方法1: APIエンドポイント（推奨）
curl -X DELETE http://localhost:5001/api/dev/clear-seed-data

# 方法2: データベースファイル削除
rm data/attendance.db

# 方法3: プログラム的にクリア
await DbInitializer.ClearSeedDataAsync(serviceProvider);
```

## 今後の拡張案

以下の機能追加を検討できます:

1. **カスタムシード期間**
   ```json
   {
     "AttendanceService": {
       "SeedDataMonths": 6  // 6ヶ月分生成
     }
   }
   ```

2. **従業員フィルタリング**
   ```csharp
   // 特定の部署の従業員のみ
   var employeeIds = await GetEmployeesByDepartmentAsync(departmentId);
   ```

3. **シードデータプロファイル**
   ```json
   {
     "AttendanceService": {
       "SeedProfile": "Demo",  // Demo, Stress, Minimal
     }
   }
   ```

## まとめ

この実装により、以下の改善が達成されました:

✅ **既存データとの整合性**: EmployeeServiceから実際の従業員IDを取得  
✅ **柔軟な制御**: 環境変数・設定・パラメータによる制御  
✅ **開発効率向上**: クリーンアップエンドポイントで簡単にリセット  
✅ **テストカバレッジ**: 新機能に対する包括的なテスト  
✅ **ドキュメント**: 使用方法と技術的詳細の文書化

## 関連ファイル

- `src/Services/AttendanceService/Infrastructure/Data/DbInitializer.cs`
- `src/Services/AttendanceService/API/Program.cs`
- `tests/AttendanceService.Integration.Tests/Data/DbInitializerTests.cs`

## 参考情報

- [AttendanceService API ドキュメント](../../docs/notification-service.md)
- [開発ガイド](../../docs/development-guide.md)
- [データベース管理](../../docs/database.md)
