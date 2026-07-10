# FULL-01 — Playwright E2E testovi (10+ koraka, +3 bonus)

**Kriterij PDF:** Kreiranje testova za sve API endpointe (Playwright scenarij 10 koraka) — **2 boda + 3 extra bonus**  
**Status:** ✅ Implementirano i prolazi

---

## 1. Teorija — što je Playwright E2E?

**End-to-end (E2E) test** pokreće **pravi browser** (Chromium), otvara aplikaciju kao korisnik i provjerava cijeli tok: login, navigacija, API, UI.

**Razlika od integracijskog testa (xUnit):**

- Integracijski: `HttpClient` → server (bez browsera)
- Playwright: browser + klikovi + tipkovnica + cookie sesija

**Zašto profesor traži 10+ koraka:** Dokaz da aplikacija radi **skupno**, ne samo pojedinačni endpoint.

---

## 2. Gdje je u projektu

```
tests/CarRent.Web.E2E/
├── CarRent.Web.E2E.csproj
├── PlaywrightFixture.cs          ← pokreće web app, browser
└── FullProjectScenarioTests.cs   ← jedan test, 13 koraka

scripts/run-e2e.sh                ← jedna naredba za pokretanje
```

**Ovisnost:** Playwright Chromium u `.playwright/` (instalira se pri prvom `./scripts/run-e2e.sh`).

---

## 3. Gdje u kodu — 13 koraka scenarija


| Korak | Što radi                           | Gdje u kodu                        |
| ----- | ---------------------------------- | ---------------------------------- |
| 1     | Bez prijave → redirect Login       | `FullProjectScenarioTests.cs` ~L19 |
| 2     | Admin prijava                      | L23–26                             |
| 3     | Navigacija `/Addon`                | L29                                |
| 4     | API POST create addon              | L33–37                             |
| 5     | AJAX pretraga na listi             | L41–47                             |
| 6     | Global search Ctrl+K               | L49–55                             |
| 7     | API GET `/api/addon`               | L57–61                             |
| 8     | API PUT edit addon                 | L63–71                             |
| 9     | Klijentski chat `/ClientChat`      | L76–80                             |
| 10    | Logs API `/api/logs/recent`        | L83–84                             |
| 11    | Odjava                             | L87–89                             |
| 12    | Manager prijava + DELETE zabranjen | L92–97                             |
| 13    | Mobilni viewport + hamburger       | L99–102                            |


**Fixture:** `PlaywrightFixture.cs` — subprocess `dotnet run` na `http://127.0.0.1:17071`.

---

## 4. Kako pokrenuti

```bash
./scripts/run-e2e.sh
```

Ili ručno:

```bash
export PLAYWRIGHT_BROWSERS_PATH="$(pwd)/.playwright"
npx playwright@1.50.0 install chromium   # samo prvi put
dotnet test tests/CarRent.Web.E2E/CarRent.Web.E2E.csproj --filter FullProject_Scenario
```

**Očekivano:** `Passed! 1/1`

---

## 5. Moguća pitanja profesora

**P: Gdje su Playwright testovi?**  
O: `tests/CarRent.Web.E2E/FullProjectScenarioTests.cs`

**P: Zašto ne koristite WebApplicationFactory za browser?**  
O: Playwright treba **pravi TCP** (Kestrel). Fixture pokreće `dotnet run` na portu 17071.

**P: Koliko koraka ima scenarij?**  
O: 13 — prelazi prag od 10 za bonus.

**P: Testirate li API?**  
O: Da — koraci 4, 7, 8, 10 koriste `page.APIRequest` s admin cookiejem. Detaljnije API testove pokriva xUnit (vidi FULL-08).

**P: Zašto create ide preko API, a ne forme?**  
O: MVC forma ima problem s decimalnim separatorom (hr kultura) u automatiziranom testu; API je pouzdaniji za CI. Ručno CRUD i dalje radi u browseru.

---

## 6. Što reći na usmenom (jedna rečenica)

> „Playwright E2E test pokreće pravu aplikaciju u browseru, prolazi login, CRUD preko API-ja, AJAX pretragu, global search, AI chat, logging API i provjeru uloga — 13 koraka u jednom testu.”

---

## 7. Daljnji koraci — implementacija (opcionalno)

- Dodati E2E korak kroz MVC Create formu (nakon fixa lokalizacije `PricePerDay`)
- Screenshot/video na fail za lakši debug
- CI job (GitHub Actions) koji pokreće `run-e2e.sh`

---

## 8. Koraci koje TI moraš poduzeti


| Korak               | Obavezno? | Akcija                                                    |
| ------------------- | --------- | --------------------------------------------------------- |
| Prvo pokretanje E2E | Da        | `./scripts/run-e2e.sh` (preuzme Chromium, ~2 min)         |
| Node/npx            | Da        | Treba `npx` za instalaciju browsera (Node iz Cursora/npm) |
| Port 17071 slobodan | Da        | E2E koristi taj port; zatvori drugi instance ako fail     |
| Prije predaje       | Da        | Pokreni test i pokaži profesoru zeleni output             |


**Nema** dodatnih API ključeva ili cloud postavki za Playwright.