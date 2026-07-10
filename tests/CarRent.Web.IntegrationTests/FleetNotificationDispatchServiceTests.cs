using CarRent.DAL;
using CarRent.Web.Services;
using CarRent.Web.Services.Notifications;
using CarRent.Model.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CarRent.Web.IntegrationTests;

public sealed class FleetNotificationDispatchServiceTests
{
    [Fact]
    public async Task ProcessPendingAsync_sends_email_to_default_recipient_not_customer()
    {
        await using var db = CreateDb();
        db.FleetNotificationOutbox.Add(new FleetNotificationOutbox
        {
            Channel = "Email",
            NotificationType = "ReturnToday",
            Subject = "Test",
            Body = "Tijelo poruke",
            Recipient = "customer@example.com",
            DedupKey = $"test:{Guid.NewGuid():N}",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var transport = new RecordingEmailTransport();
        var service = CreateService(db, transport);

        var result = await service.ProcessPendingAsync();

        result.Sent.Should().Be(1);
        transport.Messages.Should().ContainSingle()
            .Which.To.Should().Be("fleet@carrent.local");

        var row = await db.FleetNotificationOutbox.SingleAsync();
        row.SentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessPendingAsync_uses_default_recipient_when_missing()
    {
        await using var db = CreateDb();
        db.FleetNotificationOutbox.Add(new FleetNotificationOutbox
        {
            Channel = "Email",
            NotificationType = "RegistrationDue",
            Subject = "Registracija",
            Body = "Podsjetnik",
            DedupKey = $"test:{Guid.NewGuid():N}",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var transport = new RecordingEmailTransport();
        var service = CreateService(db, transport, defaultRecipient: "fleet@carrent.local");

        var result = await service.ProcessPendingAsync();

        result.Sent.Should().Be(1);
        transport.Messages.Single().To.Should().Be("fleet@carrent.local");
    }

    private static FleetNotificationDispatchService CreateService(
        CarRentDbContext db,
        IEmailTransport transport,
        string defaultRecipient = "fleet@carrent.local")
    {
        var options = Options.Create(new FleetNotificationOptions
        {
            EmailEnabled = true,
            PushEnabled = false,
            DefaultRecipient = defaultRecipient,
            Smtp = new SmtpOptions { Host = "127.0.0.1", Port = 1025 }
        });

        return new FleetNotificationDispatchService(
            db,
            transport,
            new NoOpPushTransport(),
            options,
            NullLogger<FleetNotificationDispatchService>.Instance);
    }

    private static CarRentDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<CarRentDbContext>()
            .UseInMemoryDatabase($"FleetNotify_{Guid.NewGuid():N}")
            .Options;
        return new CarRentDbContext(opts);
    }

    private sealed class RecordingEmailTransport : IEmailTransport
    {
        public List<EmailMessage> Messages { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpPushTransport : IPushTransport
    {
        public Task SendAsync(PushDeviceSubscription subscription, string subject, string body, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
