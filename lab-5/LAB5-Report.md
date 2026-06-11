# Lab 5 Report — API, Auth, Upload, Testovi (CarRent-System)

## 1. Što je novo u Lab 5

Lab 5 nadograđuje Lab 4 web aplikaciju s:

- kompletnim REST API-jem (CRUD + DTO) za svih 8 entiteta
- ASP.NET Core Identity (lokalna registracija/prijava, `AppUser` s OIB i JMBG)
- autorizacijom po ulogama (`Admin`, `Manager`) na MVC i API sloju
- Google OAuth loginom (konfiguracija preko user-secrets / appsettings)
- uploadom dokumenata vozila (Dropzone na Vehicle Edit formi)
- integracijskim testovima API endpointa (`WebApplicationFactory` + InMemory baza)

## 2. Identity i AppUser

### Model

`src/CarRent.Model/Entities/AppUser.cs` nasljeđuje `IdentityUser` i dodaje:

- `OIB` — 11 znamenki, obavezno
- `JMBG` — 13 znamenki, obavezno

### DbContext

`CarRentDbContext` sada nasljeđuje `IdentityDbContext<AppUser>`.

Migracija: `AddIdentityAndVehicleAttachments` (Identity tablice + `VehicleAttachments`).

### Program.cs

- `AddDefaultIdentity<AppUser>()` + `AddRoles<IdentityRole>()`
- `UseAuthentication()` prije `UseAuthorization()`
- `MapRazorPages()` za Identity stranice
- Google OAuth ako su `ClientId` i `ClientSecret` postavljeni
- Seed rola i korisnika: `IdentitySeedData.SeedAsync()`

### Seed korisnici (development)


| Email                   | Lozinka       | Uloga   |
| ----------------------- | ------------- | ------- |
| `admin@carrent.local`   | `Admin123!`   | Admin   |
| `manager@carrent.local` | `Manager123!` | Manager |


### Identity stranice

`Areas/Identity/Pages/Account/`:

- `Login`, `Logout` — javna prijava zaposlenika
- `Register` — **samo Admin** (`[Authorize(Roles = "Admin")]`), kreira račun zaposlenika s ulogom Manager/Admin
- `ExternalLogin` — Google samo za **postojeće** korisnike (admin ih mora prvo kreirati)

Javna self-registracija nije dostupna — management platforma.

Layout koristi `Views/Shared/_LoginPartial.cshtml` (Admin vidi link „Novi korisnik”).

## 3. Autorizacija (MVC + API)


| Akcija                         | Pristup               |
| ------------------------------ | --------------------- |
| `Index`, `SearchRows`          | Svi (anonimni)        |
| `Details`                      | Prijavljeni korisnici |
| `Create`, `Edit`               | `Admin` ili `Manager` |
| `Delete`                       | Samo `Admin`          |
| Upload/brisanje priloga vozila | `Admin` ili `Manager` |


Primijenjeno na:

- `EntityCrudControllers.cs` (svi CRUD controlleri)
- `Controllers.cs` → `PartnersController`

## 4. Web API (DTO + CRUD)

### Struktura

```
src/CarRent.Web/Api/
  Dtos/EntityDtos.cs       — response/create/update DTO klase
  Mappers/ApiMappers.cs    — ručno mapiranje entitet ↔ DTO
  Controllers/EntityApiControllers.cs — 8 API controllera
```

### API rute


| Entitet       | Base ruta             |
| ------------- | --------------------- |
| BranchOffice  | `/api/branch-office`  |
| Vehicle       | `/api/vehicle`        |
| Customer      | `/api/customer`       |
| Reservation   | `/api/reservation`    |
| Addon         | `/api/addon`          |
| ServiceRecord | `/api/service-record` |
| Employee      | `/api/employee`       |
| Partner       | `/api/partner`        |


Svaki controller podržava:

- `GET /api/{entity}` — lista (+ opcionalni `?q=` query)
- `GET /api/{entity}/search/{q}` — eksplicitna pretraga
- `GET /api/{entity}/{id}` — jedan zapis
- `POST /api/{entity}` — kreiranje → `201 Created`
- `PUT /api/{entity}/{id}` — izmjena
- `DELETE /api/{entity}/{id}` — brisanje

### DTO pravila

- Response DTO ne izlaže navigacijske kolekcije
- Povezani entiteti kroz ugniježđene summary DTO (npr. `BranchOfficeSummaryDto`)
- Create/Update DTO odvojeni od entiteta
- Validacija: DataAnnotations + `[ApiController]` automatski vraća `400`

