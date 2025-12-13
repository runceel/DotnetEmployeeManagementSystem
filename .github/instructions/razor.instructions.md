---
name: '画面開発ガイドライン'
applyTo: '**/*.razor'
description: 'MudBlazorを使用した画面開発のベストプラクティスをまとめたガイドラインです。'
---

# 画面開発ガイドライン

## デザインカタログの参照

Blazor画面を作成・修正する際は、必ず以下のデザインカタログを参照してください：

- **[デザインカタログ README](../../docs/design-catalog/README.md)** - UIパターン、コンポーネント使用ガイドのインデックス
- **[推奨・非推奨ルール](../../docs/design-catalog/dos-and-donts.md)** - UIベストプラクティス
- **[デザイントークン](../../docs/design-catalog/tokens.md)** - カラー、タイポグラフィ、スペーシング定義

### 画面パターン（必要なパターンのみ参照）

- **[一覧画面](../../docs/design-catalog/patterns/list-page.md)** - データ一覧、CRUD操作
- **[詳細画面](../../docs/design-catalog/patterns/detail-page.md)** - 単一データ詳細表示
- **[編集画面](../../docs/design-catalog/patterns/edit-dialog.md)** - ダイアログフォーム
- **[ダッシュボード](../../docs/design-catalog/patterns/dashboard.md)** - 統計、アクティビティ表示

## 基本ルール

1. **MudBlazorコンポーネントを優先使用** - 生のHTML/CSSは最小限に
2. **ローディング状態・エラー状態・空状態を必ず実装**
3. **日本語UI文言を一貫使用**
4. **レスポンシブデザイン** - `MudGrid`と`Breakpoint`を活用

