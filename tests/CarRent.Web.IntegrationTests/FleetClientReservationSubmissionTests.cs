using System.Net;
using System.Net.Http.Json;
using CarRent.DAL;
using CarRent.Model.Enums;
using CarRent.Web.Services;
using CarRent.Web.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CarRent.Web.IntegrationTests;

public sealed class FleetClientReservationSubmissionTests : IClassFixture<CarRentWebApplicationFactory>
{
    [Fact]
    public async Task Confirming_chat_creates_draft_reservation_and_outbox_email()
    {
        var factory = new CarRentWebApplicationFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        await client.AsAnonymous().GetAsync("/asistent");
        await client.AsAnonymous().PostAsJsonAsync("/asistent/ask", new AiChatRequest("trebao bi mi auto ovaj vikend"));
        await client.AsAnonymous().PostAsJsonAsync("/asistent/ask", new AiChatRequest("ok golf 7 dana"));
        await client.AsAnonymous().PostAsJsonAsync(
            "/asistent/ask",
            new AiChatRequest("Marko Testic, marko.testic@example.com, 0911111111, aerodrom"));
        var confirm = await client.AsAnonymous().PostAsJsonAsync("/asistent/ask", new AiChatRequest("da"));

        confirm.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await confirm.Content.ReadFromJsonAsync<ChatReply>();
        body!.reply.Should().Contain("Rezervacija #");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CarRentDbContext>();
        var reservation = await db.Reservations
            .OrderByDescending(r => r.Id)
            .FirstAsync();

        reservation.Status.Should().Be(ReservationStatus.Draft);
        reservation.VehicleId.Should().Be(2);

        var outbox = await db.FleetNotificationOutbox
            .Where(n => n.NotificationType == "ClientAssistantBooking")
            .ToListAsync();
        outbox.Should().NotBeEmpty();
    }

    private sealed record ChatReply(string reply);
}
