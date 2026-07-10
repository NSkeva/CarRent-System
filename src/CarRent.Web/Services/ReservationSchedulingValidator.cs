using CarRent.Web.Repositories;
using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CarRent.Web.Services;

public sealed class ReservationSchedulingValidator(ReservationRepository repository, VehicleRepository vehicles)
{
    public async Task ValidateAsync(
        ReservationFormVm model,
        ModelStateDictionary modelState,
        int excludeReservationId = 0)
    {
        if (model.EndDate <= model.StartDate)
        {
            modelState.AddModelError(nameof(model.EndDate), "Datum završetka mora biti nakon početka.");
            return;
        }

        if (model.VehicleId <= 0)
            return;

        var vehicle = await vehicles.GetByIdAsync(model.VehicleId);
        if (vehicle is { BlockedByService: true })
        {
            modelState.AddModelError(
                nameof(model.VehicleId),
                "Vozilo je na servisu i privremeno je nedostupno za nove rezervacije.");
        }

        if (vehicle is { IsActive: false, BlockedByService: false })
        {
            modelState.AddModelError(
                nameof(model.VehicleId),
                "Vozilo nije aktivno u voznom parku.");
        }

        var conflict = await repository.FindSchedulingConflictAsync(
            model.VehicleId,
            model.StartDate,
            model.EndDate,
            excludeReservationId);

        if (conflict is not null)
        {
            modelState.AddModelError(
                string.Empty,
                $"Vozilo je već rezervirano u odabranom periodu (rezervacija #{conflict.Id}, {conflict.StartDate:dd.MM.yyyy} – {conflict.EndDate:dd.MM.yyyy}).");
        }
    }
}
