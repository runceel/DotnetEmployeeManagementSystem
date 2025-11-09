# Issue Reports Directory

このディレクトリは、Issue作業完了報告やタスク実施記録などの**一時的なドキュメント**を集約管理するために設けられています。

## 目的

- Issue対応完了時の実装報告書を一元管理
- 一時的な作業記録と恒久的な設計ドキュメントの明確な区別
- プロジェクト履歴の可視化と追跡性の向上
- リポジトリルートやdocs/の肥大化防止

## このディレクトリに含めるべきドキュメント

以下のような**一時的・記録的**なドキュメントをここに配置してください：

- ✅ Issue作業完了報告 (`ISSUE_XX_VERIFICATION.md`, `IMPLEMENTATION_COMPLETE_ISSUE_XX.md` など)
- ✅ 特定バグの修正報告 (`BUGFIX_VERIFICATION_ISSUE_XX.md` など)
- ✅ 特定機能の実装完了報告 (`FEATURE_XX_IMPLEMENTATION.md` など)
- ✅ タスク実施記録・検証レポート
- ✅ 一時的なスクリーンショット集やコード例（特定Issueに紐づくもの）

## docs/ディレクトリに配置すべきドキュメント

以下のような**恒久的・参照的**なドキュメントは `docs/` に配置してください：

- ✅ アーキテクチャ設計書 (`architecture.md`, `architecture-detailed.md`)
- ✅ 開発ガイド・コーディング規約 (`development-guide.md`)
- ✅ セットアップ手順 (`getting-started.md`)
- ✅ データベース設計・マイグレーションガイド (`database.md`)
- ✅ サービス仕様書 (`notification-service.md`, `authorization-implementation.md`)
- ✅ 運用・監視ガイド (`aspire-dashboard.md`)
- ✅ 恒久的な技術ドキュメント

## 命名規則

一時的なレポートの命名には、以下のガイドラインを推奨します：

### Issue番号ベース
```
issue-{番号}-{内容}.md

例：
- issue-3-verification.md
- issue-6-implementation-summary.md
- issue-8-screenshots.md
```

### 機能・バグ修正ベース
```
{機能名}-{タイプ}-{日付?}.md

例：
- dashboard-implementation.md
- notification-service-implementation.md
- bugfix-verification-login-button.md
```

### 日本語ファイル名について
- 基本的には英数字推奨（Git/URLエンコーディング問題の回避）
- 既存の日本語ファイル（例：`バグ修正報告.md`）も受け入れ可能
- 新規作成時は英語を推奨

## ファイル管理のライフサイクル

1. **作成**: Issue対応完了時にこのディレクトリに報告を作成
2. **保持**: プロジェクト履歴として保持（削除不要）
3. **参照**: 過去の実装経緯や意思決定を振り返る際に利用
4. **アーカイブ（オプション）**: 必要に応じて年度別・リリース別のサブディレクトリを作成可能

## サブディレクトリ構成例（オプション）

プロジェクトの成長に応じて、以下のような構成も検討できます：

```
.github/issue-reports/
├── README.md
├── 2024/
│   ├── issue-1-to-10/
│   └── issue-11-to-20/
├── 2025/
│   ├── issue-21-to-30/
│   └── ...
└── archived/
    └── old-format-reports/
```

現時点ではフラットな構造で管理し、必要に応じて将来的にサブディレクトリ化を検討してください。

## 移行履歴

このディレクトリへの移行は段階的に実施されます。

### 第1フェーズ：既存ファイルの移行
- リポジトリルートの一時的レポートを移動
- docs/内の一時的レポートを移動

### 第2フェーズ：運用定着
- 新しいIssue報告はこのディレクトリに直接作成
- ドキュメント管理ルールの周知

### 移行済みファイル一覧

以下のファイルがこのディレクトリに移行されました（2025-11-09）：

#### リポジトリルートから移行
- `BUGFIX_VERIFICATION.md` - バグ修正検証レポート
- `DASHBOARD_FEATURES.md` - ダッシュボード機能一覧
- `DASHBOARD_IMPLEMENTATION.md` - ダッシュボード実装報告
- `IMPLEMENTATION_COMPLETE.md` - Issue #6 実装完了報告
- `IMPLEMENTATION_SUMMARY_ISSUE_9.md` - Issue #9 実装サマリー
- `ISSUE_3_VERIFICATION.md` - Issue #3 実装確認レポート
- `NOTIFICATION_SERVICE_IMPLEMENTATION.md` - 通知サービス実装報告
- `バグ修正報告.md` - 従業員一覧画面の編集ボタンバグ修正報告

#### docs/から移行
- `issue-5-implementation-verification.md` - Issue #5 実装検証レポート
- `issue-6-code-examples.md` - Issue #6 コード例集
- `issue-6-implementation-summary.md` - Issue #6 実装サマリー
- `issue-8-implementation-summary.md` - Issue #8 実装サマリー
- `issue-8-screenshots.md` - Issue #8 スクリーンショット集

## 参考リンク

- **恒久的ドキュメント**: [docs/README.md](../../docs/README.md)
- **AIエージェント向けガイド**: [AGENTS.md](../../AGENTS.md)
- **プロジェクトルート**: [README.md](../../README.md)

## 質問・提案

ドキュメント管理についての質問や改善提案は、GitHubのIssueで受け付けています。
