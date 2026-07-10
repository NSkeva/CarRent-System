namespace CarRent.Web.ViewModels;

public sealed record FleetChatPanelViewModel(
    string AskUrl,
    string WelcomeMessage,
    string Placeholder,
    string? PanelClass = null);

public sealed record AiChatRequest(string Message);
