namespace CarRent.Web.Services.Notifications;

public static class FleetNotificationTypes
{
    public const string RegistrationDue = "RegistrationDue";
    public const string ServiceTomorrow = "ServiceTomorrow";
    public const string ReturnToday = "ReturnToday";
    public const string DraftExpiryReminder = "DraftExpiryReminder";
    public const string DraftExpired = "DraftExpired";
    public const string NoShowCancelled = "NoShowCancelled";
    public const string MileageUpdateSuggested = "MileageUpdateSuggested";
}
