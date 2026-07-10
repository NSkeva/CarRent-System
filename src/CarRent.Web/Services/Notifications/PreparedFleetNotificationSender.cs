using CarRent.DAL;
using CarRent.Model.Entities;
using CarRent.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CarRent.Web.Services.Notifications;

/// <summary>
/// Zapisuje pripremljene poruke u outbox (Prepared + Email/Push kanali). Slanje: FleetNotificationDispatchService.
/// </summary>
public sealed class PreparedFleetNotificationSender(
    CarRentDbContext db,
    IOptions<FleetNotificationOptions> options,
    ILogger<PreparedFleetNotificationSender> logger) : IFleetNotificationSender
{
    public async Task SendAsync(FleetNotificationMessage message, CancellationToken cancellationToken = default)
    {
        var channels = new List<string> { "Prepared" };
        if (options.Value.EmailEnabled)
            channels.Add("Email");
        if (options.Value.PushEnabled)
            channels.Add("Push");

        var added = 0;
        foreach (var channel in channels)
        {
            var dedupKey = $"{message.DedupKey}:{channel}";
            var exists = await db.FleetNotificationOutbox
                .AnyAsync(n => n.DedupKey == dedupKey, cancellationToken);

            if (exists)
                continue;

            db.FleetNotificationOutbox.Add(new FleetNotificationOutbox
            {
                Channel = channel,
                NotificationType = message.NotificationType,
                Subject = message.Subject,
                Body = message.Body,
                Recipient = message.Recipient,
                DedupKey = dedupKey,
                RelatedEntityType = message.RelatedEntityType,
                RelatedEntityId = message.RelatedEntityId,
                CreatedAt = DateTime.UtcNow,
                SentAt = null
            });
            added++;
        }

        if (added == 0)
            return;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateDedupKey(ex))
        {
            logger.LogDebug("Obavijest [{Type}] već postoji u outboxu (paralelni upis).", message.NotificationType);
            foreach (var entry in db.ChangeTracker.Entries<FleetNotificationOutbox>().ToList())
                entry.State = EntityState.Detached;
            return;
        }

        logger.LogInformation(
            "Pripremljena obavijest [{Type}] za {Recipient} ({Count} kanala)",
            message.NotificationType,
            message.Recipient ?? "—",
            added);
    }

    private static bool IsDuplicateDedupKey(DbUpdateException ex)
        => ex.InnerException?.Message.Contains("DedupKey", StringComparison.OrdinalIgnoreCase) == true
           || ex.InnerException?.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) == true;
}
