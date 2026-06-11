using System.ComponentModel.DataAnnotations;
using CarRent.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRent.Web.Areas.Identity.Pages.Account;

[Authorize(Roles = "Admin")]
public class RegisterModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public IReadOnlyList<string> AvailableRoles { get; private set; } = ["Manager", "Admin"];

    public class InputModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(11, MinimumLength = 11)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "OIB smije sadržavati samo brojeve.")]
        [Display(Name = "OIB")]
        public string OIB { get; set; } = string.Empty;

        [Required, StringLength(13, MinimumLength = 13)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG smije sadržavati samo brojeve.")]
        [Display(Name = "JMBG")]
        public string JMBG { get; set; } = string.Empty;

        [Required, Display(Name = "Uloga")]
        public string RoleName { get; set; } = "Manager";

        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password), Display(Name = "Lozinka")]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Display(Name = "Potvrda lozinke")]
        [Compare(nameof(Password), ErrorMessage = "Lozinke se ne podudaraju.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (!await _roleManager.RoleExistsAsync(Input.RoleName))
            ModelState.AddModelError(nameof(Input.RoleName), "Odabrana uloga ne postoji.");

        if (!ModelState.IsValid)
            return Page();

        var user = new AppUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            EmailConfirmed = true,
            OIB = Input.OIB,
            JMBG = Input.JMBG
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        await _userManager.AddToRoleAsync(user, Input.RoleName);
        _logger.LogInformation("Admin je kreirao korisnika {Email} ({Role}).", Input.Email, Input.RoleName);
        TempData["Success"] = $"Korisnik {Input.Email} kreiran s ulogom {Input.RoleName}.";
        return RedirectToAction("Index", "Home");
    }
}
