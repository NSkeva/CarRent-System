using CarRent.DAL;
using CarRent.Model.Entities;
using CarRent.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CarRent.Web.Services.Notifications;

/// <summary>
/// Zapisuje pripremljene email/push poruke u outbox. Stvarno slanje nije implementirano.
/// </summary>
public sealed class PreparedFleetNotificationSender(
    CarRentDbContext db,
    IOptions<FleetNotificationOptions> options,
    ILogger<PreparedFleetNotificationSender> logger) : IFleetNotificationSender
{
    public async Task SendAsync(FleetNotificationMessage message, CancellationToken cancellationToken = default)
    {
        var exists = await db.FleetNotificationOutbox
            .AnyAsync(n => n.DedupKey == message.DedupKey, cancellationToken);

        if (exists)
            return;

        var channels = new List<string> { "Prepared" };
        if (options.Value.EmailEnabled)
            channels.Add("Email");
        if (options.Value.PushEnabled)
            channels.Add("Push");

        foreach (var channel in channels)
        {
            db.FleetNotificationOutbox.Add(new FleetNotificationOutbox
            {
                Channel = channel,
                NotificationType = message.NotificationType,
                Subject = message.Subject,
                Body = message.Body,
                Recipient = message.Recipient,
                DedupKey = $"{message.DedupKey}:{channel}",
                RelatedEntityType = message.RelatedEntityType,
                RelatedEntityId = message.RelatedEntityId,
                CreatedAt = DateTime.UtcNow,
                SentAt = null
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Pripremljena obavijest [{Type}] za {Recipient} (kanali: {Channels})",
            message.NotificationType,
            message.Recipient ?? "—",
            string.Join(", ", channels));
    }
}
