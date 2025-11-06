using System.Net.Http.Json;
using Shared.Contracts.AuthService;

namespace BlazorWeb.Services;

/// <summary>
/// AuthService APIクライアントの実装
/// </summary>
public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiClient> _logger;
    private const string ApiBasePath = "/api/auth";

    public AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {UserNameOrEmail}", request.UserNameOrEmail);
            
            var response = await _httpClient.PostAsJsonAsync(
                $"{ApiBasePath}/login", 
                request, 
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Login failed: Unauthorized for user {UserNameOrEmail}", request.UserNameOrEmail);
                    return null;
                }
                
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Login failed. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"ログインに失敗しました。（エラー: {response.StatusCode}）");
            }
            
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
            _logger.LogInformation("Login successful for user: {UserName}", authResponse?.UserName);
            return authResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during login for user: {UserNameOrEmail}", request.UserNameOrEmail);
            throw new InvalidOperationException("ログインに失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Unexpected error during login for user: {UserNameOrEmail}", request.UserNameOrEmail);
            throw new InvalidOperationException("ログイン中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to register user: {UserName}", request.UserName);
            
            var response = await _httpClient.PostAsJsonAsync(
                $"{ApiBasePath}/register", 
                request, 
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning("Registration failed: Bad request for user {UserName}", request.UserName);
                    return null;
                }
                
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Registration failed. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"ユーザー登録に失敗しました。（エラー: {response.StatusCode}）");
            }
            
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
            _logger.LogInformation("Registration successful for user: {UserName}", authResponse?.UserName);
            return authResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during registration for user: {UserName}", request.UserName);
            throw new InvalidOperationException("ユーザー登録に失敗しました。", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Unexpected error during registration for user: {UserName}", request.UserName);
            throw new InvalidOperationException("ユーザー登録中に予期しないエラーが発生しました。", ex);
        }
    }
}
