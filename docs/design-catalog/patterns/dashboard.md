# ダッシュボードパターン

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

## 参考実装

- [Dashboard.razor](../../../src/WebApps/BlazorWeb/Components/Pages/Dashboard.razor)

## 関連ドキュメント

- [デザインカタログTOP](../README.md)
- [推奨・非推奨ルール](../dos-and-donts.md)
- [デザイントークン](../tokens.md)
