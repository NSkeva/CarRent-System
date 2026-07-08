using System.Text;
using System.Text.Json;

namespace CarRent.McpServer;

public sealed class CarRentApiClient
{
    private readonly HttpClient _http;
    private readonly string? _mcpKey;

    public CarRentApiClient()
    {
        var baseUrl = Environment.GetEnvironmentVariable("CarRent__BaseUrl") ?? "http://localhost:5000";
        _mcpKey = Environment.GetEnvironmentVariable("CarRent__McpApiKey") ?? "carrent-mcp-dev-key";
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    private void ApplyAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_mcpKey))
            request.Headers.TryAddWithoutValidation("X-Mcp-Key", _mcpKey);
    }

    public async Task<string> GetAsync(string path, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        ApplyAuth(request);
        using var response = await _http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        return response.IsSuccessStatusCode
            ? body
            : $"HTTP {(int)response.StatusCode}: {body}";
    }

    public async Task<string> PostJsonAsync(string path, object payload, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        ApplyAuth(request);
        using var response = await _http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        return response.IsSuccessStatusCode
            ? body
            : $"HTTP {(int)response.StatusCode}: {body}";
    }
}
