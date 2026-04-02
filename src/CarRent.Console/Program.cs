using CarRent.Console.Data;
using CarRent.Console.Services;

namespace CarRent.Console;

public static class Program
{
    public static async Task Main()
    {
        var data = SeedData.Create();
        var today = DateTime.UtcNow.Date;

        System.Console.WriteLine("=== CarRent-System Lab1 Demo ===");
        System.Console.WriteLine($"Vozila: {data.Vehicles.Count}");
        System.Console.WriteLine($"Rezervacije: {data.Reservations.Count}");
        System.Console.WriteLine($"Kupci: {data.Customers.Count}");
        System.Console.WriteLine();
        PrintSeedData(data);

        var inReservations = DemoQueries.GetTodaysStarts(data.Reservations, today).ToList();
        var outReservations = DemoQueries.GetTodaysEnds(data.Reservations, today).ToList();

        System.Console.WriteLine("Dnevni plan (IN):");
        foreach (var item in inReservations)
        {
            System.Console.WriteLine($"- Rez#{item.Id} start: {item.StartDate:yyyy-MM-dd} vehicleId={item.VehicleId}");
        }

        System.Console.WriteLine("Dnevni plan (OUT):");
        foreach (var item in outReservations)
        {
            System.Console.WriteLine($"- Rez#{item.Id} end: {item.EndDate:yyyy-MM-dd} vehicleId={item.VehicleId}");
        }

        System.Console.WriteLine();
        System.Console.WriteLine("Vozila po tipu:");
        foreach (var group in DemoQueries.VehiclesByType(data.Vehicles))
        {
            System.Console.WriteLine($"- {group.Key}: {group.Count()}");
        }

        var availableNext3Days = DemoQueries.GetAvailableVehicles(
            data.Vehicles,
            data.Reservations,
            today,
            today.AddDays(3)).ToList();
        System.Console.WriteLine();
        System.Console.WriteLine($"Slobodna vozila u iduca 3 dana: {availableNext3Days.Count}");

        var firstUpcoming = DemoQueries.GetFirstUpcomingReservation(data.Reservations, DateTime.UtcNow);
        System.Console.WriteLine(firstUpcoming is null
            ? "Nema potvrdenih buducih rezervacija."
            : $"Prva buduca potvrdena rezervacija: #{firstUpcoming.Id} ({firstUpcoming.StartDate:yyyy-MM-dd})");

        var dueServices = DemoQueries.ServicesDueSoon(data.ServiceRecords, today.AddDays(30)).ToList();
        System.Console.WriteLine($"Servisi koji dolaze u iducih 30 dana: {dueServices.Count}");

        await SimulateAsyncWork();
    }

    private static async Task SimulateAsyncWork()
    {
        System.Console.WriteLine();
        System.Console.WriteLine("Pokrecem async simulaciju...");
        await Task.Delay(350);
        System.Console.WriteLine("Async simulacija gotova.");
    }

    private static void PrintSeedData(SeedResult data)
    {
        System.Console.WriteLine("=== Seed podaci ===");

        System.Console.WriteLine();
        System.Console.WriteLine("Poslovnice:");
        foreach (var branch in data.Branches)
        {
            System.Console.WriteLine($"- [{branch.Id}] {branch.Name} ({branch.LocationType}) | vozila: {branch.Vehicles.Count}");
        }

        System.Console.WriteLine();
        System.Console.WriteLine("Vozila:");
        foreach (var v in data.Vehicles)
        {
            System.Console.WriteLine(
                $"- [{v.Id}] {v.Brand} {v.Model} ({v.Type}) | reg: {v.RegistrationNumber} | cijena/dan: {v.DailyPrice} EUR | km: {v.MileageKm}");
        }

        System.Console.WriteLine();
        System.Console.WriteLine("Kupci:");
        foreach (var c in data.Customers)
        {
            System.Console.WriteLine(
                $"- [{c.Id}] {c.FirstName} {c.LastName} | email: {c.Email} | rez: {c.Reservations.Count}");
        }

        System.Console.WriteLine();
        System.Console.WriteLine("Rezervacije:");
        foreach (var r in data.Reservations.OrderBy(r => r.StartDate))
        {
            System.Console.WriteLine(
                $"- Rez#{r.Id} kupac={r.CustomerId} vozilo={r.VehicleId} | {r.StartDate:yyyy-MM-dd} -> {r.EndDate:yyyy-MM-dd} | {r.Status} | ukupno: {r.TotalCost()} EUR");
        }

        System.Console.WriteLine();
        System.Console.WriteLine("Servisi:");
        foreach (var s in data.ServiceRecords.OrderByDescending(s => s.ServiceDate))
        {
            System.Console.WriteLine(
                $"- Serv#{s.Id} vozilo={s.VehicleId} | datum: {s.ServiceDate:yyyy-MM-dd} | status: {s.Status} | trosak: {s.Cost} EUR");
        }

        System.Console.WriteLine();
        System.Console.WriteLine("Dodatne usluge:");
        foreach (var addon in data.Addons)
        {
            System.Console.WriteLine($"- [{addon.Id}] {addon.Name} | cijena/dan: {addon.PricePerDay} EUR");
        }

        System.Console.WriteLine();
    }
}
