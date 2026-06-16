# Lab 5 — puni pregled: što se mijenjalo, gdje u kodu, kako pokazati

Dokument za **usmenu predaju** i navigaciju kroz kod. Lab 5 dodaje **REST API**, **Identity/auth**, **upload**, **Google login** i **integracijske testove**, plus operativne nadogradnje (fleet lifecycle, obavijesti).

**Struktura:** sekcija 1 (tablica promjena) → **sekcija 2 (teorija — što je i zašto)** → sekcije 3–11 (kod i demo) → sekcija 13 (pitanja profesora).

---

## 1. Lab 4 → Lab 5 — što je novo (jedna tablica)


| Područje         | Lab 4                | Lab 5 (novo)                                         |
| ---------------- | -------------------- | ---------------------------------------------------- |
| Pristup podacima | Samo MVC (HTML)      | + REST API (JSON)                                    |
| Autentikacija    | Nema                 | ASP.NET Core Identity                                |
| Uloge            | Nema                 | `Admin`, `Manager`                                   |
| MVC zaštita      | Svi mogu sve         | `[Authorize]` + role + **FallbackPolicy**            |
| Vanjski login    | Nema                 | Google OAuth (opcionalno, user-secrets)              |
| Datoteke         | Nema                 | Upload dokumenata vozila (Dropzone)                  |
| DTO              | Samo Form ViewModeli | API DTO + `ApiMappers`                               |
| Testovi          | Ručno                | 55 integracijskih testova (xUnit)                    |
| Baza             | EF entiteti          | + Identity tablice, `VehicleAttachment`, fleet polja |
| Operativa        | Statički podaci      | Fleet lifecycle sync + outbox obavijesti             |


---

## 2. Teorija — što je što, zašto se koristi i kako se veže na kod

Ovo je **teorijski dio** s predavanja (Lab 5.md), povezan s našom implementacijom. Korisno kad profesor pita „što je X?” bez da odmah gleda u kod.

---

### 2.1 REST API (Web API)

**Što je:** Način da aplikacija **izloži podatke i operacije** drugim klijentima preko HTTP-a. Umjesto HTML stranice (MVC), server vraća **strukturirane podatke** — najčešće JSON.

**Zašto:** Isti backend mogu koristiti:

- web stranica (JavaScript `fetch`)
- mobilna aplikacija
- drugi server
- **integracijski testovi** (`HttpClient`)

**U našem projektu:** `src/CarRent.Web/Api/Controllers/EntityApiControllers.cs` — 8 controllera, rute `/api/vehicle`, `/api/customer`, itd.

**Razlika od MVC:** MVC controller → `return View(model)` (HTML). API controller → `return Ok(dto)` (JSON). API nasljeđuje `ControllerBase`, ne `Controller`.

---

### 2.2 HTTP metode (GET, POST, PUT, DELETE)


| Metoda     | Namjena                             | Primjer u CarRent                           |
| ---------- | ----------------------------------- | ------------------------------------------- |
| **GET**    | Dohvat podataka (ne mijenja stanje) | `GET /api/vehicle` — lista vozila           |
| **POST**   | Kreiranje novog resursa             | `POST /api/addon` — novi dodatak            |
| **PUT**    | Izmjena postojećeg resursa          | `PUT /api/vehicle/5` — ažuriraj vozilo id=5 |
| **DELETE** | Brisanje resursa                    | `DELETE /api/partner/3`                     |


**Zašto jasne metode:** Iz samog HTTP zahtjeva vidi se **namjera** (čitati, kreirati, mijenjati, brisati). U starijim sustavima sve je bilo POST — teže za održavanje i dokumentaciju.

**Teorija vs kod:** Svaka metoda je zasebna akcija u API controlleru s atributom `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`.

---

### 2.3 HTTP status kodovi

Server ne vraća samo podatke — vraća i **status** koji govori je li zahtjev uspio.


| Kod                  | Značenje                        | Kad u CarRent                |
| -------------------- | ------------------------------- | ---------------------------- |
| **200 OK**           | Uspjeh                          | GET lista, PUT uspio         |
| **201 Created**      | Novi resurs kreiran             | POST nakon `CreatedAtAction` |
| **204 No Content**   | Uspjeh bez tijela               | (rjeđe kod nas)              |
| **400 Bad Request**  | Loš zahtjev / validacija        | Prazan `Name` na DTO         |
| **401 Unauthorized** | Nisi autentificiran             | API bez prijave / cookie     |
| **403 Forbidden**    | Autentificiran, ali nemaš pravo | Manager pokuša DELETE        |
| **404 Not Found**    | Zapis ne postoji                | `GET /api/vehicle/99999`     |


**Zašto je bitno:** Klijent (ili test) odmah zna **vrstu greške** bez čitanja poruke u tijelu.

**U testovima:** `EntityApiCrudTests.cs` eksplicitno provjerava `HttpStatusCode.NotFound`, `Unauthorized`, `Forbidden`.

---

### 2.4 DTO (Data Transfer Object)

**Što je:** Klasa koja definira **oblik podataka** koje API šalje ili prima — odvojeno od EF entiteta u bazi.

**Zašto ne vraćati entitet direktno:**

1. Entitet može imati **interna polja** koja klijent ne smije vidjeti
2. **Navigacijske kolekcije** (`Vehicle.Reservations`) → ogroman ili **ciklički JSON**
3. API model može ostati **stabilan** i kad se baza promijeni
4. Create/Update mogu imati **drugačija polja** od onoga što vraćaš (npr. bez `Id` pri kreiranju)

**U projektu:**

- DTO: `Api/Dtos/EntityDtos.cs` — npr. `VehicleDto`, `VehicleCreateDto`, `VehicleUpdateDto`
- Mapiranje: `Api/Mappers/ApiMappers.cs` — `ToDto()`, `Apply(dto, entity)`

**Analogija za usmeno:** Entitet = cijela tablica + veze u bazi. DTO = „izvadak za ekran/API”.

---

### 2.5 Atribut `[ApiController]`

**Što je:** Oznaka na API controller klasi koja uključuje **API-specifična ponašanja** u ASP.NET Core-u.

**Što donosi (teorija):**

- Očekuje **attribute routing** (`[Route("api/vehicle")]`)
- Poboljšan **model binding** iz JSON tijela
- Automatski **400** kad validacija (`ModelState`) ne prođe
- Konzistentniji API odgovori za greške

**Gdje:** vrh svake klase u `EntityApiControllers.cs`.

---

### 2.6 Autentikacija vs autorizacija


| Pojam             | Pitanje          | Primjer                                |
| ----------------- | ---------------- | -------------------------------------- |
| **Autentikacija** | *Tko si?*        | Login emailom i lozinkom, Google login |
| **Autorizacija**  | *Smiješ li ovo?* | Samo Admin smije DELETE                |


**Zašto su odvojeni:** Korisnik može biti **prijavljen** (autentikacija OK), ali **nemati ulogu** za brisanje (autorizacija FAIL → 403).

**U kodu:**

- Autentikacija: Identity, cookie, `UseAuthentication()`
- Autorizacija: `[Authorize]`, `[Authorize(Roles = "Admin")]`, `UseAuthorization()`

---

### 2.7 ASP.NET Core Identity

