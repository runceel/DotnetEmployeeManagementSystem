# 編集画面パターン（ダイアログ）

このドキュメントは、ダイアログ形式のフォーム編集画面の完全なテンプレートを提供します。

## 特徴
- フォーム入力（MudForm + バリデーション）
- ダイアログ形式で表示
- 保存/キャンセル操作
- ドロップダウンやDatePickerなど多様な入力

## テンプレート構造

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

## 完全なコード例

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

## 主要ポイント

1. **MudForm**: `@bind-IsValid` でバリデーション状態を管理
2. **Required属性**: 必須フィールドには `Required="true"` と `RequiredError` を設定
3. **Variant.Outlined**: 統一感のためアウトライン形式を使用
4. **Class="mb-3"**: フィールド間のマージンを統一（3単位 = 約12px）
5. **モデルコピー**: 元のモデルを直接編集せず、コピーを作成して操作
6. **DialogResult**: 保存時は `DialogResult.Ok(model)` でモデルを返す

## 参考実装

- [EmployeeFormDialog.razor](../../../src/WebApps/BlazorWeb/Components/Dialogs/EmployeeFormDialog.razor)
- [DepartmentFormDialog.razor](../../../src/WebApps/BlazorWeb/Components/Dialogs/DepartmentFormDialog.razor)

## 関連ドキュメント

- [デザインカタログTOP](../README.md)
- [推奨・非推奨ルール](../dos-and-donts.md)
- [デザイントークン](../tokens.md)
