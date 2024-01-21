using System.Text.Json.Serialization;

namespace SensorPushTask.Models;

public class SensorPushAuthorization
{
    [JsonPropertyName("authorization")]
    public string Authorization { get; set; } = default!;
}

public class SensorPushAccessToken
{
    [JsonPropertyName("accesstoken")]
    public string AccessToken { get; set; } = default!;
}

public class SensorPushSample
{
    [JsonPropertyName("last_time")]
    public DateTimeOffset LastTime { get; set; } = default!;

    [JsonPropertyName("sensors")]
    public Dictionary<string, List<SensorData>> Sensors { get; set; } = default!;

    [JsonPropertyName("truncated")]
    public bool Truncated { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("total_samples")]
    public int TotalSamples { get; set; } = default!;

    [JsonPropertyName("total_sensors")]
    public int TotalSensors { get; set; } = default!;

}

public class SensorData
{
    [JsonPropertyName("observed")]
    public DateTimeOffset Observed { get; set; } = default!;

    [JsonPropertyName("gateways")]
    public string Gateways { get; set; } = default!;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = default!;

    [JsonPropertyName("dewpoint")]
    public double Dewpoint { get; set; } = default!;

    [JsonPropertyName("humidity")]
    public double Humidity { get; set; } = default!;

    [JsonPropertyName("abs_humidity")]
    public double AbsHumidity { get; set; } = default!;

    [JsonPropertyName("barometric_pressure")]
    public double Pressure { get; set; } = default!;

    [JsonPropertyName("vpd")]
    public double Vpd { get; set; } = default!;
}


public class Sensor
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("rssi")]
    public int Rssi { get; set; } = default!;

    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonPropertyName("battery_voltage")]
    public double BatteryVoltage { get; set; } = default!;

    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = default!;

    [JsonPropertyName("active")]
    public bool Active { get; set; } = default!;

    [JsonPropertyName("address")]
    public string Address { get; set; } = default!;

    [JsonPropertyName("calibration")]
    public Calibration Calibration { get; set; } = default!;

    [JsonPropertyName("alerts")]
    public Alerts Alerts { get; set; } = default!;

}

public class Calibration
{
    [JsonPropertyName("humidity")]
    public double Humidity { get; set; } = default!;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = default!;

}

public class Alerts
{
    [JsonPropertyName("temperature")]
    public TemperatureAlert TemperatureAlert { get; set; } = default!;

    [JsonPropertyName("humidity")]
    public HumidityAlert HumidityAlert { get; set; } = default!;

}

public class TemperatureAlert
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = default!;

    [JsonPropertyName("max")]
    public double Max { get; set; } = default!;

    [JsonPropertyName("min")]
    public double Min { get; set; } = default!;

}

public class HumidityAlert
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = default!;
}

