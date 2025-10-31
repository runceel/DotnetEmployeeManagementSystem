# 開発ガイド

従業員管理システムの開発に関するガイドラインとベストプラクティスです。

## 開発環境のセットアップ

[Getting Started](getting-started.md)を参照してください。

## プロジェクト構造

```
DotnetEmployeeManagementSystem/
├── src/
│   ├── AppHost/                      # Aspireオーケストレーション
│   ├── ServiceDefaults/              # 共通設定
│   ├── Services/
│   │   ├── EmployeeService/
│   │   │   ├── Domain/              # ドメインモデル
│   │   │   ├── Application/         # ビジネスロジック
│   │   │   ├── Infrastructure/      # データアクセス
│   │   │   └── API/                 # Web API
│   │   └── AuthService/
│   │       └── API/                 # 認証API
│   ├── WebApps/
│   │   └── BlazorWeb/               # フロントエンド
│   └── Shared/
│       └── Contracts/               # 共有DTO
├── tests/
│   ├── EmployeeService.Domain.Tests/
│   └── EmployeeService.Application.Tests/
├── docs/                            # ドキュメント
└── data/                            # SQLiteデータベース
```

## 新機能の追加

### 1. 新しいエンティティの追加

#### Step 1: Domain層でエンティティを定義

`src/Services/EmployeeService/Domain/Entities/YourEntity.cs`
```csharp
namespace EmployeeService.Domain.Entities;

public class YourEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // ビジネスロジックをここに追加
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty", nameof(newName));
        
        Name = newName;
    }
}
```

#### Step 2: Application層でDTOとインターフェースを定義

`src/Services/EmployeeService/Application/DTOs/YourEntityDto.cs`
```csharp
namespace EmployeeService.Application.DTOs;

public record YourEntityDto(int Id, string Name, DateTime CreatedAt);
```

`src/Services/EmployeeService/Application/Interfaces/IYourEntityRepository.cs`
```csharp
namespace EmployeeService.Application.Interfaces;

public interface IYourEntityRepository
{
    Task<YourEntity?> GetByIdAsync(int id);
    Task<IEnumerable<YourEntity>> GetAllAsync();
    Task<YourEntity> AddAsync(YourEntity entity);
    Task UpdateAsync(YourEntity entity);
    Task DeleteAsync(int id);
}
```

#### Step 3: Infrastructure層でDbContextとリポジトリを実装

`src/Services/EmployeeService/Infrastructure/Data/ApplicationDbContext.cs`
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<YourEntity> YourEntities { get; set; }
    
    // ... 既存のコード
}
```

`src/Services/EmployeeService/Infrastructure/Repositories/YourEntityRepository.cs`
```csharp
public class YourEntityRepository : IYourEntityRepository
{
    private readonly ApplicationDbContext _context;
    
    public YourEntityRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<YourEntity?> GetByIdAsync(int id)
    {
        return await _context.YourEntities.FindAsync(id);
    }
    
    // ... 他のメソッド実装
}
```

#### Step 4: API層でエンドポイントを公開

`src/Services/EmployeeService/API/Endpoints/YourEntityEndpoints.cs`
```csharp
public static class YourEntityEndpoints
{
    public static void MapYourEntityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/yourentity");
        
        group.MapGet("/", async (IYourEntityRepository repo) =>
        {
            var entities = await repo.GetAllAsync();
            return Results.Ok(entities);
        });
        
        group.MapGet("/{id}", async (int id, IYourEntityRepository repo) =>
        {
            var entity = await repo.GetByIdAsync(id);
            return entity is not null ? Results.Ok(entity) : Results.NotFound();
        });
        
        // ... 他のエンドポイント
    }
}
```

`src/Services/EmployeeService/API/Program.cs`で登録:
```csharp
app.MapYourEntityEndpoints();
```

### 2. 新しいマイクロサービスの追加

#### Step 1: プロジェクトの作成

```bash
# APIプロジェクト
dotnet new webapi -n YourService.API -o src/Services/YourService/API

# ソリューションに追加
dotnet sln add src/Services/YourService/API/YourService.API.csproj

# ServiceDefaultsへの参照
dotnet add src/Services/YourService/API/YourService.API.csproj reference src/ServiceDefaults/ServiceDefaults.csproj
```

#### Step 2: ServiceDefaultsの統合

`Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);

// ServiceDefaultsを追加
builder.AddServiceDefaults();

// サービス登録
builder.Services.AddOpenApi();

var app = builder.Build();

