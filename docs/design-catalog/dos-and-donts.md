# 推奨・非推奨ルール（Dos and Don'ts）

このドキュメントは、Blazor Web UI開発におけるベストプラクティスと避けるべきアンチパターンをまとめています。

## 📑 目次

- [コンポーネント選択](#コンポーネント選択)
- [状態管理](#状態管理)
- [エラーハンドリング](#エラーハンドリング)
- [パフォーマンス](#パフォーマンス)
- [アクセシビリティ](#アクセシビリティ)
- [コーディングスタイル](#コーディングスタイル)
- [セキュリティ](#セキュリティ)

---

## コンポーネント選択

### ✅ DO（推奨）

#### MudBlazorコンポーネントを優先
```razor
<!-- 推奨: MudBlazorコンポーネントを使用 -->
<MudButton Variant="Variant.Filled" Color="Color.Primary">保存</MudButton>
<MudTextField @bind-Value="_model.Name" Label="名前" />
```

#### 適切なコンポーネントを選択
```razor
<!-- 推奨: データ一覧にはMudTableを使用 -->
<MudTable Items="@_items" Hover="true" Dense="true">
    <HeaderContent>...</HeaderContent>
    <RowTemplate>...</RowTemplate>
</MudTable>

<!-- 推奨: 統計情報にはMudCardを使用 -->
<MudCard Elevation="2">
    <MudCardContent>
        <MudText Typo="Typo.h6">総数</MudText>
        <MudText Typo="Typo.h3" Color="Color.Primary">@_count</MudText>
    </MudCardContent>
</MudCard>
```

#### レスポンシブレイアウトにMudGridを使用
```razor
<!-- 推奨: MudGridでレスポンシブ対応 -->
<MudGrid>
    <MudItem xs="12" sm="6" md="4">
        <!-- モバイル: 全幅、タブレット: 半分、PC: 1/3 -->
    </MudItem>
</MudGrid>
```

### ❌ DON'T（非推奨）

#### 生のHTMLを使わない
```razor
<!-- 非推奨: 生のHTMLボタン -->
<button class="btn btn-primary" onclick="Save()">保存</button>

<!-- 非推奨: 生のHTMLテーブル -->
<table class="table">
    <tr><td>...</td></tr>
</table>

<!-- 非推奨: カスタムCSSでスタイリング -->
<div style="padding: 10px; margin: 5px; background: blue;">
    <!-- MudPaperやMudCardを使うべき -->
</div>
```

#### 不適切なコンポーネント選択
```razor
<!-- 非推奨: 大量データをMudListで表示 -->
<MudList>
    @foreach (var item in _thousandsOfItems) { ... }
</MudList>
<!-- → MudTableまたは仮想化コンポーネントを使用 -->

<!-- 非推奨: 統計情報をMudAlertで表示 -->
<MudAlert Severity="Severity.Info">総数: @_count</MudAlert>
<!-- → MudCardを使用 -->
```

---

## 状態管理

### ✅ DO（推奨）

#### 明確な状態フラグを使用
```razor
@code {
    private bool _loading = true;
    private bool _error = false;
    private string _errorMessage = string.Empty;
    private IEnumerable<ItemDto>? _items;
}
```

#### 状態に応じた条件分岐
```razor
@if (_loading)
{
    <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
}
else if (_error)
{
    <MudAlert Severity="Severity.Error">@_errorMessage</MudAlert>
}
else if (_items is null || !_items.Any())
{
    <MudAlert Severity="Severity.Info">データがありません</MudAlert>
}
else
{
    <MudTable Items="@_items">...</MudTable>
}
```

#### ローディング状態を必ず表示
```razor
<!-- 推奨: ローディング中はスピナーまたはスケルトンを表示 -->
@if (_loading)
{
    <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
    <MudText Typo="Typo.body1" Class="mt-2">データを読み込み中...</MudText>
}

<!-- または -->
@if (_loading)
{
    <MudSkeleton SkeletonType="SkeletonType.Text" Width="60%" />
    <MudSkeleton SkeletonType="SkeletonType.Text" Width="40%" Height="3rem" />
}
```

#### 状態を適切に初期化・リセット
```razor
@code {
    private async Task LoadData()
    {
        _loading = true;
        _error = false;
        _errorMessage = string.Empty;  // リセット

        try
        {
            _items = await ApiClient.GetItemsAsync();
        }
        catch (Exception ex)
        {
            _error = true;
            _errorMessage = ex.Message;
        }
        finally
        {
            _loading = false;  // 必ず実行
        }
    }
}
```

### ❌ DON'T（非推奨）

#### ローディング状態を省略
```razor
<!-- 非推奨: ローディング中に何も表示しない -->
@if (_items is not null)
{
    <MudTable Items="@_items">...</MudTable>
}
<!-- ユーザーは何が起きているか分からない -->
```

#### 複雑な状態管理
```razor
<!-- 非推奨: 複数のフラグで複雑な状態管理 -->
@code {
    private bool _isLoadingData;
    private bool _isLoadingMore;
    private bool _hasData;
    private bool _hasError;
    private bool _isInitialized;
    // 複雑すぎて管理しにくい
}

<!-- 推奨: シンプルな状態フラグ -->
@code {
    private bool _loading;
    private bool _error;
    private IEnumerable<ItemDto>? _items;
    // シンプルで明確
}
```

#### 状態の不完全なリセット
```razor
<!-- 非推奨: エラー状態をリセットしない -->
@code {
    private async Task LoadData()
    {
        _loading = true;
        // _error = false; を忘れている
        
        try { ... }
        catch (Exception ex)
        {
            _error = true;
            _errorMessage = ex.Message;
        }
        finally
        {
            _loading = false;
        }
    }
}
```

---

## エラーハンドリング

### ✅ DO（推奨）

#### try-catchで例外を適切に処理
```razor
@code {
    private async Task CreateItem(ItemFormModel model)
    {
        try
        {
            var request = new CreateItemRequest { Name = model.Name };
            await ItemApiClient.CreateItemAsync(request);
            Snackbar.Add("項目を追加しました。", Severity.Success);
            await LoadItems();
        }
        catch (UnauthorizedAccessException ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"エラー: {ex.Message}", Severity.Error);
        }
    }
}
```

#### 具体的なエラーメッセージを表示
```razor
<!-- 推奨: 具体的なエラーメッセージ -->
<MudAlert Severity="Severity.Error">
    データの読み込みに失敗しました: @_errorMessage
</MudAlert>
<MudButton OnClick="LoadData">再試行</MudButton>
```

#### 複数の例外タイプを区別
```razor
@code {
    try
    {
        await ApiClient.UpdateAsync(id, request);
    }
    catch (UnauthorizedAccessException ex)
    {
        Snackbar.Add("権限がありません。", Severity.Error);
    }
    catch (InvalidOperationException ex)
    {
        Snackbar.Add("操作が無効です。", Severity.Warning);
    }
    catch (HttpRequestException ex)
    {
        Snackbar.Add("ネットワークエラーが発生しました。", Severity.Error);
    }
    catch (Exception ex)
    {
        Snackbar.Add($"予期しないエラー: {ex.Message}", Severity.Error);
    }
}
```

### ❌ DON'T（非推奨）

#### 例外を握りつぶす
```razor
<!-- 非推奨: 例外を無視 -->
@code {
    private async Task LoadData()
    {
        try
        {
            _items = await ApiClient.GetItemsAsync();
        }
        catch
        {
            // 何もしない - エラーが隠蔽される
        }
    }
}
```

#### 曖昧なエラーメッセージ
```razor
<!-- 非推奨: 曖昧なエラーメッセージ -->
<MudAlert Severity="Severity.Error">
    エラーが発生しました。
</MudAlert>
<!-- ユーザーは何が問題か分からない -->

<!-- 推奨: 具体的なメッセージ -->
<MudAlert Severity="Severity.Error">
    データの読み込みに失敗しました: @_errorMessage
    <MudButton OnClick="LoadData">再試行</MudButton>
</MudAlert>
```

#### すべての例外を同じように扱う
```razor
<!-- 非推奨: すべての例外を同じように扱う -->
@code {
    try
    {
        await ApiClient.DeleteAsync(id);
    }
    catch (Exception ex)
    {
        Snackbar.Add("エラーが発生しました", Severity.Error);
        // UnauthorizedAccessException と HttpRequestException を区別しない
    }
}
```

---

## パフォーマンス

### ✅ DO（推奨）

#### 非同期メソッドを使用
```razor
@code {
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _items = await ApiClient.GetItemsAsync();
    }
}
```

#### 大量データには仮想化を使用
```razor
<!-- 推奨: Virtualizeで大量データを効率的に表示 -->
<Virtualize Items="@_thousandsOfItems" Context="item">
    <MudListItem>@item.Name</MudListItem>
</Virtualize>
```

#### 条件付きレンダリングを活用
```razor
<!-- 推奨: 条件に応じてレンダリング -->
@if (AuthStateService.IsAdmin)
{
    <MudButton OnClick="Delete">削除</MudButton>
}
```

#### 適切なライフサイクルメソッドを使用
```razor
@code {
    // 推奨: 初回読み込みはOnInitializedAsync
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    // 推奨: パラメータ変更時はOnParametersSetAsync
    protected override async Task OnParametersSetAsync()
    {
        if (_previousId != Id)
        {
            await LoadDataAsync();
            _previousId = Id;
        }
    }
}
```

### ❌ DON'T（非推奨）

#### 同期メソッドをブロッキング呼び出し
```razor
<!-- 非推奨: .Result でブロッキング -->
@code {
    protected override void OnInitialized()
    {
        _items = ApiClient.GetItemsAsync().Result;  // デッドロックの可能性
    }
}

<!-- 推奨: async/await を使用 -->
@code {
    protected override async Task OnInitializedAsync()
    {
        _items = await ApiClient.GetItemsAsync();
    }
}
```

#### 大量データを一度に表示
```razor
<!-- 非推奨: 数千件のデータを一度に表示 -->
<MudList>
    @foreach (var item in _thousandsOfItems)
    {
        <MudListItem>@item.Name</MudListItem>
    }
</MudList>

<!-- 推奨: 仮想化を使用 -->
<Virtualize Items="@_thousandsOfItems" Context="item">
    <MudListItem>@item.Name</MudListItem>
</Virtualize>
```

#### 不要な再レンダリング
```razor
<!-- 非推奨: すべてのプロパティ変更で再レンダリング -->
@code {
    private int _counter;
    
    private void IncrementCounter()
    {
        _counter++;
        StateHasChanged();  // 不要な場合が多い
    }
}

<!-- 推奨: 必要な場合のみStateHasChanged -->
<!-- Blazorは自動的に再レンダリングするため、以下の場合のみ明示的に呼ぶ: -->
<!-- 1. バックグラウンドスレッドからUI更新する場合 -->
<!-- 2. イベントハンドラ外でプロパティを変更した場合 -->
<!-- 3. カスタムイベントで状態変更を通知する場合 -->
```

---

## アクセシビリティ

### ✅ DO（推奨）

#### 適切なラベルを付ける
```razor
<!-- 推奨: すべての入力フィールドにラベル -->
<MudTextField @bind-Value="_model.Name" 
              Label="名前" 
              Required="true" />

<MudDatePicker @bind-Date="_date" 
               Label="入社日" />
```

#### キーボード操作をサポート
```razor
<!-- 推奨: MudBlazorコンポーネントは自動的にキーボード操作をサポート -->
<MudButton OnClick="Submit">送信</MudButton>
<!-- Tab、Enter、Spaceキーで操作可能 -->
```

#### 色だけでなく、アイコンやテキストも使用
```razor
<!-- 推奨: 色 + アイコン + テキスト -->
<MudAlert Severity="Severity.Success" Icon="@Icons.Material.Filled.CheckCircle">
    保存に成功しました。
</MudAlert>

<MudButton Variant="Variant.Filled" 
           Color="Color.Primary" 
           StartIcon="@Icons.Material.Filled.Add">
    追加
</MudButton>
```

### ❌ DON'T（非推奨）

#### ラベルなしの入力フィールド
```razor
<!-- 非推奨: ラベルなし -->
<MudTextField @bind-Value="_model.Name" />
<!-- スクリーンリーダーユーザーには何のフィールドか分からない -->
```

#### 色だけで情報を伝える
```razor
<!-- 非推奨: 色だけで状態を表現 -->
<span style="color: red;">エラー</span>
<span style="color: green;">成功</span>

<!-- 推奨: MudAlertでアイコン + 色 + テキスト -->
<MudAlert Severity="Severity.Error">エラーが発生しました</MudAlert>
<MudAlert Severity="Severity.Success">成功しました</MudAlert>
```

---

## コーディングスタイル

### ✅ DO（推奨）

#### 日本語UI文言を一貫して使用
```razor
<!-- 推奨: すべて日本語 -->
<PageTitle>従業員一覧 - 従業員管理システム</PageTitle>
<MudButton>保存</MudButton>
<MudAlert>データを読み込み中...</MudAlert>

@code {
    Snackbar.Add("保存に成功しました。", Severity.Success);
}
```

#### 日付フォーマットを統一
```razor
<!-- 推奨: 統一されたフォーマット -->
<MudTd>@item.CreatedAt.ToString("yyyy/MM/dd")</MudTd>
<MudTd>@item.UpdatedAt.ToString("yyyy/MM/dd HH:mm:ss")</MudTd>
<MudField>@item.HireDate.ToString("yyyy年MM月dd日")</MudField>
```

#### MudBlazorのVariantを統一
```razor
<!-- 推奨: フォーム内では統一 -->
<MudForm>
    <MudTextField Variant="Variant.Outlined" />
    <MudSelect Variant="Variant.Outlined" />
    <MudDatePicker Variant="Variant.Outlined" />
</MudForm>
```

#### 適切なマージンとスペーシング
```razor
<!-- 推奨: Class属性でマージンを設定 -->
<MudText Typo="Typo.h3" GutterBottom="true">タイトル</MudText>
<MudTextField Class="mb-3" />  <!-- mb-3 = margin-bottom: 12px -->
<MudButton Class="mt-4">送信</MudButton>  <!-- mt-4 = margin-top: 16px -->
```

### ❌ DON'T（非推奨）

#### 英語と日本語を混在させる
```razor
<!-- 非推奨: 混在 -->
<PageTitle>Employee List - 従業員管理システム</PageTitle>
<MudButton>Save</MudButton>
<MudAlert>データを読み込み中...</MudAlert>
```

#### 不統一な日付フォーマット
```razor
<!-- 非推奨: バラバラのフォーマット -->
<MudTd>@item.CreatedAt.ToString("yyyy-MM-dd")</MudTd>
<MudTd>@item.UpdatedAt.ToString("MM/dd/yyyy")</MudTd>
<MudField>@item.HireDate.ToShortDateString()</MudField>
```

#### インラインスタイルを乱用
```razor
<!-- 非推奨: インラインスタイル -->
<MudText style="margin-bottom: 10px; color: blue;">タイトル</MudText>

<!-- 推奨: ClassとMudBlazorカラー -->
<MudText Class="mb-3" Color="Color.Primary">タイトル</MudText>
```

---

## セキュリティ

### ✅ DO（推奨）

#### 認証チェックを実装
```razor
@inject AuthStateService AuthStateService

@if (AuthStateService.IsAdmin)
{
    <MudButton OnClick="Delete">削除</MudButton>
}
```

#### 機密情報をマスク
```razor
<!-- 推奨: パスワードフィールド -->
<MudTextField @bind-Value="_model.Password" 
              Label="パスワード" 
              InputType="InputType.Password" />
```

#### バリデーションを実装
```razor
<!-- 推奨: 必須チェックとバリデーション -->
<MudForm @ref="_form" @bind-IsValid="@_isValid">
    <MudTextField @bind-Value="_model.Email" 
                  Label="メールアドレス" 
                  Required="true"
                  RequiredError="メールアドレスを入力してください。"
                  InputType="InputType.Email" />
</MudForm>
<MudButton OnClick="Submit" Disabled="!_isValid">送信</MudButton>
```

#### 削除操作には確認ダイアログを表示
```razor
@code {
    private async Task OpenDeleteDialog(ItemDto item)
    {
        var parameters = new DialogParameters
        {
            { "ContentText", $"項目「{item.Name}」を削除してもよろしいですか？" },
            { "ButtonText", "削除" },
            { "Color", Color.Error }
        };

        var dialog = await DialogService.ShowAsync<MudMessageBox>("確認", parameters);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await DeleteItem(item.Id);
        }
    }
}
```

### ❌ DON'T（非推奨）

#### 認証チェックを省略
```razor
<!-- 非推奨: 認証チェックなし -->
<MudButton OnClick="Delete">削除</MudButton>
<!-- 誰でも削除できてしまう -->
```

#### プレーンテキストでパスワード表示
```razor
<!-- 非推奨: パスワードが見える -->
<MudTextField @bind-Value="_model.Password" Label="パスワード" />

<!-- 推奨: マスク -->
<MudTextField @bind-Value="_model.Password" 
              Label="パスワード" 
              InputType="InputType.Password" />
```

#### バリデーションなしでデータ送信
```razor
<!-- 非推奨: バリデーションなし -->
<MudTextField @bind-Value="_model.Email" />
<MudButton OnClick="Submit">送信</MudButton>

<!-- 推奨: バリデーション付き -->
<MudForm @ref="_form" @bind-IsValid="@_isValid">
    <MudTextField @bind-Value="_model.Email" 
                  InputType="InputType.Email" 
                  Required="true" />
</MudForm>
<MudButton OnClick="Submit" Disabled="!_isValid">送信</MudButton>
```

#### 確認なしで削除
```razor
<!-- 非推奨: 確認なしで即削除 -->
<MudButton OnClick="@(() => DeleteItem(item.Id))">削除</MudButton>

<!-- 推奨: 確認ダイアログ -->
<MudButton OnClick="@(() => OpenDeleteDialog(item))">削除</MudButton>
```

---

## まとめ

### クイックチェックリスト

新しいUI画面を実装する際は、以下をチェック：

- [ ] MudBlazorコンポーネントを使用している
- [ ] ローディング・エラー・空状態をすべて実装している
- [ ] エラーハンドリング（try-catch）を実装している
- [ ] 適切なバリデーションを実装している
- [ ] 日本語で一貫したUI文言を使用している
- [ ] レスポンシブデザイン（MudGrid）を実装している
- [ ] 認証チェックを実装している（必要に応じて）
- [ ] 削除操作には確認ダイアログを実装している
- [ ] Snackbarで操作結果を通知している
- [ ] 既存の類似画面のパターンを参考にしている

### 参考リソース

- [page-patterns.md](page-patterns.md) - 画面パターン別テンプレート
- [tokens.md](tokens.md) - デザイントークン
- [README.md](README.md) - デザインカタログ概要

---

**ベストプラクティスの提案や改善案は GitHub Issue でお知らせください！**
