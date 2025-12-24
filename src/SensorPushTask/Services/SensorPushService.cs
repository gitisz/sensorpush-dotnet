using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SensorPush.Providers;
using SensorPushTask.Models;
using SensorPushTask.Providers;

namespace SensorPushTask.Services;

public class SensorPushService : BackgroundService
{
    private readonly ILogger<SensorPushService> _logger;
    private readonly IConfiguration _configuration;
    private readonly SensorPushProvider _sensorPushProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public SensorPushService(
        ILogger<SensorPushService> logger,
        IConfiguration configuration,
        SensorPushProvider sensorPushProvider,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _sensorPushProvider = sensorPushProvider;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            var authorization = await _sensorPushProvider.LoginAsync();

            if (string.IsNullOrEmpty(authorization?.Authorization))
            {
                throw new ArgumentException("Failed to login");
            }

            var accessToken = await _sensorPushProvider.GetAccessTokenAsync(authorization?.Authorization);

            if (string.IsNullOrEmpty(accessToken?.AccessToken))
            {
                throw new ArgumentException("Failed to login");
            }

            var sensorPushSensors = await ListSensorPushSensorsAsync(accessToken);
            var sensorPushSamples = await GetSensorPushSamplesAsync(accessToken);

            using var scope = _scopeFactory.CreateScope();
            var writer = scope.ServiceProvider.GetRequiredService<InfluxDBWriter>();
            writer.WriteSamples(sensorPushSamples, sensorPushSensors);

            await Task.Delay(TimeSpan.FromMinutes(int.Parse(_configuration["SensorPush:Interval"] ?? "1")), stoppingToken);
        }
    }

    protected async Task<Dictionary<string, Sensor>?> ListSensorPushSensorsAsync(SensorPushAccessToken accessToken)
    {
        return await _sensorPushProvider.ListSensorsAsync(accessToken?.AccessToken);
    }

    protected async Task<SensorPushSample?> GetSensorPushSamplesAsync(SensorPushAccessToken accessToken)
    {
        return await _sensorPushProvider.GetSamplesAsync(accessToken?.AccessToken);
    }
}
