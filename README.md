# 従業員管理システム

.NET 9 + .NET Aspireを使用したマイクロサービスアーキテクチャの従業員管理システム

## 🚀 クイックスタート

### 前提条件
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- .NET Aspire Workload: `dotnet workload install aspire`

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
- **[開発ガイド](docs/development-guide.md)** - 開発者向けガイドライン
- **[Aspireダッシュボード](docs/aspire-dashboard.md)** - 監視と管理
- **[データベース管理](docs/database.md)** - マイグレーションとベストプラクティス

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
│   │   └── AuthService/             # 認証サービス
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
| フレームワーク | .NET 9 |
| オーケストレーション | .NET Aspire 9.5.2 |
| フロントエンド | Blazor Web App + MudBlazor |
| データアクセス | Entity Framework Core 9 |
| データベース | SQLite（開発・Aspire統合）、Azure SQL（本番予定） |
| 認証 | ASP.NET Core Identity |
| 可観測性 | OpenTelemetry（トレース、メトリクス、ログ） |
| ヘルスチェック | ASP.NET Core Health Checks |
| テスト | xUnit + Moq |

## 🎯 主要機能

- **マイクロサービスアーキテクチャ**: 疎結合で拡張性の高い設計
- **クリーンアーキテクチャ**: Domain駆動設計による保守性の向上
- **分散トレーシング**: OpenTelemetryによる完全な可観測性
- **Aspireダッシュボード**: リアルタイムの監視とデバッグ
- **自動サービスディスカバリー**: Aspireによる動的なサービス検出
- **統合ヘルスチェック**: すべてのサービスの健全性監視

## 📖 詳細情報

### アーキテクチャ
システムは .NET Aspire を使用したマイクロサービスアーキテクチャで構成されています。各サービスは独立して開発・デプロイ可能で、ServiceDefaults による統一された可観測性を提供します。

詳細は [アーキテクチャドキュメント](docs/architecture.md) を参照してください。

### 開発
新機能の追加、テストの作成、デバッグ方法については [開発ガイド](docs/development-guide.md) を参照してください。

### 運用
Aspireダッシュボードを使用した監視、ログの確認、トレースの分析については [Aspireダッシュボードガイド](docs/aspire-dashboard.md) を参照してください。

## 🤝 貢献

Issue や Pull Request は歓迎します！

## 📄 ライセンス

このプロジェクトは [LICENSE](LICENSE) ファイルに記載されているライセンスの下で公開されています。