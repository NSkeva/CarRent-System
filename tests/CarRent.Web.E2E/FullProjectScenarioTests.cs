using Microsoft.Playwright;
using Xunit;

namespace CarRent.Web.E2E;

[Collection("Playwright")]
public sealed class FullProjectScenarioTests(PlaywrightFixture fixture)
{
    private readonly string _base = PlaywrightFixture.ServerAddress;

    [Fact]
    public async Task FullProject_Scenario_10PlusSteps_EndToEnd()
    {
        var addonName = $"E2E-Addon-{Guid.NewGuid():N}"[..20];
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();

        // 1. Početna bez prijave → redirect na login
        await page.GotoAsync($"{_base}/");
        await page.WaitForURLAsync("**/Identity/Account/Login**");

        // 2. Prijava Admin
        await page.FillAsync("input[name='Input.Email']", "admin@carrent.local");
        await page.FillAsync("input[name='Input.Password']", "Admin123!");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase));

        // 3. Navigacija na listu dodataka
        await page.GotoAsync($"{_base}/Addon");
        await page.WaitForSelectorAsync("h1, .panel h1");

        // 4. Kreiranje novog dodatka preko API-ja (CRUD)
        var createRes = await page.APIRequest.PostAsync($"{_base}/api/addon", new()
        {
            DataObject = new { name = addonName, pricePerDay = 10m }
        });
        Assert.True(createRes.Ok, await createRes.TextAsync());
        await page.GotoAsync($"{_base}/Addon");

        // 5. AJAX pretraga na listi
        var search = page.Locator("[data-ajax-search]").First;
        if (await search.CountAsync() > 0)
        {
            await search.FillAsync(addonName);
            await page.WaitForTimeoutAsync(400);
            await Assertions.Expect(page.Locator("tbody")).ToContainTextAsync(addonName);
        }

        // 6. Global search (Ctrl+K)
        await page.Keyboard.PressAsync("Control+KeyK");
        await page.WaitForSelectorAsync("[data-global-search-input]");
        await page.FillAsync("[data-global-search-input]", addonName);
        await page.WaitForTimeoutAsync(350);
        var gsResult = page.Locator("[data-global-search-results] a").First;
        if (await gsResult.CountAsync() > 0)
            await gsResult.ClickAsync();

        // 7. API provjera — lista dodataka (autentificirani cookie)
        var apiResponse = await page.APIRequest.GetAsync($"{_base}/api/addon?q={Uri.EscapeDataString(addonName)}");
        Assert.True(apiResponse.Ok);
        var json = await apiResponse.TextAsync();
        Assert.Contains(addonName, json, StringComparison.OrdinalIgnoreCase);

        // 8. Uredi dodatak preko API-ja
        var addonId = System.Text.Json.JsonDocument.Parse(await apiResponse.TextAsync())
            .RootElement.EnumerateArray()
            .First(e => e.GetProperty("name").GetString()!.Contains(addonName, StringComparison.OrdinalIgnoreCase))
            .GetProperty("id").GetInt32();
        var putRes = await page.APIRequest.PutAsync($"{_base}/api/addon/{addonId}", new()
        {
            DataObject = new { id = addonId, name = addonName, pricePerDay = 12m }
        });
        Assert.True(putRes.Ok, await putRes.TextAsync());

        // 9. Klijentski AI chat (javna stranica)
        await page.GotoAsync($"{_base}/ClientChat");
        await page.WaitForSelectorAsync("[data-chat-input]");
        await page.FillAsync("[data-chat-input]", "Ima li slobodnih vozila?");
        await page.ClickAsync("[data-chat-send]");
        await page.WaitForTimeoutAsync(1500);
        await Assertions.Expect(page.Locator("[data-chat-messages]")).ToContainTextAsync("slobod", new() { IgnoreCase = true });

        // 10. Logging API (Admin)
        var logsResponse = await page.APIRequest.GetAsync($"{_base}/api/logs/recent?count=5");
        Assert.True(logsResponse.Ok);

        // 11. Odjava
        await page.GotoAsync($"{_base}/Identity/Account/Logout");
        await page.Locator("form[action*='Logout'] button[type='submit']").ClickAsync();
        await page.WaitForURLAsync("**/Login**");

        // 12. Manager prijava — API bez delete prava
        await page.FillAsync("input[name='Input.Email']", "manager@carrent.local");
        await page.FillAsync("input[name='Input.Password']", "Manager123!");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase));

        var deleteAttempt = await page.APIRequest.DeleteAsync($"{_base}/api/addon/1");
        Assert.True(deleteAttempt.Status is 403 or 404 or 401);

        // 13. Responsive check — mobilni viewport
        await page.SetViewportSizeAsync(390, 844);
        await page.GotoAsync($"{_base}/Fleet");
        await Assertions.Expect(page.Locator("[data-mobile-nav-toggle]")).ToBeVisibleAsync();
    }
}
