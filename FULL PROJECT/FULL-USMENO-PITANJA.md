# CarRent — USMENA PITANJA I ODGOVORI (profesor → ti)

**Cilj:** Pokriti **40 usmenih bodova** + tehnička pitanja uz demo.  
**Kako učiti:** Pročitaj odgovor naglas, otvori navedenu datoteku u IDE-u, ponovi svojim riječima.

**Legenda:** 📁 = putanja u repou

---

## A. Opća pitanja o projektu

### A1. Što radi tvoja aplikacija?

**O:** Web sustav za **upravljanje najmom vozila** — poslovnice, flota, kupci, rezervacije, servisi, zaposlenici. Admin i Manager koriste MVC sučelje; klijenti mogu koristiti **javni AI asistent** za upit dostupnosti i nacrt rezervacije.

### A2. Koje tehnologije koristiš?

**O:** **ASP.NET Core 10** MVC, **Entity Framework Core**, **SQLite** (dev), **ASP.NET Core Identity**, **Serilog**, **MailKit** (email), **Playwright** (E2E), **Docker**, **Google Cloud Run**. Frontend: Razor, CSS, vanilla JS.

### A3. Kako je organiziran repozitorij?

**O:** Slojevito: `CarRent.Model` (entiteti), `CarRent.DAL` (baza), `CarRent.Web` (MVC+API), `CarRent.Console` (Lab 1 LINQ), testovi u `tests/`. 📁 `README.md`, 📁 `FULL-TUTORIAL-KOMPLET.md` §2

### A4. Koji su glavni entiteti?

**O:** 8 poslovnih: `BranchOffice`, `Vehicle`, `Customer`, `Reservation`, `Addon`, `ServiceRecord`, `Employee`, `Partner`. Plus Identity `AppUser`, `VehicleAttachment`, `FleetNotificationOutbox`. 📁 `Entities.cs`

### A5. Kako korisnik pristupa aplikaciji?

**O:** Browser → Kestrel web server. Lokalno `localhost:5000`, produkcija **Cloud Run HTTPS URL**. Zaštićene stranice zahtijevaju cookie prijave (Identity).

### A6. Gdje je ulazna točka aplikacije?

**O:** 📁 `Program.cs` — konfigurira servise, middleware, rute, migracije pri startu.

### A7. Zašto SQLite?

**O:** Jednostavno za razvoj i predaju — jedna datoteka, bez Docker baze. `Program.cs` automatski migrira i seeda. Produkcija na Cloud Runu također SQLite u containeru (demo); za pravu produkciju Cloud SQL.

### A8. Može li SQL Server?

**O:** Da — `DatabaseProvider: SqlServer` u appsettings + `docker compose up` za SQL Server. 📁 `README.md` § SQL Server

---

## B. Lab 1–3 (model, MVC, EF)

### B1. Što si radio u Lab 1?

**O:** Domenski model i **LINQ** upiti u konzoli — filtriranje vozila, joinovi, grupiranje. Podaci u memoriji prije baze. 📁 `CarRent.Console/`

### B2. Što je MVC?

**O:** **Model** (podaci), **View** (Razor HTML), **Controller** (logika, odabir viewa). Odvojeni slojevi — promjena UI-a ne mijenja direktno bazu.

### B3. Kako radi routing?

**O:** Default `{controller=Home}/{action=Index}/{id?}` + custom rute u `Program.cs`: `/vozni-park`, `/dnevni-plan`, `/raspored`, `/asistent`. 📁 `Program.cs` linije 178–183

### B4. Što je Entity Framework Core?

**O:** **ORM** — mapira C# klase na SQL tablice. Pišem LINQ, EF generira SQL. Migracije verzioniraju shemu baze.

### B5. Što su migracije?

**O:** Datoteke u 📁 `CarRent.DAL/Migrations/` koje opisuju promjene sheme. `db.Database.Migrate()` pri startu primjenjuje nedostajuće. Snapshot: `CarRentDbContextModelSnapshot.cs`.

### B6. Što je DbContext?

**O:** `CarRentDbContext` — jedna klasa s `DbSet<T>` za svaku tablicu. Unit of Work prema bazi. 📁 `CarRent.DAL/CarRentDbContext.cs`

### B7. Što je seed?

**O:** Početni podaci (vozila, korisnici) nakon migracije — `IdentitySeedData.SeedAsync` i EF seed. Da demo odmah ima smisla.

