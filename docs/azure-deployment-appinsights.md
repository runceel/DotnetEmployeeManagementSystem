# Azure Application Insights と Log Analytics Workspace の統合

このドキュメントでは、.NET Aspire を使用して Application Insights と Log Analytics Workspace を Azure にデプロイする方法と、OpenTelemetry との統合について説明します。

## 概要

このシステムでは、Azure Developer CLI (`azd`) を使用して Azure にデプロイする際、以下のリソースが自動的にプロビジョニングされます：

- **Log Analytics Workspace** - ログデータの集約と保存
- **Application Insights** - アプリケーションパフォーマンス監視（APM）
- **OpenTelemetry 統合** - トレース、メトリクス、ログの自動収集

**重要:** Application Insights は Azure デプロイ時（`azd up`）のみ使用されます。ローカル開発環境では Aspire Dashboard がテレメトリの表示に使用されます。

## アーキテクチャ

```
┌─────────────────────────────────────────────────────────────┐
│ .NET Aspire AppHost                                         │
│                                                             │
│  ┌──────────────────────────────────────────────────┐     │
│  │ Log Analytics Workspace                           │     │
│  │  - ログの長期保存                                  │     │
│  │  - クエリと分析                                    │     │
│  └──────────────────────────────────────────────────┘     │
│                      ▲                                      │
│                      │                                      │
│  ┌──────────────────────────────────────────────────┐     │
│  │ Application Insights                              │     │
│  │  - APM（アプリケーションパフォーマンス監視）        │     │
│  │  - トレース、メトリクス、ログの可視化              │     │
│  └──────────────────────────────────────────────────┘     │
│                      ▲                                      │
│                      │ APPLICATIONINSIGHTS_CONNECTION_STRING │
│  ┌──────────────────────────────────────────────────┐     │
│  │ ServiceDefaults (OpenTelemetry)                   │     │
│  │  - Azure Monitor Exporter                         │     │
│  │  - トレース、メトリクス、ログの収集                │     │
│  └──────────────────────────────────────────────────┘     │
│                      ▲                                      │
│                      │ 各サービスが ServiceDefaults を参照  │
│  ┌──────────────────────────────────────────────────┐     │
│  │ マイクロサービス                                   │     │
│  │  - EmployeeService                                │     │
│  │  - AuthService                                    │     │
│  │  - NotificationService                            │     │
│  │  - AttendanceService                              │     │
│  │  - BlazorWeb                                      │     │
│  └──────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
```

## 実装の詳細

### 1. AppHost の構成

`src/AppHost/AppHost.cs` では、以下のように Application Insights と Log Analytics Workspace を構成しています：

```csharp
// Application Insights and Log Analytics Workspace (Azure deployment only)
// Only provision these resources when publishing to Azure (not during local development)
if (builder.ExecutionContext.IsPublishMode)
{
    builder.AddAzureApplicationInsights("appinsights")
        .WithLogAnalyticsWorkspace(
            builder.AddAzureLogAnalyticsWorkspace("loganalytics"));
}
```

#### 重要なポイント

- **`ExecutionContext.IsPublishMode`** - Azure へのパブリッシュ時（`azd up`）のみ `true` になる
- **`AddAzureLogAnalyticsWorkspace`** - Log Analytics Workspace リソースを定義
- **`AddAzureApplicationInsights`** - Application Insights リソースを定義
- **`WithLogAnalyticsWorkspace`** - Application Insights を Log Analytics Workspace に接続
- **Azure デプロイ時の動作**: `azd up` コマンド実行時に、これらのリソースが自動的にプロビジョニングされ、`APPLICATIONINSIGHTS_CONNECTION_STRING` 環境変数が各サービスに注入されます
- **ローカル開発時の動作**: `if` 条件により、これらのリソースは定義されず、Aspire Dashboard が OpenTelemetry データの表示に使用されます

### 2. ServiceDefaults の OpenTelemetry 構成

`src/ServiceDefaults/Extensions.cs` では、OpenTelemetry から Application Insights へのエクスポートを有効化しています：

```csharp
private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) 
    where TBuilder : IHostApplicationBuilder
{
    var useOtlpExporter = !string.IsNullOrWhiteSpace(
        builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

    if (useOtlpExporter)
    {
        builder.Services.AddOpenTelemetry().UseOtlpExporter();
    }

    // Azure Monitor exporter for Application Insights
    if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
    {
        builder.Services.AddOpenTelemetry()
           .UseAzureMonitor();
    }

    return builder;
}
```

