# FULL projekt — Report: što je napravljeno, bodovi, sljedeći koraci

Dokument za **predaju projekta** i pripremu za usmenu. Temelji se na `Kriteriji-projekt-pragovi.pdf`.

---

## 1. Sažetak bodovanja (PDF — 70 bodova projekta)

| Kriterij (PDF) | Bodovi | Status u CarRent | Detaljni MD |
|----------------|--------|------------------|-------------|
| Deploy cloud / VM | 3 | Kod spreman (Docker + GCP skripta), **deploy nije pokrenut** | [FULL-07-DEPLOY-CLOUD.md](FULL-07-DEPLOY-CLOUD.md) |
| Testovi API + Playwright 10+ koraka (+3 bonus) | 2 (+3) | ✅ 55 xUnit + ✅ Playwright 13 koraka | [FULL-01-PLAYWRIGHT-E2E.md](FULL-01-PLAYWRIGHT-E2E.md), [FULL-08-API-INTEGRACIJSKI-TESTOVI.md](FULL-08-API-INTEGRACIJSKI-TESTOVI.md) |
| AI integracija | 3 | ✅ Web chat + opcionalno OpenAI | [FULL-05-AI-KLIJENTSKI-CHAT.md](FULL-05-AI-KLIJENTSKI-CHAT.md) |
| Global search | 2 | ✅ Implementirano | [FULL-02-GLOBAL-SEARCH.md](FULL-02-GLOBAL-SEARCH.md) |
| Logging (file ili API) | 2 | ✅ Serilog + `/api/logs/recent` | [FULL-03-LOGGING.md](FULL-03-LOGGING.md) |
| Responsive mobile/web | 2 | ✅ Hamburger + CSS | [FULL-04-RESPONSIVE-MOBILE.md](FULL-04-RESPONSIVE-MOBILE.md) |
| CRUD bez grešaka | 2 | ✅ Postojeći CRUD + E2E pokriva | [FULL-08-API-INTEGRACIJSKI-TESTOVI.md](FULL-08-API-INTEGRACIJSKI-TESTOVI.md) |
| MCP + agentic IDE | 2 | ✅ MCP server + Cursor config | [FULL-06-MCP-SERVER.md](FULL-06-MCP-SERVER.md) |
| Okvirni dojam / bez crasha | 12 | Ovisi o demo + stabilnosti | svi gornji + ručno testiranje |
| Usmeno razumijevanje koda | 40 | Priprema: lab-5 + FULL MD-ovi | `lab-5/LAB5-PUNI-PREGLED.md` |

**Lab vježbe (30 bod., prag 50% za potpis):** prema tvojoj potvrdi — pokriveno iz Lab 1–5.

---

## 2. Procjena bodova (realistično)

| Kategorija | Očekivano ako sve pokažeš uživo |
|------------|----------------------------------|
| Tehnički kriteriji (implementirano u kodu) | ~17–21 / 20 (+ do 3 Playwright bonus) |
| Deploy (kad deployaš na GCP) | +3 |
| Stabilnost / dojam | 8–12 / 12 |
| Usmeno | ovisi o tebi / 40 |

Za ocjenu **5** treba **≥90%** = **63/70** na projektu — usmeni (40) je ključan.

---

## 3. Što je novo u repozitoriju (FULL implementacija)

```
FULL PROJECT/
├── Kriteriji-projekt-pragovi.pdf
├── FULL-REPORT.md                    ← ovaj dokument
├── FULL-01-PLAYWRIGHT-E2E.md
├── FULL-02-GLOBAL-SEARCH.md
├── FULL-03-LOGGING.md
├── FULL-04-RESPONSIVE-MOBILE.md
├── FULL-05-AI-KLIJENTSKI-CHAT.md
├── FULL-06-MCP-SERVER.md
├── FULL-07-DEPLOY-CLOUD.md
└── FULL-08-API-INTEGRACIJSKI-TESTOVI.md

tests/CarRent.Web.E2E/               ← Playwright
src/CarRent.McpServer/               ← MCP server
src/CarRent.Web/
  ├── Api/Controllers/GlobalSearchApiController.cs
  ├── Api/Controllers/LogsApiController.cs
  ├── Services/GlobalSearchService.cs
  ├── Services/AiClientChatService.cs
  ├── Controllers/ClientChatController.cs
  ├── Middleware/McpApiKeyMiddleware.cs
  └── Views/ClientChat/, _GlobalSearchPartial.cshtml

Dockerfile
scripts/run-e2e.sh
scripts/deploy-gcp.sh
.cursor/mcp.json
```