### B8. Razlika mock repozitorija (Lab 2) i EF (Lab 3)?

**O:** Lab 2: podaci u memoriji, gube se pri restartu. Lab 3: **perzistencija** u SQLite — ostaju između pokretanja.

---

## C. Lab 4 (CRUD, AJAX, validacija)

### C1. Kako radi CRUD?

**O:** Za svaki entitet: `Index` (lista), `Create`/`Edit` (forme), `Delete`, `Details`. Controller poziva Repository, mapira Entity ↔ ViewModel. 📁 `EntityCrudControllers.cs`

### C2. Što je AJAX pretraga na listi?

**O:** JS šalje zahtjev na `SearchRows` akciju, server vraća **partial view** samo redova tablice — stranica se ne osvježava cijela. 📁 `wwwroot/js/site.js`, atribut `data-ajax-search`

### C3. Što je autocomplete?

**O:** `LookupApiController` — `GET /api/lookup/customers?q=ana` vraća JSON za dropdown. 📁 `LookupApiController.cs`

### C4. Kako radi validacija?

**O:** **Klijent:** jQuery Validate na formi. **Server:** Data Annotations na ViewModelima + `ModelState.IsValid` u controlleru. Dupla validacija — sigurnost i UX.

### C5. Što je lokalizacija u projektu?

**O:** `hr` i `en-US` — `UseRequestLocalization` u `Program.cs`. Datumi i brojevi po kulturi.

---

## D. Lab 5 — REST API (VRLO BITNO)

### D1. Što je REST API?

**O:** HTTP sučelje koje vraća **JSON** umjesto HTML. Isti backend za web, mobilnu app i **automatske testove**.

### D2. Gdje su API controlleri?

**O:** 📁 `Api/Controllers/EntityApiControllers.cs` — 8 resursa, rute `/api/vehicle`, `/api/customer`, itd.

### D3. Koje HTTP metode koristiš?

**O:** **GET** čitanje, **POST** kreiranje, **PUT** izmjena, **DELETE** brisanje. Svaka ima atribut `[HttpGet]` itd.

### D4. Što je DTO i zašto?

**O:** **Data Transfer Object** — oblik JSON-a odvojen od EF entiteta. Ne šaljem navigacijske kolekcije ni interna polja; stabilan API kad se baza mijenja. 📁 `Api/Dtos/EntityDtos.cs`, `ApiMappers.cs`

### D5. Primjer status koda?

**O:** `200 OK` uspjeh, `201 Created` novi resurs, `400` loš input, `401` nisi prijavljen, `403` nemaš ulogu, `404` ne postoji ID.

### D6. Kako testiraš API?

**O:** xUnit + `WebApplicationFactory` — `HttpClient` šalje zahtjeve na in-memory server. 📁 `EntityApiCrudTests.cs` — 55+ testova.

### D7. Razlika `Controller` i `ControllerBase`?

**O:** MVC `Controller` ima View support. API `ControllerBase` — samo akcije koje vraćaju `IActionResult` / JSON, bez view enginea.

---

## E. Autentikacija i autorizacija

### E1. Što je ASP.NET Core Identity?

**O:** Framework za **korisnike, lozinke, uloge, cookie sesije**. Tablice `AspNetUsers`, `AspNetRoles`… `AppUser` proširuje `IdentityUser` s OIB poljem.

### E2. Koje uloge imaš?

**O:** **Admin** — puni pristup uključujući DELETE. **Manager** — operativa, ograničen DELETE na API-ju.

### E3. Što je FallbackPolicy?

**O:** U `Program.cs` — **svi zahtjevi zahtijevaju prijavu** osim ako metoda nema `[AllowAnonymous]`. Zato `/` redirecta na Login. 📁 `Program.cs` linije 38–42

### E4. Tko može na `/asistent`?

**O:** Svatko — `[AllowAnonymous]` na `PublicAssistantController`.

### E5. Zašto nema Google OAuth?

**O:** Namjerno **preskočeno** — koristimo seed račune email/lozinka. Kod za Google postoji u `Program.cs` ako se postave secrets. 📁 `FULL-11-GOOGLE-OAUTH.md`

### E6. Što je PendingRoleMiddleware?

**O:** Korisnik prijavljen ali bez dodijeljene uloge → stranica čekanja dok Admin ne dodijeli rolu. 📁 `Middleware/PendingRoleMiddleware.cs`

