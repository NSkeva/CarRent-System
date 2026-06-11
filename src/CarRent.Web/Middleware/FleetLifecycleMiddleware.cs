using CarRent.Web.Services;

namespace CarRent.Web.Middleware;

public sealed class FleetLifecycleMiddleware(RequestDelegate next, ILogger<FleetLifecycleMiddleware> logger)
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(5);
    private static DateTime _lastSyncUtc = DateTime.MinValue;
    private static readonly SemaphoreSlim SyncGate = new(1, 1);

    public async Task InvokeAsync(HttpContext context, FleetLifecycleService lifecycle)
    {
        if (ShouldTrySync(context))
            await TrySyncAsync(lifecycle, context.RequestAborted);

        await next(context);
    }

    private static bool ShouldTrySync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            return false;

        var path = context.Request.Path;
        if (path.StartsWithSegments("/css")
            || path.StartsWithSegments("/js")
            || path.StartsWithSegments("/lib")
            || path.StartsWithSegments("/uploads")
            || path.StartsWithSegments("/Identity"))
            return false;

        return true;
    }

    private async Task TrySyncAsync(FleetLifecycleService lifecycle, CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow - _lastSyncUtc < SyncInterval)
            return;

        await SyncGate.WaitAsync(cancellationToken);
        try
        {
            if (DateTime.UtcNow - _lastSyncUtc < SyncInterval)
                return;

            var result = await lifecycle.SyncAsync(cancellationToken);
            _lastSyncUtc = DateTime.UtcNow;

            if (result.TotalUpdates > 0)
            {
                logger.LogInformation(
                    "Automatska sinkronizacija statusa: {Reservations} rezervacija, {Services} servisa",
                    result.ReservationUpdates,
                    result.ServiceUpdates);
            }
        }
        finally
        {
            SyncGate.Release();
        }
    }
}
