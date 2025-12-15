# Azure SQL Database デプロイ対応 - 実装サマリー

## 概要
PublishMode で Azure へデプロイする際、データベースを SQLite から Azure SQL Database へ自動切り替えする機能を実装しました。

## 主な変更ファイル

### 1. パッケージ管理
- `Directory.Packages.props`: 
  - `Microsoft.EntityFrameworkCore.SqlServer` (10.0.1) を追加
  - `Aspire.Hosting.Azure.Sql` (13.0.2) を追加

### 2. AppHost (オーケストレーション)
- `src/AppHost/AppHost.cs`:
  - `IsPublishMode` による条件分岐を追加
  - PublishMode: Azure SQL Server を使用
  - ローカル: SQLite を使用

### 3. 各サービスの Infrastructure 層
以下のファイルを更新してデータベースプロバイダーの切り替えロジックを追加:

- `src/Services/EmployeeService/Infrastructure/DependencyInjection.cs`
- `src/Services/AuthService/Infrastructure/DependencyInjection.cs`
- `src/Services/AttendanceService/Infrastructure/DependencyInjection.cs`
- `src/Services/NotificationService/API/Program.cs` (NotificationServiceのみProgram.csに実装)

**実装内容:**
- Production環境 + SQL Server接続文字列 → `UseSqlServer()`
- それ以外 → `UseSqlite()`
- SQL Server使用時は自動リトライを有効化

### 4. プロジェクトファイル
全サービスの Infrastructure プロジェクトに追加:
- `Microsoft.EntityFrameworkCore.SqlServer` パッケージ
- `Microsoft.Extensions.Hosting.Abstractions` パッケージ

### 5. 本番環境設定
以下の `appsettings.Production.json` を追加:
- `src/Services/EmployeeService/API/appsettings.Production.json`
- `src/Services/AuthService/API/appsettings.Production.json`
- `src/Services/NotificationService/API/appsettings.Production.json`
- `src/Services/AttendanceService/API/appsettings.Production.json`

### 6. ツール・スクリプト
- `scripts/verify-sqlserver-migrations.sh`:
  - SQL Server マイグレーション検証スクリプト
  - Docker で SQL Server を起動
  - 全サービスのマイグレーションスクリプトを生成

### 7. ドキュメント
- `docs/database.md`:
  - Azure SQL Database への移行手順を大幅に追加
  - デプロイ方法、マイグレーション検証、トラブルシューティングを追加

## 技術的な詳細

### データベース判定ロジック
```csharp
private static bool IsSqlServerConnectionString(string connectionString)
{
    return (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase)) && 
           !connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase);
}
```

### DbContext 構成
```csharp
if (useSqlServer)
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
}
else
{
    options.UseSqlite(connectionString);
}
```

### AppHost 構成
```csharp
if (builder.ExecutionContext.IsPublishMode)
{
    var sqlServer = builder.AddAzureSqlServer("sql");
    employeeDb = sqlServer.AddDatabase("employeedb");
    // ... 他のデータベース
}
else
{
    employeeDb = builder.AddSqlite("employeedb");
    // ... 他のデータベース
}
```

## デプロイ手順

### Azure へのデプロイ
```bash
# Azure Developer CLI を使用
azd init
azd up
```

### ローカルで SQL Server をテスト
```bash
# Docker で SQL Server を起動
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

# 環境変数を設定
export ConnectionStrings__EmployeeDb="Server=localhost,1433;Database=employeedb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
export ASPNETCORE_ENVIRONMENT=Production

# サービスを起動
dotnet run --project src/Services/EmployeeService/API
```

### マイグレーション検証
```bash
# 検証スクリプトを実行
./scripts/verify-sqlserver-migrations.sh
```

## テスト結果
- ✅ 全テスト (231個) がパス
- ✅ ビルドエラーなし
- ✅ コードレビュー完了（指摘事項を修正）

## 注意事項
- ローカル開発時は引き続き SQLite を使用（変更なし）
- 既存のマイグレーションは SQLite と SQL Server 両方で動作
- 接続文字列は Aspire が自動管理（appsettings.Production.json の ConnectionStrings は空）
- データ型の互換性は検証済み（INTEGER → INT, TEXT → NVARCHAR(MAX) など）

## 将来の拡張
- Azure Key Vault による接続文字列の管理
- CI/CD パイプラインでのマイグレーション自動適用
- PostgreSQL など他のデータベースのサポート

## 参考リンク
- [.NET Aspire Azure SQL Database 統合](https://learn.microsoft.com/dotnet/aspire/database/sql-server-integration)
- [Azure SQL Database ドキュメント](https://learn.microsoft.com/azure/azure-sql/)
- [EF Core Migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