### E7. Seed korisnici?

**O:** `admin@carrent.local` / `Admin123!` (Admin), `manager@carrent.local` / `Manager123!` (Manager).

---

## F. FULL — Global search

### F1. Što je global search?

**O:** Paleta pretrage (**Ctrl+K**) — pretražuje **stranice navigacije** i **podatke u bazi** iz bilo koje stranice. 📁 `GlobalSearchService.cs`

### F2. Koji endpoint?

**O:** `GET /api/search?q=tekst` — zahtijeva prijavu.

### F3. Razlika od AJAX pretrage na listi?

**O:** AJAX filtrira **jednu tablicu** na jednoj stranici. Global search je **aplikacijski** — miješa stranice i entitete.

---

## G. FULL — Logging

### G1. Kako logiraš?

**O:** **Serilog** — u konzolu i u datoteku `logs/carrent-YYYYMMDD.log` (rolling). 📁 `Program.cs` linije 16–26

### G2. API za logove?

**O:** `GET /api/logs/recent?count=5` — zadnjih N linija iz log datoteke (Admin). 📁 `LogsApiController.cs`

### G3. Zašto logging?

**O:** Debug produkcije, audit, dokaz da sustav radi — kriterij FULL projekta.

---

## H. FULL — Responsive

### H1. Kako si napravio mobile?

**O:** CSS media queries + **hamburger** meni (`data-mobile-nav-toggle`). E2E test postavlja viewport 390×844 i provjerava vidljivost. 📁 `site-pages.css`, `FullProjectScenarioTests.cs` korak 13

---

## I. FULL — AI integracija (3 boda — DETALJNO)

### I1. Gdje su dva chata?

**O:** **Klijentski** `/asistent` (javno). **Operativni** `/operativa/ai-asistent` (Admin/Manager).

### I2. Kako AI „razumije” upit?

**O:** Nije magija — **parseri**: `FleetAiIntentParser` (namjera, „da”, kontakt), `FleetAiDateParser` (vikend, 15.6., 7 dana), `FleetAiVehicleMatcher` (golf, prvi, taj model). 📁 `Services/FleetAi*.cs`

### I3. Odakle zna koja su vozila slobodna?

**O:** `FleetAiAvailabilityService` — EF upit: aktivna vozila minus rezervacije koje se preklapaju s terminom. 📁 `FleetAiAvailabilityService.cs`

### I4. Zašto Session?

**O:** HTTP je stateless. **ASP.NET Session** (cookie) drži `FleetClientChatSession` — fazu razgovora, ponuđena vozila, kontakt. Inače „7 dana” nakon „golf” ne bi imalo kontekst. 📁 `FleetClientChatSessionStore.cs`

### I5. Koje su faze razgovora?

**O:** Npr. početak → datumi → izbor vozila → kontakt → potvrda → gotovo. Enum u `FleetClientChatModels.cs`.

### I6. Što se dogodi na „da”?

**O:** `FleetClientReservationSubmissionService` kreira **Reservation** status **Draft**, veže kupca/vozilo, šalje email timu preko outboxa. 📁 `FleetClientReservationSubmissionService.cs`

### I7. Rule-based vs OpenAI?

**O:** **Rule-based** uvijek radi (besplatno). Ako postoji `OpenAI:ApiKey`, `FleetAiOpenAiHelper` može **preformulirati** odgovor — ali činjenice dolaze iz baze/sessiona, ne iz halucinacije.

### I8. Je li to pravi ChatGPT u aplikaciji?

**O:** Hibrid: **deterministička poslovna logika** + opcionalno GPT za prirodniji jezik. Bez ključa — šablonski ali točni odgovori.

### I9. Zašto nema SMS/WhatsApp?

**O:** Kriterij je pokriven **web chatom** i **email obavijestima**. SMS bi trebao Twilio i troškove.

### I10. Kako testiraš AI?

**O:** Integracijski testovi parsera i `AiChatIntegrationTests` — POST na `/asistent/ask`. 📁 `tests/CarRent.Web.IntegrationTests/`

---

## J. FULL — Email obavijesti

### J1. Kako rade obavijesti?

**O:** **Outbox pattern** — poslovna logika samo **zapisuje** poruku u `FleetNotificationOutbox`; slanje je odvojeno.

### J2. Tko šalje email?

