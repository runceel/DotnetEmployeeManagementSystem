using System.Net.Http.Json;
using Shared.Contracts.EmployeeService;

namespace BlazorWeb.Services;

/// <summary>
/// EmployeeService APIクライアントの実装
/// </summary>
public class EmployeeApiClient : IEmployeeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmployeeApiClient> _logger;
    private readonly AuthStateService _authStateService;
    private const string ApiBasePath = "/api/employees";

    public EmployeeApiClient(
        HttpClient httpClient, 
        ILogger<EmployeeApiClient> logger,
        AuthStateService authStateService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _authStateService = authStateService;
    }

    /// <summary>
    /// HTTPリクエストに認証ヘッダーを追加
    /// </summary>
    private void AddAuthHeaders(HttpRequestMessage request)
    {
        if (_authStateService.CurrentUser is not null)
        {
            request.Headers.Add("X-User-Id", _authStateService.CurrentUser.UserId);
            request.Headers.Add("X-User-Name", _authStateService.CurrentUser.UserName);
            if (_authStateService.CurrentUser.Roles.Any())
            {
                request.Headers.Add("X-User-Roles", string.Join(",", _authStateService.CurrentUser.Roles));
            }
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching employee with ID: {EmployeeId}", id);
            
            var response = await _httpClient.GetAsync(
                $"{ApiBasePath}/{id}", 
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Employee with ID {EmployeeId} not found", id);
                    return null;
                }
                
                response.EnsureSuccessStatusCode();
            }
            
            var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>(cancellationToken: cancellationToken);
            _logger.LogInformation("Successfully fetched employee with ID: {EmployeeId}", id);
            return employee;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching employee with ID: {EmployeeId}", id);
            throw new InvalidOperationException($"従業員（ID: {id}）の取得に失敗しました。", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching employee with ID: {EmployeeId}", id);
            throw new InvalidOperationException($"従業員（ID: {id}）の取得中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new employee: {FirstName} {LastName}", request.FirstName, request.LastName);
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiBasePath)
            {
                Content = JsonContent.Create(request)
            };
            AddAuthHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create employee. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("この操作を実行する権限がありません。");
                }
                
                throw new InvalidOperationException($"従業員の作成に失敗しました。（エラー: {response.StatusCode}）");
            }
            
            var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>(cancellationToken: cancellationToken);
            if (employee is null)
            {
                throw new InvalidOperationException("従業員の作成は成功しましたが、レスポンスの解析に失敗しました。");
            }
            
            _logger.LogInformation("Successfully created employee with ID: {EmployeeId}", employee.Id);
            return employee;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while creating employee");
            throw new InvalidOperationException("従業員の作成に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Unexpected error while creating employee");
            throw new InvalidOperationException("従業員の作成中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<EmployeeDto?> UpdateEmployeeAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating employee with ID: {EmployeeId}", id);
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"{ApiBasePath}/{id}")
            {
                Content = JsonContent.Create(request)
            };
            AddAuthHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Employee with ID {EmployeeId} not found for update", id);
                    return null;
                }
                
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to update employee. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("この操作を実行する権限がありません。");
                }
                
                throw new InvalidOperationException($"従業員の更新に失敗しました。（エラー: {response.StatusCode}）");
            }
            
            var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>(cancellationToken: cancellationToken);
            _logger.LogInformation("Successfully updated employee with ID: {EmployeeId}", id);
            return employee;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while updating employee with ID: {EmployeeId}", id);
            throw new InvalidOperationException($"従業員（ID: {id}）の更新に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Unexpected error while updating employee with ID: {EmployeeId}", id);
            throw new InvalidOperationException($"従業員（ID: {id}）の更新中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting employee with ID: {EmployeeId}", id);
            
            var response = await _httpClient.DeleteAsync(
                $"{ApiBasePath}/{id}", 
                cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Employee with ID {EmployeeId} not found for deletion", id);
                return false;
            }
            
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Successfully deleted employee with ID: {EmployeeId}", id);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while deleting employee with ID: {EmployeeId}", id);
            throw new InvalidOperationException($"従業員（ID: {id}）の削除に失敗しました。", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting employee with ID: {EmployeeId}", id);
            throw new InvalidOperationException($"従業員（ID: {id}）の削除中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching dashboard statistics from API");
            
            var statistics = await _httpClient.GetFromJsonAsync<DashboardStatisticsDto>(
                $"{ApiBasePath}/dashboard/statistics", 
                cancellationToken);
            
            if (statistics is null)
            {
                throw new InvalidOperationException("ダッシュボード統計情報の取得に失敗しました。");
            }

            _logger.LogInformation("Successfully fetched dashboard statistics");
            return statistics;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching dashboard statistics");
            throw new InvalidOperationException("ダッシュボード統計情報の取得に失敗しました。", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching dashboard statistics");
            throw new InvalidOperationException("ダッシュボード統計情報の取得中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching recent activities from API");
            
            var activities = await _httpClient.GetFromJsonAsync<IEnumerable<RecentActivityDto>>(
                $"{ApiBasePath}/dashboard/recent-activities?count={count}", 
                cancellationToken);
            
            _logger.LogInformation("Successfully fetched {Count} recent activities", activities?.Count() ?? 0);
            return activities ?? Enumerable.Empty<RecentActivityDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching recent activities");
            throw new InvalidOperationException("最近のアクティビティの取得に失敗しました。", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching recent activities");
            throw new InvalidOperationException("最近のアクティビティの取得中に予期しないエラーが発生しました。", ex);
        }
    }
}
