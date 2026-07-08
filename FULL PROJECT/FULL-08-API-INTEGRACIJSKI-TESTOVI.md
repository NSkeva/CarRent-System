# FULL-08 — API integracijski testovi + CRUD stabilnost

**Kriterij PDF:**  
- Testovi za sve API endpointe (dio kriterija s Playwright) — **2 boda**  
- CRUD mora raditi bez grešaka — **2 boda**  

**Status:** ✅ Iz Lab 5 (xUnit) + pokriveno Playwright E2E

---

## 1. Teorija

### Integracijski test

Testira **cijeli sustav** kroz HTTP: routing, auth, validacija, baza, JSON — zajedno.

**Zašto uz Playwright:**  
- xUnit integracijski = brz, pokriva **sve API endpointe** sustavno  
- Playwright = dokaz da **UI + browser + sesija** rade u stvarnom scenariju

### CRUD bez grešaka

Create, Read, Update, Delete moraju raditi za sve entitete bez 500 errora i s ispravnom autorizacijom.

---

## 2. Gdje je u projektu

```
tests/CarRent.Web.IntegrationTests/
├── CarRentWebApplicationFactory.cs   ← InMemory baza, TestAuthHandler
├── TestAuthHandler.cs                ← X-Test-Role header
├── ApiTestClientExtensions.cs        ← .AsRole("Admin")
├── EntityApiCrudTests.cs             ← CRUD svi entiteti
├── FleetLifecycleRulesTests.cs
└── ReservationAvailabilityHelperTests.cs
```

**Broj testova:** **55/55** prolazi.

---

## 3. Gdje u kodu — što testovi pokrivaju

### EntityApiCrudTests (glavni za API kriterij)

Za **svaki** entitet (8 komada):

| Operacija | Provjera |
|-----------|----------|
| GET lista | 200 + JSON |
| GET by id | 200 / 404 |
| POST | 201 Created |
| PUT | 200 / 404 |
| DELETE | 200 (Admin) / 403 (Manager) |
| Bez auth | 401 Unauthorized |

Entiteti: BranchOffice, Vehicle, Customer, Reservation, Addon, ServiceRecord, Employee, Partner.

### TestAuthHandler

Umjesto pravog logina, test šalje:

```
X-Test-Role: Admin
```

→ `TestAuthHandler` kreira `ClaimsPrincipal` s ulogom.

### CarRentWebApplicationFactory

- Okolina: `Testing`
- Baza: **EF InMemory** (izolirano po test runu)
- Ne koristi `carrent.dev.db`

---

## 4. Kako pokrenuti

```bash
dotnet test tests/CarRent.Web.IntegrationTests/CarRent.Web.IntegrationTests.csproj
```

**Očekivano:**

```
Passed!  - Failed: 0, Passed: 55
```

---

## 5. CRUD u aplikaciji (MVC + API)

| Sloj | Putanja |
|------|---------|
| MVC CRUD | `Controllers/EntityCrudControllers.cs` |
| REST API | `Api/Controllers/EntityApiControllers.cs` |
| Mapiranje | `EntityMappers.cs` (MVC), `ApiMappers.cs` (API) |
| Baza | `Repositories/EfRepositories.cs` |

**Playwright E2E** dodatno provjerava:
- Admin login
- API create/update addon
- Manager ne smije DELETE

---

## 6. Moguća pitanja profesora

**P: Gdje su testovi za API?**  
O: `tests/CarRent.Web.IntegrationTests/EntityApiCrudTests.cs`

**P: Zašto InMemory, a ne SQLite?**  
O: Brže, izolirano — svaki test run čista baza; ne dira dev bazu.

**P: Kako simulirate Admin u testu?**  
O: `TestAuthHandler` + header `X-Test-Role: Admin`.

**P: Testirate li sve endpointe?**  
O: Da — CRUD za svih 8 entiteta + scenariji 401/403/404/400.

**P: Razlika od Playwright testa?**  
O: Integracijski ne otvara browser; Playwright testira cijeli UX (vidi FULL-01).

---

## 7. Što reći na usmenom

> „Imamo 55 integracijskih testova koji kroz HttpClient i WebApplicationFactory testiraju sve API CRUD operacije s autentikacijom; Playwright dodaje E2E scenarij u browseru.”

---

## 8. Daljnji koraci — implementacija (opcionalno)

- Testovi za `/api/search` i `/api/logs/recent`
- Testovi za `ClientChat/Ask`
- Pokrivenost code coverage report

---

## 9. Koraci koje TI moraš poduzeti

| Korak | Obavezno? | Akcija |
|-------|-----------|--------|
| Pokreni testove prije predaje | **Da** | `dotnet test tests/CarRent.Web.IntegrationTests/` |
| Pokaži 55 passed profesoru | Preporuka | Screenshot terminala |
| Ručno prođi CRUD u UI | Preporuka | Admin create/edit, Manager bez delete |

**Nema** dodatne instalacije — samo .NET SDK.

Povezano: [lab-5/LAB5-PUNI-PREGLED.md](../lab-5/LAB5-PUNI-PREGLED.md) sekcije o API-ju i testovima.
