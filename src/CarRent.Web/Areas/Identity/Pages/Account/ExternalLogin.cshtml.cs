using System.Security.Claims;
using CarRent.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRent.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ExternalLoginModel> _logger;

    public ExternalLoginModel(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        ILogger<ExternalLoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    public IActionResult OnGet() => RedirectToPage("./Login");

    public IActionResult OnPost(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        if (!string.IsNullOrEmpty(remoteError))
        {
            ErrorMessage = $"Greška vanjskog providera: {remoteError}";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });

        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);
        if (signInResult.Succeeded)
            return await RedirectAfterSignInAsync(returnUrl);

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            ErrorMessage = "Google račun nema email. Registracija nije moguća.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                ErrorMessage = string.Join(" ", createResult.Errors.Select(e => e.Description));
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            _logger.LogInformation("Novi Google korisnik {Email} — čeka dodjelu uloge.", email);
        }

        var linkResult = await _userManager.AddLoginAsync(user, info);
        if (!linkResult.Succeeded && !linkResult.Errors.Any(e => e.Code == "LoginAlreadyAssociated"))
        {
            ErrorMessage = "Google račun nije moguće povezati.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return await RedirectAfterSignInAsync(returnUrl);
    }

    private async Task<IActionResult> RedirectAfterSignInAsync(string returnUrl)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToPage("./Login");

        var roles = await _userManager.GetRolesAsync(user);
        return roles.Count == 0
            ? RedirectToPage("./PendingAccess")
            : LocalRedirect(returnUrl);
    }

    [TempData]
    public string? ErrorMessage { get; set; }
}
