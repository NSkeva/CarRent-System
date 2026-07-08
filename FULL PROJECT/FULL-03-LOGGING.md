# FULL-03 — Logging (Serilog file + API)

**Kriterij PDF:** Implementacija logging mehanizma (file ili API) — **2 boda**  
**Status:** ✅ Implementirano (oba: file **i** API)

---

## 1. Teorija — što je logging i zašto?

**Logging** = zapisivanje događaja tijekom rada aplikacije (info, warning, error).

**Zašto:**
- Debug produkcijskih grešaka
- Audit (tko je što napravio)
- Monitoring

**Razine (tipično):** Debug < Information < Warning < Error < Critical

**Serilog** = popularna .NET biblioteka za strukturirani log s više „sinkova” (konzola, datoteka, cloud…).

---

## 2. Gdje je u projektu

```
src/CarRent.Web/
├── Program.cs                          ← UseSerilog(), mapa logs/
├── appsettings.json                    ← Serilog + Logging:File:Directory
├── Api/Controllers/LogsApiController.cs ← GET /api/logs/recent
└── logs/carrent-YYYYMMDD.log           ← nastaje pri runu (gitignore)
```

---

## 3. Gdje u kodu

### File logging (`Program.cs`)

```csharp
builder.Host.UseSerilog(...);
.WriteTo.File(Path.Combine(logDirectory, "carrent-.log"), rollingInterval: Day);
```

- Rotacija: **dnevno** nova datoteka
- Zadržano: 14 dana (`retainedFileCountLimit: 14`)
- Mapa: `src/CarRent.Web/logs/` (relativno na ContentRoot)

### API logging (`LogsApiController.cs`)

| Metoda | Ruta | Tko smije |
|--------|------|-----------|
| GET | `/api/logs/recent?count=50` | Samo **Admin** |

Vraća zadnjih N linija iz **najnovije** `carrent-*.log` datoteke.

Primjer odgovora:

```json
{
  "file": "carrent-20260521.log",
  "lines": ["...", "..."],
  "source": "/path/to/logs"
}
```

---

## 4. Kako pokazati

**Datoteka:**

```bash
ls -la src/CarRent.Web/logs/
tail -20 src/CarRent.Web/logs/carrent-*.log
```

**API (nakon admin prijave u browseru ili MCP ključ):**

```bash
curl -H "X-Mcp-Key: carrent-mcp-dev-key" \
  "http://localhost:5000/api/logs/recent?count=10"
```

U Playwright E2E testu korak 10 provjerava `logs/recent` → 200 OK.

---

## 5. Moguća pitanja profesora

**P: Gdje se logovi spremaju?**  
O: `logs/carrent-YYYYMMDD.log` u rootu web projekta; konfiguracija u `Program.cs` i `appsettings.json` → `Logging:File:Directory`.

**P: Zašto Serilog, a ne samo ILogger?**  
O: `ILogger` je apstrakcija; Serilog je **implementacija** koja lako piše u datoteku s rotacijom.

**P: Tko smije čitati logove preko API-ja?**  
O: Samo Admin — `[Authorize(Roles = "Admin")]` na `LogsApiController`.

**P: Logirate li lozinke?**  
O: Ne — Identity i naš kod ne logiraju plain lozinke; logira se npr. „Korisnik X prijavljen”.

---

## 6. Što reći na usmenom

> „Koristimo Serilog: svi logovi idu u dnevnu datoteku u folderu logs, a Admin može dohvatiti zadnjih N linija preko REST API-ja `/api/logs/recent`.”

---

## 7. Daljnji koraci — implementacija (opcionalno)

- Seq / Elasticsearch / Azure Monitor kao dodatni sink
- Strukturirani JSON logovi
- Logiranje HTTP zahtjeva (middleware)
- Maskiranje osjetljivih podataka (OIB)

---

## 8. Koraci koje TI moraš poduzeti

| Korak | Obavezno? | Akcija |
|-------|-----------|--------|
| Pokreni app barem jednom | Da | Da se kreira `logs/` mapa |
| Provjeri datoteku | Preporuka | `tail logs/carrent-*.log` |
| Ništa instalirati | — | Serilog je NuGet u `.csproj` |

**Na deployu:** osiguraj da container ima writable `/app/logs` (Dockerfile već `mkdir -p /app/logs`).
