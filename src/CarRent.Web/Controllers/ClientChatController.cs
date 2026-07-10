using CarRent.Web.Services;
using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Controllers;

/// <summary>
/// Legacy rute — klijentski Ask za MCP; Index preusmjerava na operativni AI.
/// </summary>
[AllowAnonymous]
public sealed class ClientChatController(
    AiClientChatService chat,
    FleetClientChatSessionStore sessionStore) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true &&
            (User.IsInRole("Admin") || User.IsInRole("Manager")))
            return RedirectToActionPermanent("Index", "OperatorAi");

        return RedirectToActionPermanent("Index", "PublicAssistant");
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] AiChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Poruka je prazna." });

        var session = sessionStore.Load();
        var reply = await chat.GetReplyAsync(request.Message, session, ct);
        sessionStore.Save(session);
        return Json(new { reply });
    }
}
