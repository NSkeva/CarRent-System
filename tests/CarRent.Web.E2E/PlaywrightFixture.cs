using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;

namespace CarRent.Web.E2E;

public sealed class PlaywrightFixture : IAsyncLifetime
{
    public const int Port = 17071;
    public static string ServerAddress => $"http://127.0.0.1:{Port}";

    private Process? _webProcess;

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await StartWebAppAsync();
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
        if (_webProcess is { HasExited: false })
        {
            _webProcess.Kill(entireProcessTree: true);
            await _webProcess.WaitForExitAsync();
        }
    }

    private async Task StartWebAppAsync()
    {
        var root = FindRepoRoot();
        var webProj = Path.Combine(root, "src", "CarRent.Web", "CarRent.Web.csproj");
        var dotnet = File.Exists(Path.Combine(root, ".dotnet", "dotnet"))
            ? Path.Combine(root, ".dotnet", "dotnet")
            : "dotnet";

        _webProcess = Process.Start(new ProcessStartInfo
        {
            FileName = dotnet,
            Arguments = $"run --project \"{webProj}\" --urls {ServerAddress} --no-launch-profile",
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        }) ?? throw new InvalidOperationException("Nije moguće pokrenuti web app za E2E.");

        var deadline = DateTime.UtcNow.AddSeconds(90);
        using var http = new HttpClient();
        while (DateTime.UtcNow < deadline)
        {
            if (_webProcess.HasExited)
                throw new InvalidOperationException($"Web app je pao pri startu (exit {_webProcess.ExitCode}).");

            try
            {
                using var response = await http.GetAsync($"{ServerAddress}/Identity/Account/Login");
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch
            {
                // server još nije spreman
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"Web app nije postao dostupan na {ServerAddress}.");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "scripts", "run-local.sh")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Repo root nije pronađen.");
    }
}

[CollectionDefinition("Playwright")]
public sealed class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>;
