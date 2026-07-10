using CarRent.Model.Enums;

namespace CarRent.Web.Api.Dtos;

public sealed class TimelineReservationSchedulePatchDto
{
    public int VehicleId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class TimelineScheduleResultDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReservationStatus Status { get; set; }
}