### API autorizacija

- `GET` endpointi: `[AllowAnonymous]`
- `POST`, `PUT`: `[Authorize(Roles = "Admin,Manager")]`
- `DELETE`: `[Authorize(Roles = "Admin")]`

## 5. Upload dokumenata vozila (Dropzone)

### Model

`VehicleAttachment` u `CarRent.Model/Entities/VehicleAttachment.cs`:

- `VehicleId`, `FileName`, `FilePath`, `ContentType`, `FileSize`, `CreatedAt`

### MVC akcije (`VehicleController`)

- `UploadAttachment(vehicleId, file)` — POST, sprema u `wwwroot/uploads/vehicles/{id}/`
- `GetAttachments(vehicleId)` — partial `_AttachmentList.cshtml`
- `DeleteAttachment(id)` — AJAX POST, briše s diska i iz baze

### UI

- Dropzone na `Views/Vehicle/Edit.cshtml` (samo Edit — Create nema ID)
- CDN: dropzone 5.9.3
- Validacija: max 10 MB, ekstenzije `.pdf`, `.jpg`, `.jpeg`, `.png`

## 6. Google OAuth

Konfiguracija u `appsettings.json` / `appsettings.Development.json`:

```json
"Authentication": {
  "Google": {
    "ClientId": "",
    "ClientSecret": ""
  }
}
```

Za lokalni development (ne commitati tajne):

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_CLIENT_ID" \
  --project src/CarRent.Web/CarRent.Web.csproj
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_CLIENT_SECRET" \
  --project src/CarRent.Web/CarRent.Web.csproj
```

HTTPS: `Properties/launchSettings.json` → `https://localhost:7001`

Google Cloud Console callback: `https://localhost:7001/signin-google`

## 7. Integracijski testovi

Projekt: `tests/CarRent.Web.IntegrationTests/`

- `CarRentWebApplicationFactory` — InMemory baza, test auth handler
- `TestAuthHandler` — simulira ulogu preko headera `X-Test-Role`
- `EntityApiCrudTests` — 32 testa (svi entiteti, CRUD, auth, validacija)

### Pokretanje

```bash
dotnet test tests/CarRent.Web.IntegrationTests/CarRent.Web.IntegrationTests.csproj
```

Rezultat: **32/32 testova prolazi**.

### Testirani scenariji

- `GET all` / `GET by id` (postoji / ne postoji → 404)
- `POST` valjan → 201, nevaljan → 400
- `POST` bez auth → 401
- `PUT` postojeći / nepostojeći
- `DELETE` bez Admin uloge → 403
- Validacijske greške (npr. Reservation datumi)

## 8. Ručni test plan

1. Pokreni app: `./scripts/run-local.sh` ili `dotnet run --project src/CarRent.Web/CarRent.Web.csproj`
2. Otvori `http://localhost:5000` — liste su javne
3. Klikni Detalje bez prijave → redirect na login
4. Prijavi se kao `admin@carrent.local` / `Admin123!`
5. Kreiraj novi zapis (npr. Addon) → uspjeh
6. Odjavi se, prijavi kao `manager@carrent.local` → Create/Edit radi, Delete ne
7. Vehicle Edit → upload PDF/slike → pojavi se u listi → obriši
8. API test:
  ```bash
   curl http://localhost:5000/api/vehicle
   curl -X POST http://localhost:5000/api/addon -H "Content-Type: application/json" -d '{"name":"Test"}'  # → 401
  ```
9. Google login (ako su secrets postavljeni) → ExternalLogin forma s OIB/JMBG

## 9. Migracije

```bash
dotnet ef migrations add AddIdentityAndVehicleAttachments \
  --project src/CarRent.DAL/CarRent.DAL.csproj \
  --startup-project src/CarRent.Web/CarRent.Web.csproj

dotnet ef database update \
  --project src/CarRent.DAL/CarRent.DAL.csproj \
  --startup-project src/CarRent.Web/CarRent.Web.csproj
```

## 10. Logovi agenta

- `lab-5/agent_log.txt` — hook `log_ai_lab5.sh` (`.github/hooks.lab5.json`)
- `lab-5/ai_conversation.jsonl` — export Cursor transkripta

```bash
bash .github/hooks/export_cursor_transcript_lab5.sh
bash .github/hooks/start_transcript_watch_lab5.sh
bash .github/hooks/stop_transcript_watch_lab5.sh
```

Marker za export: `Dodao sam lab-5`.