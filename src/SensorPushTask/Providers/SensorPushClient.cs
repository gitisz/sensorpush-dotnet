using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorPushTask.Models;

namespace SensorPushTask.Providers;

public class SensorPushClient
{
    private readonly ILogger<SensorPushClient> _logger;
    private readonly IConfiguration _configuration;

    private readonly HttpClient _httpClient;

    public SensorPushClient(
        ILogger<SensorPushClient> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<SensorPushAccessToken?> GetAccessTokenAsync()
    {
        var authorization = await LoginAsync();
        if (string.IsNullOrEmpty(authorization?.Authorization))
        {
            return null;
        }
        return await GetAccessTokenAsync(authorization.Authorization);
    }

    private async Task<SensorPushAuthorization?> LoginAsync()
    {
        var email = Environment.GetEnvironmentVariable(_configuration["SensorPush:Login:UserNameKey"] ?? "");
        var password = Environment.GetEnvironmentVariable(_configuration["SensorPush:Login:PasswordKey"] ?? "");

        using var jsonContent = new StringContent(
            JsonSerializer.Serialize(new { email, password }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("oauth/authorize", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SensorPushAuthorization>(content);
        }

        return null;
    }

    private async Task<SensorPushAccessToken?> GetAccessTokenAsync(string authorization)
    {
        using var jsonContent = new StringContent(
            JsonSerializer.Serialize(new { authorization }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("oauth/accesstoken", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SensorPushAccessToken>(content);
        }

        return null;
    }

    public async Task<Dictionary<string, Sensor>?> GetSensorsAsync(string? accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var jsonContent = new StringContent(
            JsonSerializer.Serialize(new { }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("devices/sensors", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dictionary<string, Sensor>>(content);
        }

        return null;
    }

    public async Task<SensorPushSample?> FetchSamplesAsync(string? accessToken, object payload)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var jsonContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("samples", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SensorPushSample>(content);
        }

        return null;
    }
}
