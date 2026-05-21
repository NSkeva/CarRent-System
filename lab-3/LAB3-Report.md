# Lab 3 Report - EF i Routing (CarRent-System)

## 1. Sto je novo u Lab 3

Lab 3 nadogradjuje Lab 2 web aplikaciju s:

- Entity Framework Core (umjesto mock repozitorija)
- slojevitom arhitekturom: `Model` + `DAL` + `Web`
- inicijalnom migracijom i seed podacima
- prilagodenim routingom (conventional + attribute + alias rute)
- partial viewovima i Partner CRUD formama
- project skillovima za EF, list page i edit form

## 2. Struktura projekta

- `src/CarRent.Model/` - EF-ready entiteti (`[Key]`, `[ForeignKey]`, `virtual ICollection`)
- `src/CarRent.DAL/` - `CarRentDbContext`, seed, migracije
- `src/CarRent.Web/` - MVC UI, EF repozitoriji, routing, viewovi
- `docker-compose.yml` - opcionalni SQL Server
- `lab-3/semantic-model.md` - semanticki DB model
- `lab-3/sitemap.md` - mapa URL-ova
- `.cursor/skills/` - EF, list page, edit form skillovi

## 3. Entity Framework

### Konfiguracija

- Paketi: `Microsoft.EntityFrameworkCore`, `Sqlite`, `SqlServer`, `Design`
- `CarRentDbContext` registriran kroz DI u `Program.cs`
- Connection string u `appsettings.json`
- Default provider: `Sqlite` (`Data/carrent.db`)
- SQL Server varijanta: `DatabaseProvider=SqlServer` + Docker MSSQL

### Migracije

- Inicijalna migracija: `Initial`
- Seed podaci iz Lab 1 prebaceni u `SeedData.Apply(...)` (`HasData`)
- Komanda i upute: `src/CarRent.DAL/migrations-readme.md`

### Prijelaz s mock na EF

- Uklonjen mock `Repositories.cs`
- Dodan `EfRepositories.cs` sa scoped servisima
- Controlleri koriste `async` metode i LINQ upite preko DbContext-a
- `Include(...)` za relacije na Details stranicama

## 4. Routing (custom + default)

### Conventional alias rute (`Program.cs`)

1. `/vozni-park` -> `Fleet/Index`
2. `/dnevni-plan` -> `DailyPlan/Index`
3. `/raspored` -> `Timeline/Index`
4. `/pocetna` -> `Home/Index`

### Attribute rute (4+ akcije)

1. `Home/Index` -> `/` i `/pocetna`
2. `Timeline/Index` -> `/raspored/mjesecni`
3. `DailyPlan/Index` -> `/operativa/dnevni-plan`
4. `Reservation/Details` -> `/rezervacije/pregled/{id}`
5. `Vehicle/ByRegistration` -> `/vozila/reg/{registration}`
6. `Partners` controller -> `/partneri`, `/partneri/novi`, `/partneri/uredi/{id}`

Detaljna mapa: `lab-3/sitemap.md`.

## 5. Partial viewovi

- `_PageHeaderPartial.cshtml` - zajednicki naslov sekcije
- `_PartnerFormPartial.cshtml` - create/edit forma (tag helperi `asp-for`, `asp-action`)

Koristenje:

```cshtml
<partial name="_PageHeaderPartial" model="pageHeader" />
```

## 6. Proširenje aplikacije (Partner CRUD)

Implementiran puni CRUD nad `Partner` entitetom:

- lista (`/partneri`)
- create (`/partneri/novi`)
- edit (`/partneri/uredi/{id}`)
- delete (POST)

Ovo je primjer primjene **mvc-edit-form** i **mvc-list-page** skillova.

## 7. Skillovi

U repozitoriju su dodani:

- `.cursor/skills/entity-framework/SKILL.md`
- `.cursor/skills/mvc-list-page/SKILL.md`
- `.cursor/skills/mvc-edit-form/SKILL.md`

## 8. Pokretanje (Lab 3)

```bash
export DOTNET_ROOT="$PWD/.dotnet"
export PATH="$PWD/.dotnet:$PATH"
export DOTNET_CLI_HOME="$PWD/.dotnet-home"
export NUGET_PACKAGES="$PWD/.nuget/packages"

dotnet build CarRent-System.slnx
dotnet run --project src/CarRent.Web/CarRent.Web.csproj
```

URL: `http://localhost:5000`

### SQL Server (opcionalno)

```bash
docker compose up -d
```

Postavi u `appsettings.Development.json`:

```json
"DatabaseProvider": "SqlServer"
```

## 9. Checklist Lab 3 zahtjeva

- [x] EF konfiguracija (anotacije, veze, DbContext, DI)
- [x] Baza (SQLite lokalno + Docker MSSQL opcija)
- [x] Prijelaz s mock na EF repozitorije
- [x] Inicijalna migracija
- [x] Custom routing (4+ akcije)
- [x] `semantic-model.md`
- [x] `sitemap.md`
- [x] Skill datoteke (EF, list, edit form)
- [x] Partial viewovi
- [x] Proširenje app (Partner CRUD)

## 10. Logovi agenta

- `lab-3/agent_log.txt` — hook `log_ai_lab3.sh` (`.github/hooks.lab3.json`)
- `lab-3/ai_conversation.jsonl` — export Lab 3 dijela transkripta:

```bash
bash .github/hooks/export_cursor_transcript_lab3.sh
```

## 11. Priprema za usmeno

- Objasniti razliku mock repository vs EF DbContext.
- Objasniti zasto su kolekcije `virtual` i sto radi `Include`.
- Objasniti razliku `MapControllerRoute` i `[Route]` atributa.
- Objasniti sto radi partial view i zasto se koristi zajednicka forma za Create/Edit.
