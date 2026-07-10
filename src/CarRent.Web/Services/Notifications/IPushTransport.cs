namespace CarRent.Web.Services.Notifications;

public sealed record PushDeviceSubscription(string Endpoint, string P256dh, string Auth);

public interface IPushTransport
{
    Task SendAsync(
        PushDeviceSubscription subscription,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
