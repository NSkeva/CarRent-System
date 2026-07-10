using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CarRent.Web.Services;

internal static class FleetAiOpenAiHelper
{
    public static async Task<string?> TryCompleteAsync(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger logger,
        string systemPrompt,
        string userMessage,
        CancellationToken ct = default)
        => await TryConversationAsync(httpClientFactory, config, logger, systemPrompt, [], userMessage, ct);

    public static async Task<string?> TryConversationAsync(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger logger,
        string systemPrompt,
        IReadOnlyList<FleetChatTurn> history,
        string userMessage,
        CancellationToken ct = default)
    {
        var apiKey = config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var messages = new List<object> { new { role = "system", content = systemPrompt } };
            foreach (var turn in history.TakeLast(14))
            {
                var role = turn.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? "assistant" : "user";
                messages.Add(new { role, content = turn.Content });
            }

            messages.Add(new { role = "user", content = userMessage });

            var body = new
            {
                model = config["OpenAI:Model"] ?? "gpt-4o-mini",
                messages,
                max_tokens = 600,
                temperature = 0.4
            };

            using var response = await client.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
                ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("OpenAI odgovor {Status}", response.StatusCode);
                return null;
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OpenAI poziv nije uspio");
            return null;
        }
    }
}
