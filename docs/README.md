# ドキュメント

従業員管理システムの包括的なドキュメントです。

## 📚 ドキュメント一覧

### 入門

- **[Getting Started](getting-started.md)** - セットアップと実行方法
  - 前提条件
  - インストール手順
  - アプリケーションの実行
  - トラブルシューティング

### ユーザーマニュアル

- **[ユーザーマニュアル](manual/README.md)** - システムの使い方ガイド
  - [第1章: はじめに・システム概要](manual/01-intro.md) - システムの目的と利用方法
  - [第2章: ログイン・ログアウト](manual/02-login.md) - ログイン・ログアウト手順
  - [第3章: ホーム画面](manual/03-home.md) - ホーム画面の見方と基本操作
  - [第4章: ダッシュボード](manual/04-dashboard.md) - ダッシュボード画面の使い方
  - [第5章: 従業員管理](manual/05-employees.md) - 従業員情報の閲覧・登録・編集
  - [第6章: 勤怠管理](manual/06-attendance.md) - 勤怠記録の確認と集計
  - [第7章: 通知管理](manual/07-notification.md) - 通知の確認方法と手動通知の送信
  - [第8章: 画面共通操作](manual/08-common-ui.md) - ナビゲーション、エラー表示、共通UI操作
  - [第9章: トラブルシューティング](manual/09-trouble.md) - よくあるエラーと対処法
  - [第10章: FAQ](manual/10-faq.md) - よくある質問とその回答
  - [第11章: 付録](manual/11-appendix.md) - 用語解説、権限一覧、通知タイプ、問い合わせ窓口

### アーキテクチャ

- **[アーキテクチャ概要](architecture.md)** - システム設計とアーキテクチャ
  - 全体アーキテクチャ
  - コンポーネント構成
  - クリーンアーキテクチャ
  - データフロー
  - セキュリティ
  - 可観測性

- **[詳細アーキテクチャ設計書](architecture-detailed.md)** - 技術的な詳細設計
  - システムアーキテクチャ概要
  - 技術スタック詳細
  - プロジェクト構成
  - アーキテクチャパターン
  - データフロー図
  - サービス間通信
  - 可観測性とモニタリング
  - ヘルスチェック
  - セキュリティアーキテクチャ
  - データベース設計
  - デプロイメント戦略
  - スケーラビリティとパフォーマンス
  - 今後の拡張予定

### 開発

- **[開発ガイド](development-guide.md)** - 開発者向けガイドライン
  - 新機能の追加方法
  - コーディング規約
  - テスト戦略
  - デバッグ方法
  - パフォーマンス最適化

- **[Blazor レンダーモード設定ガイド](blazor-rendermode-configuration.md)** - Blazor レンダーモード設定
  - グローバル Interactive Server 設定
  - レンダーモードの種類と選択
  - 設定方法とベストプラクティス
  - トラブルシューティング

### 運用

- **[Aspireダッシュボード](aspire-dashboard.md)** - 監視と管理
  - ダッシュボードの使い方
  - ログの確認
  - トレースの分析
  - メトリクスの監視
  - トラブルシューティング

- **[データベース管理](database.md)** - データベースの管理
  - マイグレーション
  - シードデータ
  - クエリのベストプラクティス
  - 本番環境への移行

- **[Azure デプロイと Application Insights](azure-deployment-appinsights.md)** - Azureへのデプロイと監視
  - Application Insights と Log Analytics Workspace の自動プロビジョニング
  - OpenTelemetry との統合
  - Azure Developer CLI (`azd`) によるデプロイ
  - テレメトリデータの収集と分析
  - コスト管理とベストプラクティス

### サービス

- **[通知サービス実装ガイド](notification-service.md)** - イベント駆動通知システム
  - 概要とアーキテクチャ
  - イベント駆動設計
  - セットアップと使用方法
  - カスタマイズ方法
  - トラブルシューティング

### AI・MCP統合

- **[MCP統合設計書](mcp-integration-design.md)** - Model Context Protocol統合の設計方針
  - MCP概要とアーキテクチャ
  - 技術スタック・SDK選定
  - 各サービスMCPサーバー実装方針
  - チャット画面MCPクライアント実装方針
  - 通信プロトコル・Transport
  - セキュリティ設計
  - 実装ロードマップ

- **[MCP実装ガイド](mcp-implementation-guide.md)** - 実践的な開発手順とサンプルコード
  - 開発環境セットアップ
  - サービス別MCP実装例
  - Blazor MCPクライアント実装例
  - テスト実装
  - トラブルシューティング