**Što je:** Ugrađeni framework za **korisnike, lozinke, role, vanjske logine** — ne pišeš auth od nule.

**Što rješava umjesto tebe:**

- Hashiranje lozinki (ne plain text)
- Lockout, reset lozinke (ako uključiš)
- Tablice `AspNetUsers`, `AspNetRoles`, …
- Cookie sesija nakon prijave

**Zašto ne vlastiti login sustav:** Sigurnost (salt, hash algoritmi), manje grešaka, standardni pattern.

**U projektu:**

- `AppUser : IdentityUser` — proširen OIB/JMBG
- `CarRentDbContext : IdentityDbContext<AppUser>`
- `Program.cs` → `AddDefaultIdentity<AppUser>().AddRoles<IdentityRole>()`
- UI: `Areas/Identity/Pages/Account/`

---

### 2.8 Cookie autentikacija (MVC + browser)

**Što je:** Nakon prijave server pošalje **authentication cookie**; browser ga automatski šalje pri svakom sljedećem zahtjevu.

**Zašto za web:** Korisnik u browseru ne mora ručno slati token — cookie je standard za MVC stranice.

**Razlika od JWT (teorija):** JWT se često koristi za mobilne/API klijente izvan browsera. U predavanju se spominje da API **može** koristiti bearer token; u našem projektu browser i API dijele **isti Identity cookie** kad zoveš iz iste domene, a testovi koriste **TestAuthHandler**.

---

### 2.9 Role-based autorizacija (uloge)

**Što je:** Korisnik pripada **ulozi** (npr. Admin, Manager); prava se dodjeljuju po ulozi, ne po pojedinačnom korisniku za svaku akciju.

**Zašto:** Jednostavno za male sustave s jasnim tipovima korisnika (administrator vs operater).

**U CarRent:**


| Uloga       | Tipična prava            |
| ----------- | ------------------------ |
| **Admin**   | Sve uključujući DELETE   |
| **Manager** | Create, Edit — ne Delete |


**Kod:** `[Authorize(Roles = "Admin,Manager")]` na akcijama. Role u bazi: `AspNetRoles`, veza `AspNetUserRoles`. Seed: `IdentitySeedData.cs`.

**Napomena s predavanja:** Za mnogo sitnih prava koriste se **permissioni**; za ovaj lab **role su dovoljne**.

---

### 2.10 FallbackPolicy (globalna zaštita)

**Što je:** Default pravilo autorizacije: **svaki endpoint zahtijeva prijavljenog korisnika**, osim ako nije eksplicitno `[AllowAnonymous]`.

**Zašto:** Ne moraš na svaku akciju ručno stavljati `[Authorize]` — „zatvoreno po defaultu, otvori što treba”.

**Gdje:** `Program.cs` → `options.FallbackPolicy = RequireAuthenticatedUser()`.

**Posljedica:** Anoniman posjetitelj na `/` → redirect na Login. Identity stranice Login/ExternalLogin imaju `[AllowAnonymous]`.

---

### 2.11 OAuth / Google login (3rd party autentikacija)

**Što je:** Korisnik se **ne prijavljuje lozinkom kod tebe**, nego kod Googlea; Google potvrdi identitet, tvoja app kreira lokalnu sesiju.

**Pojednostavljeni tok (predavanje):**

1. Klik „Login with Google”
2. Redirect na Google
3. Korisnik se prijavi kod Googlea
4. Google vrati **authorization code** na tvoj callback URL
5. Server razmijeni code za podatke o korisniku
6. App kreira **cookie** (lokalna prijava)

**Zašto ClientId / ClientSecret:** Identificiraju tvoju aplikaciju kod providera. **Ne smiju** u git — user-secrets ili env varijable.

**Zašto HTTPS:** Google (i drugi provideri) zahtijevaju siguran callback.

**U projektu:** `Program.cs` → `AddGoogle()`; flow u `ExternalLogin.cshtml.cs`; callback `https://localhost:7001/signin-google`.

**Naš poslovni model:** Novi Google korisnik može biti kreiran **bez uloge** → `PendingAccess` dok Admin ne dodijeli ulogu (`ManageUsers`).

---

### 2.12 Upload datoteka (multipart, Dropzone)

**Što je:** Korisnik šalje **datoteku** na server (PDF, slika). HTTP koristi **multipart/form-data**, ne JSON.

**Zašto ne u bazu (teorija):** Relacijske baze nisu idealne za velike binarne datoteke; u praksi se file stavlja na **disk** ili **cloud storage** (S3, Azure Blob), a u bazu samo **metapodaci** (ime, putanja, veličina, MIME).

**Zašto Dropzone:** JavaScript komponenta za **asinkroni upload** (bez reloada stranice), drag & drop, progress.

**Zašto tek na Details/Edit (ne Create):** Prilog ima FK `VehicleId` — zapis mora **postojati u bazi** da bi imao ID.

**U projektu:**

- Model: `VehicleAttachment`
- Disk: `wwwroot/uploads/vehicles/{id}/`
- Controller: `UploadAttachment(IFormFile file)`
- UI: `Vehicle/Details.cshtml` + Dropzone CDN

**Sigurnost (teorija):** Validirati ekstenziju, veličinu, MIME — mi: max 10 MB, `.pdf/.jpg/.png`.

---

### 2.13 Middleware pipeline (Authentication → Authorization)

**Što je:** Lanac komponenti kroz koje prolazi **svaki HTTP zahtjev** prije controllera.

**Zašto redoslijed:**

```
UseAuthentication()  → tko je korisnik (popuni User)
UseAuthorization()   → smije li na ovu akciju
```

Ako zamijeniš redoslijed, `User` nije postavljen → autorizacija ne radi ispravno.

**Dodatno kod nas:** `PendingRoleMiddleware` — nakon auth, provjera ima li korisnik ulogu.

---

### 2.14 Integracijski testovi

**Što je:** Test koji provjerava **cijeli sustav** kroz HTTP — routing, binding, validacija, auth, baza, JSON odgovor — zajedno.

**Zašto ne samo unit test controllera:** Unit test s mockom dokazuje samo da mock radi. Integracijski dokazuje da **aplikacija u sklopu** radi.

**Ključni alati (predavanje + kod):**


| Alat                      | Uloga                           |
| ------------------------- | ------------------------------- |
| **xUnit**                 | Test framework (`[Fact]`)       |
| **WebApplicationFactory** | Pokreće pravu app u memoriji    |
| **HttpClient**            | Šalje HTTP kao pravi klijent    |
| **EF InMemory**           | Testna baza bez SQLite datoteke |


**Zašto InMemory u testovima:** Brzo, izolirano, ne dira `carrent.dev.db`. **Napomena:** InMemory nije 100% identičan SQL-u (npr. neki constrainti).

**Gdje:** `tests/CarRent.Web.IntegrationTests/`

---

### 2.15 TestAuthHandler (zašto postoji)

**Problem:** U testu ne želimo praviti login formu i cookie.

**Rješenje:** Zamijeniti auth scheme testnim handlerom koji iz HTTP headera `X-Test-Role: Admin` napravi `ClaimsPrincipal` s ulogom.

**Zašto je to i dalje integracijski test:** Auth je zamijenjen na granici sustava, ali controller, DTO, DbContext i routing su **pravi**.

---