#### 動作の仕組み

1. **ローカル開発環境**
   - Application Insights リソースは無視される
   - `OTEL_EXPORTER_OTLP_ENDPOINT` が設定されている場合、Aspire Dashboard に OpenTelemetry データを送信
   - `APPLICATIONINSIGHTS_CONNECTION_STRING` は設定されないため、Azure Monitor には送信しない
   - Aspire Dashboard でトレース、メトリクス、ログを確認

2. **Azure 環境**
   - Aspire が自動的に `APPLICATIONINSIGHTS_CONNECTION_STRING` を各サービスに注入
   - OpenTelemetry データが Application Insights に送信される
   - Log Analytics Workspace にデータが保存される

### 3. 必要なパッケージ

#### AppHost プロジェクト (`src/AppHost/AppHost.csproj`)

```xml
<ItemGroup>
  <PackageReference Include="Aspire.Hosting.Azure.ApplicationInsights" />
  <PackageReference Include="Aspire.Hosting.Azure.OperationalInsights" />
</ItemGroup>
```

#### ServiceDefaults プロジェクト (`src/ServiceDefaults/ServiceDefaults.csproj`)

```xml
<ItemGroup>
  <PackageReference Include="Azure.Monitor.OpenTelemetry.AspNetCore" />
</ItemGroup>
```

#### Directory.Packages.props（中央パッケージ管理）

```xml
<ItemGroup>
  <PackageVersion Include="Aspire.Hosting.Azure.ApplicationInsights" Version="13.0.2" />
  <PackageVersion Include="Aspire.Hosting.Azure.OperationalInsights" Version="13.0.2" />
  <PackageVersion Include="Azure.Monitor.OpenTelemetry.AspNetCore" Version="1.3.0" />
</ItemGroup>
```

## Azure へのデプロイ

### 前提条件

- [Azure Developer CLI (`azd`)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd) がインストールされていること
- Azure サブスクリプションへのアクセス権

### デプロイ手順

1. **Azure にログイン**

```bash
azd auth login
```

2. **初期化（初回のみ）**

```bash
azd init
```

3. **Azure にデプロイ**

```bash
azd up
```

このコマンドにより、以下のリソースが自動的にプロビジョニングされます：

- Azure Container Apps（各マイクロサービス用）
- Log Analytics Workspace
- Application Insights（Log Analytics Workspace に紐付け）
- その他必要なリソース（Storage Account、Container Registry など）

### デプロイ後の確認

1. **Application Insights の確認**

   Azure Portal で Application Insights リソースを開き、以下を確認できます：
   - **ライブメトリクス** - リアルタイムのパフォーマンスメトリクス
   - **アプリケーションマップ** - サービス間の依存関係
   - **トランザクションの検索** - 個別のリクエストトレース
   - **パフォーマンス** - 応答時間と失敗率
   - **障害** - エラーと例外の分析

2. **Log Analytics の確認**

   Log Analytics Workspace で Kusto クエリを実行してログを分析できます：

   ```kusto
   // すべてのトレースを表示
   AppTraces
   | where TimeGenerated > ago(1h)
   | order by TimeGenerated desc

   // エラーのみ表示
   AppExceptions
   | where TimeGenerated > ago(1h)
   | order by TimeGenerated desc

   // リクエストの応答時間
   AppRequests
   | where TimeGenerated > ago(1h)
   | summarize avg(DurationMs), max(DurationMs) by operation_Name
   ```

## 収集されるテレメトリデータ

### トレース（Traces）

- HTTP リクエスト（ASP.NET Core）
- HTTP クライアント呼び出し
- サービス間通信
- データベースクエリ（Entity Framework Core）

### メトリクス（Metrics）

- ASP.NET Core メトリクス（リクエスト数、レスポンスタイム）
- HTTP クライアントメトリクス
- .NET ランタイムメトリクス（GC、スレッド、例外）

### ログ（Logs）

- アプリケーションログ（`ILogger` 経由）
- 構造化ログとスコープ
- 例外とスタックトレース

## トラブルシューティング

### Application Insights にデータが表示されない

1. **環境変数の確認**

   Azure 環境で `APPLICATIONINSIGHTS_CONNECTION_STRING` が正しく設定されているか確認：

   ```bash
   azd env get-values
   ```

