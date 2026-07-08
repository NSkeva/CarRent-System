using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Api.Controllers;

[Route("api/logs")]
[ApiController]
[Authorize(Roles = "Admin")]
public sealed class LogsApiController(IWebHostEnvironment env, IConfiguration config) : ControllerBase
{
    [HttpGet("recent")]
    public IActionResult Recent([FromQuery] int count = 50)
    {
        count = Math.Clamp(count, 1, 200);
        var logDir = Path.Combine(env.ContentRootPath, config["Logging:File:Directory"] ?? "logs");
        if (!Directory.Exists(logDir))
            return Ok(new { lines = Array.Empty<string>(), source = logDir });

        var latest = Directory.GetFiles(logDir, "carrent-*.log")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault();

        if (latest is null || !latest.Exists)
            return Ok(new { lines = Array.Empty<string>(), source = logDir });

        var lines = System.IO.File.ReadLines(latest.FullName).TakeLast(count).ToArray();
        return Ok(new { file = latest.Name, lines, source = logDir });
    }
}