### 2.16 Mapiranje entitet ↔ DTO (zašto ručno, ne AutoMapper)

**Teorija:** Mapiranje može biti u controlleru, extension metodi, mapper klasi ili AutoMapper biblioteci.

**Zašto ručno (`ApiMappers`):** Za lab je dovoljno, eksplicitno, lako pokazati na usmenom. AutoMapper štedi linije koda u velikim projektima, ali skriva što se mapira.

**Pravilo s predavanja:** Ne kopirati isti mapping u svaku metodu — jedna mapper klasa, ponovno korištenje.

---

### 2.17 Fleet lifecycle i obavijesti (bonus — nije core bodovanje Lab 5)

**Što je:** Pozadinska logika koja **automatski ažurira statuse** rezervacija/vozila/servisa i **priprema obavijesti** (outbox pattern).

**Zašto outbox:** Umjesto slanja maila odmah u istoj transakciji, zapisuje se „poruka za slanje” u tablicu `FleetNotificationOutbox` — pouzdanije i testabilnije.

**Gdje:** `FleetLifecycleService`, `FleetLifecycleRules`, `FleetNotificationService`, `Program.cs` poziv `SyncAsync()` pri startu.

---

### 2.18 Poveznica teorija → kod (sažeta tablica)


| Teorija            | Koncept                 | Gdje u CarRent                               |
| ------------------ | ----------------------- | -------------------------------------------- |
| Web API            | JSON CRUD               | `Api/Controllers/EntityApiControllers.cs`    |
| DTO                | Odvojen API model       | `Api/Dtos/EntityDtos.cs`                     |
| HTTP status        | 200/201/400/401/403/404 | API metode + integracijski testovi           |
| Identity           | Korisnici i lozinke     | `AppUser`, `Areas/Identity/`                 |
| Role               | Admin / Manager         | `[Authorize(Roles=...)]`, `IdentitySeedData` |
| Cookie auth        | Browser sesija          | Identity default                             |
| OAuth              | Google login            | `AddGoogle`, `ExternalLogin`                 |
| Upload             | Datoteka + metapodaci   | `VehicleAttachment`, Dropzone                |
| Integracijski test | End-to-end HTTP         | `CarRent.Web.IntegrationTests`               |
| Middleware         | Auth redoslijed         | `Program.cs`                                 |


---

## 3. Pokretanje (obavezno prije demo)

### Jedna naredba

```bash
./scripts/run-local.sh
```

Ako baza „pukne” (stara SQLite bez novih stupaca), obriši dev bazu i pokreni ponovo:

```bash
rm -f src/CarRent.Web/Data/carrent.dev.db src/CarRent.Web/Data/carrent.dev.db-shm src/CarRent.Web/Data/carrent.dev.db-wal
./scripts/run-local.sh
```

