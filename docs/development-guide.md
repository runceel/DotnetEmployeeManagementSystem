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

### 統合テスト

統合テストではAPIエンドポイントとデータベースを含む実際のアプリケーション動作をテストします。

#### 統合テストの作成

**重要**: 統合テストを作成する際は、API層の`Program.cs`でTest環境の設定を正しく行う必要があります。

**API層のProgram.cs設定（必須）**:

Test環境では、テストコードで独自のDbContextやモックサービスを設定するため、`AddInfrastructure()`の呼び出しをスキップする必要があります。

```csharp
// Program.cs
builder.Services.AddOpenApi();

// Test環境ではテストコードでDbContextを設定するためスキップ
if (!builder.Environment.IsEnvironment("Test"))
{
    var connectionString = builder.Configuration.GetConnectionString("YourServiceDb")
        ?? "Data Source=yourservice.db";

    // Infrastructure層のサービスを追加
    builder.Services.AddInfrastructure(connectionString);

    // Redis接続の追加
    builder.AddRedisClient("redis");
}
```

**統合テスト例**:

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class YourApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public YourApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;
    }

    private HttpClient CreateClient()
    {
        var dbName = $"TestDb_{Guid.NewGuid()}";

        var client = _baseFactory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                // Add in-memory database for testing
                services.AddDbContext<YourDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                // Register repositories
                services.AddScoped<IYourRepository, YourRepository>();

                // Mock external services if needed
                services.AddScoped<IEventPublisher, MockEventPublisher>();
            });
        }).CreateClient();

        return client;
    }

    [Fact]
    public async Task PostEndpoint_WithValidData_ShouldReturn201()
    {
        // Arrange
        var client = CreateClient();
        var request = new CreateRequest { Name = "Test" };

        // Act
        var response = await client.PostAsJsonAsync("/api/endpoint", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }
}
```

**Program.cs をテストからアクセス可能にする**:

`Program.cs`の最後に以下を追加：

```csharp
app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
```

### テストの実行

```bash
# すべてのテストを実行
dotnet test

# 特定のプロジェクトのテストを実行
dotnet test tests/EmployeeService.Domain.Tests

# 統合テストのみ実行
dotnet test tests/YourService.Integration.Tests

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

## 開発時の起動・停止・監視手順

### アプリケーションの起動

#### 方法1: Aspire AppHostを使用（推奨）

すべてのサービスを統合的に起動できます。

```bash
# AppHostを起動
dotnet run --project src/AppHost
```

起動後、以下が自動的に実行されます：
- Aspireダッシュボードがブラウザで開く
- BlazorWeb、EmployeeService、AuthServiceが起動
- SQLiteデータベース（employeedb, authdb）が初期化される
- OpenTelemetryによるトレース・メトリクス収集が開始される
- Health checksが有効化される

**ダッシュボードアクセス:**
- デフォルトURL: `https://localhost:15001` または `http://localhost:15000`
- コンソールに表示されるURLを確認してください

#### 方法2: 個別サービスの起動（開発時）

特定のサービスのみをデバッグする場合：

```bash
# EmployeeService API
dotnet run --project src/Services/EmployeeService/API

# AuthService API
dotnet run --project src/Services/AuthService/API

# BlazorWeb
dotnet run --project src/WebApps/BlazorWeb
```

**注意**: 個別起動時は、Aspireダッシュボードは利用できません。

#### 方法3: Visual Studioでの起動

1. `DotnetEmployeeManagementSystem.slnx` を開く
2. スタートアッププロジェクトを `AppHost` に設定
3. F5キーを押すか、「デバッグ > デバッグの開始」
4. Aspireダッシュボードが自動的に開く

#### 方法4: Visual Studio Codeでの起動

1. リポジトリをVS Codeで開く
2. `Run and Debug` パネルを開く (Ctrl+Shift+D / Cmd+Shift+D)
3. `Aspire AppHost` 構成を選択
4. F5キーを押す

### アプリケーションの停止

#### 通常の停止

```bash
# コンソールでCtrl+Cを押す
# または、ターミナルで以下を実行
pkill -f AppHost
```

Aspireが各サービスを適切にシャットダウンします。

