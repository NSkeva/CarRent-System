using System.Text.Json;
using CarRent.Web.Services;
using Microsoft.Extensions.Options;
using WebPush;

namespace CarRent.Web.Services.Notifications;

public sealed class WebPushTransport(
    IOptions<FleetNotificationOptions> options,
    ILogger<WebPushTransport> logger) : IPushTransport
{
    public async Task SendAsync(
        PushDeviceSubscription subscription,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var wp = options.Value.WebPush;
        if (!wp.IsConfigured)
            throw new InvalidOperationException("Web Push VAPID nije konfiguriran.");

        var pushSubscription = new PushSubscription(
            subscription.Endpoint,
            subscription.P256dh,
            subscription.Auth);

        var payload = JsonSerializer.Serialize(new { title = subject, body });
        var vapid = new VapidDetails(wp.Subject, wp.PublicKey, wp.PrivateKey);
        var client = new WebPushClient();

        await client.SendNotificationAsync(pushSubscription, payload, vapid, cancellationToken);
        logger.LogInformation("Push poslan na endpoint {Endpoint}", subscription.Endpoint[..Math.Min(48, subscription.Endpoint.Length)]);
    }
}
