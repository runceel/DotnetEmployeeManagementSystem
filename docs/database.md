# データベース管理

従業員管理システムのデータベース管理に関するガイドです。

## データベース戦略

### 開発環境

**SQLite**を使用して、ローカル開発を簡素化します。

**メリット**:
- インストール不要
- ファイルベース（`data/` ディレクトリに配置）
- 軽量で高速
- バージョン管理から除外（`.gitignore`に登録済み）

**制限事項**:
- 同時書き込みの制限
- 本番環境には適さない

### 本番環境（将来）

- **Azure SQL Database** または **PostgreSQL**
- 接続文字列は環境変数またはKey Vaultで管理
- 高可用性とスケーラビリティ

## データベースファイルの配置

```
DotnetEmployeeManagementSystem/
└── data/
    ├── employees.db          # EmployeeService DB
    ├── employees.db-shm      # 共有メモリファイル
    ├── employees.db-wal      # Write-Ahead Logファイル
    ├── auth.db               # AuthService DB
    ├── auth.db-shm
    └── auth.db-wal
```

`.gitignore`で除外済み:
```gitignore
# SQLite database files
*.db
*.db-shm
*.db-wal
```

## Entity Framework Core

### DbContextの構成

**EmployeeService** の例:

`src/Services/EmployeeService/Infrastructure/Data/EmployeeDbContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using EmployeeService.Domain.Entities;

namespace EmployeeService.Infrastructure.Data;

public class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // エンティティ設定
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            
            // インデックス
            entity.HasIndex(e => e.Email).IsUnique();
            
            // 関連
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId);
        });

        // シードデータ
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Engineering" },
            new Department { Id = 2, Name = "Sales" },
            new Department { Id = 3, Name = "HR" }
        );
    }
}
```

### 接続文字列の設定

`src/Services/EmployeeService/API/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=./data/employees.db"
  }
}
```

`Program.cs`でDbContextを登録:
```csharp
builder.Services.AddDbContext<EmployeeDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

## マイグレーション

### 必要なツール

EF Core CLIツールをインストール:
```bash
dotnet tool install --global dotnet-ef
# または更新
dotnet tool update --global dotnet-ef
```

バージョン確認:
```bash
dotnet ef --version
# 出力: Entity Framework Core .NET Command-line Tools 9.0.10
```

### 初回マイグレーションの作成

```bash
cd src/Services/EmployeeService/Infrastructure

# 初回マイグレーション
dotnet ef migrations add InitialCreate --startup-project ../API

# データベース作成
dotnet ef database update --startup-project ../API
```

### マイグレーションの追加

```bash
cd src/Services/EmployeeService/Infrastructure

# 新しいマイグレーションを追加
dotnet ef migrations add AddEmployeePhoneNumber --startup-project ../API

# マイグレーションを適用
dotnet ef database update --startup-project ../API
```

### マイグレーションの確認

```bash
# マイグレーション一覧
dotnet ef migrations list --startup-project ../API

# 出力例:
# 20251031000000_InitialCreate
# 20251031120000_AddEmployeePhoneNumber (Pending)
```

### マイグレーションのロールバック

```bash
# 特定のマイグレーションまで戻す
dotnet ef database update InitialCreate --startup-project ../API

# すべてのマイグレーションを削除（データベースを空に）
dotnet ef database update 0 --startup-project ../API
```

### マイグレーションの削除

```bash
# 最後のマイグレーションを削除（未適用の場合のみ）
dotnet ef migrations remove --startup-project ../API
```

### マイグレーションスクリプトの生成

```bash
# すべてのマイグレーションのSQLスクリプト
dotnet ef migrations script --startup-project ../API -o migrations.sql

# 特定の範囲のマイグレーション
dotnet ef migrations script InitialCreate AddEmployeePhoneNumber --startup-project ../API
```

## データベース初期化

### 開発環境での自動初期化

`Program.cs`に初期化コードを追加:
```csharp
var app = builder.Build();

// 開発環境でのみデータベースを自動作成
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
    
    // データベースとスキーマを作成（マイグレーションを適用）
    await dbContext.Database.MigrateAsync();
    
    // オプション: シードデータの追加
    await SeedDataAsync(dbContext);
}

