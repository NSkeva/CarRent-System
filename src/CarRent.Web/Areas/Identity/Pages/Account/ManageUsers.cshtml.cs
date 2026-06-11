using System.ComponentModel.DataAnnotations;
using CarRent.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Web.Areas.Identity.Pages.Account;

[Authorize(Roles = "Admin")]
public class ManageUsersModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ManageUsersModel(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IList<PendingUserRow> PendingUsers { get; private set; } = [];

    public IReadOnlyList<string> AvailableRoles { get; private set; } = ["Manager", "Admin"];

    public class PendingUserRow
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Oib { get; set; }
        public string? Jmbg { get; set; }
    }

    public class AssignRoleInput
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string RoleName { get; set; } = "Manager";
    }

    [BindProperty]
    public AssignRoleInput AssignInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        PendingUsers = await LoadPendingUsersAsync();
    }

    public async Task<IActionResult> OnPostAssignRoleAsync()
    {
        if (!ModelState.IsValid)
        {
            PendingUsers = await LoadPendingUsersAsync();
            return Page();
        }

        if (!await _roleManager.RoleExistsAsync(AssignInput.RoleName))
        {
            ModelState.AddModelError(string.Empty, "Odabrana uloga ne postoji.");
            PendingUsers = await LoadPendingUsersAsync();
            return Page();
        }

        var user = await _userManager.FindByIdAsync(AssignInput.UserId);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Korisnik nije pronađen.");
            PendingUsers = await LoadPendingUsersAsync();
            return Page();
        }

        var existingRoles = await _userManager.GetRolesAsync(user);
        if (existingRoles.Count > 0)
        {
            ModelState.AddModelError(string.Empty, "Korisnik već ima dodijeljenu ulogu.");
            PendingUsers = await LoadPendingUsersAsync();
            return Page();
        }

        await _userManager.AddToRoleAsync(user, AssignInput.RoleName);
        TempData["Success"] = $"Uloga {AssignInput.RoleName} dodijeljena korisniku {user.Email}.";
        return RedirectToPage();
    }

    private async Task<IList<PendingUserRow>> LoadPendingUsersAsync()
    {
        var rows = new List<PendingUserRow>();
        var users = await _userManager.Users.AsNoTracking().OrderBy(u => u.Email).ToListAsync();
        foreach (var user in users)
        {
            if ((await _userManager.GetRolesAsync(user)).Count > 0)
                continue;

            rows.Add(new PendingUserRow
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "?",
                Oib = string.IsNullOrWhiteSpace(user.OIB) ? "—" : user.OIB,
                Jmbg = string.IsNullOrWhiteSpace(user.JMBG) ? "—" : user.JMBG
            });
        }

        return rows;
    }
}
