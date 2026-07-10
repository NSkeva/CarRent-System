using CarRent.DAL;
using CarRent.Model.Entities;
using CarRent.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebPush;

namespace CarRent.Web.Services.Notifications;

public sealed class FleetNotificationDispatchService(
    CarRentDbContext db,
    IEmailTransport emailTransport,
    IPushTransport pushTransport,
    IOptions<FleetNotificationOptions> options,
    ILogger<FleetNotificationDispatchService> logger)
{
    public async Task<FleetNotificationDispatchResult> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var result = new FleetNotificationDispatchResult();
        var opts = options.Value;

        if (opts.EmailEnabled)
            await ProcessEmailAsync(result, opts, cancellationToken);
        else
            result.EmailSkippedReason = "EmailEnabled=false";

        if (opts.PushEnabled)
            await ProcessPushAsync(result, opts, cancellationToken);
        else
            result.PushSkippedReason = "PushEnabled=false";

        return result;
    }

    private async Task ProcessEmailAsync(
        FleetNotificationDispatchResult result,
        FleetNotificationOptions opts,
        CancellationToken cancellationToken)
    {
        if (!opts.Smtp.IsConfigured)
        {
            result.EmailSkippedReason = "SMTP nije konfiguriran";
            return;
        }

        var pending = await db.FleetNotificationOutbox
            .Where(n => n.Channel == "Email" && n.SentAt == null)
            .OrderBy(n => n.CreatedAt)
            .Take(25)
            .ToListAsync(cancellationToken);

        result.EmailPendingCount = pending.Count;

        foreach (var item in pending)
        {
            var recipient = ResolveRecipient(item, opts.DefaultRecipient);
            if (recipient is null)
            {
                result.EmailFailed++;
                logger.LogWarning("Email outbox #{Id} nema primatelja.", item.Id);
                continue;
            }

            try
            {
                await emailTransport.SendAsync(new EmailMessage(recipient, item.Subject, item.Body), cancellationToken);
                item.SentAt = DateTime.UtcNow;
                result.EmailSent++;
            }
            catch (Exception ex)
            {
                result.EmailFailed++;
                logger.LogError(ex, "Email outbox #{Id} nije poslan.", item.Id);
            }
        }

        if (result.EmailSent > 0)
            await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessPushAsync(
        FleetNotificationDispatchResult result,
        FleetNotificationOptions opts,
        CancellationToken cancellationToken)
    {
        if (!opts.WebPush.IsConfigured)
        {
            result.PushSkippedReason = "VAPID ključevi nisu postavljeni";
            return;
        }

        var subscriptions = await db.FleetPushSubscriptions.AsNoTracking().ToListAsync(cancellationToken);
        if (subscriptions.Count == 0)
        {
            result.PushSkippedReason = "Nema push pretplata u browseru";
        }

        var pending = await db.FleetNotificationOutbox
            .Where(n => n.Channel == "Push" && n.SentAt == null)
            .OrderBy(n => n.CreatedAt)
            .Take(25)
            .ToListAsync(cancellationToken);

        result.PushPendingCount = pending.Count;
        if (pending.Count == 0)
            return;

        var staleEndpoints = new List<string>();

        foreach (var item in pending)
        {
            if (subscriptions.Count == 0)
            {
                item.SentAt = DateTime.UtcNow;
                result.PushSent++;
                continue;
            }

            var delivered = 0;
            foreach (var sub in subscriptions)
            {
                try
                {
                    await pushTransport.SendAsync(
                        new PushDeviceSubscription(sub.Endpoint, sub.P256dh, sub.Auth),
                        item.Subject,
                        item.Body,
                        cancellationToken);
                    delivered++;
                }
                catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone
                                                  || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    staleEndpoints.Add(sub.Endpoint);
                    logger.LogWarning("Push pretplata istekla, brišem: {Endpoint}", sub.Endpoint);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Push outbox #{Id} nije poslan na {Endpoint}.", item.Id, sub.Endpoint);
                }
            }

            if (delivered > 0 || subscriptions.Count == 0)
            {
                item.SentAt = DateTime.UtcNow;
                result.PushSent++;
            }
            else
            {
                result.PushFailed++;
            }
        }

        if (staleEndpoints.Count > 0)
        {
            var toRemove = await db.FleetPushSubscriptions
                .Where(s => staleEndpoints.Contains(s.Endpoint))
                .ToListAsync(cancellationToken);
            db.FleetPushSubscriptions.RemoveRange(toRemove);
        }

        if (result.PushSent > 0 || staleEndpoints.Count > 0)
            await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Svi emailovi idu na interni inbox (DefaultRecipient), ne kupcima.
    /// </summary>
    private static string? ResolveRecipient(FleetNotificationOutbox item, string? defaultRecipient)
    {
        _ = item;
        return string.IsNullOrWhiteSpace(defaultRecipient) ? null : defaultRecipient.Trim();
    }
}

public sealed class FleetNotificationDispatchResult
{
    public int EmailPendingCount { get; set; }
    public int EmailSent { get; set; }
    public int EmailFailed { get; set; }
    public string? EmailSkippedReason { get; set; }

    public int PushPendingCount { get; set; }
    public int PushSent { get; set; }
    public int PushFailed { get; set; }
    public string? PushSkippedReason { get; set; }

    public int PendingCount => EmailPendingCount + PushPendingCount;
    public int Sent => EmailSent + PushSent;
    public int Failed => EmailFailed + PushFailed;

    public string? SkippedReason => EmailSkippedReason ?? PushSkippedReason;
}