app.Run();
```

### シードデータの追加

```csharp
static async Task SeedDataAsync(EmployeeDbContext context)
{
    // 既にデータがある場合はスキップ
    if (await context.Employees.AnyAsync())
        return;

    var employees = new[]
    {
        new Employee
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "taro.yamada@example.com",
            DepartmentId = 1,
            HireDate = new DateTime(2020, 4, 1)
        },
        new Employee
        {
            FirstName = "花子",
            LastName = "佐藤",
            Email = "hanako.sato@example.com",
            DepartmentId = 2,
            HireDate = new DateTime(2021, 4, 1)
        }
    };

    context.Employees.AddRange(employees);
    await context.SaveChangesAsync();
}
```

## クエリのベストプラクティス

### 1. AsNoTracking を使用

読み取り専用クエリには`AsNoTracking()`を使用してパフォーマンスを向上：

```csharp
// Good: 変更追跡なし
var employees = await context.Employees
    .AsNoTracking()
    .ToListAsync();

// Bad: 不要な変更追跡
var employees = await context.Employees
    .ToListAsync();
```

### 2. 必要なデータのみ取得

```csharp
// Good: 必要なフィールドのみ
var employeeNames = await context.Employees
    .Select(e => new { e.FirstName, e.LastName })
    .ToListAsync();

// Bad: すべてのフィールド
var employees = await context.Employees.ToListAsync();
```

### 3. Include で N+1 問題を回避

```csharp
// Good: Includeで一度に取得
var employees = await context.Employees
    .Include(e => e.Department)
    .ToListAsync();

// Bad: N+1クエリ
var employees = await context.Employees.ToListAsync();
foreach (var emp in employees)
{
    var dept = await context.Departments.FindAsync(emp.DepartmentId);
}
```

### 4. 適切なインデックス

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>(entity =>
    {
        // 頻繁に検索されるフィールドにインデックス
        entity.HasIndex(e => e.Email).IsUnique();
        entity.HasIndex(e => e.LastName);
        entity.HasIndex(e => e.DepartmentId);
        
        // 複合インデックス
        entity.HasIndex(e => new { e.LastName, e.FirstName });
    });
}
```

## トラブルシューティング

### マイグレーションエラー

**エラー**: `No DbContext was found`
```bash
# 解決: --startup-project を指定
dotnet ef migrations add YourMigration --startup-project ../API
```

**エラー**: `A migration with the name 'XYZ' already exists`
```bash
# 解決: 既存のマイグレーションを削除または別の名前を使用
dotnet ef migrations remove --startup-project ../API
```

### データベースロックエラー

SQLiteは書き込みロックが厳密です。

**エラー**: `database is locked`

**解決策**:
1. すべての接続を閉じる
2. `.db-shm`と`.db-wal`ファイルを削除
3. アプリケーションを再起動

### データベースファイルが見つからない

**エラー**: `unable to open database file`

**解決策**:
```bash
# dataディレクトリを作成
mkdir -p data

# マイグレーションを再実行
cd src/Services/EmployeeService/Infrastructure
dotnet ef database update --startup-project ../API
```

## 本番環境への移行

### Azure SQL Database への切り替え（.NET Aspire PublishMode）

本システムは .NET Aspire の `PublishMode` を使用して、自動的に Azure SQL Database へ切り替わるように設計されています。

#### 自動切り替えの仕組み

**AppHost.cs の設定**:
```csharp
// PublishMode時はAzure SQL Database、開発時はSQLiteを使用
if (builder.ExecutionContext.IsPublishMode)
{
    var sqlServer = builder.AddAzureSqlServer("sql");
    employeeDb = sqlServer.AddDatabase("employeedb");
    authDb = sqlServer.AddDatabase("authdb");
    notificationDb = sqlServer.AddDatabase("notificationdb");
    attendanceDb = sqlServer.AddDatabase("attendancedb");
}
else
{
    employeeDb = builder.AddSqlite("employeedb");
    authDb = builder.AddSqlite("authdb");
    notificationDb = builder.AddSqlite("notificationdb");
    attendanceDb = builder.AddSqlite("attendancedb");
}
```

**各サービスの DependencyInjection.cs**:
```csharp
// 環境に応じて自動的にプロバイダーを切り替え
var useSqlServer = environment?.IsProduction() == true && 
                  IsSqlServerConnectionString(connectionString);

services.AddDbContext<EmployeeDbContext>(options =>
{
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
});
```