**O:** `FleetNotificationOutboxDispatcher` (BackgroundService) svakih ~30s + ručno s `/Notifications`. `SmtpEmailTransport` (MailKit) → Gmail SMTP.

### J3. Kada se kreiraju obavijesti?

**O:** Pri **startu app** (`FleetLifecycleService.SyncAsync`) — registracija ističe, povrat danas, servis sutra… I pri promjeni rezervacije (nacrt ističe, no-show).

### J4. Tko prima email?

**O:** Konfigurirano `DefaultRecipient` (nikola.skeva1@gmail.com za demo). AI rezervacija šalje timu, ne nužno kupcu.

### J5. Gdje vidim status?

**O:** UI **Operativa → Obavijesti** (`/Notifications`) — Čeka slanje / Poslano + `SentAt`.

---

## K. FULL — Push obavijesti

### K1. Imaš li push?

**O:** **Web Push** implementacija — VAPID, Service Worker, pretplata u browseru. 📁 `WebPushTransport.cs`, `FULL-10-PUSH-OBAVESTI.md`

### K2. Razlika email vs push?

**O:** Email — SMTP, radi uvijek. Push — samo ako korisnik dopusti u browseru i ima pretplatu.

---

## L. FULL — MCP i agentic IDE

### L1. Što je MCP?

**O:** **Model Context Protocol** — Cursor (AI IDE) zove **alate** na tvom serveru (npr. dohvat vozila, pitaj asistenta).

### L2. Gdje je MCP server?

**O:** 📁 `src/CarRent.McpServer/` — `CarRentTools.cs`. Konfiguracija 📁 `.cursor/mcp.json`

### L3. Kako API štiti MCP?

**O:** `McpApiKeyMiddleware` — zahtjevi s headerom `X-Mcp-Key` ako je ključ konfiguriran. 📁 `Middleware/McpApiKeyMiddleware.cs`

---

## M. FULL — Deploy i Docker

### M1. Gdje je aplikacija deployana?

**O:** **Google Cloud Run**, projekt `wehr-c55cd` (ime **CarRent Fleet**), regija `europe-west1`. URL: **https://carrent-web-hfcdfitrgq-ew.a.run.app**

### M2. Zašto Docker?

**O:** Isti **image** lokalno i u cloudu — reproducibilan build, nema „kod mi radi samo na laptopu”.

### M3. Što radi Dockerfile?

**O:** Multi-stage: `dotnet publish` u build stage, runtime `aspnet:10.0`, port **8080**, SQLite u `/app/Data/`. 📁 `Dockerfile`

### M4. Što se dogodi pri redeployu na Cloud Run?

**O:** Novi container — **SQLite se resetira**, migracije + seed ponovo. Za trajne podatke treba Cloud SQL.

### M5. Zašto ne Firebase Hosting?

**O:** Firebase Hosting je za **statičke** stranice. Mi imamo **server-side ASP.NET** — treba Kestrel u containeru.

### M6. Koje skripte koristiš?

**O:** `run-local.sh`, `run-docker-local.sh`, `setup-gcp-deploy.sh`, `deploy-gcp.sh`, `install-gcloud.sh`.

---

## N. FULL — Playwright E2E

### N1. Što je Playwright?

**O:** Automatizira **pravi browser** — klik, tipkanje, provjera teksta. E2E = cijeli scenarij od logina do odjave.

### N2. Koliko koraka ima test?

**O:** **13 koraka** — login, Addon CRUD preko API, AJAX, global search, AI chat, logs, logout, Manager bez delete, mobile viewport. 📁 `FullProjectScenarioTests.cs`

### N3. Zašto E2E ako imaš integracijske?

**O:** Integracijski testiraju **API/servise** bez UI-a. Playwright testira **cijelo iskustvo** korisnika — kriterij PDF-a.

### N4. Kako pokrenuti?

**O:** `./scripts/run-e2e.sh` — podigne app i pokrene Playwright.

---

## O. Timeline i operativa

### O1. Što je Timeline?

**O:** Mjesečni **raspored rezervacija** po vozilima — vizualni prikaz zauzetosti. Ruta `/raspored`. 📁 `TimelineController`, `TimelineApiController`

### O2. Što je Dnevni plan?

**O:** Operativni pregled današnjih izdavanja/povrata. `/dnevni-plan`

### O3. Što je Fleet lifecycle?

