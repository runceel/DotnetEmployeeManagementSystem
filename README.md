# 従業員管理システム

.NET 10 + .NET Aspireを使用したマイクロサービスアーキテクチャの従業員管理システム

## 🚀 クイックスタート

### 前提条件
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- .NET Aspire: NuGet パッケージとして提供（workload インストール不要）

### 実行方法

```bash
# リポジトリをクローン
git clone https://github.com/runceel/DotnetEmployeeManagementSystem.git
cd DotnetEmployeeManagementSystem

# 依存関係を復元
dotnet restore

# Aspire AppHostを起動
dotnet run --project src/AppHost
```

Aspireダッシュボードが自動的に開き、すべてのサービスを管理・監視できます。

## 📚 ドキュメント

詳細なドキュメントは **[docs/](docs/)** フォルダを参照してください：

- **[Getting Started](docs/getting-started.md)** - セットアップと実行の詳細手順
- **[アーキテクチャ概要](docs/architecture.md)** - システム設計とアーキテクチャ
- **[詳細アーキテクチャ設計書](docs/architecture-detailed.md)** - 技術的な詳細設計（推奨）
- **[開発ガイド](docs/development-guide.md)** - 開発者向けガイドライン
- **[通知サービス実装ガイド](docs/notification-service.md)** - イベント駆動通知システム
- **[Aspireダッシュボード](docs/aspire-dashboard.md)** - 監視と管理
- **[データベース管理](docs/database.md)** - マイグレーションとベストプラクティス
- **[Azure デプロイと Application Insights](docs/azure-deployment-appinsights.md)** - Azure へのデプロイと監視設定

## 🏗️ プロジェクト構成

```
DotnetEmployeeManagementSystem/
├── src/
│   ├── AppHost/                      # Aspireオーケストレーション
│   ├── ServiceDefaults/              # 共通設定（OpenTelemetry, Health checks）
│   ├── Services/
│   │   ├── EmployeeService/          # 従業員管理マイクロサービス
│   │   │   ├── Domain/              # ドメインモデル
│   │   │   ├── Application/         # ビジネスロジック
│   │   │   ├── Infrastructure/      # データアクセス
│   │   │   └── API/                 # Web API
│   │   ├── AuthService/             # 認証サービス
│   │   │   └── API/
│   │   └── NotificationService/     # 通知サービス
│   │       ├── Domain/
│   │       ├── Application/
│   │       ├── Infrastructure/
│   │       └── API/
│   ├── WebApps/
│   │   └── BlazorWeb/               # Blazor Web App（UI）
│   └── Shared/
│       └── Contracts/               # 共有DTO
├── tests/                           # ユニットテスト
└── docs/                            # ドキュメント
```

## 🛠️ 技術スタック

| カテゴリ | 技術 |
|---------|------|
| フレームワーク | .NET 10 |
| オーケストレーション | .NET Aspire 13.0.0 |
| フロントエンド | Blazor Web App + MudBlazor |
| データアクセス | Entity Framework Core 10 |
| データベース | SQLite（開発・Aspire統合）、Azure SQL（本番予定） |
| メッセージング | Redis (Pub/Sub) |
| 認証 | ASP.NET Core Identity |
| 可観測性 | OpenTelemetry（トレース、メトリクス、ログ） |
| ヘルスチェック | ASP.NET Core Health Checks |
| テスト | xUnit + Moq |

## 🎯 主要機能

- **マイクロサービスアーキテクチャ**: 疎結合で拡張性の高い設計
- **クリーンアーキテクチャ**: Domain駆動設計による保守性の向上
- **イベント駆動通知**: Redisを使用したリアルタイム通知システム
- **分散トレーシング**: OpenTelemetryによる完全な可観測性
- **Aspireダッシュボード**: リアルタイムの監視とデバッグ
- **自動サービスディスカバリー**: Aspireによる動的なサービス検出
- **統合ヘルスチェック**: すべてのサービスの健全性監視

## 📊 プロジェクト規模

### ソースコード統計（src フォルダー配下）

| ファイル種別 | ファイル数 | 行数 | 説明 |
|------------|----------|------|------|
| C# (.cs) | 146 | 11,667 | アプリケーションコード |
| Razor (.razor) | 19 | 3,143 | Blazor UIコンポーネント |
| プロジェクトファイル (.csproj) | 20 | 304 | プロジェクト定義 |
| JSON (.json) | 18 | 307 | 設定ファイル |
| CSS (.css) | 3 | 217 | カスタムスタイル |
| Markdown (.md) | 4 | 524 | ドキュメント |
| **合計** | **210** | **16,162** | ソースコード（サードパーティライブラリ除く） |

> **注記**: 上記の統計にはサードパーティライブラリ（Bootstrap等のwwwroot/lib配下のファイル）は含まれていません。

### レイヤー別の C# コード規模

| レイヤー | 行数 | 説明 |
|---------|------|------|
| Domain層 | 933 | ビジネスロジック・エンティティ |
| Application層 | 941 | ユースケース・サービス |
| Infrastructure層 | 3,201 | データアクセス・外部連携 |
| API層 | 3,529 | Web APIエンドポイント |
| UI層（Blazor） | 5,418 | フロントエンドUI |
| 共有ライブラリ | 748 | 共通コンポーネント |
| AppHost | 40 | Aspireオーケストレーション |

## 📖 詳細情報

### アーキテクチャ
システムは .NET Aspire を使用したマイクロサービスアーキテクチャで構成されています。各サービスは独立して開発・デプロイ可能で、ServiceDefaults による統一された可観測性を提供します。

- [アーキテクチャ概要](docs/architecture.md) - 高レベルアーキテクチャと設計原則
- [詳細アーキテクチャ設計書](docs/architecture-detailed.md) - 技術スタック詳細、データフロー、デプロイメント戦略

### 開発
新機能の追加、テストの作成、デバッグ方法については [開発ガイド](docs/development-guide.md) を参照してください。

### 運用
Aspireダッシュボードを使用した監視、ログの確認、トレースの分析については [Aspireダッシュボードガイド](docs/aspire-dashboard.md) を参照してください。

## 🤝 貢献

Issue や Pull Request は歓迎します！

## 📄 ライセンス

このプロジェクトは [LICENSE](LICENSE) ファイルに記載されているライセンスの下で公開されています。