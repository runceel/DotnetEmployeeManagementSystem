using Shared.Contracts.NotificationService;

namespace BlazorWeb.Services;

public class NotificationApiClient : INotificationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationApiClient> _logger;

    public NotificationApiClient(HttpClient httpClient, ILogger<NotificationApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<NotificationDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/notifications");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<NotificationDto>>() ?? Array.Empty<NotificationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通知一覧の取得に失敗しました");
            return Array.Empty<NotificationDto>();
        }
    }

    public async Task<IEnumerable<NotificationDto>> GetRecentAsync(int count = 50)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/notifications/recent?count={count}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<NotificationDto>>() ?? Array.Empty<NotificationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "最近の通知の取得に失敗しました");
            return Array.Empty<NotificationDto>();
        }
    }

    public async Task<NotificationDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/notifications/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<NotificationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通知の取得に失敗しました: {NotificationId}", id);
            return null;
        }
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/notifications", request);
        response.EnsureSuccessStatusCode();
        var notification = await response.Content.ReadFromJsonAsync<NotificationDto>();
        return notification ?? throw new InvalidOperationException("通知の作成に失敗しました");
    }
}
