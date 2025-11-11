using System.Net.Http.Json;
using Shared.Contracts.DepartmentService;

namespace BlazorWeb.Services;

/// <summary>
/// DepartmentService APIクライアントの実装
/// </summary>
public class DepartmentApiClient : IDepartmentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DepartmentApiClient> _logger;
    private readonly AuthStateService _authStateService;
    private const string ApiBasePath = "/api/departments";

    public DepartmentApiClient(
        HttpClient httpClient, 
        ILogger<DepartmentApiClient> logger,
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
    public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching all departments from API");
            
            var departments = await _httpClient.GetFromJsonAsync<IEnumerable<DepartmentDto>>(
                ApiBasePath, 
                cancellationToken);
            
            _logger.LogInformation("Successfully fetched {Count} departments", departments?.Count() ?? 0);
            return departments ?? Enumerable.Empty<DepartmentDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching all departments");
            throw new InvalidOperationException("部署一覧の取得に失敗しました。", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all departments");
            throw new InvalidOperationException("部署一覧の取得中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching department with ID: {DepartmentId}", id);
            
            var response = await _httpClient.GetAsync(
                $"{ApiBasePath}/{id}", 
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Department with ID {DepartmentId} not found", id);
                    return null;
                }
                
                response.EnsureSuccessStatusCode();
            }
            
            var department = await response.Content.ReadFromJsonAsync<DepartmentDto>(cancellationToken: cancellationToken);
            _logger.LogInformation("Successfully fetched department with ID: {DepartmentId}", id);
            return department;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching department with ID: {DepartmentId}", id);
            throw new InvalidOperationException($"部署（ID: {id}）の取得に失敗しました。", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching department with ID: {DepartmentId}", id);
            throw new InvalidOperationException($"部署（ID: {id}）の取得中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new department: {Name}", request.Name);
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiBasePath)
            {
                Content = JsonContent.Create(request)
            };
            AddAuthHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create department. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("この操作を実行する権限がありません。");
                }
                
                throw new InvalidOperationException($"部署の作成に失敗しました。（エラー: {response.StatusCode}）");
            }
            
            var department = await response.Content.ReadFromJsonAsync<DepartmentDto>(cancellationToken: cancellationToken);
            if (department is null)
            {
                throw new InvalidOperationException("部署の作成は成功しましたが、レスポンスの解析に失敗しました。");
            }
            
            _logger.LogInformation("Successfully created department with ID: {DepartmentId}", department.Id);
            return department;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while creating department");
            throw new InvalidOperationException("部署の作成に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Unexpected error while creating department");
            throw new InvalidOperationException("部署の作成中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<DepartmentDto?> UpdateDepartmentAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating department with ID: {DepartmentId}", id);
            
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
                    _logger.LogWarning("Department with ID {DepartmentId} not found for update", id);
                    return null;
                }
                
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to update department. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("この操作を実行する権限がありません。");
                }
                
                throw new InvalidOperationException($"部署の更新に失敗しました。（エラー: {response.StatusCode}）");
            }
            
            var department = await response.Content.ReadFromJsonAsync<DepartmentDto>(cancellationToken: cancellationToken);
            _logger.LogInformation("Successfully updated department with ID: {DepartmentId}", id);
            return department;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while updating department with ID: {DepartmentId}", id);
            throw new InvalidOperationException($"部署（ID: {id}）の更新に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Unexpected error while updating department with ID: {DepartmentId}", id);
            throw new InvalidOperationException($"部署（ID: {id}）の更新中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting department with ID: {DepartmentId}", id);
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{ApiBasePath}/{id}");
            AddAuthHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Department with ID {DepartmentId} not found for deletion", id);
                return false;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Cannot delete department with ID {DepartmentId}: {Error}", id, errorContent);
                throw new InvalidOperationException("従業員が所属している部署は削除できません。");
            }
            
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Successfully deleted department with ID: {DepartmentId}", id);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while deleting department with ID: {DepartmentId}", id);
            throw new InvalidOperationException($"部署（ID: {id}）の削除に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Unexpected error while deleting department with ID: {DepartmentId}", id);
            throw new InvalidOperationException($"部署（ID: {id}）の削除中に予期しないエラーが発生しました。", ex);
        }
    }
}
