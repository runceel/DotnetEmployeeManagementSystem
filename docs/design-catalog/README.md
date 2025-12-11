# Blazor Web UI デザインカタログ

このデザインカタログは、AI（GitHub Copilot等）や開発者が一貫性のあるUI/UX を実装するためのガイドラインとリファレンスを提供します。

## 📋 目的

- **一貫性**: 全画面で統一されたデザインパターンを適用
- **効率性**: AI・開発者が素早く正しい実装を選択できる
- **保守性**: 再利用可能なパターンでメンテナンスコストを削減
- **品質**: ベストプラクティスに基づいた実装を促進

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

## 🎨 MudBlazor 主要コンポーネント クイックリファレンス

### レイアウト

#### MudLayout
```razor
<MudLayout>
    <MudAppBar>...</MudAppBar>
    <MudDrawer>...</MudDrawer>
    <MudMainContent>
        <MudContainer MaxWidth="MaxWidth.Large">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>
```

#### MudGrid / MudItem
```razor
<MudGrid>
    <MudItem xs="12" sm="6" md="4">
        <!-- コンテンツ -->
    </MudItem>
</MudGrid>
```
- `xs`: 0-599px（モバイル）
- `sm`: 600-959px（タブレット縦）
- `md`: 960-1279px（タブレット横/小型ノートPC）
- `lg`: 1280-1919px（デスクトップ）
- `xl`: 1920px以上（大型ディスプレイ）

### データ表示

#### MudTable
```razor
<MudTable Items="@_items" Hover="true" Breakpoint="Breakpoint.Sm" Dense="true">
    <HeaderContent>
        <MudTh>列名</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="列名">@context.Property</MudTd>
    </RowTemplate>
</MudTable>
```

#### MudCard
```razor
<MudCard Elevation="2">
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">タイトル</MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        <!-- コンテンツ -->
    </MudCardContent>
    <MudCardActions>
        <MudButton>アクション</MudButton>
    </MudCardActions>
</MudCard>
```

### フォーム

#### MudForm + MudTextField
```razor
<MudForm @ref="_form" @bind-IsValid="@_isValid">
    <MudTextField @bind-Value="_model.Name" 
                  Label="名前" 
                  Required="true" 
                  RequiredError="名前を入力してください。"
                  Variant="Variant.Outlined"
                  Class="mb-3" />
</MudForm>
```

#### MudDialog
```razor
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">@Title</MudText>
    </TitleContent>
    <DialogContent>
        <!-- フォーム等 -->
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="OnCancel">キャンセル</MudButton>
        <MudButton Color="Color.Primary" OnClick="OnSubmit">保存</MudButton>
    </DialogActions>
</MudDialog>
```

### フィードバック

#### ローディング（インライン）
```razor
@if (_loading)
{
    <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
    <MudText Typo="Typo.body1" Class="mt-2">データを読み込み中...</MudText>
}
```

#### ローディング（スケルトン）
```razor
@if (_isLoading)
{
    <MudSkeleton SkeletonType="SkeletonType.Text" Width="60%" />
    <MudSkeleton SkeletonType="SkeletonType.Text" Width="40%" Height="3rem" />
}
```

#### エラー表示
```razor
@if (_error)
{
    <MudAlert Severity="Severity.Error" Class="my-4">
        @_errorMessage
    </MudAlert>
    <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="Retry">
        再試行
    </MudButton>
}
```

#### 通知（Snackbar）
```razor
@code {
    [Inject] ISnackbar Snackbar { get; set; }
    
    private void ShowNotification()
    {
        Snackbar.Add("操作が完了しました。", Severity.Success);
        Snackbar.Add("エラーが発生しました。", Severity.Error);
        Snackbar.Add("警告メッセージ", Severity.Warning);
        Snackbar.Add("情報メッセージ", Severity.Info);
    }
}
```

### ナビゲーション

#### MudBreadcrumbs
```razor
<MudBreadcrumbs Items="_breadcrumbItems" Class="mb-4"></MudBreadcrumbs>

@code {
    private List<BreadcrumbItem> _breadcrumbItems = new()
    {
        new BreadcrumbItem("ホーム", href: "/"),
        new BreadcrumbItem("一覧", href: "/items"),
        new BreadcrumbItem("詳細", href: null, disabled: true)
    };
}
```

### ボタン

#### MudButton（バリエーション）
```razor
<!-- プライマリアクション -->
<MudButton Variant="Variant.Filled" 
           Color="Color.Primary" 
           StartIcon="@Icons.Material.Filled.Add">
    追加
</MudButton>

<!-- セカンダリアクション -->
<MudButton Variant="Variant.Outlined" 
           Color="Color.Default">
    キャンセル
</MudButton>

<!-- テキストボタン -->
<MudButton Variant="Variant.Text" 
           Color="Color.Primary">
    詳細
</MudButton>

<!-- アイコンボタン -->
<MudIconButton Icon="@Icons.Material.Filled.Edit" 
               Color="Color.Primary" 
               Size="Size.Small" />
```

#### MudButtonGroup
```razor
<MudButtonGroup Variant="Variant.Text" Size="Size.Small">
    <MudButton Color="Color.Primary" StartIcon="@Icons.Material.Filled.Edit">
        編集
    </MudButton>
    <MudButton Color="Color.Info" StartIcon="@Icons.Material.Filled.Visibility">
        詳細
    </MudButton>
    <MudButton Color="Color.Error" StartIcon="@Icons.Material.Filled.Delete">
        削除
    </MudButton>
</MudButtonGroup>
```

## 📚 ドキュメント構成

このデザインカタログは以下のドキュメントで構成されています：

1. **[page-patterns.md](page-patterns.md)** - 画面パターン別テンプレート
   - 一覧画面
   - 詳細画面
   - 編集画面（ダイアログ/ページ）
   - ダッシュボード

2. **[dos-and-donts.md](dos-and-donts.md)** - 推奨・非推奨ルール
   - コンポーネント選択
   - 状態管理
   - エラーハンドリング
   - パフォーマンス

3. **[tokens.md](tokens.md)** - デザイントークン
   - カラーパレット
   - タイポグラフィ
   - スペーシング
   - ブレークポイント

## 🚀 使い方（AI・開発者向け）

### 新しいUI画面を追加する場合

1. **画面タイプを特定**: 一覧/詳細/編集/ダッシュボードのいずれか
2. **[page-patterns.md](page-patterns.md)** から該当パターンを選択
3. テンプレートコードをコピーして必要な箇所をカスタマイズ
4. **[dos-and-donts.md](dos-and-donts.md)** でベストプラクティスを確認
5. **[tokens.md](tokens.md)** でデザイントークンを参照

### 既存画面を修正する場合

1. 類似画面を `src/WebApps/BlazorWeb/Components/Pages/` から検索
2. このカタログのパターンと照合
3. 一貫性を保ちながら修正

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
- 一覧画面: `src/WebApps/BlazorWeb/Components/Pages/Employees.razor`
- 詳細画面: `src/WebApps/BlazorWeb/Components/Pages/EmployeeDetail.razor`
- ダイアログ: `src/WebApps/BlazorWeb/Components/Dialogs/EmployeeFormDialog.razor`
- ダッシュボード: `src/WebApps/BlazorWeb/Components/Pages/Dashboard.razor`

## 📝 更新履歴

| 日付 | バージョン | 変更内容 |
|------|-----------|---------|
| 2025-12-11 | 1.0 | 初版作成 - UIデザインカタログ策定 |

---

**このカタログは常に最新に保つことを目指しています。**  
新しいパターンや改善提案があれば、GitHub Issueでお知らせください。
