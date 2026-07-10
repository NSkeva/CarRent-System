using System.ComponentModel;
using ModelContextProtocol.Server;

namespace CarRent.McpServer;

[McpServerToolType]
public sealed class CarRentTools(CarRentApiClient api)
{
    [McpServerTool, Description("Pretraži vozila u CarRent floti (brand, model, registracija).")]
    public Task<string> SearchVehicles(
        [Description("Tekst pretrage")] string query,
        CancellationToken cancellationToken = default)
        => api.GetAsync($"/api/vehicle?q={Uri.EscapeDataString(query)}", cancellationToken);

    [McpServerTool, Description("Dohvati sve dodatke (addons) za najam.")]
    public Task<string> ListAddons(CancellationToken cancellationToken = default)
        => api.GetAsync("/api/addon", cancellationToken);

    [McpServerTool, Description("Globalna pretraga stranica i podataka u CarRent sustavu.")]
    public Task<string> GlobalSearch(
        [Description("Upit")] string query,
        CancellationToken cancellationToken = default)
        => api.GetAsync($"/api/search?q={Uri.EscapeDataString(query)}", cancellationToken);

    [McpServerTool, Description("Pitaj CarRent AI asistenta (dostupnost vozila, rezervacije).")]
    public Task<string> AskClientAssistant(
        [Description("Poruka klijenta")] string message,
        CancellationToken cancellationToken = default)
        => api.PostJsonAsync("/asistent/ask", new { message }, cancellationToken);

    [McpServerTool, Description("Dohvati zadnjih N linija iz CarRent log datoteke (Admin).")]
    public Task<string> GetRecentLogs(
        [Description("Broj linija")] int count = 20,
        CancellationToken cancellationToken = default)
        => api.GetAsync($"/api/logs/recent?count={count}", cancellationToken);
}
