using SensorPush.Providers;
using SensorPushTask.Services;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient<SensorPushProvider>(client =>
    {
        // Set the base address of the named client.
        client.BaseAddress = new Uri("https://api.sensorpush.com/api/v1/");
    });
builder.Services.AddHostedService<SensorPushService>();

var host = builder.Build();
host.Run();
