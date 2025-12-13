# デザイントークン

このドキュメントは、Blazor Web UIで使用するデザイントークン（色、タイポグラフィ、スペーシングなど）を定義します。

## 📑 目次

- [カラーパレット](#カラーパレット)
- [タイポグラフィ](#タイポグラフィ)
- [スペーシング](#スペーシング)
- [ブレークポイント](#ブレークポイント)
- [エレベーション（影）](#エレベーション影)
- [アイコン](#アイコン)

---

## カラーパレット

### MudBlazor カラーシステム

MudBlazorは Material Design のカラーシステムを使用します。

#### プライマリカラー
```razor
<!-- Primary: ブランドカラー、主要アクション -->
<MudButton Color="Color.Primary">プライマリ</MudButton>
<MudText Color="Color.Primary">プライマリテキスト</MudText>
```

**用途**:
- 主要アクションボタン（保存、追加、検索）
- 重要な統計値
- アクティブな状態

#### セカンダリカラー
```razor
<!-- Secondary: 補助的な要素 -->
<MudButton Color="Color.Secondary">セカンダリ</MudButton>
<MudText Color="Color.Secondary">セカンダリテキスト</MudText>
```

**用途**:
- 補助的なアクション
- 補足情報テキスト
- 非アクティブな状態の説明

#### セマンティックカラー

```razor
<!-- Success: 成功、完了 -->
<MudButton Color="Color.Success">成功</MudButton>
<MudAlert Severity="Severity.Success">保存に成功しました</MudAlert>
<MudChip Color="Color.Success">アクティブ</MudChip>

<!-- Info: 情報 -->
<MudButton Color="Color.Info">情報</MudButton>
<MudAlert Severity="Severity.Info">追加情報があります</MudAlert>
<MudChip Color="Color.Info">情報</MudChip>

<!-- Warning: 警告 -->
<MudButton Color="Color.Warning">警告</MudButton>
<MudAlert Severity="Severity.Warning">注意が必要です</MudAlert>
<MudChip Color="Color.Warning">保留中</MudChip>

<!-- Error: エラー、削除 -->
<MudButton Color="Color.Error">削除</MudButton>
<MudAlert Severity="Severity.Error">エラーが発生しました</MudAlert>
<MudChip Color="Color.Error">エラー</MudChip>
```

#### その他のカラー

```razor
<!-- Default: デフォルト -->
<MudButton Color="Color.Default">デフォルト</MudButton>

<!-- Dark: ダーク -->
<MudButton Color="Color.Dark">ダーク</MudButton>

<!-- Tertiary: 3番目の強調色 -->
<MudButton Color="Color.Tertiary">ターシャリ</MudButton>

<!-- Inherit: 親要素から継承 -->
<MudText Color="Color.Inherit">継承</MudText>
```

### カラー使用ガイドライン

| 用途 | カラー | 例 |
|------|--------|-----|
| 主要アクション | Primary | 保存、追加、検索 |
| 補助アクション | Secondary | キャンセル、戻る |
| 削除操作 | Error | 削除、無効化 |
| 成功メッセージ | Success | 保存完了、登録完了 |
| 警告メッセージ | Warning | 注意喚起、確認要求 |
| 情報メッセージ | Info | 追加情報、ヒント |
| 詳細表示 | Info | 詳細ボタン、展開 |
| 編集操作 | Primary | 編集ボタン |

---

## タイポグラフィ

### MudText Typo（タイポグラフィ）

```razor
<!-- 見出し -->
<MudText Typo="Typo.h1">見出し1 (H1)</MudText>
<MudText Typo="Typo.h2">見出し2 (H2)</MudText>
<MudText Typo="Typo.h3">見出し3 (H3)</MudText>
<MudText Typo="Typo.h4">見出し4 (H4)</MudText>
<MudText Typo="Typo.h5">見出し5 (H5)</MudText>
<MudText Typo="Typo.h6">見出し6 (H6)</MudText>

<!-- 本文 -->
<MudText Typo="Typo.body1">本文1 (デフォルト)</MudText>
<MudText Typo="Typo.body2">本文2 (小さめ)</MudText>

<!-- その他 -->
<MudText Typo="Typo.subtitle1">サブタイトル1</MudText>
<MudText Typo="Typo.subtitle2">サブタイトル2</MudText>
<MudText Typo="Typo.caption">キャプション (小さい)</MudText>
<MudText Typo="Typo.button">ボタンテキスト</MudText>
<MudText Typo="Typo.overline">オーバーライン</MudText>
```

### 使用ガイドライン

| 要素 | Typo | 例 |
|------|------|-----|
| ページタイトル | h3 | `<MudText Typo="Typo.h3">従業員一覧</MudText>` |
| セクションタイトル | h4 | `<MudText Typo="Typo.h4">基本情報</MudText>` |
| カードタイトル | h6 | `<MudText Typo="Typo.h6">統計情報</MudText>` |
| 本文 | body1 | `<MudText Typo="Typo.body1">説明文</MudText>` |
| 補足テキスト | body2 | `<MudText Typo="Typo.body2" Color="Color.Secondary">補足情報</MudText>` |
| 日時表示 | caption | `<MudText Typo="Typo.caption">2025/12/11 10:30</MudText>` |

### タイポグラフィの例

```razor
<!-- ページヘッダー -->
<MudText Typo="Typo.h3" GutterBottom="true">従業員一覧</MudText>
<MudText Typo="Typo.body1" Class="mb-4">登録されている従業員の一覧です。</MudText>

<!-- カード内 -->
<MudCard>
    <MudCardContent>
        <MudText Typo="Typo.h6">総従業員数</MudText>
        <MudText Typo="Typo.h3" Color="Color.Primary">120</MudText>
        <MudText Typo="Typo.body2" Color="Color.Secondary">登録済み従業員</MudText>
    </MudCardContent>
</MudCard>

<!-- リスト項目 -->
<MudListItem>
    <MudText Typo="Typo.body1">山田太郎</MudText>
    <MudText Typo="Typo.caption" Color="Color.Secondary">2025/01/15 入社</MudText>
</MudListItem>
```

---

## スペーシング

### MudBlazor マージン/パディング クラス

MudBlazorは `.ma-*`, `.mt-*`, `.mb-*`, `.ml-*`, `.mr-*`, `.mx-*`, `.my-*` クラスを提供します。

#### マージン

```razor
<!-- 全方向マージン -->
<MudButton Class="ma-0">マージンなし (0px)</MudButton>
<MudButton Class="ma-1">マージン1 (4px)</MudButton>
<MudButton Class="ma-2">マージン2 (8px)</MudButton>
<MudButton Class="ma-3">マージン3 (12px)</MudButton>
<MudButton Class="ma-4">マージン4 (16px)</MudButton>
<MudButton Class="ma-5">マージン5 (20px)</MudButton>
<MudButton Class="ma-6">マージン6 (24px)</MudButton>

<!-- 上下マージン -->
<MudButton Class="my-2">上下マージン2 (8px)</MudButton>
<MudButton Class="my-4">上下マージン4 (16px)</MudButton>

<!-- 左右マージン -->
<MudButton Class="mx-2">左右マージン2 (8px)</MudButton>
<MudButton Class="mx-4">左右マージン4 (16px)</MudButton>

<!-- 個別マージン -->
<MudButton Class="mt-2">上マージン2 (8px)</MudButton>
<MudButton Class="mb-3">下マージン3 (12px)</MudButton>
<MudButton Class="ml-4">左マージン4 (16px)</MudButton>
<MudButton Class="mr-2">右マージン2 (8px)</MudButton>
```

#### パディング

```razor
<!-- 全方向パディング -->
<MudPaper Class="pa-2">パディング2 (8px)</MudPaper>
<MudPaper Class="pa-4">パディング4 (16px)</MudPaper>

<!-- 上下パディング -->
<MudPaper Class="py-2">上下パディング2 (8px)</MudPaper>

<!-- 左右パディング -->
<MudPaper Class="px-4">左右パディング4 (16px)</MudPaper>

<!-- 個別パディング -->
<MudPaper Class="pt-2 pb-3 pl-4 pr-2">個別パディング</MudPaper>
```

### スペーシングスケール

| クラス | サイズ | 用途 |
|--------|--------|------|
| `*-0` | 0px | スペースなし |
| `*-1` | 4px | 最小限のスペース |
| `*-2` | 8px | 密なスペース |
| `*-3` | 12px | 標準スペース（フォームフィールド間） |
| `*-4` | 16px | ゆとりのあるスペース（セクション間） |
| `*-5` | 20px | 大きなスペース |
| `*-6` | 24px | 最大スペース |

### 使用例

```razor
<!-- フォーム内のフィールド -->
<MudForm>
    <MudTextField Label="名前" Class="mb-3" />
    <MudTextField Label="メールアドレス" Class="mb-3" />
    <MudSelect Label="部署" Class="mb-3" />
</MudForm>

<!-- セクション間 -->
<MudText Typo="Typo.h3" Class="mb-4">セクション1</MudText>
<MudCard Class="mb-4">...</MudCard>

<MudText Typo="Typo.h3" Class="mb-4 mt-6">セクション2</MudText>
<MudCard>...</MudCard>

<!-- ボタングループ -->
<div class="d-flex">
    <MudButton Class="mr-2">キャンセル</MudButton>
    <MudButton>保存</MudButton>
</div>
```

---

## ブレークポイント

### MudGrid ブレークポイント

MudBlazorは以下のブレークポイントを定義しています：

| サイズ | ブレークポイント | デバイス |
|--------|-----------------|----------|
| `xs` | 0 - 599px | モバイル（縦） |
| `sm` | 600 - 959px | モバイル（横）、タブレット（縦） |
| `md` | 960 - 1279px | タブレット（横）、小型ノートPC |
| `lg` | 1280 - 1919px | デスクトップ |
| `xl` | 1920px以上 | 大型ディスプレイ |

### レスポンシブレイアウト例

```razor
<!-- 統計カード: モバイル全幅、タブレット半分、PC 1/4 -->
<MudGrid>
    <MudItem xs="12" sm="6" md="3">
        <MudCard>統計1</MudCard>
    </MudItem>
    <MudItem xs="12" sm="6" md="3">
        <MudCard>統計2</MudCard>
    </MudItem>
    <MudItem xs="12" sm="6" md="3">
        <MudCard>統計3</MudCard>
    </MudItem>
    <MudItem xs="12" sm="6" md="3">
        <MudCard>統計4</MudCard>
    </MudItem>
</MudGrid>

<!-- フォーム: モバイル全幅、タブレット以上は半分 -->
<MudGrid>
    <MudItem xs="12" sm="6">
        <MudTextField Label="名" />
    </MudItem>
    <MudItem xs="12" sm="6">
        <MudTextField Label="姓" />
    </MudItem>
</MudGrid>

<!-- メインコンテンツ + サイドバー -->
<MudGrid>
    <MudItem xs="12" md="8">
        <MudCard>メインコンテンツ</MudCard>
    </MudItem>
    <MudItem xs="12" md="4">
        <MudCard>サイドバー</MudCard>
    </MudItem>
</MudGrid>
```

### Breakpoint 指定（MudTable等）

```razor
<!-- テーブル: タブレット以下でカード表示に切り替え -->
<MudTable Items="@_items" Breakpoint="Breakpoint.Sm">
    ...
</MudTable>

<!-- Smサイズ以下ではカード形式、Md以上ではテーブル形式 -->
```

---

## エレベーション（影）

### MudCard / MudPaper のエレベーション

```razor
<!-- エレベーション（影の深さ） -->
<MudCard Elevation="0">影なし</MudCard>
<MudCard Elevation="1">影1（最小）</MudCard>
<MudCard Elevation="2">影2（標準）</MudCard>
<MudCard Elevation="4">影4（強調）</MudCard>
<MudCard Elevation="8">影8（浮遊）</MudCard>
<MudCard Elevation="16">影16（最大）</MudCard>
```

### 使用ガイドライン

| エレベーション | 用途 |
|----------------|------|
| 0 | フラット、ボーダーのみ |
| 1 | 控えめなカード |
| 2 | **標準カード（推奨）** |
| 4 | 強調カード、ホバー状態 |
| 8 | モーダル、ダイアログ |
| 16 | ドロワー、フローティング要素 |

### 使用例

```razor
<!-- 標準カード -->
<MudCard Elevation="2">
    <MudCardContent>...</MudCardContent>
</MudCard>

<!-- ホバーで強調 -->
<MudCard Elevation="2" @onmouseover="() => _elevation = 4" @onmouseout="() => _elevation = 2">
    <!-- 実際にはMudCardにはHover属性を使う -->
</MudCard>
```

---

## アイコン

### MudBlazor アイコン

MudBlazorは Material Design Icons を使用します。

#### よく使うアイコン

```razor
<!-- アクション -->
<MudButton StartIcon="@Icons.Material.Filled.Add">追加</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Edit">編集</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Delete">削除</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Save">保存</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Search">検索</MudButton>

<!-- ナビゲーション -->
<MudButton StartIcon="@Icons.Material.Filled.ArrowBack">戻る</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.ArrowForward">次へ</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Home">ホーム</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Menu">メニュー</MudButton>

<!-- 情報 -->
<MudButton StartIcon="@Icons.Material.Filled.Info">情報</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Help">ヘルプ</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Visibility">詳細</MudButton>

<!-- 状態 -->
<MudChip Icon="@Icons.Material.Filled.CheckCircle" Color="Color.Success">完了</MudChip>
<MudChip Icon="@Icons.Material.Filled.Error" Color="Color.Error">エラー</MudChip>
<MudChip Icon="@Icons.Material.Filled.Warning" Color="Color.Warning">警告</MudChip>

<!-- その他 -->
<MudButton StartIcon="@Icons.Material.Filled.Download">ダウンロード</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Upload">アップロード</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Settings">設定</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Person">ユーザー</MudButton>
<MudButton StartIcon="@Icons.Material.Filled.Notifications">通知</MudButton>
```

#### Outlined アイコン

```razor
<!-- 軽量なアウトラインアイコン -->
<MudButton StartIcon="@Icons.Material.Outlined.Add">追加</MudButton>
<MudButton StartIcon="@Icons.Material.Outlined.Edit">編集</MudButton>
<MudButton StartIcon="@Icons.Material.Outlined.Delete">削除</MudButton>
```

### アイコン使用ガイドライン

| アクション | アイコン | 色 |
|-----------|---------|-----|
| 追加 | `Icons.Material.Filled.Add` | Primary |
| 編集 | `Icons.Material.Filled.Edit` | Primary |
| 削除 | `Icons.Material.Filled.Delete` | Error |
| 詳細 | `Icons.Material.Filled.Visibility` | Info |
| 保存 | `Icons.Material.Filled.Save` | Primary |
| 検索 | `Icons.Material.Filled.Search` | Primary |
| キャンセル | `Icons.Material.Filled.Close` | Default |
| 成功 | `Icons.Material.Filled.CheckCircle` | Success |
| エラー | `Icons.Material.Filled.Error` | Error |
| 警告 | `Icons.Material.Filled.Warning` | Warning |

### アイコン配置

```razor
<!-- 左側アイコン -->
<MudButton StartIcon="@Icons.Material.Filled.Add">追加</MudButton>

<!-- 右側アイコン -->
<MudButton EndIcon="@Icons.Material.Filled.ArrowForward">次へ</MudButton>

<!-- アイコンのみ -->
<MudIconButton Icon="@Icons.Material.Filled.Edit" Color="Color.Primary" />

<!-- Alert内 -->
<MudAlert Severity="Severity.Success" Icon="@Icons.Material.Filled.CheckCircle">
    成功しました
</MudAlert>
```

---

## まとめ

### デザイントークン チートシート

#### カラー
- **Primary**: 主要アクション
- **Secondary**: 補助的要素
- **Success**: 成功、完了
- **Info**: 情報、詳細
- **Warning**: 警告
- **Error**: エラー、削除

#### タイポグラフィ
- **h3**: ページタイトル
- **h4**: セクションタイトル
- **h6**: カードタイトル
- **body1**: 本文
- **body2**: 補足テキスト
- **caption**: 日時、小さい情報

#### スペーシング
- **mb-3**: フォームフィールド間（12px）
- **mb-4**: セクション間（16px）
- **mt-6**: 大きなセクション間隔（24px）

#### ブレークポイント
- **xs="12"**: モバイル全幅
- **sm="6"**: タブレット半分
- **md="4"**: PC 1/3

#### エレベーション
- **2**: 標準カード
- **4**: 強調カード
- **8**: モーダル

### 参考リソース

- [README.md](README.md) - デザインカタログ概要
- [page-patterns.md](page-patterns.md) - 画面パターン別テンプレート
- [dos-and-donts.md](dos-and-donts.md) - 推奨・非推奨ルール
- [MudBlazor Documentation](https://mudblazor.com/) - 公式ドキュメント

---

**デザイントークンの追加提案や改善案は GitHub Issue でお知らせください！**
