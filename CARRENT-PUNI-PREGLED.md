# CarRent Control Surface — puni pregled dizajna i funkcionalnosti

Jedan dokument: što aplikacija radi, kako izgleda, gdje je u kodu i koje su sve mogućnosti.

**Verzija koda:** Lab 1–4 implementirano u `src/CarRent.Web`  
**Dizajn izvor:** `design/` + handoff `design/handoff/` (Open Design → `glass.css`)

---

## 1. Što je aplikacija

**CarRent Control Surface** je web aplikacija za upravljanje rent-a-car flotom:

- operativni pregled (dashboard, timeline, dnevni plan, vozni park)
- upravljanje podacima (rezervacije, vozila, kupci, servisi, poslovnice, dodaci, zaposlenici, partneri)
- puni **CRUD** s AJAX pretragom, autocompleteom, validacijom i custom datumskom kontrolom

**Tehnologija:** ASP.NET Core MVC (.NET 10), Entity Framework Core, SQLite (default) ili SQL Server (Docker).

**Pokretanje:**

```bash
./scripts/run-local.sh
# → http://localhost:5000
```

---

## 2. Arhitektura projekta


| Sloj         | Putanja                     | Uloga                                 |
| ------------ | --------------------------- | ------------------------------------- |
| **Model**    | `src/CarRent.Model/`        | EF entiteti, enumi, anotacije         |
| **DAL**      | `src/CarRent.DAL/`          | `CarRentDbContext`, migracije, seed   |
| **Web**      | `src/CarRent.Web/`          | MVC UI, kontroleri, viewovi, JS/CSS   |
| **Console**  | `src/CarRent.Console/`      | Lab 1 demo (nije web app)             |
| **Dizajn**   | `design/`                   | Tokeni, inventar ekrana, HTML mockupi |
| **Lab docs** | `lab2/`, `lab-3/`, `lab-4/` | Upute, reporti, AI logovi             |


### Backend vs frontend (u jednoj app)


