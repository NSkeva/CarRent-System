# Lab 2 Report - HTML Binding (CarRent-System)

## 1. Sto je implementirano

Lab 2 je implementiran kao novi ASP.NET Core MVC projekt:

- `src/CarRent.Web/CarRent.Web.csproj`
- `src/CarRent.Web/Program.cs`

Projekt koristi klasicni MVC pristup (`Controller + View + Model`) i routing:

- `{controller=Home}/{action=Index}/{id?}`

Podaci se ne citaju iz baze, nego iz mock sloja temeljenog na Lab 1 modelu i seed podacima.

## 1.1 Kako pokrenuti program

Preduvjet:
- instaliran .NET SDK (`net10.0` target)

Koraci iz roota repozitorija:

```bash
dotnet --version
dotnet build CarRent-System.slnx
dotnet run --project src/CarRent.Web/CarRent.Web.csproj
```

Nakon pokretanja:
- otvori URL koji ispiše konzola (tipicno `http://localhost:xxxx` ili `https://localhost:xxxx`)
- pocetna stranica je `Home/Index`

Ako je problem s pravima na cache direktorije:

```bash
DOTNET_CLI_HOME="$PWD/.dotnet-home" NUGET_PACKAGES="$PWD/.nuget/packages" dotnet run --project src/CarRent.Web/CarRent.Web.csproj
```

## 2. Poveznica na predavanje (teorija -> kod)

### MVC paradigma

S predavanja:

- Controller obraduje request i priprema model.
- View renderira HTML.
- Model nosi podatke.

U kodu:

- Controlleri: `src/CarRent.Web/Controllers/Controllers.cs`
- Viewovi: `src/CarRent.Web/Views/**`
- Model podaci: `CarRent.Console` entiteti + `CarRent.Web/ViewModels/PageViewModels.cs`

### Routing

S predavanja:

- URL mapiranje ide preko route definicije u `Program.cs`.

U kodu:

- `src/CarRent.Web/Program.cs` s default route pravilom.

### Mock repository + DI

S predavanja:

- Repozitorij vraca staticke podatke (`GetAll`, `GetById`).
- Controller prima dependency kroz konstruktor (DI).

U kodu:

- Repozitoriji: `src/CarRent.Web/Repositories/Repositories.cs`
- Registracija kroz DI: `builder.Services.AddSingleton<...>()` u `Program.cs`
- `SeedSnapshotRepository` koristi `SeedData.Create()` iz Lab 1.

### HTML Binding / strongly-typed view

S predavanja:

- View je tipiziran preko `@model`.
- Podaci se ispisuju kroz `@Model`.

U kodu:

- Svaki `Index` i `Details` view koristi tipizirani model (`@model ...`) i Razor binding.

## 3. Entiteti i obavezne stranice (Index + Details)

Implementirani su `Index` i `Details` prikazi za:

1. Poslovnice (`BranchOffice`)
2. Vozila (`Vehicle`)
3. Kupce (`Customer`)
4. Rezervacije (`Reservation`)
5. Dodatke (`Addon`)
6. Servise (`ServiceRecord`)
7. Zaposlenike (`Employee`)

Controlleri i viewovi su strukturirani po MVC konvenciji:

- `Controllers/XyzController` (u ovom projektu objedinjeni u `Controllers.cs`)
- `Views/Xyz/Index.cshtml`
- `Views/Xyz/Details.cshtml`

## 4. Custom stranice prema zahtjevu i dogovoru

### Timeline

- Putanja: `Timeline/Index`
- Resource schedule month view
- Stupci su dani u mjesecu, redovi su vozila
- Toolbar filteri:
  - text search
  - filter po statusu rezervacije
  - filter po vrsti vozila
  - odabir datuma/mjeseca

### Dnevni plan

- Putanja: `DailyPlan/Index`
- Dva stupca:
  - vozila koja se vracaju danas
  - vozila koja odlaze danas
- Podrzan print (`window.print`) i `@media print` stil za A4.

### Vozni park

- Putanja: `Fleet/Index`
- Karticni grid vozila: slika, opis, naziv, registracija, cijena, poslovnica
- Akcijski gumbi kao priprema za buduci CRUD i operativne module.

### Partneri

- Putanja: `Partners/Index`
- Tablicni prikaz: tvrtka, kontakt osoba, broj, email
- Predvideni edit/delete gumbi kao buduci korak.

## 5. Navigacija

Implementirana je kompletna navigacija:

- Globalni top meni u `Views/Shared/_Layout.cshtml`
- Linkovi s lista na detalje (`asp-action="Details"`, `asp-route-id`)
- Breadcrumb prikaz preko `ViewData["Breadcrumbs"]`

## 6. Unique UX (glassmorphism)

Vizualni sustav je implementiran u:

- `src/CarRent.Web/wwwroot/css/site.css`

Karakteristike:

- stakleni paneli (`backdrop-filter`, transparentne pozadine)
- gradijentna pozadina i naglasci
- custom tipografija, spacing i komponente
- responsive layout
- print fallback bez blur/transparency

Time je ispunjen zahtjev da UX ne bude default bootstrap template.

## 7. Sub-agent za UX i dokaz poziva

Artefakti:

- instrukcije sub-agenta:
  - `lab2/ux-subagent-instructions.md`
- log poziva i smjernice:
  - `lab2/ux-subagent-call-log.md`

## 8. Lab2 log struktura (transcript + agent log)

Dodane su Lab 2 varijante skripti:

- `.github/hooks/log_ai_lab2.sh`
- `.github/hooks/export_cursor_transcript_lab2.sh`
- `.github/hooks/watch_cursor_transcript_lab2.sh`
- `.github/hooks/start_transcript_watch_lab2.sh`
- `.github/hooks/stop_transcript_watch_lab2.sh`
- `.github/hooks.lab2.json`

Izlazne datoteke:

- `lab2/agent_log.txt`
- `lab2/ai_conversation.jsonl`

## 9. Checklist uskladenosti sa zahtjevima iz "Lab 2 - HTML Binding.md"

- Kod u repozitoriju i MVC struktura
- Custom UX sub-agent instruction file commitan
- Log koji dokazuje pozivanje UX sub-agenta
- Mock repository/staticki podaci iz Lab 1
- Index/lista za svaki glavni entitet
- Details stranice za svaki glavni entitet
- Specifična custom stranica (vise njih: timeline, dnevni plan, vozni park, partneri)
- Kompletna navigacija (menu + lista->detalji + breadcrumbs)
- Unique/non-standard UX (glassmorphism)

## 10. Sto je pripremljeno za buducnost

Iako je implementacija uskladena s Lab 2 scopeom, struktura je namjerno pripremljena za nadogradnju:

- kartice voznog parka i partneri imaju akcijske ulaze za buduci CRUD
- timeline moze dobiti napredne scheduler mogucnosti (drag/drop, konflikt pravila)
- dnevni plan se moze spojiti na print/export pipeline i operativne procese
- repository sloj je spreman za zamjenu baze bez lomljenja view/controller sloja