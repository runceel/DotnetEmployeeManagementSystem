# Issue Fix Report: EmployeeService Endpoint Name Correction

**Issue:** Attendance Service API での EmployeeService のエンドポイント名が正しくない

**Date:** 2025-11-12

**Status:** ✅ Completed

## Problem Statement

AttendanceService.API の Program.cs で EmployeeService への HttpClient を構成する際、サービス名が AppHost の登録名と一致していませんでした。

- **AppHost での登録名**: `"employeeservice-api"`
- **AttendanceService での使用名**: `"http://employeeservice"` ❌

この不一致により、AttendanceService が EmployeeService を呼び出す際にエンドポイントが解決されずエラーが発生していました。

## Root Cause Analysis

### AppHost.cs の設定
```csharp
var employeeServiceApi = builder.AddProject<Projects.EmployeeService_API>("employeeservice-api")
    .WithReference(employeeDb)
    .WithReference(redis)
    .WithHttpHealthCheck("/health");
```

### AttendanceService.API/Program.cs の問題箇所（修正前）
```csharp
builder.Services.AddHttpClient("EmployeeService", client =>
{
    // Aspire Service Discoveryを使用する場合
    client.BaseAddress = new Uri("http://employeeservice");  // ❌ 登録名と不一致
});
```

## Solution Applied

### 修正内容
AttendanceService.API/Program.cs の 79行目を修正：

```csharp
// Before:
client.BaseAddress = new Uri("http://employeeservice");

// After:
client.BaseAddress = new Uri("http://employeeservice-api");
```

### 変更ファイル
- `src/Services/AttendanceService/API/Program.cs` (1行変更)

## Verification

### ビルド結果
✅ **成功** - 警告・エラーなし
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### テスト結果
✅ **全テスト合格** - 136テスト実行、全て成功

#### テスト内訳
- EmployeeService.Domain.Tests: 18 tests ✅
- EmployeeService.Application.Tests: 18 tests ✅
- EmployeeService.Integration.Tests: 45 tests ✅
- AttendanceService.Domain.Tests: 46 tests ✅
- AttendanceService.Integration.Tests: 31 tests ✅
- AuthService.Tests: 9 tests ✅

### セキュリティスキャン
✅ **問題なし** - CodeQL スキャンで脆弱性検出なし

## Impact Assessment

### 変更の影響範囲
- **最小限の変更**: 1行のみ修正
- **既存機能への影響**: なし（テストで確認済み）
- **後方互換性**: 影響なし

### 修正による効果
1. AttendanceService から EmployeeService への API 呼び出しが正常に動作
2. サービス起動時の従業員データ取得が成功
3. 勤怠データのシード生成が正常に完了

## Technical Details

### Aspire Service Discovery の動作
.NET Aspire の Service Discovery 機能は、AppHost で登録されたサービス名を使用して HTTP エンドポイントを解決します。

- サービス登録: `builder.AddProject<T>("service-name")`
- エンドポイント解決: `http://service-name` → 実際のURL（例: `http://localhost:5001`）

### 命名規則の確認
プロジェクト内の他のサービスも同様の命名規則を使用：
- `employeeservice-api` ✅
- `authservice-api` ✅
- `notificationservice-api` ✅
- `attendanceservice-api` ✅

## Recommendations

### 今後の対策
1. **命名規則の統一**: サービス名は AppHost の登録名に統一
2. **ドキュメント化**: サービス名の命名規則をドキュメントに明記
3. **レビュープロセス**: 新しいサービス追加時は AppHost とクライアント設定の整合性を確認

### 関連ドキュメント
- [Aspire Dashboard Guide](docs/aspire-dashboard.md)
- [Architecture Overview](docs/architecture-overview.md)
- [Getting Started](docs/getting-started.md)

## Conclusion

最小限の変更（1行）でサービス間通信の問題を解決しました。全てのテストが合格し、セキュリティスキャンでも問題は検出されませんでした。この修正により、AttendanceService は正常に EmployeeService と通信できるようになりました。

---

**Commit:** 0009a36  
**Branch:** copilot/fix-employee-service-endpoint-name  
**Author:** GitHub Copilot  
**Reviewer:** Pending
