using CarRent.DAL;
using CarRent.Model.Entities;
using CarRent.Web.Api.Controllers;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Web.Services.Notifications;

public sealed class PushSubscriptionService(CarRentDbContext db)
{
    public async Task SaveAsync(string userId, PushSubscribeDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await db.FleetPushSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == dto.Endpoint, cancellationToken);

        if (existing is not null)
        {
            existing.P256dh = dto.Keys.P256dh;
            existing.Auth = dto.Keys.Auth;
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        db.FleetPushSubscriptions.Add(new FleetPushSubscription
        {
            UserId = userId,
            Endpoint = dto.Endpoint,
            P256dh = dto.Keys.P256dh,
            Auth = dto.Keys.Auth,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(string userId, string endpoint, CancellationToken cancellationToken = default)
    {
        var existing = await db.FleetPushSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == endpoint, cancellationToken);
        if (existing is null) return;

        db.FleetPushSubscriptions.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<List<FleetPushSubscription>> GetAllAsync(CancellationToken cancellationToken = default)
        => db.FleetPushSubscriptions.AsNoTracking().ToListAsync(cancellationToken);
}