2. **ServiceDefaults の参照確認**

   各サービスプロジェクトで `ServiceDefaults` プロジェクトを参照し、`AddServiceDefaults()` を呼び出していることを確認：

   ```csharp
   // Program.cs
   var builder = WebApplication.CreateBuilder(args);
   builder.AddServiceDefaults(); // この行が必要
   ```

3. **パッケージのバージョン確認**

   `Azure.Monitor.OpenTelemetry.AspNetCore` パッケージが正しくインストールされているか確認：

   ```bash
   dotnet list package | grep Azure.Monitor
   ```

### ローカル開発環境での Application Insights テスト

ローカル環境で Application Insights への送信をテストする場合：

1. Azure Portal で Application Insights の接続文字列を取得
2. 環境変数を設定：

   ```bash
   # Windows (PowerShell)
   $env:APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=..."

   # Linux/macOS
   export APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=..."
   ```

3. AppHost を起動：

   ```bash
   dotnet run --project src/AppHost
   ```

## ベストプラクティス

### 1. カスタムテレメトリの追加

アプリケーション固有のメトリクスやトレースを追加する場合：

```csharp
// カスタムメトリクスの追加
public class EmployeeService : IEmployeeService
{
    private readonly ActivitySource _activitySource;
    
    public EmployeeService()
    {
        _activitySource = new ActivitySource("EmployeeService");
    }

    public async Task<Employee> GetEmployeeAsync(int id)
    {
        using var activity = _activitySource.StartActivity("GetEmployee");
        activity?.SetTag("employee.id", id);
        
        // ビジネスロジック
        var employee = await _repository.GetByIdAsync(id);
        
        activity?.SetTag("employee.department", employee.DepartmentName);
        return employee;
    }
}
```

### 2. サンプリングの構成

高トラフィック環境でコストを最適化する場合、サンプリングを構成できます：

```csharp
// ServiceDefaults/Extensions.cs
builder.Services.Configure<ApplicationInsightsServiceOptions>(options =>
{
    options.EnableAdaptiveSampling = true;
    options.InstrumentationKey = "...";
});
```

### 3. ログレベルの設定

本番環境では、ログレベルを適切に設定してノイズを削減：

```json
// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

## コスト管理

### データ取り込み量の確認

Application Insights のコストは主にデータ取り込み量に基づきます。以下のクエリでデータ量を確認できます：

```kusto
Usage
| where TimeGenerated > ago(30d)
// Quantity is in bytes, convert to MB (1000 bytes = 1 KB, 1000 KB = 1 MB for billing purposes)
| summarize DataVolumeMB = sum(Quantity) / 1000 by DataType
| order by DataVolumeMB desc
```

### コスト削減のヒント

1. **サンプリングの有効化** - 高トラフィック時のデータ量を削減
2. **ログレベルの最適化** - 詳細ログは開発環境のみ
3. **保持期間の設定** - 必要以上に長期間保持しない
4. **アラートの最適化** - 重要なアラートのみ設定

## 関連リンク

- [.NET Aspire と Application Insights（Microsoft Learn）](https://learn.microsoft.com/ja-jp/dotnet/aspire/deployment/aspire-deploy/application-insights)
- [Azure Monitor OpenTelemetry（Microsoft Learn）](https://learn.microsoft.com/ja-jp/azure/azure-monitor/app/opentelemetry-enable?tabs=aspnetcore)
- [Application Insights の価格（Azure）](https://azure.microsoft.com/ja-jp/pricing/details/monitor/)
- [Log Analytics ワークスペースの概要（Microsoft Learn）](https://learn.microsoft.com/ja-jp/azure/azure-monitor/logs/log-analytics-workspace-overview)

## まとめ

このシステムでは、.NET Aspire の Hosting 拡張を使用して、Application Insights と Log Analytics Workspace の自動プロビジョニングと構成を実現しています。

- **AppHost** - Azure リソースの定義と各サービスへの参照注入
- **ServiceDefaults** - OpenTelemetry から Application Insights へのデータエクスポート
- **自動構成** - `azd` によるシームレスなデプロイ

この構成により、開発者はインフラストラクチャの詳細を意識することなく、包括的な可観測性を持つマイクロサービスアプリケーションを Azure にデプロイできます。
