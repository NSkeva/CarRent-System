using CarRent.Model.Enums;

namespace CarRent.Web.Services;

public static class UiDisplayHelper
{
    public static string ReservationChipClass(ReservationStatus status) => status switch
    {
        ReservationStatus.Draft => "chip-draft",
        ReservationStatus.Confirmed => "chip-confirmed",
        ReservationStatus.Active => "chip-active",
        ReservationStatus.Completed => "chip-completed",
        ReservationStatus.Cancelled => "chip-cancelled",
        _ => "chip-draft"
    };

    public static string ReservationTimelineClass(ReservationStatus status) => status switch
    {
        ReservationStatus.Draft => "draft",
        ReservationStatus.Confirmed => "confirmed",
        ReservationStatus.Active => "active",
        ReservationStatus.Completed => "completed",
        ReservationStatus.Cancelled => "cancelled",
        _ => "draft"
    };

    public static string ServiceChipClass(ServiceStatus status) => status switch
    {
        ServiceStatus.Planned => "chip-planned",
        ServiceStatus.InProgress => "chip-inprogress",
        ServiceStatus.Completed => "chip-completed",
        ServiceStatus.Cancelled => "chip-cancelled",
        _ => "chip-planned"
    };

    public static string RegistrationTimelineClass() => "registration";

    public static string RegistrationChipClass() => "chip-registration";

    public static string ReservationStatusLabel(ReservationStatus status) => status switch
    {
        ReservationStatus.Draft => "Nacrt",
        ReservationStatus.Confirmed => "Potvrđena",
        ReservationStatus.Active => "Aktivna",
        ReservationStatus.Completed => "Završena",
        ReservationStatus.Cancelled => "Otkazana",
        _ => status.ToString()
    };
}
