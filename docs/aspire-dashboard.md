# Aspireダッシュボード使用ガイド

.NET Aspireダッシュボードは、マイクロサービスアプリケーションの監視と管理を行うための統合ツールです。

## ダッシュボードへのアクセス

### 起動方法

```bash
dotnet run --project src/AppHost
```

AppHostが起動すると、ブラウザが自動的に開き、Aspireダッシュボードが表示されます。

通常、以下のURLでアクセスできます：
- `https://localhost:15001` または `http://localhost:15000`

コンソールに表示されるURLを確認してください。

## ダッシュボードの主要機能

### 1. リソース（Resources）ビュー

**概要**: すべてのサービスとリソースの状態を一覧表示

**表示内容**:
- **名前**: サービス/リソース名
- **状態**: Running、Stopped、Failed など
- **エンドポイント**: サービスのURL（クリックでアクセス可能）
- **環境**: 開発、ステージング、本番など

**主な用途**:
- サービスの起動状態確認
- 各サービスのエンドポイントへの素早いアクセス
- 障害が発生しているサービスの特定

### 2. コンソールログ（Console Logs）

**概要**: 各サービスのコンソール出力をリアルタイム表示

**機能**:
- **ログレベルフィルター**: Information、Warning、Error など
- **サービス選択**: 特定のサービスのログのみ表示
- **検索**: キーワードでログを検索
- **ダウンロード**: ログファイルとしてエクスポート

**使用例**:
```
# アプリケーション起動時のログ確認
[14:32:15 INF] Application started. Press Ctrl+C to shut down.
[14:32:15 INF] Hosting environment: Development
[14:32:15 INF] Content root path: /path/to/EmployeeService.API

# エラーログの確認
[14:35:22 ERR] An error occurred while processing request
System.Exception: Database connection failed
```

**ベストプラクティス**:
- エラー発生時は、まずコンソールログを確認
- ログレベルでフィルターして問題を絞り込む
- 本番環境ではログレベルをWarning以上に設定

### 3. 構造化ログ（Structured Logs）

**概要**: 構造化されたログデータの詳細表示

**機能**:
- **タイムスタンプ**: 正確な発生時刻
- **ログレベル**: Trace、Debug、Information、Warning、Error、Critical
- **カテゴリ**: ロガーのカテゴリ名
- **メッセージ**: ログメッセージ
- **スコープ**: ログスコープ情報
- **例外**: スタックトレースを含む例外詳細

**検索とフィルター**:
- タイムレンジでフィルター
- サービスでフィルター
- ログレベルでフィルター
- キーワード検索

**例**:
```json
{
  "Timestamp": "2025-10-31T14:35:22.123Z",
  "Level": "Error",
  "Category": "EmployeeService.API.Controllers.EmployeeController",
  "Message": "Failed to retrieve employee",
  "Exception": {
    "Type": "System.InvalidOperationException",
    "Message": "Employee not found",
    "StackTrace": "..."
  },
  "Scopes": {
    "RequestId": "0HN2G6KJ7N5M0",
    "EmployeeId": "12345"
  }
}
```

### 4. トレース（Traces）

**概要**: 分散トレーシングによるリクエストフローの可視化

**主要概念**:
- **トレース**: 1つのリクエストの完全なライフサイクル
- **スパン**: トレース内の個別の操作（例：HTTPリクエスト、DBクエリ）

**表示内容**:
- トレースID
- スパン階層（親子関係）
- 各スパンの開始時刻と継続時間
- タグと属性

**ウォーターフォール図**:
```
BlazorWeb [200ms]
  └─ GET /api/employees [180ms]
      ├─ EmployeeService.GetAll [150ms]
      │   ├─ Database Query [100ms]
      │   └─ Data Mapping [20ms]
      └─ Response Serialization [10ms]
```

**使用例**:
1. 遅いリクエストの特定
2. ボトルネックの発見（どのスパンが最も時間がかかっているか）
3. サービス間呼び出しのフロー確認
4. エラー発生箇所の特定

**トラブルシューティング**:
- レスポンスタイムが遅い → ウォーターフォール図で最も長いスパンを確認
- エラーが発生 → エラーステータスのスパンを探す
- タイムアウト → トレース全体の時間を確認

### 5. メトリクス（Metrics）

**概要**: アプリケーションとシステムのメトリクスをリアルタイムで表示

