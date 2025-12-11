# 一覧画面パターン

このドキュメントは、データ一覧表示画面の完全なテンプレートを提供します。

## 特徴
- データの一覧表示（MudTable使用）
- ローディング・エラー・空状態の処理
- CRUD操作（作成・編集・削除）
- 認証チェック（管理者権限など）

## テンプレート構造

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

## 完全なコード例

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

## 主要ポイント

1. **状態管理**: `_loading`, `_error`, `_errorMessage` で画面状態を管理
2. **条件分岐**: ローディング → エラー → 空 → データ表示の順で確認
3. **認証チェック**: `AuthStateService.IsAdmin` で権限に応じた表示切り替え
4. **エラーハンドリング**: try-catch で例外を捕捉し、Snackbarで通知
5. **再読み込み**: CRUD操作後に `LoadItems()` を呼び出してデータを最新化

## 参考実装

- [Employees.razor](../../../src/WebApps/BlazorWeb/Components/Pages/Employees.razor)
- [Departments.razor](../../../src/WebApps/BlazorWeb/Components/Pages/Departments.razor)

## 関連ドキュメント

- [デザインカタログTOP](../README.md)
- [推奨・非推奨ルール](../dos-and-donts.md)
- [デザイントークン](../tokens.md)
