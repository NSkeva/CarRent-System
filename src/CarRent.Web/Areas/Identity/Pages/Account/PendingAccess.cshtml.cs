using CarRent.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRent.Web.Areas.Identity.Pages.Account;

[Authorize]
public class PendingAccessModel(UserManager<AppUser> userManager) : PageModel
{
    public string Email { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        Email = user?.Email ?? User.Identity?.Name ?? "korisnik";

        if (user is not null && (await userManager.GetRolesAsync(user)).Count > 0)
            return RedirectToAction("Index", "Home");

        return Page();
    }
}
