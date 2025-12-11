# Blazor Web UI デザインカタログ

このデザインカタログは、AI（GitHub Copilot等）や開発者が一貫性のあるUI/UX を実装するためのガイドラインとリファレンスを提供します。

## 📋 目的

- **一貫性**: 全画面で統一されたデザインパターンを適用
- **効率性**: AI・開発者が素早く正しい実装を選択できる
- **保守性**: 再利用可能なパターンでメンテナンスコストを削減
- **品質**: ベストプラクティスに基づいた実装を促進
- **トークン効率**: AIが必要なパターンのみ読み込めるよう分割

## 🎯 クイックスタート

### UI画面を追加する場合

1. **画面タイプを特定**
   - データ一覧を表示？ → [一覧画面パターン](patterns/list-page.md)
   - 単一データの詳細？ → [詳細画面パターン](patterns/detail-page.md)
   - データ入力/編集？ → [編集画面パターン](patterns/edit-dialog.md)
   - 統計・概要表示？ → [ダッシュボードパターン](patterns/dashboard.md)

2. **テンプレートをコピー＆カスタマイズ**
   - 選択したパターンの「完全なコード例」をコピー
   - プロジェクトに合わせて変数名・API呼び出しをカスタマイズ

3. **チェックリストで確認**
   - [推奨・非推奨ルール](dos-and-donts.md)でベストプラクティスを確認
   - [デザイントークン](tokens.md)でカラー・スペーシングを参照

## 📁 ドキュメント構成

### 画面パターン（patterns/）

各画面タイプ別の完全なテンプレートと実装例：

| パターン | 用途 | ファイル |
|---------|------|---------|
| **一覧画面** | データ一覧表示、CRUD操作 | [list-page.md](patterns/list-page.md) |
| **詳細画面** | 単一データの詳細表示 | [detail-page.md](patterns/detail-page.md) |
| **編集画面** | ダイアログフォーム、バリデーション | [edit-dialog.md](patterns/edit-dialog.md) |
| **ダッシュボード** | 統計カード、アクティビティ表示 | [dashboard.md](patterns/dashboard.md) |

### ガイドライン

| ドキュメント | 内容 |
|-------------|------|
| [dos-and-donts.md](dos-and-donts.md) | 推奨・非推奨ルール（DO/DON'T） |
| [tokens.md](tokens.md) | デザイントークン（カラー、タイポグラフィ、スペーシング） |

## 🎯 デザイン原則

### 1. MudBlazor優先
- **常にMudBlazorコンポーネントを使用**してください
- カスタムCSSは最小限に抑え、MudBlazorのテーマシステムを活用
- Material Design ガイドラインに準拠

### 2. レスポンシブデザイン
- **モバイルファースト**のアプローチ
- `MudGrid`と`Breakpoint`を活用してレスポンシブレイアウト
- タッチデバイスでの操作性を考慮

### 3. アクセシビリティ
- 適切なラベルとARIA属性
- キーボード操作のサポート
- 色だけでなく、アイコンやテキストでも情報を伝達

### 4. ユーザーフィードバック
- **ローディング状態を必ず表示**（`MudProgressCircular`または`MudSkeleton`）
- **エラー状態を適切に処理**（`MudAlert`）
- **成功/失敗を通知**（`ISnackbar`）

### 5. 日本語優先
- すべてのUI文言は日本語
- 日付フォーマット: `yyyy/MM/dd` または `yyyy年MM月dd日`
- 数値フォーマット: カンマ区切り（例: 1,000）

## 💡 ベストプラクティス

### DO（推奨）
✅ MudBlazorコンポーネントを使用  
✅ ローディング・エラー・空状態を必ず実装  
✅ 日本語で一貫したUI文言  
✅ レスポンシブデザイン（MudGrid使用）  
✅ 適切なバリデーションメッセージ  
✅ Snackbarで操作結果を通知  
✅ 既存画面のパターンを参考にする  

### DON'T（非推奨）
❌ 生のHTML/CSSを使わない（MudBlazorで代替可能な場合）  
❌ ローディング状態を省略しない  
❌ エラーを握りつぶさない  
❌ ハードコードされた色やサイズ  
❌ 英語のUI文言  
❌ 例外を無視する  

## 🔍 参考リソース

### 公式ドキュメント
- [MudBlazor Documentation](https://mudblazor.com/)
- [Material Design Guidelines](https://material.io/design)

### プロジェクト内の参考実装
- 一覧画面: [Employees.razor](../../src/WebApps/BlazorWeb/Components/Pages/Employees.razor)
- 詳細画面: [EmployeeDetail.razor](../../src/WebApps/BlazorWeb/Components/Pages/EmployeeDetail.razor)
- ダイアログ: [EmployeeFormDialog.razor](../../src/WebApps/BlazorWeb/Components/Dialogs/EmployeeFormDialog.razor)
- ダッシュボード: [Dashboard.razor](../../src/WebApps/BlazorWeb/Components/Pages/Dashboard.razor)

## 📝 更新履歴

| 日付 | バージョン | 変更内容 |
|------|-----------|---------|
| 2025-12-11 | 1.1 | パターンファイルを分割してAIトークン効率化 |
| 2025-12-11 | 1.0 | 初版作成 - UIデザインカタログ策定 |

---

**このカタログは常に最新に保つことを目指しています。**  
新しいパターンや改善提案があれば、GitHub Issueでお知らせください。
