# SensorPush .NET
This application connects to the SensorPush API to retrieve metrics for SensorPush devices configured through the SensorPush Gateway.

When a successful authorization is made, samples are returned and then written to an InfluxDB v2 instance that runs separately.  For more information on how to get InfluxDB v2 installed, please see their [installation documentation](https://docs.influxdata.com/influxdb/v2/install/?t=Docker).

## Configuration
Reference `appsettings.Development.json` and update the correct values for your environment.  You will need to update the following sections:

### - `InfluxDBv2`
  - **Protocol**: Specify either `http` or `https` depending on if your InfluxDB v2 instance requires SSL.
  - **Url**: Provide the IP address or DNS name of the InfluxDB instance.
  - **Port**: Specify the port number that InfluxDB v2 is accessible from.
  - **TokenKey**: Specify an environment variable key for the InfluxDB v2 token that allows access to your InfluxDB v2 bucket.
    - Note: Do not provide the actual token. When starting this application, an environment variable should be set with the actual token that matches the key.
  - **OrgKey**: Specify an environment variable key for the InfluxDB v2 organization that allows access to your InfluxDB v2 bucket.
    - Note: Do not provide the actual organization. When starting this application, an environment variable should be set with the actual organization that matches the key.
  - **Bucket**: specify the InfluxDB v2 bucket where you would like the SensorPush metric data to be written.

### - `SensorPush`

  - **Interval**: Specify the interval in number of minutes to retrieve metric data from the SensorPush API.
  - **Limit**: Specify the number of samples to retrieve from the SensorPush API.
  - **Login**:
    - **UserNameKey**: Specify an environment variable key for the SensorPush login.
      - Note: Do not provide the actual login . When starting this application, an environment variable should be set with the actual login that matches the key.
    - **PasswordKey**: Specify an environment variable key for the SensorPush password.
      - Note: Do not provide the actual password . When starting this application, an environment variable should be set with the actual password that matches the key.
  - **"Measures"**: Specify an array of the measurements you wish to collect.
    - [ "temperature", "humidity", "vpd", "barometric_pressure", "dewpoint" ]


