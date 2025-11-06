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
    private const string ApiBasePath = "/api/employees";

    public EmployeeApiClient(HttpClient httpClient, ILogger<EmployeeApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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
            
            var response = await _httpClient.PostAsJsonAsync(
                ApiBasePath, 
                request, 
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create employee. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
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
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
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
            
            var response = await _httpClient.PutAsJsonAsync(
                $"{ApiBasePath}/{id}", 
                request, 
                cancellationToken);
            
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
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
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
}