---

## 4. Brzi demo scenarij (10 minuta za profesora)

1. `./scripts/run-local.sh` → http://localhost:5000
2. Prijava `admin@carrent.local` / `Admin123!`
3. **Ctrl+K** → pretraga „vozilo” ili „rezervacija”
4. **Operativa → Klijentski chat** → „Ima li slobodnih vozila?”
5. Terminal: `curl -H "X-Mcp-Key: carrent-mcp-dev-key" http://localhost:5000/api/vehicle`
6. Terminal: `curl -H "Cookie: ..." http://localhost:5000/api/logs/recent?count=5` (ili nakon admin prijave u browseru)
7. Mobilni viewport u DevTools (F12) → hamburger meni
8. `./scripts/run-e2e.sh` → Playwright 13 koraka prolazi
9. `dotnet test tests/CarRent.Web.IntegrationTests/` → 55/55
10. (Opcionalno) pokaži MCP u Cursor Settings → MCP → carrent

---

## 5. Daljnji koraci implementacije (u kodu — opcionalno)

| Prioritet | Što | Zašto |
|-----------|-----|-------|
| Nisko | Pravi WhatsApp (Twilio sandbox) | AI kriterij već pokriven web chatom |
| Nisko | AutoMapper umjesto ručnih mappera | Nije u kriterijima |
| Srednje | E2E kroz MVC formu (ne samo API) za Create/Edit | Forma trenutno ima problem s lokalizacijom decimala u E2E |
| Visoko | **Deploy na GCP** | Jedini kriterij bez live URL-a |

---

## 6. Koraci koje TI moraš poduzeti

### Obavezno prije predaje

- [ ] Pokreni app: `./scripts/run-local.sh`
- [ ] Pokreni integracijske testove: `dotnet test tests/CarRent.Web.IntegrationTests/`
- [ ] Pokreni E2E: `./scripts/run-e2e.sh` (prvi put instalira Chromium ~100 MB)
- [ ] Prođi demo scenarij iz §4 ručno
- [ ] Pripremi usmenu (lab-5 pregled + FULL MD-ovi)

### Za deploy kriterij (3 boda)

- [ ] Google Cloud račun + `gcloud` CLI
- [ ] `export GCP_PROJECT_ID=tvoj-projekt`
- [ ] `./scripts/deploy-gcp.sh`
- [ ] Spremi **javni URL** za report

### Za puni AI (opcionalno, ne obavezno)

- [ ] `dotnet user-secrets set "OpenAI:ApiKey" "sk-..."` u `CarRent.Web`
- [ ] Restart app → chat koristi GPT umjesto rule-based odgovora

### Za MCP u Cursoru

- [ ] App mora raditi na `http://localhost:5000`
- [ ] Cursor: Settings → MCP → provjeri `.cursor/mcp.json` (već u repou)
- [ ] Restart Cursor ako MCP ne vidi server

### Za produkciju (ako deployaš)

- [ ] Promijeni `Mcp:ApiKey` u sigurnu vrijednost (ne `carrent-mcp-dev-key`)
- [ ] SQLite na Cloud Run je ephemeral — razmisli o Cloud SQL ili volume

---

## 7. Indeks dokumentacije

| Datoteka | Tema |
|----------|------|
| [FULL-01-PLAYWRIGHT-E2E.md](FULL-01-PLAYWRIGHT-E2E.md) | Playwright E2E, 13 koraka |
| [FULL-02-GLOBAL-SEARCH.md](FULL-02-GLOBAL-SEARCH.md) | Globalna pretraga |
| [FULL-03-LOGGING.md](FULL-03-LOGGING.md) | Serilog + Logs API |
| [FULL-04-RESPONSIVE-MOBILE.md](FULL-04-RESPONSIVE-MOBILE.md) | Responsive UI |
| [FULL-05-AI-KLIJENTSKI-CHAT.md](FULL-05-AI-KLIJENTSKI-CHAT.md) | AI asistent |
| [FULL-06-MCP-SERVER.md](FULL-06-MCP-SERVER.md) | MCP + agentic IDE |
| [FULL-07-DEPLOY-CLOUD.md](FULL-07-DEPLOY-CLOUD.md) | Docker + GCP |
| [FULL-08-API-INTEGRACIJSKI-TESTOVI.md](FULL-08-API-INTEGRACIJSKI-TESTOVI.md) | xUnit API testovi + CRUD |

Povezano: [lab-5/LAB5-PUNI-PREGLED.md](../lab-5/LAB5-PUNI-PREGLED.md) za Lab 5 (API, Identity, upload).
