using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SensorPushTask.Models;
using SensorPushTask.Services;
using SensorPushTask.Providers;
using System.Collections.Generic;

namespace SensorPushTask.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackfillController : ControllerBase
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly IServiceProvider _services;
    private readonly ILogger<BackfillController> _logger;
    private readonly SensorPushClient _client;
    private readonly IConfiguration _configuration;

    public BackfillController(IConfiguration configuration, IBackgroundTaskQueue queue, IServiceProvider services, ILogger<BackfillController> logger, SensorPushClient client)
    {
        _configuration = configuration;
        _queue = queue;
        _services = services;
        _logger = logger;
        _client = client;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestSamples()
    {
        try
        {
            var accessToken = await _client.GetAccessTokenAsync();
            if (accessToken?.AccessToken == null)
                return BadRequest("Failed to authenticate");

            var sensorsDict = await _client.GetSensorsAsync(accessToken.AccessToken);
            var payload = new
            {
                limit = 10,
                measures = GetConfiguredMeasures(),
            };

            var samples = await _client.FetchSamplesAsync(accessToken.AccessToken, payload);

            return Ok(new { Sensors = sensorsDict, Samples = samples });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("run")]
    public IActionResult StartBackfill([FromBody] BackfillRequest request)
    {
        if (request.StartTime >= request.EndTime) return BadRequest("StartTime must be before EndTime");

        var jobId = Guid.NewGuid();  // For tracking if you add status later
        _logger.LogInformation("Queuing backfill job {JobId} from {Start} to {End}", jobId, request.StartTime, request.EndTime);

        _queue.QueueAsync(async token =>
        {
            using var scope = _services.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<SensorPushClient>();
            var writer = scope.ServiceProvider.GetRequiredService<InfluxDBWriter>();

            await PerformBackfillAsync(client, writer, request, _configuration, token);
        });

        return Accepted(new { JobId = jobId, Message = "Backfill queued successfully" });
    }

    // PRIVATE METHODS (alphabetized)
    private static async Task PerformBackfillAsync(
        SensorPushClient client,
        InfluxDBWriter writer,
        BackfillRequest request,
        IConfiguration configuration,
        CancellationToken token)
    {
        var measures = configuration.GetSection("SensorPush:Measures").Get<string[]>()
                        ?? new[] { "temperature", "humidity" };

        var accessToken = await client.GetAccessTokenAsync();
        var sensorsDict = await client.GetSensorsAsync(accessToken?.AccessToken);
        var allSensors = request.SensorIds ?? sensorsDict?.Keys.ToList();

        var currentStart = request.StartTime.ToUniversalTime();

        while (currentStart < request.EndTime.ToUniversalTime() && !token.IsCancellationRequested)
        {
            var chunkEnd = currentStart.AddHours(12);
            if (chunkEnd > request.EndTime) chunkEnd = request.EndTime;

            var payload = new
            {
                startTime = currentStart.ToString("yyyy-MM-ddTHH:mm:ss.000Z"),
                stopTime = chunkEnd.ToString("yyyy-MM-ddTHH:mm:ss.000Z"),
                limit = 10000,
                sensors = allSensors?.ToArray(),
                measures
            };

            var samples = await client.FetchSamplesAsync(accessToken?.AccessToken, payload);

            if (samples?.Sensors?.Any() == true)
            {
                writer.WriteSamples(samples, sensorsDict);
            }

            currentStart = chunkEnd;

            // Respect rate limit: wait ~61 seconds between requests
            await Task.Delay(TimeSpan.FromSeconds(61), token);
        }
    }

    private string[] GetConfiguredMeasures() =>
        _configuration.GetSection("SensorPush:Measures").Get<string[]>()
        ?? new[] { "temperature", "humidity" };  // Fallback if missing
}