### ユーザーマニュアル

- **[ユーザーマニュアル](manual/README.md)** - システムの使い方ガイド
  - [第8章: 画面共通操作](manual/08-common-ui.md) - ナビゲーション、エラー表示、共通UI操作

## 🚀 クイックスタート

### エンドユーザー向け
システムの使い方を知りたい方は：
1. [ユーザーマニュアル](manual/README.md) - 各機能の使い方ガイド

### 開発者向け
初めての方は、以下の順序でドキュメントを読むことをお勧めします：

1. [Getting Started](getting-started.md) - 開発環境のセットアップ
2. [アーキテクチャ概要](architecture.md) - システムの全体像を理解
3. [詳細アーキテクチャ設計書](architecture-detailed.md) - 技術的な詳細を理解（アーキテクトやシニア開発者向け）
4. [開発ガイド](development-guide.md) - コーディングを始める
5. [Aspireダッシュボード](aspire-dashboard.md) - デバッグと監視

## 🎯 ユースケース別ガイド

### ダッシュボードの使い方を知りたい
→ [第4章: ダッシュボード](manual/04-dashboard.md)

### 従業員情報を管理したい
→ [第5章: 従業員管理](manual/05-employees.md)

### 勤怠管理の使い方を知りたい
→ [第6章: 勤怠管理](manual/06-attendance.md)

### 新しい機能を追加したい
→ [開発ガイド - 新機能の追加](development-guide.md#新機能の追加)

### パフォーマンス問題を調査したい
→ [Aspireダッシュボード - パフォーマンス問題の調査](aspire-dashboard.md#シナリオ1-パフォーマンス問題の調査)

### データベースマイグレーションを作成したい
→ [データベース管理 - マイグレーション](database.md#マイグレーション)

### エラーをデバッグしたい
→ [Aspireダッシュボード - エラーのデバッグ](aspire-dashboard.md#シナリオ2-エラーのデバッグ)

### Azure にデプロイしたい
→ [Azure デプロイと Application Insights](azure-deployment-appinsights.md)

### Application Insights でアプリケーションを監視したい
→ [Azure デプロイと Application Insights - テレメトリデータの収集](azure-deployment-appinsights.md#収集されるテレメトリデータ)

### 新しいマイクロサービスを追加したい
→ [開発ガイド - 新しいマイクロサービスの追加](development-guide.md#2-新しいマイクロサービスの追加)

### 通知サービスについて知りたい
→ [通知サービス実装ガイド](notification-service.md)

### Blazor のレンダーモード設定について知りたい
→ [Blazor レンダーモード設定ガイド](blazor-rendermode-configuration.md)

### システムの操作方法を知りたい
→ [ユーザーマニュアル - 第8章: 画面共通操作](manual/08-common-ui.md)

### システムの用語や権限について知りたい
→ [ユーザーマニュアル - 第11章: 付録](manual/11-appendix.md)

### MCP（Model Context Protocol）による自然言語操作を実装したい
→ [MCP統合設計書](mcp-integration-design.md) → [MCP実装ガイド](mcp-implementation-guide.md)

### チャット機能でAIアシスタントを追加したい
→ [MCP統合設計書 - チャット画面MCPクライアント実装方針](mcp-integration-design.md#5-チャット画面mcpクライアント実装方針)

## 📖 技術スタック

### フレームワークとライブラリ

| 技術 | バージョン | 用途 |
|------|----------|------|
| .NET | 9.0 | アプリケーションフレームワーク |
| .NET Aspire | 9.5.2 | マイクロサービスオーケストレーション |
| Entity Framework Core | 9.0.10 | ORM |
| MudBlazor | 8.14.0 | UIコンポーネントライブラリ |
| xUnit | 2.x | テストフレームワーク |
| Moq | 4.20.72 | モックライブラリ |
| OpenTelemetry | 1.13.x | 可観測性 |

### データベース

- **開発環境**: SQLite
- **本番環境（予定）**: Azure SQL Database / PostgreSQL

## 🔗 外部リンク

- [.NET 10 ドキュメント](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/)
- [.NET Aspire ドキュメント](https://learn.microsoft.com/dotnet/aspire/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [MudBlazor](https://mudblazor.com/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)

## 📝 貢献

ドキュメントの改善提案やバグ報告は、GitHubのIssueまたはPull Requestで受け付けています。

## 📄 ライセンス

このプロジェクトは[LICENSE](../LICENSE)ファイルに記載されているライセンスの下で公開されています。
