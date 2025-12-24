using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SensorPushTask.Models;

namespace SensorPush.Providers;

public class SensorPushProvider
{
    private readonly ILogger<SensorPushProvider> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public SensorPushProvider(ILogger<SensorPushProvider> logger
        , IConfiguration configuration
        , HttpClient httpClient
        )
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<SensorPushAuthorization?> LoginAsync()
    {
        var emailKey = _configuration["SensorPush:Login:UserNameKey"] ?? "USERNAME";
        var passwordKey = _configuration["SensorPush:Login:PasswordKey"] ?? "PASSWORD";

        var email = Environment.GetEnvironmentVariable(emailKey);
        var password = Environment.GetEnvironmentVariable(passwordKey);

        _logger.LogDebug("Login attempt - Email present: {EmailPresent}, Password present: {PasswordPresent}",
            !string.IsNullOrEmpty(email), !string.IsNullOrEmpty(password));
        _logger.LogDebug("Using env keys: {EmailKey}, {PasswordKey}", emailKey, passwordKey);

        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                email,
                password,
            }),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("oauth/authorize", jsonContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger.LogWarning("OAuth authorize response: {StatusCode} - {Content}",
            response.StatusCode, responseContent);

        if (response.IsSuccessStatusCode)
        {
            var authorization = JsonSerializer.Deserialize<SensorPushAuthorization>(responseContent);
            _logger.LogInformation("Login successful");
            return authorization;
        }

        _logger.LogError("Login failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
        return null;
    }

    public async Task<SensorPushAccessToken?> GetAccessTokenAsync(string? authorization)
    {
        var accessToken = new SensorPushAccessToken { };
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                authorization,
            }),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("oauth/accesstoken", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            accessToken = JsonSerializer.Deserialize<SensorPushAccessToken>(content);
        }

        return accessToken;
    }

    public async Task<Dictionary<string, Sensor>?> ListSensorsAsync(string? accessToken)
    {
        var sensorPushSample = new Dictionary<string, Sensor> { };

        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new { }),
            Encoding.UTF8,
            "application/json"
        );

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.PostAsync("devices/sensors", jsonContent);

        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            sensorPushSample = JsonSerializer.Deserialize<Dictionary<string, Sensor>>(content);
        }

        return sensorPushSample;
    }

    public async Task<SensorPushSample?> GetSamplesAsync(string? accessToken)
    {
        var sensorPushSample = new SensorPushSample { };

        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                limit = int.Parse(_configuration["SensorPush:Limit"] ?? "10"),
                measures = _configuration.GetSection("SensorPush:Measures").Get<string[]>()
            }),
            Encoding.UTF8,
            "application/json"
        );

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.PostAsync("samples", jsonContent);

        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            sensorPushSample = JsonSerializer.Deserialize<SensorPushSample>(content);
        }

        return sensorPushSample;
    }

}
