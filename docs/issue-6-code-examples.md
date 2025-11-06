# Issue #6 コード例集

## 1. APIクライアントインターフェース

```csharp
// src/WebApps/BlazorWeb/Services/IEmployeeApiClient.cs

using Shared.Contracts.EmployeeService;

namespace BlazorWeb.Services;

/// <summary>
/// EmployeeService APIクライアントのインターフェース
/// </summary>
public interface IEmployeeApiClient
{
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> UpdateEmployeeAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default);
}
```

## 2. エラーハンドリング実装例

```csharp
// src/WebApps/BlazorWeb/Services/EmployeeApiClient.cs (抜粋)

public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
{
    try
    {
        _logger.LogInformation("Fetching all employees from API");
        
        var employees = await _httpClient.GetFromJsonAsync<IEnumerable<EmployeeDto>>(
            ApiBasePath, 
            cancellationToken);
        
        _logger.LogInformation("Successfully fetched {Count} employees", employees?.Count() ?? 0);
        return employees ?? Enumerable.Empty<EmployeeDto>();
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "HTTP request failed while fetching all employees");
        throw new InvalidOperationException("従業員一覧の取得に失敗しました。", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error while fetching all employees");
        throw new InvalidOperationException("従業員一覧の取得中に予期しないエラーが発生しました。", ex);
    }
}
```

## 3. サービス登録

```csharp
// src/WebApps/BlazorWeb/Program.cs (抜粋)

using BlazorWeb.Services;

// Add HttpClient for EmployeeService with Aspire service discovery
builder.Services.AddHttpClient<IEmployeeApiClient, EmployeeApiClient>("employeeservice-api", client =>
{
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("http://employeeservice-api");
});
```

ServiceDefaultsにより、以下が自動適用されます：

```csharp
// src/ServiceDefaults/Extensions.cs (既存)

builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Turn on resilience by default
    http.AddStandardResilienceHandler();
    
    // Turn on service discovery by default
    http.AddServiceDiscovery();
});
```

## 4. Blazorコンポーネントでの使用例

```razor
@* src/WebApps/BlazorWeb/Components/Pages/Employees.razor (抜粋) *@

@page "/employees"
@using Shared.Contracts.EmployeeService
@using BlazorWeb.Services
@inject IEmployeeApiClient EmployeeApiClient
@inject ISnackbar Snackbar

<PageTitle>従業員一覧 - 従業員管理システム</PageTitle>

@if (_loading)
{
    <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
    <MudText Typo="Typo.body1" Class="mt-2">従業員データを読み込み中...</MudText>
}
else if (_error)
{
    <MudAlert Severity="Severity.Error" Class="my-4">
        @_errorMessage
    </MudAlert>
    <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="LoadEmployees">
        再試行
    </MudButton>
}
else
{
    <MudTable Items="_employees" Hover="true" Breakpoint="Breakpoint.Sm">
        @* テーブル内容 *@
    </MudTable>
}

@code {
    private IEnumerable<EmployeeDto>? _employees;
    private bool _loading = true;
    private bool _error = false;
    private string _errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadEmployees();
    }

    private async Task LoadEmployees()
    {
        _loading = true;
        _error = false;

        try
        {
            _employees = await EmployeeApiClient.GetAllEmployeesAsync();
            Snackbar.Add("従業員データを正常に読み込みました。", Severity.Success);
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
}
```

## 5. リトライポリシーのカスタマイズ例

デフォルトのポリシーで十分な場合が多いですが、必要に応じてカスタマイズできます：

```csharp
builder.Services.AddHttpClient<IEmployeeApiClient, EmployeeApiClient>("employeeservice-api", client =>
{
    client.BaseAddress = new Uri("http://employeeservice-api");
})
.AddStandardResilienceHandler(options =>
{
    // リトライ設定
    options.Retry.MaxRetryAttempts = 5;
    options.Retry.Delay = TimeSpan.FromSeconds(1);
    options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
    
    // サーキットブレーカー設定
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
    options.CircuitBreaker.MinimumThroughput = 20;
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
    
    // タイムアウト設定
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
});
```

## 6. 共有型（Contracts）の使用

```csharp
// src/Shared/Contracts/EmployeeService/EmployeeDto.cs (既存)

namespace Shared.Contracts.EmployeeService;

public record EmployeeDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime HireDate { get; init; }
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
```

BlazorWebとEmployeeService.APIの両方で同じ型を使用することで、型安全性を確保しています。

## 7. ナビゲーション統合

```razor
@* src/WebApps/BlazorWeb/Components/Layout/NavMenu.razor (抜粋) *@

<MudNavMenu>
    <MudNavLink Href="" Match="NavLinkMatch.All" Icon="@Icons.Material.Filled.Home">
        ホーム
    </MudNavLink>
    <MudNavLink Href="dashboard" Icon="@Icons.Material.Filled.Dashboard">
        ダッシュボード
    </MudNavLink>
    <MudNavLink Href="employees" Icon="@Icons.Material.Filled.People">
        従業員管理
    </MudNavLink>
</MudNavMenu>
```

## 8. ログ出力例

実装されたログ記録により、以下のような出力が得られます：

```
info: BlazorWeb.Services.EmployeeApiClient[0]
      Fetching all employees from API
info: BlazorWeb.Services.EmployeeApiClient[0]
      Successfully fetched 5 employees

error: BlazorWeb.Services.EmployeeApiClient[0]
      HTTP request failed while fetching employee with ID: 12345678-1234-1234-1234-123456789abc
      System.Net.Http.HttpRequestException: Response status code does not indicate success: 404 (Not Found).
```

## まとめ

これらのコード例は、以下のベストプラクティスを示しています：

- ✅ 依存性注入を使用したクリーンな設計
- ✅ インターフェースによる疎結合
- ✅ 包括的なエラーハンドリング
- ✅ 構造化ロギング
- ✅ ユーザーフレンドリーなUI
- ✅ 型安全な契約共有
- ✅ レジリエンスパターンの適用
