namespace CarRent.Web.Services;

public sealed class FleetLifecycleOptions
{
    public const string SectionName = "FleetLifecycle";

    public int DraftExpiryDays { get; set; } = 7;
    public int DraftReminderDaysBeforeExpiry { get; set; } = 2;
    public int NoShowHoursAfterStart { get; set; } = 24;
    public int RegistrationNotificationDaysAhead { get; set; } = 14;
}

public sealed class FleetNotificationOptions
{
    public const string SectionName = "FleetNotifications";

    /// <summary>Kad je false, poruke se samo zapisuju u outbox (priprema).</summary>
    public bool EmailEnabled { get; set; }

    public bool PushEnabled { get; set; }
}