**O:** Automatska pravila — nacrt ističe, no-show, kilometraža, obavijesti. `FleetLifecycleService` + `FleetLifecycleRules`. 📁 `Services/FleetLifecycleService.cs`

---

## P. Testiranje i kvaliteta

### P1. Koliko imaš testova?

**O:** **55+** integracijskih (xUnit) + **13** E2E koraka (Playwright) + unit testovi parsera/timeline helpera.

### P2. Što je WebApplicationFactory?

**O:** Podigne testnu instancu appa u memoriji — brzi integracijski testovi bez ručnog `dotnet run`. 📁 `CarRentWebApplicationFactory.cs`

### P3. Kako dokazuješ da CRUD radi?

**O:** `EntityApiCrudTests` — CREATE, READ, UPDATE, DELETE za entitete + 401/403 scenariji.

---

## Q. Teorija s predavanja (brzi flashcards)

| Pojam | Jedna rečenica |
|-------|----------------|
| **ORM** | Mapiranje objekata na tablice — EF Core |
| **Migracija** | Verzionirana promjena sheme baze |
| **Repository** | Sloj između controllera i DbContexta |
| **DTO** | Oblik podataka za API, ne entitet |
| **Middleware** | Pipeline obrade zahtjeva prije controllera |
| **Dependency Injection** | `AddScoped` u Program.cs — servisi u konstruktoru |
| **Cookie auth** | Identity sprema sesiju u encrypted cookie |
| **BackgroundService** | Duga pozadinska radnja (email dispatcher) |
| **Outbox** | Zapis poruke prije slanja — pouzdanost |
| **Stateless HTTP** | Session/cookie dodaje stanje |

---

## R. „Zamka” pitanja — kako odgovoriti

### R1. „Nisi li samo copy-pasteao AI kod?”

**O:** Mogu pokazati **tok u kodu** — controller → service → EF → test. Mogu objasniti zašto je `OrderBy` prije `Select` u availability servisu (EF prijevod). Mogu pokrenuti testove uživo.

### R2. „Što bi promijenio za produkciju?”

**O:** Cloud SQL umjesto SQLite, Secret Manager za SMTP/OpenAI, HTTPS strogo, jači MCP ključ, rate limiting na `/asistent`, Redis za session ako skaliram.

### R3. „Gdje je sigurnost slaba?”

**O:** Demo seed lozinke, MCP dev ključ, SQLite u containeru, anoniman AI chat (namjerno za demo). Identity i role na ostatku appa.

### R4. „Objasni jedan bug koji si imao.”

**O:** EF nije mogao prevesti `OrderBy` nakon `Select` na `FleetVehicleSnapshot` — riješeno sortiranjem po `v.Brand` prije projekcije. Migracija push tablice bez snapshot ažuriranja — crash pri startu — dodana migracija.

---

## S. Minimalni set za 35 bodova (ako paničiš)

Znaš napamet:

1. **Pokretanje:** `./scripts/run-local.sh` → :5000  
2. **Login:** admin@carrent.local / Admin123!  
3. **3 entiteta:** Vozilo, Kupac, Rezervacija  
4. **API:** GET `/api/vehicle` vraća JSON  
5. **Identity:** Admin vs Manager  
6. **AI:** `/asistent` → rezervacija u bazu  
7. **Cloud URL:** https://carrent-web-hfcdfitrgq-ew.a.run.app  
8. **Test:** `dotnet test tests/CarRent.Web.IntegrationTests/`

---

## T. Poveznice na detaljne reporte

| Tema | Datoteka |
|------|----------|
| Cijeli tutorijal | `FULL-TUTORIAL-KOMPLET.md` |
| Lab 5 dubinski | `lab-5/LAB5-PUNI-PREGLED.md` |
| AI | `FULL-05-AI-KLIJENTSKI-CHAT.md` |
| Deploy | `FULL-07-DEPLOY-CLOUD.md` |
| Email | `FULL-09-EMAIL-OBAVESTI.md` |
| E2E | `FULL-01-PLAYWRIGHT-E2E.md` |
| API testovi | `FULL-08-API-INTEGRACIJSKI-TESTOVI.md` |
| Sažetak bodova | `FULL-REPORT.md` |

---

*Preporuka: 1 sat — pročitaj sekcije A, D, E, I, M. 1 sat — demo §7 iz tutorijala s otvorenim kodom. 30 min — sekcija S ako trebaš sigurnosnu mrežu za 35 bodova.*
