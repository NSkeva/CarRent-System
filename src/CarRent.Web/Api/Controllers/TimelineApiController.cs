using CarRent.DAL;
using CarRent.Model.Enums;
using CarRent.Web.Api.Dtos;
using CarRent.Web.Repositories;
using CarRent.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Api.Controllers;

[Route("api/timeline")]
[ApiController]
[Authorize(Roles = "Admin,Manager")]
public sealed class TimelineApiController(
    CarRentDbContext db,
    ReservationRepository reservationRepository,
    VehicleRepository vehicleRepository) : ControllerBase
{
    [HttpPatch("reservation/{id:int}/schedule")]
    public async Task<ActionResult<TimelineScheduleResultDto>> PatchSchedule(
        int id,
        [FromBody] TimelineReservationSchedulePatchDto dto)
    {
        if (dto.EndDate.Date < dto.StartDate.Date)
            return BadRequest(new { error = "Datum završetka mora biti nakon početka." });

        var entity = await db.Reservations.FindAsync(id);
        if (entity is null)
            return NotFound();

        if (!IsEditable(entity.Status))
            return BadRequest(new { error = "Rezervacija u ovom statusu nije moguće premjestiti." });

        var vehicle = await vehicleRepository.GetByIdAsync(dto.VehicleId);
        if (vehicle is null)
            return BadRequest(new { error = "Vozilo nije pronađeno." });

        if (vehicle.BlockedByService)
            return BadRequest(new { error = "Vozilo je na servisu i privremeno je nedostupno." });

        if (!vehicle.IsActive)
            return BadRequest(new { error = "Vozilo nije aktivno u voznom parku." });

        var conflict = await reservationRepository.FindSchedulingConflictAsync(
            dto.VehicleId,
            dto.StartDate,
            dto.EndDate,
            id);

        if (conflict is not null)
        {
            return Conflict(new
            {
                error = "Preklapanje s drugom rezervacijom na istom vozilu.",
                conflictId = conflict.Id
            });
        }

        entity.VehicleId = dto.VehicleId;
        entity.StartDate = dto.StartDate.Date;
        entity.EndDate = dto.EndDate.Date;
        FleetLifecycleRules.ApplyReservationLifecycle(entity);
        await db.SaveChangesAsync();

        return Ok(new TimelineScheduleResultDto
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status
        });
    }

    private static bool IsEditable(ReservationStatus status)
        => status is ReservationStatus.Draft or ReservationStatus.Confirmed or ReservationStatus.Active;
}