#### デプロイ手順

1. **Azure へのデプロイ**:
```bash
# Azure Developer CLI (azd) を使用
azd init
azd up
```

2. **マニフェスト生成** (確認用):
```bash
dotnet run --project src/AppHost -- --publisher manifest --output-path ./aspire-manifest.json
```

### 接続文字列の管理

本番環境では Aspire が自動的に接続文字列を注入します。手動設定が必要な場合：

**appsettings.Production.json**:
```json
{
  "ConnectionStrings": {
    "EmployeeDb": ""  // 空にしておく（Aspireが自動注入）
  }
}
```

**環境変数による上書き**（オプション）:
```bash
export ConnectionStrings__EmployeeDb="Server=..."
```

**Azure Key Vault** (将来の拡張):
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### SQL Server 用マイグレーションの検証

既存の SQLite マイグレーションは SQL Server でも動作しますが、本番デプロイ前に検証を推奨します。

#### ローカルで SQL Server を使用してテスト

```bash
# Docker で SQL Server を起動
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

# 接続文字列を設定
export ConnectionStrings__EmployeeDb="Server=localhost,1433;Database=employeedb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
export ASPNETCORE_ENVIRONMENT=Production

# マイグレーション適用
cd src/Services/EmployeeService/Infrastructure
dotnet ef database update --startup-project ../API
```

#### マイグレーションスクリプトの生成と確認

```bash
cd src/Services/EmployeeService/Infrastructure

# SQL Server用のスクリプト生成
dotnet ef migrations script --startup-project ../API -o migrations-sqlserver.sql

# 生成されたSQLを確認
cat migrations-sqlserver.sql
```

**注意事項**:
- SQLite と SQL Server でデータ型が異なる場合があります
- `INTEGER PRIMARY KEY` → `INT IDENTITY(1,1) PRIMARY KEY`
- `TEXT` → `NVARCHAR(MAX)`
- インデックスや外部キー制約は両方で動作します

### マイグレーションの適用戦略

**オプション1: 自動マイグレーション**（開発・ステージング環境）
```csharp
// Program.cs で既に実装済み
if (app.Environment.IsDevelopment())
{
    await DbInitializer.InitializeAsync(app.Services);
}
```

**オプション2: SQLスクリプト**（本番環境推奨）
```bash
# 各サービスでスクリプト生成
cd src/Services/EmployeeService/Infrastructure
dotnet ef migrations script --startup-project ../API -o employee-migration.sql

cd ../../../AuthService/Infrastructure
dotnet ef migrations script --startup-project ../API -o auth-migration.sql

# DBAレビュー後、本番環境で実行
```

**オプション3: CI/CD パイプライン**
- GitHub Actions / Azure DevOps
- マイグレーションを自動適用（ステージング環境）
- 本番環境は手動承認後に適用

### データベースの初期化確認

Azure にデプロイ後、データベースが正しく作成されたか確認：

```bash
# Azure SQL Database に接続
sqlcmd -S <server-name>.database.windows.net -d employeedb -U <username> -P <password>

# テーブル確認
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;

# マイグレーション履歴確認
SELECT * FROM __EFMigrationsHistory;
```

### トラブルシューティング

**エラー**: `Login failed for user`
```bash
# Azure SQL のファイアウォール設定を確認
# Azure Portal → SQL Server → Networking → Firewall rules
```

**エラー**: `Cannot open database "employeedb"`
```bash
# データベースが作成されているか確認
# Aspire が自動作成するはずだが、手動作成も可能
```

**エラー**: `A network-related or instance-specific error`
```bash
# 接続文字列を確認
# TrustServerCertificate=True が必要な場合がある
```

## 関連ドキュメント

- [Getting Started](getting-started.md)
- [アーキテクチャ](architecture.md)
- [開発ガイド](development-guide.md)
- [Entity Framework Core ドキュメント](https://learn.microsoft.com/ef/core/)
- [.NET Aspire Azure SQL Database 統合](https://learn.microsoft.com/dotnet/aspire/database/sql-server-integration)
- [Azure SQL Database ドキュメント](https://learn.microsoft.com/azure/azure-sql/)