#### 強制停止（問題が発生した場合）

```bash
# すべてのdotnetプロセスを停止
pkill -f dotnet

# または、特定のサービスのみ停止
pkill -f EmployeeService.API
pkill -f AuthService.API
pkill -f BlazorWeb
```

#### ポートの解放確認

停止後もポートが占有されている場合：

```bash
# 使用中のポートを確認（Linux/Mac）
lsof -i :7000-7100
netstat -tuln | grep :7000

# 使用中のポートを確認（Windows）
netstat -ano | findstr :7000

# プロセスを強制終了（Linux/Mac）
kill -9 <PID>

# プロセスを強制終了（Windows）
taskkill /PID <PID> /F
```

### 監視とトラブルシューティング

#### Aspireダッシュボードでの監視

**リソースビュー:**
1. すべてのサービスの状態を確認
2. 各サービスのエンドポイントにアクセス
3. 失敗したサービスを特定

**コンソールログ:**
- リアルタイムでログを確認
- ログレベルでフィルター（Information, Warning, Error）
- サービスごとにログを表示

**構造化ログ:**
- 詳細なログエントリを検索
- タイムスタンプ、カテゴリ、例外情報を確認
- スコープ情報でコンテキストを把握

**トレース:**
- サービス間のリクエストフローを可視化
- パフォーマンスボトルネックを特定
- エラー発生箇所を特定

**メトリクス:**
- HTTPリクエスト数・レスポンス時間
- エラー率
- CPU・メモリ使用率

詳細は[Aspireダッシュボードガイド](aspire-dashboard.md)を参照。

#### ログの確認

**コマンドラインからのログ確認:**

```bash
# リアルタイムでログを表示
dotnet run --project src/AppHost --verbosity detailed

# ログをファイルに出力
dotnet run --project src/AppHost > logs/aspire.log 2>&1
```

**ログレベルの変更:**

`appsettings.Development.json` で設定：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "EmployeeService": "Debug"
    }
  }
}
```

#### Health Checksの確認

各サービスはHealth checkエンドポイントを公開します（開発環境のみ）：

```bash
# EmployeeService
curl https://localhost:7001/health
curl https://localhost:7001/alive

# AuthService
curl https://localhost:7002/health
curl https://localhost:7002/alive

# BlazorWeb
curl https://localhost:7000/health
curl https://localhost:7000/alive
```

**レスポンス例:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "self": {
      "status": "Healthy",
      "duration": "00:00:00.0001234"
    }
  }
}
```

#### SQLiteデータベースの確認

データベースファイルは一時ディレクトリに作成されます：

```bash
# データベースファイルの場所を確認
# AppHost起動時のログに表示されます

# SQLiteコマンドラインでデータベースを確認
sqlite3 /tmp/employeedb.db
.tables
.schema employees
SELECT * FROM employees;
.quit
```

#### トラブルシューティング

**問題: サービスが起動しない**

1. ログを確認：
   ```bash
   dotnet run --project src/AppHost --verbosity detailed
   ```

2. ポート競合を確認：
   ```bash
   netstat -tuln | grep 7000
   ```

3. 依存関係を再インストール：
   ```bash
   dotnet restore
   dotnet build
   ```

**問題: データベース接続エラー**

1. データベースファイルのパーミッションを確認
2. AppHostログでデータベースパスを確認
3. データベースを再作成：
   ```bash
   # 一時ファイルを削除してAppHostを再起動
   rm -rf /tmp/employeedb.db /tmp/authdb.db
   dotnet run --project src/AppHost
   ```

**問題: Aspireダッシュボードにアクセスできない**

1. AppHostが正常に起動しているか確認
2. ファイアウォール設定を確認
3. ブラウザで別のポートを試す：
   - `https://localhost:15001`
   - `http://localhost:15000`

**問題: サービス間通信が失敗する**

1. Aspireダッシュボードでサービスディスカバリーの状態を確認
2. 各サービスのエンドポイントを確認
3. トレースでリクエストフローを確認

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
- [通知サービス実装ガイド](notification-service.md)
- [Aspireダッシュボード](aspire-dashboard.md)
- [データベース管理](database.md)