| Backend (C# / server)                         | Frontend (browser)                   |
| --------------------------------------------- | ------------------------------------ |
| `Controllers/`, `Repositories/`, `Program.cs` | `Views/*.cshtml` (HTML)              |
| `LookupApiController` (JSON API)              | `wwwroot/css/`, `wwwroot/js/site.js` |
| EF + SQLite/SQL Server                        | Animacije, AJAX, datum picker        |


---

## 3. Dizajn sustav — „Glassmorphism Control Surface”

### 3.1 Vizualni identitet

- **Tamna pozadina** s plavo-ljubičastim gradijentom (`#0B1020`, radial `#1F3164`)
- **Stakleni paneli** — `backdrop-filter: blur`, poluprozirne plohe, tanki rubovi
- **Akcenti:** ljubičasta `#8A7DFF`, cyan `#3FD0FF`
- **Status boje:** success / warning / danger + chip varijante za rezervacije i servise
- **Tipografija:** Inter (+ system fallback), jasna hijerarhija (hero, KPI, tablice)

Detaljni tokeni: `design/design-tokens.md`

### 3.2 CSS datoteke (redoslijed učitavanja)

```text
wwwroot/css/site.css          → @import glass + compat + pages
wwwroot/css/glass.css         → glavni dizajn sustav (iz design/handoff)
wwwroot/css/site-compat.css   → Lab 4 kompatibilnost (tablice, autocomplete, kalendar)
wwwroot/css/site-pages.css    → specifične stranice (timeline, fleet, vehicle hero)
```

### 3.3 Ključne UI komponente


| Komponenta       | CSS klase / partial                            | Gdje se koristi                   |
| ---------------- | ---------------------------------------------- | --------------------------------- |
| Globalni shell   | `.page-shell`, `.site-header`                  | `_Layout.cshtml`                  |
| Stakleni panel   | `.glass`, `.glass-card`, `.panel`              | Dashboard, forme, detalji         |
| Hero blok        | `.hero-panel`, `.hero-title`                   | Početna, Vehicle Details          |
| KPI kartice      | `.kpi-grid`, `.kpi-card`                       | Početna                           |
| Tablica podataka | `.data-table`, `.table-wrap`                   | Svi Index CRUD                    |
| Status chip      | `_StatusChip.cshtml`                           | Rezervacije, servisi              |
| CRUD toolbar     | `_CrudIndexHeaderPartial`                      | Liste + AJAX pretraga             |
| Autocomplete     | `_AutocompletePartial`                         | Forme (kupac, vozilo, poslovnica) |
| Datum+vrijeme    | `_DateTimePartial`                             | Forme s datumima                  |
| Breadcrumbs      | `_Breadcrumbs`                                 | Ispod headera                     |
| Flash poruke     | `_FlashMessages`                               | Nakon Spremi/Delete/greške        |
| Fleet kartica    | `.fleet-card`, `.fleet-grid`                   | Vozni park                        |
| Timeline Gantt   | `.timeline-*` (site-pages)                     | Raspored                          |
| Gumbi            | `.btn-primary`, `.btn-secondary`, `.btn-ghost` | Cijela app                        |
| Nav dropdown     | `.nav-group`, `.nav-dropdown`                  | Operativa / Podaci                |
| User chip        | `.user-chip`                                   | Header (dekorativno)              |


### 3.4 Animacije (JavaScript + CSS)

- `**rise-in**` — ulaz kartica/panela pri loadu (`site.js` → `initCardAnimations`)
- `**fade-in**` — nakon AJAX zamjene redova tablice
- **Nav dropdown** — hover/focus s delay ~420 ms (`initNavDropdowns`)
- **Print** — `@media print` na dnevnom planu (A4, bez blur gdje treba)

### 3.5 Responzivnost

Breakpointi (iz design plana):

- **< 640px** — mobilni layout
- **640–980px** — tablet
- **> 980px** — desktop, max širina ~1360px

Handoff mockupi za provjeru: `design/handoff/*.html`

### 3.6 Design handoff (referenca)


| Datoteka                                | Sadržaj                                        |
| --------------------------------------- | ---------------------------------------------- |
| `design/handoff/dashboard.html`         | Početna / KPI                                  |
| `design/handoff/timeline.html`          | Mjesečni raspored                              |
| `design/handoff/dnevni-plan.html`       | Odlasci / povrati                              |
| `design/handoff/vozni-park.html`        | Kartice vozila                                 |
| `design/handoff/rezervacije.html`       | Lista rezervacija                              |
| `design/handoff/rezervacije-forma.html` | Forma rezervacije                              |
| `design/handoff/vozilo-detalji.html`    | Detalj vozila + tabovi                         |
| `design/handoff/component-library.html` | Komponente                                     |
| `design/handoff/glass.css`              | Izvorni CSS (kopija u `wwwroot/css/glass.css`) |
| `design/END-GOAL-Design-Plan.md`        | Dugoročni plan                                 |
| `design/screen-inventory.md`            | Inventar svih ekrana (S01–S22)                 |


---

## 4. Navigacija i globalni layout

### 4.1 Header (`Views/Shared/_Layout.cshtml`)

**Logo** → Početna (`Home/Index`)

**Operativa** (dropdown):


| Link        | Controller | URL                                      |
| ----------- | ---------- | ---------------------------------------- |
| Početna     | Home       | `/`, `/pocetna`                          |
| Timeline    | Timeline   | `/raspored`, `/raspored/mjesecni`        |
| Dnevni plan | DailyPlan  | `/dnevni-plan`, `/operativa/dnevni-plan` |
| Vozni park  | Fleet      | `/vozni-park`, `/Fleet`                  |


**Podaci** (dropdown):


| Link        | Controller             |
| ----------- | ---------------------- |
| Rezervacije | Reservation            |
| Vozila      | Vehicle                |
| Kupci       | Customer               |
| Servisi     | ServiceRecord          |
| Poslovnice  | BranchOffice           |
| Dodaci      | Addon                  |
| Zaposlenici | Employee               |
| Partneri    | Partners (`/partneri`) |


**Desno:** HR jezik (dekorativno), user chip „Operater”.

### 4.2 Ostalo na svakoj stranici

- Breadcrumbs (`ViewData["Breadcrumbs"]`)
- Flash (`TempData` success/error)
- Footer: „CarRent Control Surface · ASP.NET Core MVC”
- Skripte: jQuery Validate + `site.js`

---

## 5. Svi ekrani — URL, dizajn, funkcionalnost

### 5.1 Operativa

#### S01 — Početna / Dashboard

- **URL:** `/`, `/pocetna`, `/Home`
- **View:** `Views/Home/Index.cshtml`
- **Dizajn:** Hero „Fleet Pulse”, 4 KPI kartice, lista današnjih odlazaka/povrata
- **Funkcionalnost:**
  - KPI: ukupno vozila, aktivne rezervacije, broj poslovnica, servisi u 30 dana
  - Klik na KPI vodi na Fleet / Rezervacije / Poslovnice / Servise
  - Brzi link: Nova rezervacija, Dnevni plan
  - Podaci: `DashboardRepository.BuildHomeVmAsync()`

#### S02 — Timeline (mjesečni raspored)

- **URL:** `/raspored`, `/Timeline`, `/raspored/mjesecni`
- **View:** `Views/Timeline/Index.cshtml`
- **Dizajn:** Toolbar filteri + Gantt-lite trake (red = vozilo, stupci = dani)
- **Funkcionalnost:**
  - Filter: tekst, tip vozila, status rezervacije, mjesec
  - Boje traka po statusu rezervacije (chip klase)
  - Layout: `TimelineLayoutHelper` + `DashboardRepository`

#### S03 — Dnevni plan

- **URL:** `/dnevni-plan`, `/operativa/dnevni-plan`
- **View:** `Views/DailyPlan/Index.cshtml`
- **Dizajn:** Dva stupca — povrati danas / odlasci danas
- **Funkcionalnost:**
  - Odabir dana (query `?day=`)
  - Print friendly (`window.print`, print CSS)
  - Podaci filtrirani po datumu start/end rezervacije

#### S04 — Vozni park

- **URL:** `/vozni-park`, `/Fleet`
- **View:** `Views/Fleet/Index.cshtml`
- **Dizajn:** Grid kartica vozila (slika placeholder, registracija, cijena, status)
- **Funkcionalnost:**
  - Link na detalje / uređivanje
  - Akcijski gumbi (priprema za budući workflow)
  - `DashboardRepository.BuildFleetCardsAsync()`

---

### 5.2 CRUD entiteti — zajednički obrazac

Svaki entitet (osim Partnera koji ima custom rute) ima:


| Akcija         | Tip            | Opis                                           |
| -------------- | -------------- | ---------------------------------------------- |
| **Index**      | Puna stranica  | Tablica + AJAX pretraga + gumb Create          |
| **SearchRows** | AJAX partial   | Vraća samo `<tr>` redove (`_IndexRows.cshtml`) |
| **Details**    | Puna stranica  | Pregled jednog zapisa                          |
| **Create**     | GET/POST forma | Novi zapis                                     |
| **Edit**       | GET/POST forma | Uređivanje                                     |
| **Delete**     | POST           | Brisanje (+ anti-forgery)                      |


**AJAX pretraga na listi:** polje „AJAX pretraga…” → `site.js` → `fetch(.../SearchRows?q=)` → zamjena `tbody`.

**Kontroleri:** `Controllers/EntityCrudControllers.cs` (+ `PartnersController` u `Controllers.cs`).

---

### 5.3 Entiteti — detaljno

#### Poslovnice (BranchOffice)


| Stranica | URL                                 |
| -------- | ----------------------------------- |
| Lista    | `/BranchOffice`                     |
| CRUD     | `/BranchOffice/Create`, `Edit/{id}` |
| Detalji  | `/BranchOffice/Details/{id}`        |
| AJAX     | `/BranchOffice/SearchRows`          |


**Posebno:** Delete blokiran ako poslovnica ima vozila → `TempData["Error"]`.

**Forma:** naziv, tip lokacije, adresa, telefon.

---

#### Vozila (Vehicle)


| Stranica        | URL                                           |
| --------------- | --------------------------------------------- |
| Lista           | `/Vehicle`                                    |
| Detalji         | `/Vehicle/Details/{id}`                       |
| Po registraciji | `/vozila/reg/{registration}` (attribute ruta) |
| CRUD            | Create / Edit / Delete                        |


**Detalji (S13):** Hero s registracijom, cijenom, chip statusa; **tabovi** Pregled / Rezervacije / Servisi; linkovi Nova rezervacija, Novi servis.

**Forma:** registracija, marka, model, godina, tip, km, aktivno, cijena/dan, **autocomplete poslovnica** (`/api/lookup/branches`).

---

#### Kupci (Customer)


| Stranica       | URL         |
| -------------- | ----------- |
| Lista          | `/Customer` |
| CRUD + Details | standard    |


**Forma:** ime, prezime, email, telefon, **datum rođenja** (`_DateTimePartial`), vozačka.

---

#### Rezervacije (Reservation)


| Stranica | URL                         |
| -------- | --------------------------- |
| Lista    | `/Reservation`              |
| Pregled  | `/rezervacije/pregled/{id}` |
| CRUD     | Create / Edit               |


**Forma:**

- **Autocomplete** kupac + vozilo
- **Datum početak / završetak** (custom picker)
- Lokacije preuzimanja/odjave, status, osnovna cijena

**Lista:** status chip (`_StatusChip`), boje po `ReservationStatus`.

---

#### Dodaci (Addon)


| Stranica     | URL      |
| ------------ | -------- |
| Lista + CRUD | `/Addon` |


**Forma:** naziv, cijena po danu.

---

#### Servisi (ServiceRecord)


| Stranica     | URL              |
| ------------ | ---------------- |
| Lista + CRUD | `/ServiceRecord` |


**Forma:**

- Autocomplete vozilo
- Datum servisa, sljedeći preporučeni servis (datum)
- Status, opis, kilometraža, trošak

---

#### Zaposlenici (Employee)


| Stranica     | URL         |
| ------------ | ----------- |
| Lista + CRUD | `/Employee` |


**Forma:** ime, pozicija, autocomplete poslovnica, datum zaposlenja, plaća (ako u VM).

---

#### Partneri (Partners)


| Stranica | URL                          |
| -------- | ---------------------------- |
| Lista    | `/partneri`                  |
| Novi     | `/partneri/novi`             |
| Uredi    | `/partneri/uredi/{id}`       |
| Obriši   | POST `/partneri/obrisi/{id}` |
| AJAX     | `/partneri/SearchRows`       |


**Forma:** `_PartnerFormPartial` — tvrtka, kontakt, email, telefon.

---

### 5.4 Lab 2 naslijeđe — Index/Details bez punog CRUD na svim

Svi glavni entiteti imali su **Index + Details** već u Lab 2; Lab 3 dodao EF; Lab 4 dodao Create/Edit/Delete + AJAX.

---

## 6. Lab 4 — napredne web funkcionalnosti

### 6.1 AJAX pretraga lista (partial HTML)


| Što      | Gdje                                                          |
| -------- | ------------------------------------------------------------- |
| UI polje | `_CrudIndexHeaderPartial` → `data-ajax-search`, `data-target` |
| JS       | `site.js` → `initAjaxTables`                                  |
| Server   | `SearchRows` → `PartialView("_IndexRows", ...)`               |


**Stranice s AJAX pretragom:** Vehicle, BranchOffice, Customer, Reservation, Addon, ServiceRecord, Employee, Partneri.

---

### 6.2 Autocomplete (JSON + AJAX)


| Što     | Gdje                                                                    |
| ------- | ----------------------------------------------------------------------- |
| API     | `LookupApiController` → `/api/lookup/customers`, `vehicles`, `branches` |
| Odgovor | JSON `[{ "id", "label" }]`                                              |
| UI      | `_AutocompletePartial`                                                  |
| JS      | `site.js` → `initAutocomplete` → `res.json()`                           |


**Koristi se na:** Reservation (kupac, vozilo), Vehicle (poslovnica), Employee (poslovnica), ServiceRecord (vozilo).

---

### 6.3 Validacija


| Sloj       | Implementacija                                                                                                                    |
| ---------- | --------------------------------------------------------------------------------------------------------------------------------- |
| **Server** | `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Phone]` na `FormViewModels.cs`; `ModelState.IsValid` u controlleru |
| **Client** | `_ValidationScriptsPartial` (jQuery Validate unobtrusive)                                                                         |
| **Blur**   | `site.js` → `initBlurValidation` na `[data-val='true']`                                                                           |
| **Prikaz** | `<span asp-validation-for="..." class="field-error">`                                                                             |


---

### 6.4 Datumska kontrola (partial + JS)


| Što     | Gdje                                                                       |
| ------- | -------------------------------------------------------------------------- |
| Partial | `Views/Shared/_DateTimePartial.cshtml`                                     |
| VM      | `DateTimeFieldVm`                                                          |
| JS      | `site.js` → `initDateTimePickers`, `formatDisplayDate`, `parseDisplayDate` |
| Format  | hr: `dd.mm.yyyy hh:mm`, en: `MM/dd/yyyy hh:mm` (`navigator.language`)      |
| Submit  | Skriveno polje `name="StartDate"` itd. s ISO vrijednošću                   |


**Stranice:** Customer (rođenje), Reservation (start/end), Employee (zaposlenje), ServiceRecord (datumi).

**Nije** browser `type="date"`.

---

### 6.5 Lokalizacija

- `Program.cs` → `UseRequestLocalization`, kulture `hr` i `en-US`
- Datum picker prati jezik preglednika u JS-u

---

## 7. API endpointi


| Metoda | URL                           | Odgovor      | Namjena                 |
| ------ | ----------------------------- | ------------ | ----------------------- |
| GET    | `/api/lookup/customers?q=`    | JSON         | Autocomplete kupci      |
| GET    | `/api/lookup/vehicles?q=`     | JSON         | Autocomplete vozila     |
| GET    | `/api/lookup/branches?q=`     | JSON         | Autocomplete poslovnice |
| GET    | `/{Controller}/SearchRows?q=` | HTML partial | AJAX tablica            |


---

## 8. Baza podataka i entiteti

### 8.1 Provider

- **Default:** SQLite → `src/CarRent.Web/Data/carrent.dev.db` (Development)
- **Opcija:** SQL Server → `docker compose up -d` + `DatabaseProvider: SqlServer`

Migracije se primjenjuju **automatski pri startu** (`Program.cs` → `db.Database.Migrate()`).

### 8.2 Entiteti (tablice)


| Entitet           | Veze / napomena                      |
| ----------------- | ------------------------------------ |
| **BranchOffice**  | 1-N vozila, zaposlenici              |
| **Vehicle**       | N-1 poslovnica; rezervacije, servisi |
| **Customer**      | rezervacije                          |
| **Reservation**   | kupac, vozilo; status enum; datumi   |
| **Addon**         | N-N preko ReservationAddon           |
| **ServiceRecord** | vozilo                               |
| **Employee**      | poslovnica                           |
| **Partner**       | zaseban (partneri firme)             |


Definicija: `src/CarRent.Model/Entities/Entities.cs`  
DbContext: `src/CarRent.DAL/CarRentDbContext.cs`

---

## 9. Routing — sve važne rute

### 9.1 Alias rute (`Program.cs`)


| URL            | Controller |
| -------------- | ---------- |
| `/pocetna`     | Home       |
| `/vozni-park`  | Fleet      |
| `/dnevni-plan` | DailyPlan  |
| `/raspored`    | Timeline   |


### 9.2 Attribute rute


| URL                                                   | Akcija                          |
| ----------------------------------------------------- | ------------------------------- |
| `/`                                                   | Home                            |
| `/raspored/mjesecni`                                  | Timeline                        |
| `/operativa/dnevni-plan`                              | DailyPlan                       |
| `/rezervacije/pregled/{id}`                           | Reservation Details             |
| `/vozila/reg/{registration}`                          | Vehicle Details po registraciji |
| `/partneri`, `/partneri/novi`, `/partneri/uredi/{id}` | Partneri CRUD                   |


### 9.3 Default MVC

`{controller=Home}/{action=Index}/{id?}`

---

## 10. Mapa ključnih datoteka u kodu

```text
src/CarRent.Web/
├── Program.cs                 # DI, baza, lokalizacija, rute
├── Controllers/
│   ├── Controllers.cs         # Home, Timeline, DailyPlan, Fleet, Partners
│   ├── EntityCrudControllers.cs
│   └── LookupApiController.cs
├── Repositories/EfRepositories.cs
├── Services/
│   ├── EntityMappers.cs
│   ├── TimelineLayoutHelper.cs
│   └── UiDisplayHelper.cs
├── ViewModels/
│   ├── FormViewModels.cs      # CRUD forme + DateTimeFieldVm
│   └── PageViewModels.cs      # Dashboard, Timeline, Fleet
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _DateTimePartial.cshtml
│   │   ├── _AutocompletePartial.cshtml
│   │   ├── _CrudIndexHeaderPartial.cshtml
│   │   ├── _StatusChip.cshtml
│   │   └── ...
│   ├── Home/, Timeline/, DailyPlan/, Fleet/
│   └── {Entity}/Index, Create, Edit, Details, _FormPartial, _IndexRows
└── wwwroot/
    ├── css/glass.css, site.css, site-compat.css, site-pages.css
    └── js/site.js
```

---

## 11. Pokretanje i testiranje

```bash
./scripts/run-local.sh
# ili
dotnet run --project src/CarRent.Web/CarRent.Web.csproj
```

**Brzi test Lab 4:**

1. Otvori `/Vehicle` → tipkaj u AJAX pretragu
2. `/Reservation/Create` → autocomplete + datum picker
3. Prazna forma → validacijske poruke (client + server)
4. `/BranchOffice` → pokušaj obrisati poslovnicu s vozilima → greška
5. `/dnevni-plan` → print preview

---

## 12. Labovi — što je koji dodao


| Lab                | Dizajn                                                              | Funkcionalnost                                                                |
| ------------------ | ------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| **Lab 1**          | —                                                                   | Konzolni model, seed, LINQ                                                    |
| **Lab 2**          | Glassmorphism, layout, Timeline, Dnevni plan, Fleet, Partneri lista | MVC, mock→kasnije EF, Index/Details, navigacija                               |
| **Lab 3**          | —                                                                   | EF, Model+DAL, migracije, alias/attribute rute, Partner CRUD, partiali        |
| **Lab 4**          | site-compat, CRUD toolbar                                           | Pun CRUD, AJAX lista, autocomplete JSON, validacija, datum partial, animacije |
| **Design handoff** | `glass.css`, novi layout, KPI, Gantt, fleet kartice                 | Vizualna nadogradnja operativnih ekrana                                       |


---

## 13. END GOAL / budućnost (još nije u kodu)

Iz `design/screen-inventory.md` i END GOAL plana:


| ID    | Planirano                              |
| ----- | -------------------------------------- |
| G04   | Modal potvrde brisanja                 |
| G05   | 404 / Error stranica                   |
| F2-01 | Login / autentifikacija                |
| F2-02 | Izvještaji                             |
| S18   | Rezervacije — puni N-N dodaci na formi |
| —     | Filter chipovi na listi rezervacija    |
| —     | Drag/drop na timelineu                 |


---

## 14. Dokumentacija i logovi (izvan runtime app)


| Putanja                                                   | Sadržaj                       |
| --------------------------------------------------------- | ----------------------------- |
| `README.md`                                               | Pokretanje, lab workflow      |
| `lab2/LAB2-Report.md`                                     | Lab 2                         |
| `lab-3/LAB3-Report.md`, `semantic-model.md`, `sitemap.md` | Lab 3                         |
| `lab-4/LAB4-Report.md`                                    | Lab 4 + usmena pitanja        |
| `lab-4/Lab4.md`                                           | Službene upute predmeta       |
| `lab-*/ai_conversation.jsonl`                             | AI log razgovora              |
| `.github/hooks/`                                          | Export transkripta, agent log |


---

## 15. Sažetak jednom rečenicom

**CarRent** je tamno-stakleni ASP.NET MVC dashboard za rent-a-car operacije, s EF bazom, operativnim ekranima (početna, timeline, dnevni plan, fleet), punim CRUD-om svih entiteta, AJAX listama, JSON autocompleteom, dvostrukom validacijom i custom datumskim pickerom — implementirano u `src/CarRent.Web`, dizajnirano prema `design/handoff` sustavu **Control Surface**.

---

*Generirano kao jedinstveni pregled projekta. Za detalje po labu vidi pojedinačne reporte u `lab2/`, `lab-3/`, `lab-4/`.*