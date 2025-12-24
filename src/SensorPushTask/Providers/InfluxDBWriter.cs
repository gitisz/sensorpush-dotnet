using InfluxDB.Client;
using SensorPushTask.Models;

namespace SensorPushTask.Providers;

public class InfluxDBWriter : IDisposable
{
    private readonly ILogger<InfluxDBWriter> _logger;
    private readonly IConfiguration _configuration;
    private readonly InfluxDBClient _client;
    private readonly WriteApi _writeApi;
    private readonly string? _bucket;
    private readonly string _org;

    public InfluxDBWriter(ILogger<InfluxDBWriter> logger
        , IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        var protocolKey = _configuration["InfluxDBv2:ProtocolKey"] ?? "";
        var protocol = Environment.GetEnvironmentVariable(protocolKey) ?? "http";

        var urlKey = _configuration["InfluxDBv2:UrlKey"] ?? "";
        var url = Environment.GetEnvironmentVariable(urlKey) ?? "localhost";

        var portKey = _configuration["InfluxDBv2:PortKey"] ?? "";
        var port = Environment.GetEnvironmentVariable(portKey) ?? "8086";

        var influxDBUri = $"{protocol}://{url}:{port}";

        var tokenKey = _configuration["InfluxDBv2:TokenKey"] ?? "";
        var token = Environment.GetEnvironmentVariable(tokenKey);
        var orgKey = _configuration["InfluxDBv2:OrgKey"] ?? "";
        _org = Environment.GetEnvironmentVariable(orgKey) ?? "";

        _bucket = _configuration["InfluxDBv2:Bucket"];

        _logger.LogInformation("InfluxDB connection to {Uri} (org: {Org}, bucket: {Bucket})", influxDBUri, _org, _bucket);

        _client = new InfluxDBClient(influxDBUri, token);
        _writeApi = _client.GetWriteApi();

        // Verify connection
        try
        {
            _client.PingAsync().Wait();
            _logger.LogInformation("InfluxDB connection verified: OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InfluxDB connection failed");
            throw;
        }
    }

    public void WriteSamples(SensorPushSample? samples, Dictionary<string, Sensor>? sensorPushSensors)
    {
        if (samples?.Sensors == null) return;

        var sensorPushList = new List<global::SensorPushTask.Models.SensorPush>();

        // Log sensors being written
        foreach (var sensor in samples.Sensors)
        {
            if (sensorPushSensors?.TryGetValue(sensor.Key, out var s) == true)
            {
                var times = sensor.Value.Select(sp => sp.Observed);
                var timeRange = times.Any() ? $"{times.Min():yyyy-MM-dd HH:mm:ss} to {times.Max():yyyy-MM-dd HH:mm:ss}" : "no timestamps";

                _logger.LogInformation("Writing samples for sensor ID: {SensorId}, Name: {Name}, DeviceID: {DeviceId}, Count: {Count}, Time: {TimeRange}",
                    sensor.Key, s.Name, s.DeviceId, sensor.Value.Count, timeRange);
            }
        }

        foreach (var sensor in samples.Sensors)
        {
            foreach (var sp in sensor.Value)
            {
                if (sensorPushSensors?.TryGetValue(sensor.Key, out var s) == true)
                {
                    var sensorPush = new global::SensorPushTask.Models.SensorPush
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
                    sensorPushList.Add(sensorPush);
                }
            }
        }

        const int BATCH_SIZE = 250;
        for (int i = 0; i < sensorPushList.Count; i += BATCH_SIZE)
        {
            var batchEnd = Math.Min(i + BATCH_SIZE, sensorPushList.Count);
            for (int j = i; j < batchEnd; j++)
            {
                _writeApi.WriteMeasurement(sensorPushList[j], InfluxDB.Client.Api.Domain.WritePrecision.Ms, _bucket, _org);
            }
            _writeApi.Flush();
        }

        // Write status
        if (sensorPushSensors != null)
        {
            var sensorPushVList = new List<global::SensorPushTask.Models.SensorPushV>();

            foreach (var kv in sensorPushSensors)
            {
                var sensorPushV = new global::SensorPushTask.Models.SensorPushV
                {
                    SensorID = kv.Key,
                    SensorName = kv.Value.Name,
                    Rssi = kv.Value.Rssi,
                    Voltage = kv.Value.BatteryVoltage,
                    Time = DateTimeOffset.UtcNow,
                };
                sensorPushVList.Add(sensorPushV);
            }

            const int STATUS_BATCH_SIZE = 50;
            for (int i = 0; i < sensorPushVList.Count; i += STATUS_BATCH_SIZE)
            {
                var batchEnd = Math.Min(i + STATUS_BATCH_SIZE, sensorPushVList.Count);
                for (int j = i; j < batchEnd; j++)
                {
                    _writeApi.WriteMeasurement(sensorPushVList[j], InfluxDB.Client.Api.Domain.WritePrecision.Ms, _bucket, _org);
                }
                _writeApi.Flush();
            }
        }
    }

    public void Dispose()
    {
        _writeApi.Dispose();
        _client.Dispose();
    }
}
