# FULL-06 — MCP server (agentic IDE pristup)

**Kriterij PDF:** Expose MCP i pristup kroz agentic IDE — **2 boda**  
**Status:** ✅ Implementirano

---

## 1. Teorija — što je MCP?

**MCP (Model Context Protocol)** = otvoreni protokol koji omogućuje **AI agentu u IDE-u** (npr. Cursor) da koristi **alate** tvoje aplikacije — čita podatke, poziva API, ne dira ručno kod.

**Analogija:** MCP server je kao „API za AI asistenta” — agent vidi popis alata (`search_vehicles`, `global_search`…) i poziva ih kad korisnik pita nešto o projektu.

**Agentic IDE:** Cursor s MCP-om može reći „pronađi sva slobodna vozila u CarRent bazi” → pozove tvoj alat → vrati JSON.

---

## 2. Gdje je u projektu

```
src/CarRent.McpServer/
├── Program.cs              ← stdio MCP server
├── CarRentTools.cs         ← alati (McpServerTool)
└── CarRentApiClient.cs     ← HTTP prema web app

src/CarRent.Web/
├── Middleware/McpApiKeyMiddleware.cs  ← X-Mcp-Key → Admin sesija
└── appsettings.json                   ← Mcp:ApiKey

.cursor/mcp.json            ← Cursor konfiguracija
```

---

## 3. Gdje u kodu — alati


| MCP alat             | Što radi          | HTTP poziv             |
| -------------------- | ----------------- | ---------------------- |
| `SearchVehicles`     | Pretraga vozila   | GET `/api/vehicle?q=`  |
| `ListAddons`         | Lista dodataka    | GET `/api/addon`       |
| `GlobalSearch`       | Globalna pretraga | GET `/api/search?q=`   |
| `AskClientAssistant` | AI chat           | POST `/ClientChat/Ask` |
| `GetRecentLogs`      | Zadnje log linije | GET `/api/logs/recent` |


### Autentikacija MCP → Web

MCP server šalje header:

```
X-Mcp-Key: carrent-mcp-dev-key
```

`McpApiKeyMiddleware` (nakon `UseAuthentication`) postavlja korisnika kao **Admin** ako ključ odgovara `Mcp:ApiKey` u configu.

---

## 4. Kako pokazati

### Preduvjet: web app mora raditi

```bash
./scripts/run-local.sh
```

### Cursor MCP

1. Otvori projekt u Cursoru
2. Settings → **MCP** → trebao bi vidjeti server **carrent** (iz `.cursor/mcp.json`)
3. U chatu pitaj: „Koristi CarRent MCP i pretraži vozila BMW”

### Ručno test (terminal)

```bash
curl -H "X-Mcp-Key: carrent-mcp-dev-key" \
  "http://localhost:5000/api/vehicle?q=bmw"
```

### Pokreni MCP server ručno (debug)

```bash
export CarRent__BaseUrl=http://localhost:5000
export CarRent__McpApiKey=carrent-mcp-dev-key
dotnet run --project src/CarRent.McpServer/
```

(Server komunicira preko stdio — koristi se iz Cursora, ne interaktivno u terminalu.)

---

## 5. Moguća pitanja profesora

**P: Gdje je MCP?**  
O: Zaseban projekt `src/CarRent.McpServer/`, config u `.cursor/mcp.json`.

**P: Kako MCP pristupa podacima?**  
O: Ne direktno bazi — HTTP pozivi na naš REST API s `X-Mcp-Key` headerom.

**P: Zašto API ključ, a ne login forma?**  
O: Agent ne može lako prolaziti Identity cookie login; API ključ je standardan pattern za machine-to-machine.

**P: Je li to sigurno?**  
O: Za dev OK; u produkciji promijeni `Mcp:ApiKey` i ne objavljuj ga.

**P: Što je stdio transport?**  
O: MCP server čita/piše JSON preko stdin/stdout — Cursor ga pokreće kao subprocess.

---

## 6. Što reći na usmenom

> „Imamo MCP server koji izlaže alate za pretragu vozila, global search i AI chat; Cursor se spaja preko .cursor/mcp.json, a web app prepoznaje agenta preko X-Mcp-Key middlewarea.”

---

## 7. Daljnji koraci — implementacija (opcionalno)

- Alat `create_reservation` s validacijom
- MCP resources (read-only dokumenti)
- OAuth umjesto statičkog API ključa
- HTTP transport umjesto stdio (remote MCP)

---

## 8. Koraci koje TI moraš poduzeti


| Korak                            | Obavezno?  | Akcija                                                  |
| -------------------------------- | ---------- | ------------------------------------------------------- |
| App na :5000                     | **Da**     | `./scripts/run-local.sh` prije korištenja MCP u Cursoru |
| Restart Cursor                   | Preporuka  | Nakon dodavanja `.cursor/mcp.json`                      |
| Provjeri MCP u Settings          | Da za demo | Vidi je li server zelen                                 |
| Promijeni API ključ u produkciji | Na deployu | Ne ostavljaj `carrent-mcp-dev-key`                      |


**Ako MCP ne radi u Cursoru:**

- Provjeri putanju u `mcp.json` → `dotnet` mora biti u PATH (ili koristi puni path do `.dotnet/dotnet`)
- Provjeri da app radi i da port 5000 odgovara

