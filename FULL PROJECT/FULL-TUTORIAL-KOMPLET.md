# CarRent — KOMPLETNI TUTORIJAL (znaš projekt kao da si ga ti pisao)

**Autor pripreme:** Nikola Skeva · **Datum:** 2026-07-10  
**Cilj:** Znati pokrenuti, pokazati i objasniti cijeli projekt — minimum **35/70** bodova, cilj **63+/70** (ocjena 5).

---

## Sadržaj

1. [Što je CarRent u jednoj rečenici](#1-što-je-carrent-u-jednoj-rečenici)
2. [Arhitektura repozitorija](#2-arhitektura-repozitorija)
3. [8 entiteta i poslovni domen](#3-8-entiteta-i-poslovni-domen)
4. [Lab 1 → Lab 5 — evolucija](#4-lab-1--lab-5--evolucija)
5. [FULL projekt — što smo dodali](#5-full-projekt--što-smo-dodali)
6. [Pokretanje — sve načine](#6-pokretanje--sve-načine)
7. [Demo scenarij 15 min (za profesora)](#7-demo-scenarij-15-min-za-profesora)
8. [Mapa važnih datoteka](#8-mapa-važnih-datoteka)
9. [Konfiguracija i tajne](#9-konfiguracija-i-tajne)
10. [Testiranje](#10-testiranje)
11. [Strategija bodova — kako doći do 35+](#11-strategija-bodova--kako-doći-do-35)
12. [Indeks dokumentacije](#12-indeks-dokumentacije)

---

## 1. Što je CarRent u jednoj rečenici

**CarRent** je web aplikacija za **upravljanje flotom vozila za najam** — poslovnice, vozila, kupci, rezervacije, servisi, zaposlenici i partneri — s **MVC sučeljem**, **REST API-jem**, **autentikacijom po ulogama**, **AI asistentom za klijente i operativu**, **email obavijestima** i **deployem na Google Cloud Run**.

**Domena:** tvrtka koja iznajmljuje automobile (npr. u Splitu/Zagrebu). Admin vodi sve; Manager radi operativu bez brisanja kritičnih stvari.

---

## 2. Arhitektura repozitorija

```
CarRent-System/
├── src/
│   ├── CarRent.Console/     ← Lab 1: LINQ demo, seed podaci
│   ├── CarRent.Model/       ← Lab 3: EF entiteti (POCO klase)
│   ├── CarRent.DAL/         ← Lab 3: DbContext, migracije, seed
│   ├── CarRent.Web/         ← Lab 2–5 + FULL: MVC + API + servisi
│   └── CarRent.McpServer/   ← FULL: MCP alati za Cursor IDE
├── tests/
│   ├── CarRent.Web.IntegrationTests/  ← xUnit API + servisi
│   └── CarRent.Web.E2E/               ← Playwright 13 koraka
├── lab-1/ … lab-5/          ← reporti laboratorijskih vježbi
├── FULL PROJECT/            ← reporti FULL kriterija + ovaj tutorijal
├── scripts/                 ← run-local, deploy, gmail, gcloud
├── Dockerfile               ← Cloud Run / lokalni Docker
└── design/                  ← UI handoff (glassmorphism)
```

### Slojevi (što reći na usmenom)


| Sloj             | Što radi                              | Tehnologija                        |
| ---------------- | ------------------------------------- | ---------------------------------- |
| **Presentation** | HTML stranice, forme, JS              | ASP.NET Core MVC, Razor            |
| **API**          | JSON za testove / vanjske klijente    | Web API controllers                |
| **Business**     | Validacija, lifecycle, AI, obavijesti | `Services/`                        |
| **Data access**  | CRUD prema bazi                       | EF Core + `Repositories/`          |
| **Persistence**  | Tablice, migracije                    | SQLite (dev) / SQL Server (opcija) |


**Tok jednog HTTP zahtjeva (MVC):**

```
Browser → Kestrel → Middleware (auth, lifecycle) → Controller → Repository/Service → DbContext → SQLite
                                                                              ↓
                                                                         View (Razor) → HTML
```

**Tok API zahtjeva:**

```
HttpClient/curl → ControllerBase → Repository → DTO mapper → JSON (200/404/403)
```

---

## 3. 8 entiteta i poslovni domen

Svi su u `src/CarRent.Model/Entities/Entities.cs` (+ `AppUser`, `VehicleAttachment`, `FleetNotificationOutbox`).


| Entitet         | Hrvatski       | Što predstavlja                         |
| --------------- | -------------- | --------------------------------------- |
| `BranchOffice`  | Poslovnica     | Lokacija izdavanja vozila               |
| `Vehicle`       | Vozilo         | Auto u floti (marka, model, cijena/dan) |
| `Customer`      | Kupac          | Osoba koja najmljuje                    |
| `Reservation`   | Rezervacija    | Ugovor najma (datumi, status)           |
| `Addon`         | Dodatak        | GPS, dječje sjedalo…                    |
| `ServiceRecord` | Servisni zapis | Održavanje vozila                       |
| `Employee`      | Zaposlenik     | Osoblje poslovnice                      |
| `Partner`       | Partner        | Vanjski suradnik (npr. osiguranje)      |


**Veze (EF):** Vozilo pripada poslovnici; rezervacija povezuje kupca + vozilo; rezervacija ima N dodataka (`ReservationAddon`).

**Statusi rezervacije** (bitno za AI i lifecycle): `Draft`, `Confirmed`, `Active`, `Completed`, `Cancelled`, `NoShow`…

---

## 4. Lab 1 → Lab 5 — evolucija

### Lab 1 — Model i LINQ (`CarRent.Console`)

- Definirani entiteti i seed podaci u memoriji
- LINQ upiti: filtriranje vozila, grupiranje, joinovi
- **Dokumentacija:** `lab-1/`, `CarRent.Console/Services/DemoQueries.cs`

### Lab 2 — MVC + HTML Binding (`CarRent.Web`)

- Routing `{controller}/{action}/{id}`
- Mock repozitoriji (prije baze)
- Index + Details za sve entitete
- Custom stranice: Timeline, Dnevni plan, Vozni park
- **Dokumentacija:** `lab2/LAB2-Report.md`

### Lab 3 — Entity Framework

- Prijelaz s mocka na **EF Core + SQLite**
- Migracije u `CarRent.DAL/Migrations/`
- `Program.cs` → `Migrate()` + seed pri startu
- Custom rute: `/vozni-park`, `/dnevni-plan`, `/raspored`
- **Dokumentacija:** `lab-3/LAB3-Report.md`, `lab-3/semantic-model.md`

### Lab 4 — CRUD + AJAX + validacija

- Puni CRUD (Create/Edit/Delete) za sve entitete
- AJAX pretraga na listama (`data-ajax-search`)
- Autocomplete (`LookupApiController`)
- jQuery Validate + custom datum kontrola
- Lokalizacija `hr` / `en-US`
- **Dokumentacija:** `lab-4/LAB4-Report.md`

### Lab 5 — API + Identity + Upload + testovi

- REST API `/api/{entitet}` — GET/POST/PUT/DELETE + DTO
- **ASP.NET Core Identity** — `AppUser`, role `Admin` / `Manager`
- **FallbackPolicy** — sve zahtijeva prijavu osim `[AllowAnonymous]`
- Dropzone upload na Vehicle Edit
- 55+ integracijskih testova
- **Dokumentacija:** `lab-5/LAB5-PUNI-PREGLED.md` ← **GLAVNI ZA USMENO Lab 5**

---

## 5. FULL projekt — što smo dodali


| #   | Značajka                      | Bodovi       | URL / kako pokazati                   | MD report |
| --- | ----------------------------- | ------------ | ------------------------------------- | --------- |
| F01 | Playwright E2E 13 koraka      | 2 (+3 bonus) | `./scripts/run-e2e.sh`                | FULL-01   |
| F02 | Global search Ctrl+K          | 2            | Prijava → Ctrl+K → „vozilo”           | FULL-02   |
| F03 | Serilog + `/api/logs/recent`  | 2            | Terminal curl nakon prijave           | FULL-03   |
| F04 | Responsive mobile             | 2            | F12 → 390px → hamburger               | FULL-04   |
| F05 | AI chat (klijent + operativa) | 3            | `/asistent`, `/operativa/ai-asistent` | FULL-05   |
| F06 | MCP server + Cursor           | 2            | `.cursor/mcp.json`                    | FULL-06   |
| F07 | Cloud deploy                  | 3            | Javni URL (dolje)                     | FULL-07   |
| F08 | CRUD + API testovi            | 2            | `dotnet test`                         | FULL-08   |
| —   | Email obavijesti (Gmail)      | dojam        | `/Notifications`                      | FULL-09   |
| —   | Web Push                      | dojam        | pretplata u browseru                  | FULL-10   |
| —   | Timeline API                  | dojam        | `/raspored`                           | FULL-12   |


### Javni deploy (Cloud Run)


| Polje           | Vrijednost                                                                                                   |
| --------------- | ------------------------------------------------------------------------------------------------------------ |
| **URL**         | [https://carrent-web-hfcdfitrgq-ew.a.run.app](https://carrent-web-hfcdfitrgq-ew.a.run.app)                   |
| **GCP projekt** | `wehr-c55cd` (prikazno ime: **CarRent Fleet**)                                                               |
| **Regija**      | `europe-west1`                                                                                               |
| **AI asistent** | [https://carrent-web-hfcdfitrgq-ew.a.run.app/asistent](https://carrent-web-hfcdfitrgq-ew.a.run.app/asistent) |


### Seed korisnici (lokalno i cloud)


| Email                   | Lozinka       | Uloga   |
| ----------------------- | ------------- | ------- |
| `admin@carrent.local`   | `Admin123!`   | Admin   |
| `manager@carrent.local` | `Manager123!` | Manager |


### AI chat — kratko objašnjenje (moraš znati napamet)

1. **Klijentski** `/asistent` — javno, bez prijave
2. Korisnik piše prirodnim jezikom → parser datuma/namjere → provjera baze (slobodna vozila)
3. **Session** pamti korake (datumi → izbor vozila → kontakt → potvrda)
4. Na „da” → `FleetClientReservationSubmissionService` kreira **Draft** rezervaciju + email timu
5. **Operativni** chat — samo Admin/Manager, kontekst flote i rasporeda
6. **OpenAI** opcionalno (`OpenAI:ApiKey`); inače rule-based odgovori

### Email obavijesti — kratko

- Lifecycle pri startu i na promjenama generira poruke → **outbox** tablica
- Background worker svakih 30s šalje preko **Gmail SMTP** (MailKit)
- UI: **Operativa → Obavijesti** (`/Notifications`)

---

## 6. Pokretanje — sve načine

### A) Lokalno (najčešće za demo)

```bash
cd /home/nskeva/Documents/Github/CarRent-System
./scripts/run-local.sh
```

→ [http://localhost:5000](http://localhost:5000)  
Baza: `src/CarRent.Web/Data/carrent.dev.db` (automatski migrate + seed)

### B) Ručno dotnet

```bash
dotnet run --project src/CarRent.Web/CarRent.Web.csproj
```

### C) Docker lokalno

```bash
./scripts/run-docker-local.sh
# ili: docker run -p 8080:8080 carrent-local:latest
```

→ [http://localhost:8080](http://localhost:8080)

### D) Cloud (već deployano)

Otvori: [https://carrent-web-hfcdfitrgq-ew.a.run.app](https://carrent-web-hfcdfitrgq-ew.a.run.app)

### E) Gmail SMTP (email obavijesti lokalno)

```bash
./scripts/setup-gmail-secrets.sh
# unesi app password za Gmail
./scripts/run-local.sh
```

### F) OpenAI (opcionalno, prirodniji AI ton)

```bash
./scripts/setup-openai-secrets.sh
```

---

## 7. Demo scenarij 15 min (za profesora)

**Priprema prije ulaska:** `./scripts/run-local.sh` ili otvori cloud URL.

### Minuta 0–2: Uvod

> „CarRent je MVC aplikacija za najam vozila. Koristim EF Core, SQLite, Identity s ulogama Admin i Manager, REST API za sve entitete, i FULL nadogradnje: global search, logging, AI chat, email obavijesti i deploy na Google Cloud Run.”

### Minuta 2–4: Prijava i navigacija

1. Otvori `/` → redirect na Login (FallbackPolicy)
2. Prijava `admin@carrent.local` / `Admin123!`
3. Pokaži **Operativa** sidebar: Početna, Raspored, Dnevni plan, Obavijesti
4. **Podaci** dropdown: Vozila, Kupci, Rezervacije…

### Minuta 4–6: Global search + CRUD

1. **Ctrl+K** → upiši „golf” ili „rezervacija” → klik na rezultat
2. Otvori `/Vehicle` → Details jednog vozila
3. (Opcionalno) `/Addon` → pokaži AJAX pretragu

### Minuta 6–9: AI asistent (klijentski)

1. Nova kartica: `/asistent` (javno, bez prijave)
2. Poruka: **„Trebam auto ovaj vikend”**
3. Zatim: **„Golf 7 dana”** ili **„prvi”**
4. Daj kontakt: ime, email, mobitel
5. **„da”** → potvrda rezervacije
6. Admin: `/Reservation` → filtriraj **Nacrt** — vidiš novu rezervaciju
7. Spomeni: email stiže na `nikola.skeva1@gmail.com`

### Minuta 9–11: Operativa + obavijesti

1. `/operativa/ai-asistent` → „Što je danas na rasporedu?”
2. `/Notifications` → tablica outboxa, status Poslano/Čeka

### Minuta 11–13: API + testovi + logging

Terminal (dok app radi):

```bash
# API (s cookie ili u browseru dok si prijavljen)
curl -s http://localhost:5000/api/vehicle | head -c 200

# MCP ključ za API bez cookie (vozila)
curl -s -H "X-Mcp-Key: carrent-mcp-dev-key" http://localhost:5000/api/vehicle | head -c 200

# Logovi (treba auth cookie — ili pokaži u Playwright testu)
curl -s "http://localhost:5000/api/logs/recent?count=3"
```

```bash
dotnet test tests/CarRent.Web.IntegrationTests/
./scripts/run-e2e.sh
```

### Minuta 13–15: Mobile + Cloud + MCP

1. F12 → viewport **390×844** → hamburger meni
2. Otvori **cloud URL** — ista aplikacija
3. Cursor → Settings → MCP → `carrent` server (FULL-06)

**Završna rečenica:**

> „Arhitektura je slojevita: MVC za ljude, API za strojno testiranje, servisi za poslovnu logiku, EF za perzistenciju. AI ne izmišlja podatke — čita iz baze. Obavijesti idu kroz outbox pattern. Deploy je Docker + Cloud Run.”

---

## 8. Mapa važnih datoteka


| Što tražiš              | Gdje                                                                 |
| ----------------------- | -------------------------------------------------------------------- |
| Startup, DI, middleware | `src/CarRent.Web/Program.cs`                                         |
| Entiteti                | `src/CarRent.Model/Entities/Entities.cs`                             |
| DbContext               | `src/CarRent.DAL/CarRentDbContext.cs`                                |
| API CRUD                | `src/CarRent.Web/Api/Controllers/EntityApiControllers.cs`            |
| DTO + mapperi           | `src/CarRent.Web/Api/Dtos/`, `Api/Mappers/ApiMappers.cs`             |
| Identity login          | `src/CarRent.Web/Areas/Identity/Pages/Account/`                      |
| Global search           | `Services/GlobalSearchService.cs`                                    |
| AI klijent              | `Services/AiClientChatService.cs`, `FleetClientChatConversation.cs`  |
| AI operativa            | `Services/AiOperatorChatService.cs`                                  |
| Email SMTP              | `Services/Notifications/SmtpEmailTransport.cs`                       |
| Outbox worker           | `Services/Notifications/FleetNotificationOutboxDispatcher.cs`        |
| MCP alati               | `src/CarRent.McpServer/CarRentTools.cs`                              |
| E2E test                | `tests/CarRent.Web.E2E/FullProjectScenarioTests.cs`                  |
| Layout + navigacija     | `Views/Shared/_Layout.cshtml`                                        |
| Chat UI                 | `Views/Shared/_FleetChatPanel.cshtml`, `wwwroot/js/fleet-ai-chat.js` |


---

## 9. Konfiguracija i tajne


| Postavka      | Gdje                           | Napomena                    |
| ------------- | ------------------------------ | --------------------------- |
| SQLite put    | auto u `Program.cs`            | `Data/carrent.dev.db`       |
| Gmail SMTP    | user-secrets / env             | `FleetNotifications:Smtp:`* |
| OpenAI        | user-secrets                   | `OpenAI:ApiKey`             |
| MCP API ključ | `appsettings.Development.json` | `Mcp:ApiKey`                |
| Google OAuth  | user-secrets                   | **preskočeno** u projektu   |


**Nikad u git:** lozinke, API ključevi → `dotnet user-secrets` ili Cloud Run env.

---

## 10. Testiranje

```bash
# Integracijski (API, AI parseri, lifecycle, email dispatch)
dotnet test tests/CarRent.Web.IntegrationTests/

# E2E Playwright (13 koraka — prvi put instalira Chromium)
./scripts/run-e2e.sh

# Docker
./scripts/run-docker-local.sh
curl -I http://localhost:8080/asistent
```

**E2E koraci (13):** login → Addon CRUD API → AJAX search → global search → AI chat → logs API → logout → Manager bez delete → mobile viewport.

---

## 11. Strategija bodova — kako doći do 35+

### Bodovi (ukupno 70)


| Kategorija             | Bodovi | Tvoja strategija                                            |
| ---------------------- | ------ | ----------------------------------------------------------- |
| **Usmeno**             | 40     | Uči `FULL-USMENO-PITANJA.md` + `lab-5/LAB5-PUNI-PREGLED.md` |
| **Tehnički u kodu**    | ~20    | Sve implementirano — pokaži uživo                           |
| **Dojam / stabilnost** | 12     | Demo bez crasha, cloud URL radi                             |


**Minimum 35 bodova** = ~50% — dovoljno ako pokažeš **3–4 stvari sigurno** + osnovno razumijevanje:

1. ✅ Pokreneš app (`./scripts/run-local.sh`)
2. ✅ Prijava + jedan CRUD ili lista
3. ✅ AI `/asistent` — jedan razgovor do rezervacije
4. ✅ Spomeneš API (`/api/vehicle`) i testove (`dotnet test`)
5. ✅ Cloud URL otvoriš u browseru

**Za 63+ (ocjena 5):** usmeno moraš znati REST, DTO, Identity, EF, AI tok, outbox, Docker — vidi `FULL-USMENO-PITANJA.md`.

### Što NE raditi na usmenom

- Ne tvrdi da si implementirao Google OAuth (preskočeno)
- Ne obećavaj WhatsApp/SMS (nije u projektu)
- Ne kaži da SQLite na cloudu traje zauvijek (ephemeral container)

---

## 12. Indeks dokumentacije


| Datoteka                       | Za što                       |
| ------------------------------ | ---------------------------- |
| **FULL-TUTORIAL-KOMPLET.md**   | ← ovaj dokument              |
| **FULL-USMENO-PITANJA.md**     | Pitanja profesora + odgovori |
| **FULL-REPORT.md**             | Sažetak bodova i checklist   |
| **FULL-00-MASTER-PLAN.md**     | Redoslijed koraka            |
| **FULL-01 … FULL-12**          | Pojedinačni kriteriji        |
| **lab-5/LAB5-PUNI-PREGLED.md** | Lab 5 usmeno                 |
| **lab-2 … lab-4/**             | Laboratorijski reporti       |


---

*Uči ovaj dokument 2–3 puta pročitaj + jednom prođi demo scenarij §7 s otvorenim kodom u IDE-u. Tada znaš projekt kao da si ga pisao.*