**標準メトリクス**:
- **HTTPリクエスト数**: `/api/employees` へのリクエスト数
- **HTTPレスポンス時間**: P50、P90、P99パーセンタイル
- **エラー率**: 4xx、5xxレスポンスの割合
- **アクティブリクエスト数**: 現在処理中のリクエスト
- **CPU使用率**: プロセスのCPU使用率
- **メモリ使用量**: プロセスのメモリ使用量
- **GC情報**: ガベージコレクションの統計

**カスタムメトリクス**:
```csharp
public class EmployeeService
{
    private static readonly Counter<long> _employeeCreatedCounter = 
        Meter.CreateCounter<long>("employee.created", "count", "Number of employees created");
    
    public async Task CreateEmployeeAsync(Employee employee)
    {
        // ... 従業員作成処理
        
        _employeeCreatedCounter.Add(1, new KeyValuePair<string, object?>("department", employee.Department));
    }
}
```

**グラフタイプ**:
- ライングラフ（時系列）
- ヒストグラム
- ゲージ

**アラート設定**:
（将来実装予定）
- しきい値を超えた場合の通知
- メール/Slack通知

### 6. 環境変数（Environment Variables）

**概要**: 各サービスの環境変数を表示・管理

**機能**:
- 現在の環境変数一覧
- 値の表示（機密情報はマスク）
- サービスごとの環境設定

**使用例**:
```
ASPNETCORE_ENVIRONMENT=Development
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
ConnectionStrings__DefaultConnection=Data Source=./data/employees.db
```

### 7. プロパティ（Properties）

**概要**: サービスのメタデータと設定情報

**表示内容**:
- .NETバージョン
- Aspireバージョン
- プロジェクトパス
- 起動時間
- プロセスID

## 実践例

### シナリオ1: パフォーマンス問題の調査

1. **メトリクス**で全体のパフォーマンスを確認
   - レスポンスタイムが通常より遅いことを確認

2. **トレース**で遅いリクエストを特定
   - トレース一覧を継続時間でソート
   - 最も遅いトレースを選択

3. **スパン詳細**でボトルネックを特定
   - データベースクエリが100ms → 最適化が必要
   - または、外部API呼び出しが遅い → タイムアウト設定を見直し

4. **構造化ログ**で追加コンテキストを確認
   - クエリパラメータ、返されたレコード数など

### シナリオ2: エラーのデバッグ

1. **リソース**ビューでサービスの状態確認
   - どのサービスがFailedステータスか

2. **コンソールログ**でエラーメッセージ確認
   - スタックトレースを確認

3. **構造化ログ**で詳細なエラー情報取得
   - 例外タイプ、メッセージ、スコープ情報

4. **トレース**でエラー発生のコンテキスト確認
   - どのリクエストが失敗したか
   - 失敗の前後で何が起きていたか

### シナリオ3: サービス間通信の確認

1. **トレース**で全体フローを可視化
   - BlazorWeb → EmployeeService → Database

2. **各スパン**の詳細を確認
   - HTTPヘッダー
   - リクエスト/レスポンスボディ（サンプリング有効時）
   - エラー情報

3. **メトリクス**で成功率を確認
   - 各サービス間のエラー率

## ダッシュボードのカスタマイズ

### OpenTelemetryの設定

ServiceDefaultsで自動構成されますが、カスタマイズも可能：

```csharp
builder.Services.ConfigureOpenTelemetryMetrics(metrics =>
{
    metrics.AddMeter("YourCustomMeter");
});

builder.Services.ConfigureOpenTelemetryTracing(tracing =>
{
    tracing.AddSource("YourCustomSource");
});
```

## トラブルシューティング

### ダッシュボードに接続できない

```bash
# ポート競合の確認
netstat -an | grep 15001

# AppHostを再起動
pkill -f AppHost
dotnet run --project src/AppHost
```

### トレースが表示されない

- ServiceDefaultsが各サービスで正しく構成されているか確認
- `builder.AddServiceDefaults()` が呼ばれているか確認
- OpenTelemetryエクスポーターが正しく動作しているかログで確認

### メトリクスが更新されない

- リクエストを送信してメトリクスを生成
- メトリクスの収集間隔を確認（デフォルト: 10秒）

## 参考リンク

- [.NET Aspire 公式ドキュメント](https://learn.microsoft.com/dotnet/aspire/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- [分散トレーシングの基礎](https://opentelemetry.io/docs/concepts/observability-primer/#distributed-traces)

## 関連ドキュメント

- [Getting Started](getting-started.md)
- [アーキテクチャ](architecture.md)
- [開発ガイド](development-guide.md)
