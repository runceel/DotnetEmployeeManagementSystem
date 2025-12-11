# 画面パターン別テンプレート

このドキュメントは、よくある画面タイプ（一覧、詳細、編集、ダッシュボード）のテンプレートとコード例を提供します。

## 📑 目次

- [一覧画面パターン](#一覧画面パターン)
- [詳細画面パターン](#詳細画面パターン)
- [編集画面パターン（ダイアログ）](#編集画面パターンダイアログ)
- [ダッシュボードパターン](#ダッシュボードパターン)

---

## 一覧画面パターン

### 特徴
- データの一覧表示（MudTable使用）
- ローディング・エラー・空状態の処理
- CRUD操作（作成・編集・削除）
- 認証チェック（管理者権限など）

### テンプレート構造

```
[ページディレクティブ]
[using ステートメント]
[依存性注入]

[PageTitle]
[ヘッダー（タイトル + アクションボタン）]
[認証状態表示]
[ローディング状態]
[エラー状態]
[空状態]
[データテーブル]
[合計件数表示]

@code {
    [状態管理変数]
    [OnInitializedAsync]
    [データ読み込みメソッド]
    [CRUD操作メソッド]
}
```

### 完全なコード例

```razor
@page "/items"
@using Shared.Contracts.ItemService
@using BlazorWeb.Services
@using BlazorWeb.Models
@using BlazorWeb.Components.Dialogs
@inject IItemApiClient ItemApiClient
@inject AuthStateService AuthStateService
@inject ISnackbar Snackbar
@inject NavigationManager Navigation
@inject IDialogService DialogService

<PageTitle>項目一覧 - 従業員管理システム</PageTitle>

<div class="d-flex justify-space-between align-center mb-4">
    <MudText Typo="Typo.h3">項目一覧</MudText>
    @if (AuthStateService.IsAdmin)
    {
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary" 
                   StartIcon="@Icons.Material.Filled.Add"
                   OnClick="OpenCreateDialog">
            項目を追加
        </MudButton>
    }
</div>

@if (AuthStateService.IsAuthenticated)
{
    <MudChip T="string" Icon="@Icons.Material.Filled.CheckCircle" Color="Color.Success" Size="Size.Small" Class="mb-4">
        ログイン中: @AuthStateService.CurrentUser?.UserName
    </MudChip>
}
else
{
    <MudAlert Severity="Severity.Warning" Class="mb-4">
        この機能を利用するには<MudLink Href="/login">ログイン</MudLink>することをお勧めします。
    </MudAlert>
}

@if (_loading)
{
    <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
    <MudText Typo="Typo.body1" Class="mt-2">データを読み込み中...</MudText>
}
else if (_error)
{
    <MudAlert Severity="Severity.Error" Class="my-4">
        @_errorMessage
    </MudAlert>
    <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="LoadItems">
        再試行
    </MudButton>
}
else if (_items is null || !_items.Any())
{
    <MudAlert Severity="Severity.Info" Class="my-4">
        データが登録されていません。
    </MudAlert>
}
else
{
    <MudTable Items="_items" Hover="true" Breakpoint="Breakpoint.Sm" Dense="true" Class="mt-4">
        <HeaderContent>
            <MudTh>ID</MudTh>
            <MudTh>名前</MudTh>
            <MudTh>作成日時</MudTh>
            @if (AuthStateService.IsAdmin)
            {
                <MudTh>アクション</MudTh>
            }
            else
            {
                <MudTh>詳細</MudTh>
            }
        </HeaderContent>
        <RowTemplate>
            <MudTd DataLabel="ID">@context.Id.ToString("N").Substring(0, 8)...</MudTd>
            <MudTd DataLabel="名前">@context.Name</MudTd>
            <MudTd DataLabel="作成日時">@context.CreatedAt.ToString("yyyy/MM/dd")</MudTd>
            <MudTd DataLabel="アクション">
                @if (AuthStateService.IsAdmin)
                {
                    <MudButtonGroup Variant="Variant.Text" Size="Size.Small">
                        <MudButton Color="Color.Primary" 
                                   StartIcon="@Icons.Material.Filled.Edit"
                                   OnClick="@(() => OpenEditDialog(context))">
                            編集
                        </MudButton>
                        <MudButton Color="Color.Info"
                                   StartIcon="@Icons.Material.Filled.Visibility"
                                   OnClick="@(() => NavigateToDetail(context.Id))">
                            詳細
                        </MudButton>
                        <MudButton Color="Color.Error"
                                   StartIcon="@Icons.Material.Filled.Delete"
                                   OnClick="@(() => OpenDeleteDialog(context))">
                            削除
                        </MudButton>
                    </MudButtonGroup>
                }
                else
                {
                    <MudButton 
                        Variant="Variant.Text" 
                        Color="Color.Primary" 
                        Size="Size.Small"
                        StartIcon="@Icons.Material.Filled.Visibility"
                        OnClick="@(() => NavigateToDetail(context.Id))">
                        詳細
                    </MudButton>
                }
            </MudTd>
        </RowTemplate>
    </MudTable>
    
    <MudText Typo="Typo.body2" Class="mt-4">
        合計: @_items.Count() 件
    </MudText>
}

@code {
    private IEnumerable<ItemDto>? _items;
    private bool _loading = true;
    private bool _error = false;
    private string _errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadItems();
    }

    private async Task LoadItems()
    {
        _loading = true;
        _error = false;
        _errorMessage = string.Empty;

        try
        {
            _items = await ItemApiClient.GetAllItemsAsync();
            Snackbar.Add("データを正常に読み込みました。", Severity.Success);
        }
        catch (Exception ex)
        {
            _error = true;
            _errorMessage = ex.Message;
            Snackbar.Add($"エラー: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private void NavigateToDetail(Guid id)
    {
        Navigation.NavigateTo($"/items/{id}");
    }

    private async Task OpenCreateDialog()
    {
        var parameters = new DialogParameters<ItemFormDialog>
        {
            { x => x.Model, new ItemFormModel() }
        };

        var options = new DialogOptions 
        { 
            CloseButton = true, 
            MaxWidth = MaxWidth.Small, 
            FullWidth = true 
        };

        var dialog = await DialogService.ShowAsync<ItemFormDialog>("項目を追加", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is ItemFormModel model)
        {
            await CreateItem(model);
        }
    }

    private async Task OpenEditDialog(ItemDto item)
    {
        var model = new ItemFormModel
        {
            Name = item.Name,
            // その他のプロパティ
        };

        var parameters = new DialogParameters<ItemFormDialog>
        {
            { x => x.Model, model }
        };

        var options = new DialogOptions 
        { 
            CloseButton = true, 
            MaxWidth = MaxWidth.Small, 
            FullWidth = true 
        };

        var dialog = await DialogService.ShowAsync<ItemFormDialog>("項目を編集", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is ItemFormModel updatedModel)
        {
            await UpdateItem(item.Id, updatedModel);
        }
    }

    private async Task OpenDeleteDialog(ItemDto item)
    {
        var parameters = new DialogParameters
        {
            { "ContentText", $"項目「{item.Name}」を削除してもよろしいですか？" },
            { "ButtonText", "削除" },
            { "Color", Color.Error }
        };

        var options = new DialogOptions 
        { 
            CloseButton = true, 
            MaxWidth = MaxWidth.ExtraSmall 
        };

        var dialog = await DialogService.ShowAsync<MudMessageBox>("項目の削除", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await DeleteItem(item.Id);
        }
    }

    private async Task CreateItem(ItemFormModel model)
    {
        try
        {
            var request = new CreateItemRequest
            {
                Name = model.Name,
                // その他のプロパティ
            };

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

    private async Task UpdateItem(Guid id, ItemFormModel model)
    {
        try
        {
            var request = new UpdateItemRequest
            {
                Name = model.Name,
                // その他のプロパティ
            };

            var result = await ItemApiClient.UpdateItemAsync(id, request);
            if (result is not null)
            {
                Snackbar.Add("項目情報を更新しました。", Severity.Success);
                await LoadItems();
            }
            else
            {
                Snackbar.Add("項目が見つかりませんでした。", Severity.Warning);
            }
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

    private async Task DeleteItem(Guid id)
    {
        try
        {
            var result = await ItemApiClient.DeleteItemAsync(id);
            if (result)
            {
                Snackbar.Add("項目を削除しました。", Severity.Success);
                await LoadItems();
            }
            else
            {
                Snackbar.Add("項目が見つかりませんでした。", Severity.Warning);
            }
        }
        catch (InvalidOperationException ex)
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

### 主要ポイント

1. **状態管理**: `_loading`, `_error`, `_errorMessage` で画面状態を管理
2. **条件分岐**: ローディング → エラー → 空 → データ表示の順で確認
3. **認証チェック**: `AuthStateService.IsAdmin` で権限に応じた表示切り替え
4. **エラーハンドリング**: try-catch で例外を捕捉し、Snackbarで通知
5. **再読み込み**: CRUD操作後に `LoadItems()` を呼び出してデータを最新化

---

## 詳細画面パターン

### 特徴
- 単一エンティティの詳細表示
- MudCard + MudGrid レイアウト
- Breadcrumbs でナビゲーション
- 読み取り専用表示

### テンプレート構造

```
[ページディレクティブ（パラメータ付き）]
[using ステートメント]
[依存性注入]

[PageTitle]
[Breadcrumbs]
[ローディング状態]
[エラー状態]
[データなし状態]
[データ表示（MudCard + MudGrid）]

@code {
    [Parameter] Guid Id
    [状態管理変数]
    [Breadcrumb定義]
    [OnInitializedAsync]
    [データ読み込みメソッド]
    [ナビゲーションメソッド]
}
```

### 完全なコード例

```razor
@page "/items/{id:guid}"
@using Shared.Contracts.ItemService
@using BlazorWeb.Services
@inject IItemApiClient ItemApiClient
@inject ISnackbar Snackbar
@inject NavigationManager Navigation

<PageTitle>項目詳細 - 従業員管理システム</PageTitle>

<MudBreadcrumbs Items="_breadcrumbItems" Class="mb-4"></MudBreadcrumbs>

@if (_loading)
{
    <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
    <MudText Typo="Typo.body1" Class="mt-2">データを読み込み中...</MudText>
}
else if (_error)
{
    <MudAlert Severity="Severity.Error" Class="my-4">
        @_errorMessage
    </MudAlert>
    <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="LoadItem">
        再試行
    </MudButton>
    <MudButton Variant="Variant.Text" Color="Color.Default" OnClick="NavigateToList" Class="ml-2">
        一覧に戻る
    </MudButton>
}
else if (_item is null)
{
    <MudAlert Severity="Severity.Warning" Class="my-4">
        項目が見つかりませんでした。
    </MudAlert>
    <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="NavigateToList">
        一覧に戻る
    </MudButton>
}
else
{
    <MudText Typo="Typo.h3" GutterBottom="true">項目詳細</MudText>
    
    <MudCard Class="mt-4">
        <MudCardContent>
            <MudGrid>
                <MudItem xs="12" sm="6">
                    <MudField Label="項目ID" Variant="Variant.Text">
                        @_item.Id
                    </MudField>
                </MudItem>
                <MudItem xs="12" sm="6">
                    <MudField Label="名前" Variant="Variant.Text">
                        @_item.Name
                    </MudField>
                </MudItem>
                <MudItem xs="12" sm="6">
                    <MudField Label="作成日時" Variant="Variant.Text">
                        @_item.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss")
                    </MudField>
                </MudItem>
                <MudItem xs="12" sm="6">
                    <MudField Label="更新日時" Variant="Variant.Text">
                        @_item.UpdatedAt.ToString("yyyy/MM/dd HH:mm:ss")
                    </MudField>
                </MudItem>
            </MudGrid>
        </MudCardContent>
        <MudCardActions>
            <MudButton 
                Variant="Variant.Filled" 
                Color="Color.Primary" 
                OnClick="NavigateToList">
                一覧に戻る
            </MudButton>
        </MudCardActions>
    </MudCard>
}

@code {
    [Parameter]
    public Guid Id { get; set; }

    private ItemDto? _item;
    private bool _loading = true;
    private bool _error = false;
    private string _errorMessage = string.Empty;
    
    private List<BreadcrumbItem> _breadcrumbItems = new()
    {
        new BreadcrumbItem("ホーム", href: "/"),
        new BreadcrumbItem("項目一覧", href: "/items"),
        new BreadcrumbItem("詳細", href: null, disabled: true)
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadItem();
    }

    private async Task LoadItem()
    {
        _loading = true;
        _error = false;
        _errorMessage = string.Empty;

        try
        {
            _item = await ItemApiClient.GetItemByIdAsync(Id);
            
            if (_item is not null)
            {
                Snackbar.Add("データを正常に読み込みました。", Severity.Success);
            }
            else
            {
                Snackbar.Add("項目が見つかりませんでした。", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            _error = true;
            _errorMessage = ex.Message;
            Snackbar.Add($"エラー: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private void NavigateToList()
    {
        Navigation.NavigateTo("/items");
    }
}
```

### 主要ポイント

1. **ルートパラメータ**: `{id:guid}` でGUID型のIDを受け取る
2. **Breadcrumbs**: ユーザーの現在位置を明示
3. **MudField**: 読み取り専用フィールドで情報を表示
4. **レスポンシブグリッド**: `xs="12" sm="6"` でモバイル/タブレットで適切に表示
5. **日時フォーマット**: `yyyy/MM/dd HH:mm:ss` 形式で統一

---

## 編集画面パターン（ダイアログ）

### 特徴
- フォーム入力（MudForm + バリデーション）
- ダイアログ形式で表示
- 保存/キャンセル操作
- ドロップダウンやDatePickerなど多様な入力

### テンプレート構造

```
[using ステートメント]
[依存性注入]

<MudDialog>
    <TitleContent>
    <DialogContent>
        <MudForm>
            [入力フィールド]
        </MudForm>
    </DialogContent>
    <DialogActions>

@code {
    [CascadingParameter] IMudDialogInstance
    [Parameter] Model
    [フォーム状態]
    [OnInitializedAsync]
    [Submit/Cancelメソッド]
}
```

### 完全なコード例

```razor
@using Shared.Contracts.ItemService
@using BlazorWeb.Services
@inject ISnackbar Snackbar

<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">@Title</MudText>
    </TitleContent>
    <DialogContent>
        <MudForm @ref="_form" @bind-IsValid="@_isValid">
            <MudTextField @bind-Value="_model.Name" 
                          Label="名前" 
                          Required="true" 
                          RequiredError="名前を入力してください。"
                          Variant="Variant.Outlined"
                          Class="mb-3" />
            
            <MudTextField @bind-Value="_model.Description" 
                          Label="説明" 
                          Lines="3"
                          Variant="Variant.Outlined"
                          Class="mb-3" />
            
            <MudSelect @bind-Value="_model.Category" 
                       Label="カテゴリ" 
                       Required="true" 
                       RequiredError="カテゴリを選択してください。"
                       Variant="Variant.Outlined"
                       Class="mb-3">
                <MudSelectItem Value="@("カテゴリA")">カテゴリA</MudSelectItem>
                <MudSelectItem Value="@("カテゴリB")">カテゴリB</MudSelectItem>
                <MudSelectItem Value="@("カテゴリC")">カテゴリC</MudSelectItem>
            </MudSelect>
            
            <MudDatePicker @bind-Date="_date" 
                           Label="日付" 
                           Required="true" 
                           RequiredError="日付を入力してください。"
                           Variant="Variant.Outlined"
                           Class="mb-3" />
            
            <MudSwitch @bind-Value="_model.IsActive" 
                       Label="有効" 
                       Color="Color.Primary" />
        </MudForm>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="OnCancel">キャンセル</MudButton>
        <MudButton Color="Color.Primary" OnClick="OnSubmit" Disabled="!_isValid">保存</MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter]
    IMudDialogInstance? MudDialog { get; set; }

    [Parameter]
    public string Title { get; set; } = "項目情報";

    [Parameter]
    public ItemFormModel Model { get; set; } = new();

    private MudForm _form = null!;
    private bool _isValid;
    private ItemFormModel _model = new();
    private DateTime? _date;

    protected override Task OnInitializedAsync()
    {
        // モデルのコピーを作成（元のモデルを直接編集しない）
        _model = new ItemFormModel
        {
            Name = Model.Name,
            Description = Model.Description,
            Category = Model.Category,
            Date = Model.Date,
            IsActive = Model.IsActive
        };
        _date = _model.Date;

        return Task.CompletedTask;
    }

    private void OnSubmit()
    {
        // DatePickerの値をモデルに反映
        if (_date.HasValue)
        {
            _model.Date = _date.Value;
        }
        
        MudDialog?.Close(DialogResult.Ok(_model));
    }

    private void OnCancel()
    {
        MudDialog?.Cancel();
    }
}
```

### 主要ポイント

1. **MudForm**: `@bind-IsValid` でバリデーション状態を管理
2. **Required属性**: 必須フィールドには `Required="true"` と `RequiredError` を設定
3. **Variant.Outlined**: 統一感のためアウトライン形式を使用
4. **Class="mb-3"**: フィールド間のマージンを統一（3単位 = 約12px）
5. **モデルコピー**: 元のモデルを直接編集せず、コピーを作成して操作
6. **DialogResult**: 保存時は `DialogResult.Ok(model)` でモデルを返す

---

## ダッシュボードパターン

### 特徴
- 統計情報カード
- スケルトンローディング
- グリッドレイアウト
- タイムライン/アクティビティ表示

### テンプレート構造

```
[ページディレクティブ]
[using ステートメント]
[依存性注入]

[PageTitle]
[タイトル]
[ローディング状態（スケルトン）]
[エラー状態]
[統計カードグリッド]
[詳細情報グリッド（アクティビティ等）]

@code {
    [状態管理変数]
    [OnInitializedAsync]
    [データ取得メソッド]
    [ヘルパーメソッド]
}
```

### 完全なコード例

```razor
@page "/dashboard"
@inject IHttpClientFactory HttpClientFactory
@using Shared.Contracts.ItemService
@inject BlazorWeb.Services.IItemApiClient ItemApiClient

<PageTitle>ダッシュボード - 従業員管理システム</PageTitle>

<MudText Typo="Typo.h3" GutterBottom="true">ダッシュボード</MudText>
<MudText Typo="Typo.body1" Class="mb-4">システムの概要と統計情報</MudText>

@if (_isLoading)
{
    <MudGrid>
        @for (int i = 0; i < 4; i++)
        {
            <MudItem xs="12" sm="6" md="3">
                <MudCard Elevation="2">
                    <MudCardContent>
                        <MudSkeleton SkeletonType="SkeletonType.Text" Width="60%" />
                        <MudSkeleton SkeletonType="SkeletonType.Text" Width="40%" Height="3rem" />
                        <MudSkeleton SkeletonType="SkeletonType.Text" Width="80%" />
                    </MudCardContent>
                </MudCard>
            </MudItem>
        }
    </MudGrid>
    return;
}

@if (_hasError)
{
    <MudAlert Severity="Severity.Error" Class="mb-4">
        データの取得に失敗しました。@_errorMessage
    </MudAlert>
}

<MudGrid>
    <MudItem xs="12" sm="6" md="3">
        <MudCard Elevation="2">
            <MudCardContent>
                <MudText Typo="Typo.h6">総項目数</MudText>
                <MudText Typo="Typo.h3" Color="Color.Primary">@_totalItems</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary">登録済み項目</MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>
    
    <MudItem xs="12" sm="6" md="3">
        <MudCard Elevation="2">
            <MudCardContent>
                <MudText Typo="Typo.h6">アクティブ項目</MudText>
                <MudText Typo="Typo.h3" Color="Color.Success">@_activeItems</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary">有効な項目</MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>
    
    <MudItem xs="12" sm="6" md="3">
        <MudCard Elevation="2">
            <MudCardContent>
                <MudText Typo="Typo.h6">カテゴリ数</MudText>
                <MudText Typo="Typo.h3" Color="Color.Info">@_categories</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary">登録カテゴリ</MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>
    
    <MudItem xs="12" sm="6" md="3">
        <MudCard Elevation="2">
            <MudCardContent>
                <MudText Typo="Typo.h6">今月の新規登録</MudText>
                <MudText Typo="Typo.h3" Color="Color.Warning">@_newThisMonth</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary">新規追加</MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>
</MudGrid>

<MudGrid Class="mt-4">
    <MudItem xs="12" md="8">
        <MudCard>
            <MudCardHeader>
                <CardHeaderContent>
                    <MudText Typo="Typo.h6">最近の活動</MudText>
                </CardHeaderContent>
            </MudCardHeader>
            <MudCardContent>
                @if (_recentActivities.Any())
                {
                    <MudTimeline TimelineOrientation="TimelineOrientation.Vertical" TimelinePosition="TimelinePosition.Start">
                        @foreach (var activity in _recentActivities)
                        {
                            var color = activity.Type == "Created" ? Color.Success : Color.Info;
                            var title = activity.Type == "Created" ? "新規登録" : "情報更新";
                            
                            <MudTimelineItem Color="@color" Elevation="2">
                                <ItemContent>
                                    <MudText Typo="Typo.body1"><strong>@title</strong></MudText>
                                    <MudText Typo="Typo.body2" Color="Color.Secondary">@activity.Description</MudText>
                                    <MudText Typo="Typo.caption" Color="Color.Secondary">@GetRelativeTime(activity.Timestamp)</MudText>
                                </ItemContent>
                            </MudTimelineItem>
                        }
                    </MudTimeline>
                }
                else
                {
                    <MudText Typo="Typo.body2" Color="Color.Secondary" Class="pa-4">最近のアクティビティはありません</MudText>
                }
            </MudCardContent>
        </MudCard>
    </MudItem>
    
    <MudItem xs="12" md="4">
        <MudCard>
            <MudCardHeader>
                <CardHeaderContent>
                    <MudText Typo="Typo.h6">クイックアクション</MudText>
                </CardHeaderContent>
            </MudCardHeader>
            <MudCardContent>
                <MudStack Spacing="2">
                    <MudButton Variant="Variant.Filled" 
                               Color="Color.Primary" 
                               StartIcon="@Icons.Material.Filled.Add" 
                               FullWidth="true"
                               Href="/items">
                        新規項目登録
                    </MudButton>
                    <MudButton Variant="Variant.Filled" 
                               Color="Color.Secondary" 
                               StartIcon="@Icons.Material.Filled.Search" 
                               FullWidth="true"
                               Href="/items">
                        項目検索
                    </MudButton>
                </MudStack>
            </MudCardContent>
        </MudCard>
    </MudItem>
</MudGrid>

@code {
    private bool _isLoading = true;
    private bool _hasError = false;
    private string _errorMessage = string.Empty;
    
    private int _totalItems = 0;
    private int _activeItems = 0;
    private int _categories = 0;
    private int _newThisMonth = 0;
    private List<ActivityDto> _recentActivities = new();

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _hasError = false;

        try
        {
            // ダッシュボード統計情報を取得
            var statistics = await ItemApiClient.GetDashboardStatisticsAsync();
            _totalItems = statistics.TotalItems;
            _activeItems = statistics.ActiveItems;
            _categories = statistics.CategoryCount;
            _newThisMonth = statistics.NewItemsThisMonth;

            // 最近のアクティビティを取得
            var activities = await ItemApiClient.GetRecentActivitiesAsync(5);
            _recentActivities = activities.ToList();
        }
        catch (Exception ex)
        {
            _hasError = true;
            _errorMessage = ex.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }

    // タイムスタンプ（UTC）から相対時間表示を生成
    // 例: "5分前", "2時間前", "3日前"
    private string GetRelativeTime(DateTime timestamp)
    {
        var now = DateTime.UtcNow;
        var diff = now - timestamp;

        if (diff.TotalMinutes < 1)
            return "たった今";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes}分前";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours}時間前";
        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays}日前";
        if (diff.TotalDays < 30)
            return $"{(int)(diff.TotalDays / 7)}週間前";
        if (diff.TotalDays < 365)
            return $"{(int)(diff.TotalDays / 30)}ヶ月前";
        return $"{(int)(diff.TotalDays / 365)}年前";
    }
}
```

### 主要ポイント

1. **MudSkeleton**: ローディング中は実際のカードと同じレイアウトでスケルトンを表示
2. **統計カード**: 4列グリッド（`md="3"`）で均等に配置
3. **カラー使用**: Primary/Success/Info/Warning で視覚的に区別
4. **MudTimeline**: アクティビティを時系列で表示
5. **相対時間**: `GetRelativeTime` で「〇分前」形式の表示
6. **レスポンシブ**: `xs="12" md="8"` でモバイル/デスクトップで適切に表示

---

## まとめ

これらのパターンを組み合わせることで、ほとんどのUI画面を実装できます。

### パターン選択のフローチャート

```
新しい画面を作成
    ↓
データを一覧表示？
    → はい → 一覧画面パターン
    → いいえ
        ↓
    単一データの詳細表示？
        → はい → 詳細画面パターン
        → いいえ
            ↓
        データ入力/編集？
            → はい → 編集画面パターン（ダイアログ）
            → いいえ
                ↓
            統計・概要表示？
                → はい → ダッシュボードパターン
```

### 次のステップ

- [dos-and-donts.md](dos-and-donts.md) で推奨・非推奨ルールを確認
- [tokens.md](tokens.md) でデザイントークンを参照
- 既存の実装（`src/WebApps/BlazorWeb/Components/Pages/`）も参考に

---

**パターンの改善提案やフィードバックは GitHub Issue でお知らせください！**
