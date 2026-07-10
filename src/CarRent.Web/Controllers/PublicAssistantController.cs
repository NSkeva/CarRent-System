using CarRent.Web.Services;
using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Controllers;

[AllowAnonymous]
[Route("asistent")]
public sealed class PublicAssistantController(
    AiClientChatService chat,
    FleetClientChatSessionStore sessionStore) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "CarRent asistent";
        ViewData["IsPublicAssistant"] = true;
        return View(new FleetChatPanelViewModel(
            Url.Action("Ask", "PublicAssistant")!,
            "Bok! Recite mi što trebate — npr. „Trebam auto ovaj vikend” — i vodim vas do rezervacije korak po korak.",
            "Npr. Trebam auto ovaj vikend",
            "chat-panel chat-panel-public"));
    }

    [HttpPost("ask")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Ask([FromBody] AiChatRequest? request, CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Poruka je prazna." });

        var session = sessionStore.Load();
        var reply = await chat.GetReplyAsync(request.Message, session, ct);
        sessionStore.Save(session);
        return Json(new { reply });
    }
}
