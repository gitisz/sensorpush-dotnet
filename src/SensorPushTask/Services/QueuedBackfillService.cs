namespace SensorPushTask.Services;

public class QueuedBackfillService : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly IServiceProvider _services;
    private readonly ILogger<QueuedBackfillService> _logger;

    public QueuedBackfillService(IBackgroundTaskQueue queue, IServiceProvider services, ILogger<QueuedBackfillService> logger)
    {
        _queue = queue;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(stoppingToken);
            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing backfill task");
            }
        }
    }
}