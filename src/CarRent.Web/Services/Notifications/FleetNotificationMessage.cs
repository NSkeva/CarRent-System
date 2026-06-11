namespace CarRent.Web.Services.Notifications;

public sealed class FleetNotificationMessage
{
    public required string NotificationType { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public string? Recipient { get; init; }
    public required string DedupKey { get; init; }
    public string? RelatedEntityType { get; init; }
    public int? RelatedEntityId { get; init; }
}
