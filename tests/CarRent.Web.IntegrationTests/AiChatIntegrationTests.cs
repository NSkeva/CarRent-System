using System.Net;
using System.Net.Http.Json;
using CarRent.Web.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CarRent.Web.IntegrationTests;

public sealed class AiChatIntegrationTests : IClassFixture<CarRentWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AiChatIntegrationTests(CarRentWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    [Fact]
    public async Task PublicAssistant_Ask_anonymous_returns_availability_reply()
    {
        var response = await _client.AsAnonymous().PostAsJsonAsync(
            "/asistent/ask",
            new AiChatRequest("Ima li slobodnih vozila?"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ChatReply>();
        body!.reply.Should().NotBeNullOrWhiteSpace();
        body.reply.ToLowerInvariant().Should().Contain("slobod");
    }

    [Fact]
    public async Task OperatorAi_Ask_requires_auth()
    {
        var response = await _client.AsAnonymous().PostAsJsonAsync(
            "/operativa/ai-asistent/ask",
            new AiChatRequest("Što je danas na rasporedu?"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OperatorAi_Ask_manager_returns_operational_reply()
    {
        var response = await _client.AsRole("Manager").PostAsJsonAsync(
            "/operativa/ai-asistent/ask",
            new AiChatRequest("Što je danas na rasporedu?"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ChatReply>();
        body!.reply.Should().NotBeNullOrWhiteSpace();
        body.reply.ToLowerInvariant().Should().MatchRegex("danas|odlaz|povrat|plan");
    }

    [Fact]
    public async Task ClientChat_Ask_legacy_route_still_works()
    {
        var response = await _client.AsAnonymous().PostAsJsonAsync(
            "/ClientChat/Ask",
            new AiChatRequest("Bok"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ChatReply>();
        body!.reply.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PublicAssistant_multi_turn_remembers_context()
    {
        var client = _client.AsAnonymous();

        var first = await client.PostAsJsonAsync("/asistent/ask", new AiChatRequest("trebao bi mi auto ovaj vikend"));
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstBody = await first.Content.ReadFromJsonAsync<ChatReply>();
        firstBody!.reply.ToLowerInvariant().Should().MatchRegex("slobod|golf|octavia|imamo|vikend");

        var second = await client.PostAsJsonAsync("/asistent/ask", new AiChatRequest("ok golf 7 dana"));
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondBody = await second.Content.ReadFromJsonAsync<ChatReply>();
        secondBody!.reply.ToLowerInvariant().Should().MatchRegex("golf|ime|email|mobitel|super|procjena");
    }

    private sealed record ChatReply(string reply);
}
