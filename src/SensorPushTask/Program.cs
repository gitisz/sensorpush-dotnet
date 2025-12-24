using dotenv.net;
using Microsoft.AspNetCore.Builder;
using SensorPush.Providers;
using SensorPushTask.Providers;
using SensorPushTask.Services;


var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
}

builder.Services.AddHttpClient<SensorPushProvider>(client =>
    {
        client.BaseAddress = new Uri("https://api.sensorpush.com/api/v1/");
    });

builder.Services.AddHttpClient<SensorPushClient>(client =>
    {
        client.BaseAddress = new Uri("https://api.sensorpush.com/api/v1/");
    });

builder.Services.AddScoped<InfluxDBWriter>();
builder.Services.AddScoped<SensorPushTask.Services.SensorPushService>();
builder.Services.AddHostedService<SensorPushTask.Services.SensorPushService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddControllers();

builder.Services.AddScoped<SensorPushTask.Services.QueuedBackfillService>();
builder.Services.AddHostedService<SensorPushTask.Services.QueuedBackfillService>();

var app = builder.Build();

app.MapControllers();

app.MapGet("/health", () => "OK");

app.Run();