**URL:** [http://localhost:5000](http://localhost:5000)  
**HTTPS (Google OAuth):** [https://localhost:7001](https://localhost:7001) (`launchSettings.json`)

### Integracijski testovi

```bash
dotnet test tests/CarRent.Web.IntegrationTests/CarRent.Web.IntegrationTests.csproj
```

**Očekivano:** 55/55 prolazi.

### Seed korisnici (kreiraju se pri startu)


| Email                   | Lozinka       | Uloga   |
| ----------------------- | ------------- | ------- |
| `admin@carrent.local`   | `Admin123!`   | Admin   |
| `manager@carrent.local` | `Manager123!` | Manager |


Kod: `src/CarRent.DAL/IdentitySeedData.cs`

---

## 4. REST API — gdje je i kako radi

### 4.1 Struktura datoteka

```
src/CarRent.Web/Api/
├── Dtos/EntityDtos.cs              ← response + create/update DTO
├── Mappers/ApiMappers.cs           ← entitet ↔ DTO (ručno mapiranje)
└── Controllers/EntityApiControllers.cs  ← 8 API controllera
```

### 4.2 API rute (svi entiteti)


| Entitet     | Base ruta             |
| ----------- | --------------------- |
| Poslovnica  | `/api/branch-office`  |
| Vozilo      | `/api/vehicle`        |
| Kupac       | `/api/customer`       |
| Rezervacija | `/api/reservation`    |
| Dodatak     | `/api/addon`          |
| Servis      | `/api/service-record` |
| Zaposlenik  | `/api/employee`       |
| Partner     | `/api/partner`        |


### 4.3 Operacije po controlleru

Svaki API controller ima:


| HTTP   | Ruta                       | Što radi                                      |
| ------ | -------------------------- | --------------------------------------------- |
| GET    | `/api/{entity}`            | Lista (+ opcionalno `?q=pretraga`)            |
| GET    | `/api/{entity}/search/{q}` | Eksplicitna pretraga                          |
| GET    | `/api/{entity}/{id}`       | Jedan zapis → `200` ili `404`                 |
| POST   | `/api/{entity}`            | Kreiranje → `201 Created` + `CreatedAtAction` |
| PUT    | `/api/{entity}/{id}`       | Izmjena → `200` ili `404`                     |
| DELETE | `/api/{entity}/{id}`       | Brisanje → `200` ili `404`                    |


Primjer u kodu: `BranchOfficeApiController` na vrhu `EntityApiControllers.cs`.

### 4.4 Zašto DTO, a ne entitet?

- API **ne vraća** navigacijske kolekcije (izbjegava ciklički JSON)
- Create/Update DTO **odvojeni** od response DTO
- Povezani entiteti kao **summary** (npr. `BranchOfficeSummaryDto` unutar `VehicleDto`)

**Pokazati:** `EntityDtos.cs` + `ApiMappers.ToDto()` / `Apply()`.

### 4.5 `[ApiController]` ponašanje

- Automatski `400 Bad Request` kad `ModelState` nije validan
- Attribute routing (`[Route("api/vehicle")]`)
- Nasljeđuje `ControllerBase` (nema viewova)

### 4.6 API autorizacija (stvarno stanje u kodu)

- Controller ima `[Authorize]` → **GET također traži prijavu**
- `POST` / `PUT`: `[Authorize(Roles = "Admin,Manager")]`
- `DELETE`: `[Authorize(Roles = "Admin")]`

**Demo curl:**

```bash
# Bez prijave → 401
curl -i http://localhost:5000/api/vehicle

# Lista (u browseru nakon prijave cookie radi; za API izvana treba auth)
```

U **testovima** auth simulira `TestAuthHandler` + header `X-Test-Role`.

---

## 5. ASP.NET Core Identity — gdje je

### 5.1 AppUser (prošireni korisnik)

**Datoteka:** `src/CarRent.Model/Entities/AppUser.cs`

- Nasljeđuje `IdentityUser`
- Dodatna polja: `OIB` (11), `JMBG` (13) — opcionalna u bazi nakon migracije `MakeAppUserOibJmbgOptional`

### 5.2 DbContext

**Datoteka:** `src/CarRent.DAL/CarRentDbContext.cs`

- `IdentityDbContext<AppUser>` umjesto običnog `DbContext`
- Migracija: `AddIdentityAndVehicleAttachments` (AspNet* tablice + prilozi)

### 5.3 Program.cs — registracija

**Datoteka:** `src/CarRent.Web/Program.cs`

Ključne stvari za pokazati:

```csharp
builder.Services.AddDefaultIdentity<AppUser>(...)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<CarRentDbContext>();

// Fallback: gotovo sve traži prijavu
builder.Services.AddAuthorization(options => {
    options.FallbackPolicy = ... RequireAuthenticatedUser();
});

app.UseAuthentication();   // PRIJE Authorization!
app.UseAuthorization();
app.MapRazorPages();       // Identity UI
```

Redoslijed middleware-a je **bitan za usmeno**.

### 5.4 Identity stranice (Razor Pages)

**Mapa:** `src/CarRent.Web/Areas/Identity/Pages/Account/`


| Stranica               | Namjena                                         |
| ---------------------- | ----------------------------------------------- |
| `Login.cshtml`         | Prijava (anonimno)                              |
| `Logout.cshtml`        | Odjava                                          |
| `Register.cshtml`      | **Samo Admin** kreira nove korisnike            |
| `ExternalLogin.cshtml` | Google — samo postojeći korisnici               |
| `ManageUsers.cshtml`   | Admin dodjeljuje ulogu korisnicima „na čekanju” |
| `PendingAccess.cshtml` | Korisnik bez uloge čeka odobrenje               |


**Važno za predmet:** nema javne self-registracije — management platforma.

### 5.5 Layout — login UI

**Datoteka:** `Views/Shared/_LoginPartial.cshtml`

- Neprijavljen → gumb „Prijava”
- Admin → „Novi korisnik”, „Na čekanju”, Odjava

### 5.6 PendingRoleMiddleware

**Datoteka:** `Middleware/PendingRoleMiddleware.cs`

- Prijavljen korisnik **bez role** → redirect na `/Identity/Account/PendingAccess`
- Sprječava pristup app dok Admin ne dodijeli ulogu (`ManageUsers`)

---

## 6. Autorizacija MVC (Lab 5 pravila)

### Globalno

`FallbackPolicy` u `Program.cs` → anonimac **ne može** na većinu stranica (redirect na login).

### Po akcijama (EntityCrudControllers + Partners)


| Akcija                         | Tko smije                                                                      |
| ------------------------------ | ------------------------------------------------------------------------------ |
| `Index`, `SearchRows` (liste)  | **Admin** (CRUD liste)                                                         |
| `Details`                      | Prijavljeni (bez `[Authorize(Roles=...)]` na nekim — ali fallback traži login) |
| `Create`, `Edit`               | **Admin** ili **Manager**                                                      |
| `Delete`                       | Samo **Admin**                                                                 |
| Upload/brisanje priloga vozila | **Admin** ili **Manager**                                                      |
| Partneri `Index`               | **Admin**                                                                      |


**Pokazati:** `EntityCrudControllers.cs` — traži `[Authorize(Roles = "...")]` iznad akcija.

### Operativa (Home, Timeline, Fleet…)

`Controllers.cs` — Home/Timeline/DailyPlan/Fleet **nemaju** `[Authorize]` na controlleru, ali **FallbackPolicy** i dalje traži prijavu za sve osim Identity/static.

---

## 7. Google OAuth (3rd party login)

### Konfiguracija

`appsettings.json` / user-secrets:

```json
"Authentication": {
  "Google": {
    "ClientId": "...",
    "ClientSecret": "..."
  }
}
```

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "..." --project src/CarRent.Web/CarRent.Web.csproj
dotnet user-secrets set "Authentication:Google:ClientSecret" "..." --project src/CarRent.Web/CarRent.Web.csproj
```

**Kod:** `Program.cs` → `AddGoogle()` samo ako su oba postavljena.

**Callback URL:** `https://localhost:7001/signin-google`

**Flow:** Google potvrdi identitet → `ExternalLogin` → ako email postoji u bazi, prijava; inače poruka da Admin mora prvo kreirati korisnika.

---

## 8. Upload datoteka (Dropzone)

### Model

**Datoteka:** `src/CarRent.Model/Entities/VehicleAttachment.cs`

- `VehicleId`, `FileName`, `FilePath`, `ContentType`, `FileSize`, `CreatedAt`

### MVC akcije

**Datoteka:** `EntityCrudControllers.cs` → `VehicleController`


| Akcija             | Metoda | Što radi                      |
| ------------------ | ------ | ----------------------------- |
| `UploadAttachment` | POST   | Sprema na disk + zapis u bazu |
| `GetAttachments`   | GET    | Partial `_AttachmentList`     |
| `DeleteAttachment` | POST   | Briše s diska i iz baze       |


**Disk:** `wwwroot/uploads/vehicles/{vehicleId}/{guid}.ext`

### UI — gdje na stranici

**Datoteka:** `Views/Vehicle/Details.cshtml` (tab Dokumenti)

- Dropzone 5.9.3 (CDN)
- Samo na **Details** (vozilo već ima ID) — ne na Create
- Max 10 MB, ekstenzije: `.pdf`, `.jpg`, `.jpeg`, `.png`
- AJAX: nakon uploada osvježi listu; brisanje preko `fetch`/POST

**Pokazati demo:** Prijava kao Admin → Vozila → Detalji → tab Dokumenti → povuci PDF.

---

## 9. Fleet automatizacija (bonus iz Lab 5 commita)

Nije u osnovnom Lab5.md bodovanju, ali je u kodu — korisno znati.


| Komponenta | Datoteka                                 | Što radi                                       |
| ---------- | ---------------------------------------- | ---------------------------------------------- |
| Pravila    | `Services/FleetLifecycleRules.cs`        | Promjene statusa rezervacija, servisa, vozila  |
| Sync       | `Services/FleetLifecycleService.cs`      | Pri startu app-a (`Program.cs`)                |
| Middleware | `Middleware/FleetLifecycleMiddleware.cs` | Periodički sync tijekom rada                   |
| Obavijesti | `Services/Notifications/`*               | Outbox u tablici `FleetNotificationOutbox`     |
| UI         | `Views/Notifications/Index.cshtml`       | Admin/Manager pregled pripremljenih obavijesti |
| Timeline   | `Views/Timeline/Index.cshtml`            | Traka registracije vozila                      |


Nova polja: `Reservation.MileageUpdateSuggested`, `Vehicle.BlockedByService`, `Vehicle.RegistrationDueDate`, itd.

**Migracija:** `20260611150000_AddFleetAutomation` (+ Designer datoteka).

---

## 10. Integracijski testovi — gdje i što pokrivaju

### Projekt

`tests/CarRent.Web.IntegrationTests/`


| Datoteka                                | Uloga                                               |
| --------------------------------------- | --------------------------------------------------- |
| `CarRentWebApplicationFactory.cs`       | `WebApplicationFactory`, InMemory baza, test config |
| `TestAuthHandler.cs`                    | Lažna autentikacija preko `X-Test-Role`             |
| `ApiTestClientExtensions.cs`            | `.AsRole("Admin")`, `.AsAnonymous()`                |
| `EntityApiCrudTests.cs`                 | CRUD za sve entitete + 401/403/404/400              |
| `FleetLifecycleRulesTests.cs`           | Unit pravila lifecycle-a                            |
| `ReservationAvailabilityHelperTests.cs` | Dostupnost vozila                                   |


### Zašto InMemory, a ne SQLite u testovima?

- Brže, izolirano po test runu
- Nema ovisnosti o lokalnoj dev bazi
- Okolina: `Testing` (preskače seed/migrate u `Program.cs`)

### Što reći profesoru

„Integracijski test ne mocka controller — poziva pravi HTTP endpoint kroz `HttpClient`, s pravim `DbContext`-om u memoriji.”

---

## 11. Mapa „gdje pokazati u kodu” (checklist za usmenu)


| Tema          | Otvori datoteku                                | Što pokazati                                     |
| ------------- | ---------------------------------------------- | ------------------------------------------------ |
| API CRUD      | `Api/Controllers/EntityApiControllers.cs`      | `[ApiController]`, GET/POST/PUT/DELETE           |
| DTO           | `Api/Dtos/EntityDtos.cs`                       | Create vs Response DTO                           |
| Mapiranje     | `Api/Mappers/ApiMappers.cs`                    | `ToDto`, `Apply`                                 |
| Identity user | `CarRent.Model/Entities/AppUser.cs`            | OIB, JMBG                                        |
| DbContext     | `CarRentDbContext.cs`                          | `IdentityDbContext<AppUser>`                     |
| Startup auth  | `Program.cs`                                   | Identity, Google, FallbackPolicy, middleware red |
| Seed          | `IdentitySeedData.cs`                          | Admin/Manager korisnici                          |
| MVC auth      | `EntityCrudControllers.cs`                     | `[Authorize(Roles=...)]`                         |
| Login UI      | `_LoginPartial.cshtml`                         | Prijava / Admin linkovi                          |
| Registracija  | `Areas/Identity/.../Register.cshtml.cs`        | `[Authorize(Roles="Admin")]`                     |
| Upload        | `Vehicle/Details.cshtml` + `VehicleController` | Dropzone + `UploadAttachment`                    |
| Prilozi model | `VehicleAttachment.cs`                         | Metapodaci u bazi                                |
| Testovi       | `EntityApiCrudTests.cs`                        | Jedan CRUD test + 401 scenarij                   |
| Test factory  | `CarRentWebApplicationFactory.cs`              | InMemory + TestAuthHandler                       |


---

## 12. Ručni demo scenarij (5 minuta)

1. **Pokreni** `./scripts/run-local.sh` → [http://localhost:5000](http://localhost:5000)
2. **Bez prijave** → redirect na login (FallbackPolicy)
3. **Prijavi se** `admin@carrent.local` / `Admin123!`
4. **Početna** → KPI, operativa radi
5. **Vozila** → lista (Admin) → **Detalji** → tab **Dokumenti** → upload datoteke
6. **Novi dodatak** (Create) → uspjeh; **Delete** kao Admin
7. **Odjavi se**, prijavi **manager@** → Create/Edit OK, Delete ne
8. **API u terminalu:** `curl -i http://localhost:5000/api/addon` → 401
9. **Testovi:** `dotnet test tests/...` → 55 passed

---

## 13. Pitanja profesora / AI pregled koda — detaljno s putanjama

Ovo je prošireni vodič za usmenu kad profesor (ili njegov AI) pita **„gdje je X?”** ili traži objašnjenje **nejasnijeg dijela koda**. Za svako pitanje: **gdje otvoriti datoteku**, **što točno pokazati**, **kako objasniti**.

---

### A) Lokacijska pitanja — „Gdje se nalazi…?”

#### A1. Gdje su integracijski testovi?


| Što                         | Putanja                                                              |
| --------------------------- | -------------------------------------------------------------------- |
| Cijeli test projekt         | `tests/CarRent.Web.IntegrationTests/`                                |
| CRUD testovi API-ja         | `tests/CarRent.Web.IntegrationTests/EntityApiCrudTests.cs`           |
| Testna aplikacija (factory) | `tests/CarRent.Web.IntegrationTests/CarRentWebApplicationFactory.cs` |
| Lažna prijava u testovima   | `tests/CarRent.Web.IntegrationTests/TestAuthHandler.cs`              |
| Helper za role              | `tests/CarRent.Web.IntegrationTests/ApiTestClientExtensions.cs`      |
| Unit testovi pravila        | `tests/CarRent.Web.IntegrationTests/FleetLifecycleRulesTests.cs`     |


**Kako objasniti:** „Integracijski test ne poziva metodu direktno — kreira `HttpClient` preko `WebApplicationFactory<Program>` i šalje pravi HTTP zahtjev na `/api/...`. Provjerava status kod i JSON tijelo.”

**Pokazati u kodu:** `EntityApiCrudTests` konstruktor (prima factory) i npr. test `BranchOffice_GetAll_WhenUnauthorized_ReturnsUnauthorized` — linije s `_client.AsAnonymous().GetAsync(...)`.

---

#### A2. Gdje su definirane uloge (Admin, Manager)?


| Što                           | Putanja                                                                         |
| ----------------------------- | ------------------------------------------------------------------------------- |
| Imena uloga (stringovi)       | `src/CarRent.DAL/IdentitySeedData.cs` → `Roles = ["Admin", "Manager"]`          |
| Kreiranje uloga u bazi        | Ista datoteka → `roleManager.CreateAsync(new IdentityRole(role))`               |
| Dodjela korisniku             | `userManager.AddToRoleAsync(user, role)` u `EnsureUserAsync`                    |
| Registracija Identity za role | `src/CarRent.Web/Program.cs` → `.AddRoles<IdentityRole>()` (oko linije 56)      |
| Tablica u bazi                | `AspNetRoles`, `AspNetUserRoles` (migracija `AddIdentityAndVehicleAttachments`) |


**Kako objasniti:** „Uloga nije hardkodirana u svakom controlleru kao logika — to su zapisi u Identity tablicama. U kodu ih koristimo preko `[Authorize(Roles = "Admin,Manager")]`.”

---

#### A3. Gdje se uloge provjeravaju na MVC stranicama?


| Što                              | Putanja                                                             |
| -------------------------------- | ------------------------------------------------------------------- |
| CRUD kontroleri                  | `src/CarRent.Web/Controllers/EntityCrudControllers.cs`              |
| Partneri                         | `src/CarRent.Web/Controllers/Controllers.cs` → `PartnersController` |
| Globalno „moraš biti prijavljen” | `src/CarRent.Web/Program.cs` → `FallbackPolicy` (linije 16–21)      |


**Primjer za pokazati** — `VehicleController` u `EntityCrudControllers.cs`:

- `Index` / `SearchRows` → `[Authorize(Roles = "Admin")]`
- `Create` / `Edit` → `[Authorize(Roles = "Admin,Manager")]`
- `Delete` → `[Authorize(Roles = "Admin")]`
- `UploadAttachment` → `[Authorize(Roles = "Admin,Manager")]` (oko linije 178)

**Kako objasniti:** „ASP.NET prije izvršavanja akcije čita atribute na metodi. Ako korisnik nema ulogu, vraća 403 Forbidden ili redirect na login.”

---

#### A4. Gdje se uloge provjeravaju na API-ju?

**Datoteka:** `src/CarRent.Web/Api/Controllers/EntityApiControllers.cs`

- Na razini controllera: `[Authorize]` — svaki poziv treba autentikaciju
- `POST` / `PUT`: `[Authorize(Roles = "Admin,Manager")]`
- `DELETE`: `[Authorize(Roles = "Admin")]`

**Primjer:** `BranchOfficeApiController` — prve ~65 linija.

**Test koji to dokazuje:** `EntityApiCrudTests.cs` → `Partner_Delete_AsManager_ReturnsForbidden` — Manager dobije **403**, ne 401.

---

#### A5. Gdje je REST API (svi controlleri)?


| Što                  | Putanja                                                                          |
| -------------------- | -------------------------------------------------------------------------------- |
| Svi API controlleri  | `src/CarRent.Web/Api/Controllers/EntityApiControllers.cs` (~474 linije, 8 klasa) |
| DTO klase            | `src/CarRent.Web/Api/Dtos/EntityDtos.cs`                                         |
| Mapiranje            | `src/CarRent.Web/Api/Mappers/ApiMappers.cs`                                      |
| Lookup (Lab 4, JSON) | `src/CarRent.Web/Controllers/LookupApiController.cs` — `/api/lookup/...`         |


**Napomena:** Lookup API je ostao iz Lab 4 (autocomplete u formama). Lab 5 CRUD API je u `Api/Controllers/`.

---

#### A6. Gdje je ASP.NET Identity (login, registracija)?


| Što                       | Putanja                                                                                         |
| ------------------------- | ----------------------------------------------------------------------------------------------- |
| Konfiguracija             | `src/CarRent.Web/Program.cs` — `AddDefaultIdentity<AppUser>()`                                  |
| Korisnički model          | `src/CarRent.Model/Entities/AppUser.cs`                                                         |
| DbContext s Identity      | `src/CarRent.DAL/CarRentDbContext.cs` — `IdentityDbContext<AppUser>`                            |
| Login stranica            | `Areas/Identity/Pages/Account/Login.cshtml` + `Login.cshtml.cs`                                 |
| Registracija (samo Admin) | `Areas/Identity/Pages/Account/Register.cshtml.cs` — `[Authorize(Roles = "Admin")]` na liniji 10 |
| Google login              | `Areas/Identity/Pages/Account/ExternalLogin.cshtml.cs`                                          |
| Dodjela uloga             | `Areas/Identity/Pages/Account/ManageUsers.cshtml.cs`                                            |
| Čekanje uloge             | `Areas/Identity/Pages/Account/PendingAccess.cshtml`                                             |
| Gumb u headeru            | `Views/Shared/_LoginPartial.cshtml`                                                             |


---

#### A7. Gdje je upload datoteka (Dropzone)?


| Što              | Putanja                                                                           |
| ---------------- | --------------------------------------------------------------------------------- |
| Model u bazi     | `src/CarRent.Model/Entities/VehicleAttachment.cs`                                 |
| DbSet            | `CarRentDbContext.cs` → `VehicleAttachments`                                      |
| POST upload      | `EntityCrudControllers.cs` → `VehicleController.UploadAttachment` (~linija 178)   |
| Lista (partial)  | `GetAttachments` → vraća `Views/Vehicle/_AttachmentList.cshtml`                   |
| Brisanje         | `DeleteAttachment` (~linija 228)                                                  |
| UI + Dropzone JS | `Views/Vehicle/Details.cshtml` — tab Dokumenti, CDN Dropzone, `new Dropzone(...)` |
| Fizički disk     | `wwwroot/uploads/vehicles/{vehicleId}/{guid}.ext`                                 |


---

#### A8. Gdje je Google OAuth konfiguracija?


| Što                    | Putanja                                                                                |
| ---------------------- | -------------------------------------------------------------------------------------- |
| Registracija providera | `Program.cs` linije 59–68 — `AddGoogle(...)` samo ako su ClientId i Secret postavljeni |
| Tajne                  | `appsettings.json` / `appsettings.Development.json` → sekcija `Authentication:Google`  |
| Dev tajne              | `dotnet user-secrets` (ne commitati)                                                   |
| HTTPS port             | `Properties/launchSettings.json` → `https://localhost:7001`                            |
| Callback flow          | `ExternalLogin.cshtml.cs` — `OnPost` (Challenge), `OnGetCallbackAsync`                 |


---

#### A9. Gdje su DTO klase i zašto nisu entiteti?

**Datoteka:** `src/CarRent.Web/Api/Dtos/EntityDtos.cs`

- Response: npr. `VehicleDto` (ima `BranchOffice` kao `BranchOfficeSummaryDto`, ne cijelu kolekciju rezervacija)
- Create: `VehicleCreateDto` — samo polja koja smijemo primiti pri kreiranju
- Update: `VehicleUpdateDto` — uključuje `Id`

**Mapiranje:** `Api/Mappers/ApiMappers.cs` — metode `ToDto`, `ToSummary`, `Apply(dto, entity)`.

**Kako objasniti:** „Entitet `Vehicle` ima `ICollection<Reservation>` — da ga vratimo direktno u JSON, dobio bi se ogroman ili ciklički odgovor. DTO određuje točno što API klijent vidi.”

---

#### A10. Gdje je seed admin/manager korisnika?

**Datoteka:** `src/CarRent.DAL/IdentitySeedData.cs`

- `SeedAsync` — kreira role, zatim `admin@carrent.local` i `manager@carrent.local`
- Poziv: `Program.cs` linija ~100 → `await IdentitySeedData.SeedAsync(...)` (pri startu app-a, ne u test okolini)

---

### B) Konceptualna pitanja — s odgovorom i kodom

#### B1. Razlika MVC controllera i API controllera?

**MVC** (`Controllers/EntityCrudControllers.cs`):

- Nasljeđuje `Controller`
- Vraća `View(...)` ili `PartialView(...)` — HTML
- Primjer: `return View(await repository.GetAllAsync());` na `Index`

**API** (`Api/Controllers/EntityApiControllers.cs`):

- Nasljeđuje `ControllerBase`
- Ima `[ApiController]` i `[Route("api/vehicle")]`
- Vraća `Ok(dto)`, `CreatedAtAction(...)`, `NotFound()` — JSON + HTTP status

**Jedna rečenica:** „Isti projekt, dva načina izlaza — HTML za ljude u browseru, JSON za programe/testove.”

---

#### B2. Objasni tok jednog API POST zahtjeva (npr. nova poslovnica)

1. Klijent šalje `POST /api/branch-office` s JSON tijelom `BranchOfficeCreateDto`
2. `[ApiController]` + model binding mapira JSON na C# objekt
3. DataAnnotations na DTO (`[Required]` na `Name`) — ako ne valja → **400**
4. `[Authorize(Roles = "Admin,Manager")]` — ako nema uloge → **401/403**
5. `ApiMappers.Apply(model, entity)` — puni EF entitet
6. `db.BranchOffices.Add(entity); SaveChangesAsync()`
7. `return CreatedAtAction(nameof(GetById), new { id }, dto)` → **201** + lokacija

**Kod:** `BranchOfficeApiController.Post` u `EntityApiControllers.cs`.

---

#### B3. Što radi `[ApiController]` atribut?

**Gdje:** na vrhu svake klase u `EntityApiControllers.cs`.

**Objašnjenje (4 točke):**

1. Uključuje automatsku validaciju — nevaljan model → 400 bez ručnog `if (!ModelState.IsValid)` (iako možeš dodati)
2. Attribute routing je obavezan
3. Binding pravilno čita JSON iz tijela zahtjeva
4. Greške validacije u konzistentnom API formatu

---

#### B4. Zašto `UseAuthentication()` mora biti prije `UseAuthorization()`?

**Gdje:** `Program.cs` linije 125–126:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

**Objašnjenje:** Authentication middleware čita cookie (ili drugi scheme) i popunjava `HttpContext.User`. Authorization middleware tek onda može provjeriti `[Authorize]` i role. Obrnuti redoslijed → `User` je prazan → svi zaštićeni endpointi padaju.

---

#### B5. Što je FallbackPolicy i zašto me redirecta na login?

**Gdje:** `Program.cs` linije 16–21:

```csharp
options.FallbackPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();
```

**Objašnjenje:** To je **default pravilo za cijelu aplikaciju** — svaka akcija koja nema `[AllowAnonymous]` traži prijavljenog korisnika. Zato anoniman korisnik na `/` dobije redirect na `/Identity/Account/Login`.

**Iznimke:** stranice s `[AllowAnonymous]` — npr. `Login.cshtml.cs`, `ExternalLogin.cshtml.cs`.

---

#### B6. Zašto Register nije javan?

**Gdje:** `Areas/Identity/Pages/Account/Register.cshtml.cs`:

```csharp
[Authorize(Roles = "Admin")]
public class RegisterModel : PageModel
```

**Objašnjenje:** Samo Admin može kreirati nove zaposlenike s ulogom Manager ili Admin. To je poslovni model „internal management platform”, ne javni signup.

**UI link:** `_LoginPartial.cshtml` — gumb „Novi korisnik” vidljiv samo ako `User.IsInRole("Admin")`.

---

#### B7. Što se dogodi kad se netko prijavi Googleom, a nema ulogu?

**Tok u kodu** (`ExternalLogin.cshtml.cs`):

1. `OnGetCallbackAsync` — Google vrati podatke
2. Ako korisnik ne postoji → `CreateAsync` (bez uloge) — linija ~71
3. `SignInAsync` — prijavljen je
4. `RedirectAfterSignInAsync` — provjeri `GetRolesAsync` — linija 98–101
5. Ako `roles.Count == 0` → redirect na `PendingAccess`

**Middleware:** `PendingRoleMiddleware.cs` — blokira pristup ostatku app dok Admin ne dodijeli ulogu preko `ManageUsers`.

**Admin dodjela uloge:** `ManageUsers.cshtml.cs` → `OnPostAssignRoleAsync` → `AddToRoleAsync`.

---

#### B8. Zašto upload nije na Create formi vozila?

**Kod:** `UploadAttachment(int vehicleId, ...)` u `VehicleController` — parametar **mora** imati `vehicleId`.

**UI:** Dropzone je u `Vehicle/Details.cshtml`, ne u `Create.cshtml`.

**Objašnjenje:** U relacijskoj bazi prilog ima FK `VehicleId`. Dok vozilo ne postoji u tablici, nema ID-a na koji vezati datoteku. Lab upute eksplicitno traže upload na Edit (kod nas Details + Edit layout).

---

#### B9. Kako Dropzone šalje datoteku i kako se lista osvježava?

**UI:** `Vehicle/Details.cshtml` — forma s `class="dropzone"`, `asp-action="UploadAttachment"`, `asp-route-vehicleId`.

**Server:** `UploadAttachment` prima `IFormFile file` — multipart/form-data.

**Nakon uspjeha (JS u Details):** Dropzone `success` callback poziva funkciju koja učitava partial (npr. `loadAttachments()` → fetch/GetAttachments ili `.load()`).

**Partial:** `_AttachmentList.cshtml` — tablica s linkom na datoteku i gumbom Obriši.

---

#### B10. Gdje se fizička datoteka briše iz diska?

**Kod:** `DeleteAttachment` u `EntityCrudControllers.cs` (~228–240):

```csharp
var physicalPath = Path.Combine(env.WebRootPath, attachment.FilePath.TrimStart('/'));
if (System.IO.File.Exists(physicalPath))
    System.IO.File.Delete(physicalPath);
db.VehicleAttachments.Remove(attachment);
```

**Objašnjenje:** Brišemo i fajl i red u bazi — inače ostaje „sirovi” file na disku.

---

### C) Integracijski testovi — detaljna pitanja

#### C1. Kako testovi pokreću aplikaciju bez `dotnet run`?

**Datoteka:** `CarRentWebApplicationFactory.cs`

- Nasljeđuje `WebApplicationFactory<Program>`
- `ConfigureWebHost` postavlja okolinu `"Testing"`
- Zamjenjuje SQLite s `UseInMemoryDatabase(_dbName)` — jedinstveno ime po factory instanci
- `CreateHost` poziva `db.Database.EnsureCreated()` — kreira shemu u memoriji

**Zašto `public partial class Program` na kraju `Program.cs`?** Da test projekt može referencirati entry point (`WebApplicationFactory<Program>`).

---

#### C2. Kako test simulira Admina bez pravog passworda?

**Lanac:**

1. `TestAuthHandler.cs` — čita HTTP header `X-Test-Role`
2. Ako je npr. `Admin`, kreira `ClaimsPrincipal` s `ClaimTypes.Role = "Admin"`
3. `ApiTestClientExtensions.AsRole("Admin")` — dodaje header na `HttpClient`
4. `CarRentWebApplicationFactory` registrira `TestAuthHandler` kao default auth scheme u testovima

**Pokazati:** test `BranchOffice_Post_WhenValid_ReturnsCreated` — `_client.AsRole("Admin")`.

---

#### C3. Što test `BranchOffice_GetAll_WhenUnauthorized_ReturnsUnauthorized` dokazuje?

**Datoteka:** `EntityApiCrudTests.cs` ~linija 70–75

- `AsAnonymous()` — ukloni header
- `GET /api/branch-office`
- Očekuje `401 Unauthorized`

**Objašnjenje:** API controller ima `[Authorize]` — bez identiteta ne smije proći.

---

#### C4. Razlika integracijskog i unit testa u ovom projektu?


| Tip           | Primjer datoteke              | Što testira                                                    |
| ------------- | ----------------------------- | -------------------------------------------------------------- |
| Integracijski | `EntityApiCrudTests.cs`       | HTTP → pipeline → controller → baza → JSON                     |
| Unit          | `FleetLifecycleRulesTests.cs` | Samo `FleetLifecycleRules.EvaluateReservation(...)` bez HTTP-a |


**Zašto ne mockati repozitorij u integracijskom?** Jer cilj je dokazati da cijeli sustav radi zajedno — mock bi testirao samo mock.

---

#### C5. Koliko testova ima i kako ih pokrenuti?

```bash
dotnet test tests/CarRent.Web.IntegrationTests/CarRent.Web.IntegrationTests.csproj
```

**Očekivano:** 55 passed (API CRUD za sve entitete + auth scenariji + unit testovi helpera/pravila).

---

### D) Baza i migracije — pitanja

#### D1. Gdje se Identity tablice dodaju?

**Migracija:** `src/CarRent.DAL/Migrations/20260611092315_AddIdentityAndVehicleAttachments.cs`

- Kreira `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, itd.
- Dodaje tablicu `VehicleAttachments`

**DbContext:** `CarRentDbContext : IdentityDbContext<AppUser>` — `base.OnModelCreating` registrira Identity shemu.

---

#### D2. Gdje se migracije primjenjuju pri pokretanju?

**Datoteka:** `Program.cs` linije 90–104:

```csharp
if (!app.Environment.IsEnvironment("Testing"))
{
    db.Database.Migrate();
    await IdentitySeedData.SeedAsync(...);
    await lifecycle.SyncAsync();
}
```

**Objašnjenje:** Pri `dotnet run` baza se automatski migrira. U testovima (`Testing`) to se preskače — factory koristi InMemory.

---

### E) „Teže čitljivi” dijelovi koda — što AI često pita

#### E1. Zašto imamo i `EntityMappers` (MVC) i `ApiMappers` (API)?


| Mapper             | Putanja                     | Koristi se za                          |
| ------------------ | --------------------------- | -------------------------------------- |
| `EntityMappers.cs` | `Services/EntityMappers.cs` | MVC forme (`FormViewModels` ↔ entitet) |
| `ApiMappers.cs`    | `Api/Mappers/ApiMappers.cs` | REST API (`EntityDtos` ↔ entitet)      |


**Objašnjenje:** MVC forma ima autocomplete display polja i drugačija polja od API JSON-a. Dva kanala — dva mappera. Isto pravilo „ne izlaži entitet direktno”.

---

#### E2. Zašto `LookupApiController` i puni REST API?

- **Lookup** (`/api/lookup/customers`) — Lab 4, mali JSON za autocomplete u **MVC formama**
- **REST CRUD** (`/api/vehicle`, `/api/customer`, …) — Lab 5, puni CRUD za **vanjske klijente i testove**

Oba su API, ali različit scope.

---

#### E3. Što radi `PendingRoleMiddleware`?

**Datoteka:** `Middleware/PendingRoleMiddleware.cs`

- Nakon prijave, prije controllera
- Ako korisnik nema nijednu ulogu → redirect na `/Identity/Account/PendingAccess`
- Dozvoljeni pathovi: login, logout, static files (`/css/`, `/js/`, `/uploads/`)

**Zašto postoji:** Google ili ručno kreiran korisnik može biti prijavljen ali bez role — ne smije koristiti CRUD dok Admin ne odobri.

---

#### E4. Što radi `FleetLifecycleService` pri startu?

**Datoteka:** `Services/FleetLifecycleService.cs`  
**Poziv:** `Program.cs` → `await lifecycle.SyncAsync()`

**Objašnjenje:** Prolazi kroz rezervacije/servise/vozila i primjenjuje poslovna pravila (npr. promjena statusa rezervacije, blokada vozila na servisu). Pravila su u `FleetLifecycleRules.cs`. Obavijesti idu u `FleetNotificationOutbox` preko `FleetNotificationService`.

**Bonus** — nije core Lab 5 bodovanje, ali AI može naći ovaj kod kao „kompleksan”.

---

#### E5. Razlika `401 Unauthorized` i `403 Forbidden` u našem projektu?


| Status  | Značenje                          | Primjer u testu                                                     |
| ------- | --------------------------------- | ------------------------------------------------------------------- |
| **401** | Nisi prijavljen / nema identiteta | `BranchOffice_GetAll_WhenUnauthorized` — `AsAnonymous()`            |
| **403** | Prijavljen si, ali nemaš ulogu    | `Partner_Delete_AsManager_ReturnsForbidden` — Manager pokuša DELETE |


**Kod:** API `DELETE` ima `[Authorize(Roles = "Admin")]` — Manager je autenticiran ali nema pravo.

---

### F) Brzi indeks — „profesor pita jednom rečenicom”


| Pitanje                        | Odgovor u jednoj rečenici + datoteka                                            |
| ------------------------------ | ------------------------------------------------------------------------------- |
| Gdje su integracijski testovi? | `tests/CarRent.Web.IntegrationTests/EntityApiCrudTests.cs`                      |
| Gdje je WebApplicationFactory? | `tests/.../CarRentWebApplicationFactory.cs`                                     |
| Gdje su role?                  | Seed: `IdentitySeedData.cs`; provjera: `[Authorize(Roles=...)]` u controllerima |
| Gdje je API?                   | `src/CarRent.Web/Api/Controllers/EntityApiControllers.cs`                       |
| Gdje su DTO?                   | `src/CarRent.Web/Api/Dtos/EntityDtos.cs`                                        |
| Gdje je Identity login?        | `Areas/Identity/Pages/Account/Login.cshtml.cs`                                  |
| Gdje je AppUser?               | `src/CarRent.Model/Entities/AppUser.cs`                                         |
| Gdje je upload?                | `VehicleController.UploadAttachment` + `Vehicle/Details.cshtml`                 |
| Gdje je Google?                | `Program.cs` AddGoogle + `ExternalLogin.cshtml.cs`                              |
| Gdje je FallbackPolicy?        | `Program.cs` linije 16–21                                                       |
| Gdje se mapira entitet→DTO?    | `Api/Mappers/ApiMappers.cs`                                                     |
| Kako testiram auth?            | `TestAuthHandler` + header `X-Test-Role`                                        |


---

### G) Što otvoriti u IDE prije usmenog (2 minute)

1. `Program.cs` — Identity, Google, FallbackPolicy, middleware red
2. `EntityApiControllers.cs` — jedan cijeli controller (npr. BranchOffice)
3. `EntityDtos.cs` + `ApiMappers.cs` — jedan entitet
4. `EntityCrudControllers.cs` — `[Authorize]` na Vehicle + Upload
5. `IdentitySeedData.cs` — seed korisnici
6. `EntityApiCrudTests.cs` — 2–3 testa (201, 401, 403)
7. `Vehicle/Details.cshtml` — Dropzone
8. `_LoginPartial.cshtml` — login UI

---

## 14. Migracije (Lab 5)

Redoslijed:

1. `Initial`
2. `AddIdentityAndVehicleAttachments`
3. `MakeAppUserOibJmbgOptional`
4. `AddVehicleMainImagePath`
5. `AddVehicleRegistrationDueDate`
6. `AddFleetAutomation`

Primjenjuju se automatski u `Program.cs` → `db.Database.Migrate()`.

Ako dev baza ostane stara: obriši `carrent.dev.db` i restartaj.

---

## 15. Logovi (predaja)

```bash
bash .github/hooks/export_cursor_transcript_lab5.sh
```

- `lab-5/ai_conversation.jsonl`
- `lab-5/agent_log.txt`
- `lab-5/LAB5-Report.md` — kraći tehnički report
- `lab-5/Lab5.md` — službene upute s predavanja

---

## 16. Jedna rečenica za cijeli Lab 5

Lab 5 pretvara CarRent u **zaštićenu** web aplikaciju s **ASP.NET Identity** (Admin/Manager), **REST API-jem s DTO-ima** za svih 8 entiteta, **Dropzone uploadom** dokumenata na vozilu, opcionalnim **Google loginom** i **55 integracijskih testova** koji provjeravaju API CRUD i autorizaciju — uz MVC iz Lab 4 koji i dalje služi operativnom UI-u.

---

*Ažurirano za commit `Lab 5: API, Identity, upload, testovi i fleet automatizacija`.*