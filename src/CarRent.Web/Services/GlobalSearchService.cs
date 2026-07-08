using CarRent.DAL;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Web.Services;

public sealed record GlobalSearchResult(
    string Type,
    string Title,
    string Subtitle,
    string Url,
    int? EntityId = null);

public sealed class GlobalSearchService(CarRentDbContext db)
{
    private static readonly GlobalSearchResult[] AdminPages =
    [
        new("page", "Rezervacije", "Podaci", "/Reservation"),
        new("page", "Vozila", "Podaci", "/Vehicle"),
        new("page", "Kupci", "Podaci", "/Customer"),
        new("page", "Servisi", "Podaci", "/ServiceRecord"),
        new("page", "Poslovnice", "Podaci", "/BranchOffice"),
        new("page", "Dodaci", "Podaci", "/Addon"),
        new("page", "Zaposlenici", "Podaci", "/Employee"),
        new("page", "Partneri", "Podaci", "/Partners"),
        new("page", "Upravljanje korisnicima", "Admin", "/Identity/Account/ManageUsers"),
    ];

    private static readonly GlobalSearchResult[] SharedPages =
    [
        new("page", "Početna", "Operativa", "/"),
        new("page", "Timeline", "Operativa", "/Timeline"),
        new("page", "Dnevni plan", "Operativa", "/DailyPlan"),
        new("page", "Vozni park", "Operativa", "/Fleet"),
        new("page", "Obavijesti", "Operativa", "/Notifications"),
        new("page", "Klijentski chat", "AI asistent", "/ClientChat"),
        new("page", "Prijava", "Račun", "/Identity/Account/Login"),
    ];

    public async Task<IReadOnlyList<GlobalSearchResult>> SearchAsync(string? query, bool isAdmin, CancellationToken ct = default)
    {
        var pages = isAdmin ? SharedPages.Concat(AdminPages) : SharedPages;
        if (string.IsNullOrWhiteSpace(query))
            return pages.Take(10).ToList();

        var term = query.Trim();
        var termLower = term.ToLowerInvariant();
        var results = new List<GlobalSearchResult>();

        foreach (var page in pages)
        {
            if (page.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                page.Subtitle.Contains(term, StringComparison.OrdinalIgnoreCase))
                results.Add(page);
        }

        var vehicles = await db.Vehicles.AsNoTracking()
            .Where(v => v.Brand.Contains(term) || v.Model.Contains(term) || v.RegistrationNumber.Contains(term))
            .OrderBy(v => v.Brand).Take(6).ToListAsync(ct);
        foreach (var v in vehicles)
        {
            results.Add(new GlobalSearchResult(
                "vehicle",
                $"{v.Brand} {v.Model}",
                v.RegistrationNumber,
                $"/Vehicle/Details/{v.Id}",
                v.Id));
        }

        var customers = await db.Customers.AsNoTracking()
            .Where(c => c.FirstName.Contains(term) || c.LastName.Contains(term) || c.Email.Contains(term))
            .OrderBy(c => c.LastName).Take(5).ToListAsync(ct);
        foreach (var c in customers)
        {
            results.Add(new GlobalSearchResult(
                "customer",
                $"{c.FirstName} {c.LastName}",
                c.Email,
                $"/Customer/Details/{c.Id}",
                c.Id));
        }

        var reservations = await db.Reservations.AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .Where(r => r.Id.ToString() == term ||
                        (r.Customer != null && (r.Customer.FirstName.Contains(term) || r.Customer.LastName.Contains(term))) ||
                        (r.Vehicle != null && (r.Vehicle.Brand.Contains(term) || r.Vehicle.RegistrationNumber.Contains(term))))
            .OrderByDescending(r => r.StartDate).Take(5).ToListAsync(ct);
        foreach (var r in reservations)
        {
            var cust = r.Customer is null ? "—" : $"{r.Customer.FirstName} {r.Customer.LastName}";
            var veh = r.Vehicle is null ? "—" : $"{r.Vehicle.Brand} {r.Vehicle.Model}";
            results.Add(new GlobalSearchResult(
                "reservation",
                $"Rezervacija #{r.Id}",
                $"{cust} · {veh}",
                $"/Reservation/Details/{r.Id}",
                r.Id));
        }

        if (isAdmin)
        {
            var addons = await db.Addons.AsNoTracking()
                .Where(a => a.Name.Contains(term))
                .OrderBy(a => a.Name).Take(5).ToListAsync(ct);
            foreach (var a in addons)
            {
                results.Add(new GlobalSearchResult(
                    "addon",
                    a.Name,
                    $"{a.PricePerDay:N2} EUR/dan",
                    $"/Addon/Details/{a.Id}",
                    a.Id));
            }
        }

        return results
            .DistinctBy(r => $"{r.Type}:{r.Url}")
            .Take(20)
            .ToList();
    }
}
