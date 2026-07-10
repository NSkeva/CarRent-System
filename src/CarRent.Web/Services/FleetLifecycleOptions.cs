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

    /// <summary>Zapisuje Email kanal u outbox (uz Prepared).</summary>
    public bool EmailEnabled { get; set; }

    public bool PushEnabled { get; set; }

    /// <summary>Pokreće pozadinski worker koji šalje Email stavke iz outboxa.</summary>
    public bool DispatchEnabled { get; set; }

    public int DispatchIntervalSeconds { get; set; } = 30;

    /// <summary>Primatelj kad poruka nema Recipient (npr. interni fleet inbox).</summary>
    public string? DefaultRecipient { get; set; }

    public SmtpOptions Smtp { get; set; } = new();

    public WebPushOptions WebPush { get; set; } = new();
}

public sealed class WebPushOptions
{
    public string Subject { get; set; } = "mailto:admin@carrent.local";

    public string PublicKey { get; set; } = string.Empty;

    public string PrivateKey { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(PrivateKey);
}

public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public bool UseSsl { get; set; } = true;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string FromAddress { get; set; } = "noreply@carrent.local";

    public string FromName { get; set; } = "CarRent";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
