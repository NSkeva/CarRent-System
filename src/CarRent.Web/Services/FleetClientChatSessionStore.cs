using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace CarRent.Web.Services;

public sealed class FleetClientChatSessionStore(IHttpContextAccessor httpContextAccessor)
{
    private const string SessionKey = "FleetClientChat";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public FleetClientChatSession Load()
    {
        var session = httpContextAccessor.HttpContext?.Session;
        if (session is null)
            return new FleetClientChatSession();

        var json = session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json))
            return new FleetClientChatSession();

        return JsonSerializer.Deserialize<FleetClientChatSession>(json, JsonOptions) ?? new FleetClientChatSession();
    }

    public void Save(FleetClientChatSession state)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        if (session is null)
            return;

        session.SetString(SessionKey, JsonSerializer.Serialize(state, JsonOptions));
    }
}
