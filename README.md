# SensorPush .NET

This application connects to the SensorPush API to retrieve metrics for SensorPush devices configured through the SensorPush Gateway and writes them to **InfluxDB v2**.

## Prerequisites

**Required**: External **InfluxDB v2** instance (bring your own):
- [InfluxDB v2 installation](https://docs.influxdata.com/influxdb/v2/install/)
- Create org, bucket (`sensorpush_dotnet`), and API token with write permissions

## Quick Start (Local)

```bash
# 1. Setup .env
cp .env.example .env
# Edit .env: INFLUX_DB_V2_URL=your-influxdb-host, token, etc.

# 2. Run locally
cd src/SensorPushTask
dotnet run
```

## Docker

### Build
```bash
docker build -t sensorpush .
```

### Run
```bash
docker run -p 5000:80 --env-file .env sensorpush
```

## Docker Compose (App Only)

**Requires external InfluxDB v2**. Uses `docker-compose.yml`:

```bash
docker-compose up -d
```

**Ports**: `http://localhost:5000`

## Environment Configuration

**Copy and edit `.env`**:
```bash
cp .env.example .env
```

### Required `.env` Variables

```
# InfluxDB v2 (REQUIRED - your external instance)
INFLUX_DB_V2_PROTOCOL=http          # or https
INFLUX_DB_V2_URL=your-influxdb.com  # YOUR InfluxDB host
INFLUX_DB_V2_PORT=8086
INFLUX_DB_V2_TOKEN=your-write-token
INFLUX_DB_V2_ORG=your-org

# SensorPush
USERNAME=your-email@example.com
PASSWORD=your-password
```

## Configuration Files

### `appsettings.json` (maps env var **keys**)

#### `InfluxDBv2`
- **ProtocolKey**: `INFLUX_DB_V2_PROTOCOL`
- **UrlKey**: `INFLUX_DB_V2_URL_KEY` → `INFLUX_DB_V2_URL`
- **PortKey**: `INFLUX_DB_V2_PORT`
- **TokenKey**: `INFLUX_DB_V2_TOKEN`
- **OrgKey**: `INFLUX_DB_V2_ORG`
- **Bucket**: `sensorpush_dotnet`

## API Endpoints

```
GET    /api/backfill/latest    - Latest samples preview
POST   /api/backfill/run       - Start backfill job
```

**Backfill Request**:
```json
{
  "startTime": "2025-01-01T00:00:00Z",
  "endTime": "2025-01-02T00:00:00Z",
  "sensorIds": ["sensor1", "sensor2"]  // optional
}
```

## Features

- **Backfill**: 12hr chunks, 10k limit per chunk
- **Live polling**: Configurable interval
- **Chunked writes**: 250pt batches (no timeouts)
- **Connection verification**: Startup ping
- **Detailed logging**: Sensor ID/Name/DeviceID + time range + count

## Logs

**Startup**:
```
InfluxDB connection to http://your-influxdb:8086 (org: myorg, bucket: sensorpush_dotnet)
InfluxDB connection verified: OK
```

**Backfill**:
```
Writing samples for sensor ID: 16884884..., Name: SP6, DeviceID: 16884884, Count: 1234, Time: 2025-01-01 10:00:00 to 22:00:00
influx-client-write: The data was successfully written to InfluxDB 2.
```

## Troubleshooting

1. **Connection failed**: Verify `INFLUX_DB_V2_*` vars point to **your** InfluxDB v2
2. **Auth errors**: Check SensorPush `USERNAME`/`PASSWORD`
3. **No data**: Verify InfluxDB token has write access to `sensorpush_dotnet` bucket

## Grafana Dashboard

Import `grafana-dashboard.json` into your Grafana instance connected to the `sensorpush_dotnet` bucket to visualize SensorPush metrics.

## Acknowledgements

This project uses a Grafana dashboard that was based from [bolausson/SensorPush](https://github.com/bolausson/SensorPush).
