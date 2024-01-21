using System.Text;
using System.Text.Json;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using SensorPush.Providers;
using SensorPushTask.Models;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SensorPushTask.Services;

public class SensorPushService : BackgroundService
{
    private readonly ILogger<SensorPushService> _logger;
    private readonly IConfiguration _configuration;
    private readonly SensorPushProvider _sensorPushProvider;

    public SensorPushService(ILogger<SensorPushService> logger
        , IConfiguration configuration
        , SensorPushProvider sensorPushProvider
        )
    {
        _logger = logger;
        _configuration = configuration;
        _sensorPushProvider = sensorPushProvider;
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

            UpdateSensorPushBucket(sensorPushSensors, sensorPushSamples);

            await Task.Delay(TimeSpan.FromMinutes(int.Parse(_configuration["SensorPush:Interval"] ?? "1")), stoppingToken);
        }
    }

    protected async Task<Dictionary<string, Sensor>?> ListSensorPushSensorsAsync(SensorPushAccessToken accessToken)
    {
        return await _sensorPushProvider.ListSensorsAsync(accessToken?.AccessToken); ;
    }

    protected async Task<SensorPushSample?> GetSensorPushSamplesAsync(SensorPushAccessToken accessToken)
    {
        return await _sensorPushProvider.GetSamplesAsync(accessToken?.AccessToken); ;
    }

    private void UpdateSensorPushBucket(Dictionary<string, Sensor>? sensorPushSensors, SensorPushSample? sensorPushSamples)
    {
        var influxDBProtocol = _configuration["InfluxDBv2:Protocol"];
        var influxDBUrl = _configuration["InfluxDBv2:Url"];
        var influxDBPort = _configuration["InfluxDBv2:Port"];
        var influxDBBucket = _configuration["InfluxDBv2:Bucket"];
        var influxDBToken = Environment.GetEnvironmentVariable(_configuration["InfluxDBv2:TokenKey"] ?? "");
        var influxDBOrg = Environment.GetEnvironmentVariable(_configuration["InfluxDBv2:OrgKey"] ?? "");

        var influxDBUri = $"{influxDBProtocol}://{influxDBUrl}:{influxDBPort}";

        using var client = new InfluxDBClient(influxDBUri, influxDBToken);
        using var writeApi = client.GetWriteApi();

        foreach (var sensor in sensorPushSamples?.Sensors ?? new Dictionary<string, List<SensorData>> { })
        {
            foreach (var sp in sensor.Value)
            {
                Sensor? s = null;

                sensorPushSensors?.TryGetValue(sensor.Key, out s);

                if (s is not null)
                {
                    var sensorPush = new SensorPush
                    {
                        SensorID = sensor.Key,
                        SensorName = s.Name,
                        AbsHumidity = sp.AbsHumidity,
                        Dewpoint = sp.Dewpoint,
                        Humidity = sp.Humidity,
                        Pressure = sp.Pressure,
                        Temperature = sp.Temperature,
                        Vpd = sp.Vpd,
                        Time = sp.Observed,
                    };
                    writeApi.WriteMeasurement(sensorPush, WritePrecision.Ns, influxDBBucket, influxDBOrg);
                }
            }
        }

        foreach (var sensor in sensorPushSensors ?? new Dictionary<string, Sensor> { })
        {
            Sensor? s = null;

            sensorPushSensors?.TryGetValue(sensor.Key, out s);

            if (s is not null)
            {
                var SensorPushV = new SensorPushV
                {
                    SensorID = sensor.Key,
                    SensorName = s.Name,
                    Rssi = sensor.Value.Rssi,
                    Voltage = sensor.Value.BatteryVoltage,
                    Time = DateTimeOffset.UtcNow,
                };

                writeApi.WriteMeasurement(SensorPushV, WritePrecision.Ns, influxDBBucket, influxDBOrg);
            }
        }
    }
}


[Measurement("SensorPush_V")]
public class SensorPushV
{
    [Column("sensor_id", IsTag = true)]
    public string? SensorID { get; set; }

    [Column("sensor_name", IsTag = true)]
    public string? SensorName { get; set; }

    [Column("rssi")]
    public int Rssi { get; set; }

    [Column("voltage")]
    public double Voltage { get; set; }

    [Column(IsTimestamp = true)]
    public DateTimeOffset Time { get; set; }

}


[Measurement("SensorPush")]
public class SensorPush
{
    [Column("sensor_id", IsTag = true)]
    public string? SensorID { get; set; }

    [Column("sensor_name", IsTag = true)]
    public string? SensorName { get; set; }

    [Column("abs_humidity")]
    public double AbsHumidity { get; set; }

    [Column("dewpoint")]
    public double Dewpoint { get; set; }

    [Column("humidity")]
    public double Humidity { get; set; }

    [Column("pressure")]
    public double Pressure { get; set; }

    [Column("temperature")]
    public double Temperature { get; set; }

    [Column("vpd")]
    public double Vpd { get; set; }

    [Column(IsTimestamp = true)]
    public DateTimeOffset Time { get; set; }
}