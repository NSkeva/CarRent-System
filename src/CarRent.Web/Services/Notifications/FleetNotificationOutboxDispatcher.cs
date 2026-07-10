using CarRent.Web.Services;
using Microsoft.Extensions.Options;

namespace CarRent.Web.Services.Notifications;

public sealed class FleetNotificationOutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    IOptions<FleetNotificationOptions> options,
    ILogger<FleetNotificationOutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = options.Value;
        if (!opts.DispatchEnabled)
        {
            logger.LogInformation("Fleet email dispatcher je isključen (DispatchEnabled=false).");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(5, opts.DispatchIntervalSeconds));
        logger.LogInformation("Fleet email dispatcher pokrenut (interval {Interval}s).", interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dispatch = scope.ServiceProvider.GetRequiredService<FleetNotificationDispatchService>();
                var result = await dispatch.ProcessPendingAsync(stoppingToken);

                if (result.Sent > 0)
                    logger.LogInformation("Outbox dispatch: poslano {Sent}, neuspjelo {Failed}.", result.Sent, result.Failed);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Greška u fleet email dispatcheru.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
