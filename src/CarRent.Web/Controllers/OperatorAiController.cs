using CarRent.Web.Services;
using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
[Route("operativa/ai-asistent")]
public sealed class OperatorAiController(AiOperatorChatService chat) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "AI asistent (operativa)";
        ViewData["NavSection"] = NavContext.OperativaSection;
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "AI asistent");
        return View(new FleetChatPanelViewModel(
            Url.Action("Ask", "OperatorAi")!,
            "Bok! Pitaj me o današnjem planu, slobodnim vozilima, nacrtima ili rezervacijama.",
            "Npr. Što je danas na rasporedu?",
            "chat-panel chat-panel-operator"));
    }

    [HttpPost("ask")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Ask([FromBody] AiChatRequest? request, CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Poruka je prazna." });

        var reply = await chat.GetReplyAsync(request.Message, ct);
        return Json(new { reply });
    }
}
