# 詳細画面パターン

このドキュメントは、単一データ詳細表示画面の完全なテンプレートを提供します。

## 特徴
- 単一エンティティの詳細表示
- MudCard + MudGrid レイアウト
- Breadcrumbs でナビゲーション
- 読み取り専用表示

## テンプレート構造

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

## 完全なコード例

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

## 主要ポイント

1. **ルートパラメータ**: `{id:guid}` でGUID型のIDを受け取る
2. **Breadcrumbs**: ユーザーの現在位置を明示
3. **MudField**: 読み取り専用フィールドで情報を表示
4. **レスポンシブグリッド**: `xs="12" sm="6"` でモバイル/タブレットで適切に表示
5. **日時フォーマット**: `yyyy/MM/dd HH:mm:ss` 形式で統一

## 参考実装

- [EmployeeDetail.razor](../../../src/WebApps/BlazorWeb/Components/Pages/EmployeeDetail.razor)

## 関連ドキュメント

- [デザインカタログTOP](../README.md)
- [推奨・非推奨ルール](../dos-and-donts.md)
- [デザイントークン](../tokens.md)
