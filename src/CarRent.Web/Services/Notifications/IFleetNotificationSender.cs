namespace CarRent.Web.Services.Notifications;

/// <summary>
/// Sučelje za slanje obavijesti. Trenutna implementacija samo priprema poruke (outbox).
/// </summary>
public interface IFleetNotificationSender
{
    Task SendAsync(FleetNotificationMessage message, CancellationToken cancellationToken = default);
}
