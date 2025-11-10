# ドキュメント

従業員管理システムの包括的なドキュメントです。

## 📚 ドキュメント一覧

### 入門

- **[Getting Started](getting-started.md)** - セットアップと実行方法
  - 前提条件
  - インストール手順
  - アプリケーションの実行
  - トラブルシューティング

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

### サービス

- **[通知サービス実装ガイド](notification-service.md)** - イベント駆動通知システム
  - 概要とアーキテクチャ
  - イベント駆動設計
  - セットアップと使用方法
  - カスタマイズ方法
  - トラブルシューティング

### ユーザーマニュアル

- **[ユーザーマニュアル](manual/)** - エンドユーザー向けマニュアル
  - [第11章: 付録](manual/11-appendix.md) - 用語解説、権限一覧、通知タイプ、問い合わせ窓口
  - ※ 他の章は順次追加予定

## 🚀 クイックスタート

初めての方は、以下の順序でドキュメントを読むことをお勧めします：

1. [Getting Started](getting-started.md) - 開発環境のセットアップ
2. [アーキテクチャ概要](architecture.md) - システムの全体像を理解
3. [詳細アーキテクチャ設計書](architecture-detailed.md) - 技術的な詳細を理解（アーキテクトやシニア開発者向け）
4. [開発ガイド](development-guide.md) - コーディングを始める
5. [Aspireダッシュボード](aspire-dashboard.md) - デバッグと監視

## 🎯 ユースケース別ガイド

### 新しい機能を追加したい
→ [開発ガイド - 新機能の追加](development-guide.md#新機能の追加)

### パフォーマンス問題を調査したい
→ [Aspireダッシュボード - パフォーマンス問題の調査](aspire-dashboard.md#シナリオ1-パフォーマンス問題の調査)

### データベースマイグレーションを作成したい
→ [データベース管理 - マイグレーション](database.md#マイグレーション)

### エラーをデバッグしたい
→ [Aspireダッシュボード - エラーのデバッグ](aspire-dashboard.md#シナリオ2-エラーのデバッグ)

### 新しいマイクロサービスを追加したい
→ [開発ガイド - 新しいマイクロサービスの追加](development-guide.md#2-新しいマイクロサービスの追加)

### 通知サービスについて知りたい
→ [通知サービス実装ガイド](notification-service.md)

### Blazor のレンダーモード設定について知りたい
→ [Blazor レンダーモード設定ガイド](blazor-rendermode-configuration.md)

### システムの用語や権限について知りたい
→ [ユーザーマニュアル - 第11章: 付録](manual/11-appendix.md)

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

- [.NET 9 ドキュメント](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-9/)
- [.NET Aspire ドキュメント](https://learn.microsoft.com/dotnet/aspire/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [MudBlazor](https://mudblazor.com/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)

## 📝 貢献

ドキュメントの改善提案やバグ報告は、GitHubのIssueまたはPull Requestで受け付けています。

## 📄 ライセンス

このプロジェクトは[LICENSE](../LICENSE)ファイルに記載されているライセンスの下で公開されています。
