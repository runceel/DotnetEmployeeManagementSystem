using System.Net.Http.Json;
using Shared.Contracts.AttendanceService;

namespace BlazorWeb.Services;

/// <summary>
/// AttendanceService APIクライアントの実装
/// </summary>
public class AttendanceApiClient : IAttendanceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AttendanceApiClient> _logger;
    private readonly AuthStateService _authStateService;
    private const string ApiBasePath = "/api/attendances";

    public AttendanceApiClient(
        HttpClient httpClient,
        ILogger<AttendanceApiClient> logger,
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
    public async Task<IEnumerable<AttendanceDto>> GetAttendancesByEmployeeAsync(
        Guid employeeId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching attendances for employee {EmployeeId}", employeeId);

            var queryParams = new List<string>();
            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var url = $"{ApiBasePath}/employee/{employeeId}{queryString}";

            var attendances = await _httpClient.GetFromJsonAsync<IEnumerable<AttendanceDto>>(
                url,
                cancellationToken);

            _logger.LogInformation("Successfully fetched {Count} attendances for employee {EmployeeId}",
                attendances?.Count() ?? 0, employeeId);
            return attendances ?? Enumerable.Empty<AttendanceDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching attendances for employee {EmployeeId}", employeeId);
            throw new InvalidOperationException("勤怠履歴の取得に失敗しました。", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching attendances for employee {EmployeeId}", employeeId);
            throw new InvalidOperationException("勤怠履歴の取得中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<MonthlyAttendanceSummaryDto> GetMonthlyAttendanceSummaryAsync(
        Guid employeeId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching monthly summary for employee {EmployeeId}, {Year}/{Month}",
                employeeId, year, month);

            var url = $"{ApiBasePath}/employee/{employeeId}/summary/{year}/{month}";

            var summary = await _httpClient.GetFromJsonAsync<MonthlyAttendanceSummaryDto>(
                url,
                cancellationToken);

            if (summary is null)
            {
                throw new InvalidOperationException("月次勤怠集計の取得に失敗しました。");
            }

            _logger.LogInformation("Successfully fetched monthly summary for employee {EmployeeId}", employeeId);
            return summary;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching monthly summary for employee {EmployeeId}", employeeId);
            throw new InvalidOperationException("月次勤怠集計の取得に失敗しました。", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching monthly summary for employee {EmployeeId}", employeeId);
            throw new InvalidOperationException("月次勤怠集計の取得中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<AttendanceDto> CheckInAsync(
        CheckInRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Recording check-in for employee {EmployeeId}", request.EmployeeId);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{ApiBasePath}/checkin")
            {
                Content = JsonContent.Create(request)
            };
            AddAuthHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to record check-in. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("この操作を実行する権限がありません。");
                }

                throw new InvalidOperationException($"出勤記録の登録に失敗しました。（エラー: {response.StatusCode}）");
            }

            var attendance = await response.Content.ReadFromJsonAsync<AttendanceDto>(cancellationToken: cancellationToken);
            if (attendance is null)
            {
                throw new InvalidOperationException("出勤記録の登録は成功しましたが、レスポンスの解析に失敗しました。");
            }

            _logger.LogInformation("Successfully recorded check-in for employee {EmployeeId}", request.EmployeeId);
            return attendance;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while recording check-in for employee {EmployeeId}", request.EmployeeId);
            throw new InvalidOperationException("出勤記録の登録に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Unexpected error while recording check-in for employee {EmployeeId}", request.EmployeeId);
            throw new InvalidOperationException("出勤記録の登録中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<AttendanceDto> CheckOutAsync(
        CheckOutRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Recording check-out for employee {EmployeeId}", request.EmployeeId);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{ApiBasePath}/checkout")
            {
                Content = JsonContent.Create(request)
            };
            AddAuthHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to record check-out. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("この操作を実行する権限がありません。");
                }

                throw new InvalidOperationException($"退勤記録の登録に失敗しました。（エラー: {response.StatusCode}）");
            }

            var attendance = await response.Content.ReadFromJsonAsync<AttendanceDto>(cancellationToken: cancellationToken);
            if (attendance is null)
            {
                throw new InvalidOperationException("退勤記録の登録は成功しましたが、レスポンスの解析に失敗しました。");
            }

            _logger.LogInformation("Successfully recorded check-out for employee {EmployeeId}", request.EmployeeId);
            return attendance;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while recording check-out for employee {EmployeeId}", request.EmployeeId);
            throw new InvalidOperationException("退勤記録の登録に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Unexpected error while recording check-out for employee {EmployeeId}", request.EmployeeId);
            throw new InvalidOperationException("退勤記録の登録中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<AttendanceDto> GetAttendanceByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching attendance {AttendanceId}", id);

            var attendance = await _httpClient.GetFromJsonAsync<AttendanceDto>(
                $"{ApiBasePath}/{id}",
                cancellationToken);

            if (attendance is null)
            {
                throw new InvalidOperationException("勤怠記録の取得に失敗しました。");
            }

            _logger.LogInformation("Successfully fetched attendance {AttendanceId}", id);
            return attendance;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching attendance {AttendanceId}", id);
            throw new InvalidOperationException("勤怠記録の取得に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Unexpected error while fetching attendance {AttendanceId}", id);
            throw new InvalidOperationException("勤怠記録の取得中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<AttendanceDto> CreateAttendanceAsync(
        CreateAttendanceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating attendance for employee {EmployeeId}", request.EmployeeId);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiBasePath)
            {
                Content = JsonContent.Create(request)
            };
            AddAuthHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create attendance. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("この操作を実行する権限がありません。");
                }

                throw new InvalidOperationException($"勤怠記録の作成に失敗しました。（エラー: {response.StatusCode}）");
            }

            var attendance = await response.Content.ReadFromJsonAsync<AttendanceDto>(cancellationToken: cancellationToken);
            if (attendance is null)
            {
                throw new InvalidOperationException("勤怠記録の作成は成功しましたが、レスポンスの解析に失敗しました。");
            }

            _logger.LogInformation("Successfully created attendance for employee {EmployeeId}", request.EmployeeId);
            return attendance;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while creating attendance for employee {EmployeeId}", request.EmployeeId);
            throw new InvalidOperationException("勤怠記録の作成に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Unexpected error while creating attendance for employee {EmployeeId}", request.EmployeeId);
            throw new InvalidOperationException("勤怠記録の作成中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<AttendanceDto> UpdateAttendanceAsync(
        Guid id,
        UpdateAttendanceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating attendance {AttendanceId}", id);

            var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"{ApiBasePath}/{id}")
            {
                Content = JsonContent.Create(request)
            };
            AddAuthHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to update attendance. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("この操作を実行する権限がありません。");
                }

                throw new InvalidOperationException($"勤怠記録の更新に失敗しました。（エラー: {response.StatusCode}）");
            }

            var attendance = await response.Content.ReadFromJsonAsync<AttendanceDto>(cancellationToken: cancellationToken);
            if (attendance is null)
            {
                throw new InvalidOperationException("勤怠記録の更新は成功しましたが、レスポンスの解析に失敗しました。");
            }

            _logger.LogInformation("Successfully updated attendance {AttendanceId}", id);
            return attendance;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while updating attendance {AttendanceId}", id);
            throw new InvalidOperationException("勤怠記録の更新に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Unexpected error while updating attendance {AttendanceId}", id);
            throw new InvalidOperationException("勤怠記録の更新中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAttendanceAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting attendance {AttendanceId}", id);

            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{ApiBasePath}/{id}");
            AddAuthHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to delete attendance. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("この操作を実行する権限がありません。");
                }

                throw new InvalidOperationException($"勤怠記録の削除に失敗しました。（エラー: {response.StatusCode}）");
            }

            _logger.LogInformation("Successfully deleted attendance {AttendanceId}", id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while deleting attendance {AttendanceId}", id);
            throw new InvalidOperationException("勤怠記録の削除に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Unexpected error while deleting attendance {AttendanceId}", id);
            throw new InvalidOperationException("勤怠記録の削除中に予期しないエラーが発生しました。", ex);
        }
    }
}
