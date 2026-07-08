using CarRent.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Controllers;

[AllowAnonymous]
public sealed class ClientChatController(AiClientChatService chat) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Klijentski chat";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Poruka je prazna." });

        var reply = await chat.GetReplyAsync(request.Message, ct);
        return Json(new { reply });
    }

    public sealed record ChatRequest(string Message);
}