// デフォルトエンドポイントをマップ
app.MapDefaultEndpoints();

app.MapOpenApi();
app.Run();
```

#### Step 3: AppHostに登録

`src/AppHost/AppHost.cs`:
```csharp
var yourService = builder.AddProject<Projects.YourService_API>("yourservice-api");

// 他のサービスから参照する場合
builder.AddProject<Projects.BlazorWeb>("blazorweb")
    .WithReference(yourService);
```

## コーディング規約

### C# コーディングスタイル

- **.NET コーディング規約**に従う
- **nullable参照型**を有効化（すべてのプロジェクトで既に有効）
- **record型**をDTOに使用
- **async/await**を適切に使用

### 命名規則

- **PascalCase**: クラス、メソッド、プロパティ
- **camelCase**: ローカル変数、パラメータ
- **_camelCase**: プライベートフィールド
- **接尾辞**:
  - インターフェース: `IYourInterface`
  - DTO: `YourEntityDto`
  - リポジトリ: `YourEntityRepository`

### ファイル構成

- 1ファイル1クラスを原則とする
- ファイル名はクラス名と一致させる
- 名前空間はフォルダ構造に合わせる

## テスト戦略

### ユニットテスト

Domain層とApplication層に対してユニットテストを書く。

**Domain層テスト例**:
```csharp
public class YourEntityTests
{
    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var entity = new YourEntity { Name = "Old Name" };
        
        // Act
        entity.UpdateName("New Name");
        
        // Assert
        Assert.Equal("New Name", entity.Name);
    }
    
    [Fact]
    public void UpdateName_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var entity = new YourEntity { Name = "Old Name" };
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => entity.UpdateName(""));
    }
}
```

**Application層テスト例（Moq使用）**:
```csharp
public class YourServiceTests
{
    [Fact]
    public async Task GetById_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var mockRepo = new Mock<IYourEntityRepository>();
        var entity = new YourEntity { Id = 1, Name = "Test" };
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
        
        var service = new YourService(mockRepo.Object);
        
        // Act
        var result = await service.GetByIdAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }
}
```

### テストの実行

```bash
# すべてのテストを実行
dotnet test

# 特定のプロジェクトのテストを実行
dotnet test tests/EmployeeService.Domain.Tests

# カバレッジレポート付き（要coverlet.collector）
dotnet test /p:CollectCoverage=true
```

## データベースマイグレーション

### 新しいマイグレーションの作成

```bash
cd src/Services/EmployeeService/Infrastructure

# マイグレーションの作成
dotnet ef migrations add YourMigrationName --startup-project ../API

# マイグレーションの適用
dotnet ef database update --startup-project ../API
```

### マイグレーションの確認

```bash
# マイグレーション一覧
dotnet ef migrations list --startup-project ../API

# 最新のマイグレーションスクリプトを確認
dotnet ef migrations script --startup-project ../API
```

詳細は[データベース管理](database.md)を参照。

## デバッグ

### Aspireダッシュボードでのデバッグ

1. AppHostを起動: `dotnet run --project src/AppHost`
2. ダッシュボードでサービスのログとトレースを確認
3. 問題のあるサービスに対してIDEでデバッガをアタッチ

### ログの追加

ServiceDefaultsにより、ILoggerが自動的に構成されます：

```csharp
public class YourService
{
    private readonly ILogger<YourService> _logger;
    
    public YourService(ILogger<YourService> logger)
    {
        _logger = logger;
    }
    
    public async Task DoSomethingAsync()
    {
        _logger.LogInformation("Doing something...");
        
        try
        {
            // 処理
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while doing something");
            throw;
        }
    }
}
```

## パフォーマンス最適化

### データベースクエリ

- **AsNoTracking()** を読み取り専用クエリに使用
- **Select()** で必要なフィールドのみ取得
- **Include()** でN+1問題を回避

```csharp
// Good
var entities = await _context.YourEntities
    .AsNoTracking()
    .Select(e => new YourEntityDto(e.Id, e.Name, e.CreatedAt))
    .ToListAsync();

// Bad - すべてのフィールドを取得
var entities = await _context.YourEntities.ToListAsync();
```

### キャッシング

将来的にRedisなどのキャッシュを統合予定。

## CI/CD

（今後実装予定）

- GitHub Actions
- 自動テスト
- 自動デプロイ

## 関連ドキュメント

- [Getting Started](getting-started.md)
- [アーキテクチャ](architecture.md)
- [Aspireダッシュボード](aspire-dashboard.md)
- [データベース管理](database.md)
