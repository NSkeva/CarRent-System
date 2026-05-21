# CarRent-System

Projekt za kolegij Programiranje web aplikacija u ASP.NET.

## Struktura repozitorija

- `src/CarRent.Console/` - Lab 1 model, seed podaci i LINQ upiti
- `src/CarRent.Model/` - Lab 3 EF entiteti (anotacije i veze)
- `src/CarRent.DAL/` - Lab 3 DbContext, migracije, seed
- `src/CarRent.Web/` - Lab 2/3 ASP.NET Core MVC aplikacija
- `lab-1/` - Lab 1 dokumentacija i logovi
- `lab2/` - Lab 2 upute, report i log artefakti
- `lab-3/` - Lab 3 upute, semantic model, sitemap, report
- `lab-4/` - Lab 4 upute, CRUD/JS report i logovi
- `design/` - END GOAL plan dizajna (Open Design handoff)
- `.cursor/skills/` - project skillovi (EF, list page, edit form)
- `.github/hooks/` - skripte za transcript/agent logging workflow

## Lab 2 (MVC + HTML Binding)

Web aplikacija je u projektu `src/CarRent.Web/` i koristi:

- MVC routing: `{controller=Home}/{action=Index}/{id?}`
- mock repository sloj nad statickim podacima iz Lab 1 (`SeedData`)
- obavezne `Index` i `Details` stranice po entitetima
- custom stranice: `Timeline`, `Dnevni plan`, `Vozni park`, `Partneri`
- unique glassmorphism UI

## Pokretanje (baza lokalno — jedan korak)

**Default (SQLite):** ne trebaš Docker ni zasebno paliti bazu. Kad pokreneš web app:

1. Kreira se mapa `src/CarRent.Web/Data/` (ako ne postoji)
2. Kreira se datoteka `carrent.dev.db` (Development) ili `carrent.db` (Production)
3. `Program.cs` automatski pokrene **migracije** + **seed** podatke
4. Aplikacija sluša na `http://localhost:5000`

```bash
chmod +x scripts/run-local.sh
./scripts/run-local.sh
```

Ili ručno:

```bash
dotnet run --project src/CarRent.Web/CarRent.Web.csproj
```

**SQL Server (opcionalno):** tada moraš **prije** app-a podignuti Docker:

```bash
docker compose up -d
# u appsettings.Development.json: "DatabaseProvider": "SqlServer"
```

Projekti ciljaju `net10.0`.

### Lab2 - tocno pokretanje (provjereno)

Ako na sustavu nemas globalni `dotnet`, pokreni lokalni SDK u repozitoriju:

```bash
mkdir -p .dotnet
curl -fsSL https://dot.net/v1/dotnet-install.sh -o .dotnet/dotnet-install.sh
bash .dotnet/dotnet-install.sh --channel 10.0 --install-dir .dotnet
```

Ako `dotnet-install.sh` javi `Cannot change ownership`, koristi fallback:

```bash
rm -rf .dotnet && mkdir -p .dotnet
curl -fsSL https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.202/dotnet-sdk-10.0.202-linux-x64.tar.gz -o .dotnet/dotnet-sdk.tar.gz
tar -xzf .dotnet/dotnet-sdk.tar.gz --no-same-owner -C .dotnet
rm .dotnet/dotnet-sdk.tar.gz
```

Build + run (s lokalnim cache putanjama):

```bash
DOTNET_CLI_HOME="$PWD/.dotnet-home" NUGET_PACKAGES="$PWD/.nuget/packages" ./.dotnet/dotnet build CarRent-System.slnx
DOTNET_CLI_HOME="$PWD/.dotnet-home" NUGET_PACKAGES="$PWD/.nuget/packages" ./.dotnet/dotnet run --project src/CarRent.Web/CarRent.Web.csproj
```

Nakon pokretanja otvori:

- `http://localhost:5000`

Brza provjera endpointa:

```bash
curl -I http://localhost:5000
```

### Pokretanje s globalnim dotnet (ako je instaliran)

```bash
dotnet --version
dotnet build CarRent-System.slnx
dotnet run --project src/CarRent.Web/CarRent.Web.csproj
```

Opcionalno za konzolni demo:

```bash
dotnet run --project src/CarRent.Console/CarRent.Console.csproj
```

Ako imas problem s pravima na `~/.dotnet` ili NuGet cache:

```bash
DOTNET_CLI_HOME="$PWD/.dotnet-home" NUGET_PACKAGES="$PWD/.nuget/packages" dotnet run --project src/CarRent.Web/CarRent.Web.csproj
```

## Lab 3 (EF + Routing)

Lab 3 dodaje:

- Entity Framework Core (SQLite default, SQL Server preko Docker-a)
- prijelaz s mock repozitorija na EF repozitorije
- inicijalnu migraciju `Initial` + seed podataka
- custom routing (alias + attribute rute)
- partial viewove i Partner CRUD (create/edit/delete)
- dokumentaciju: `lab-3/semantic-model.md`, `lab-3/sitemap.md`, `lab-3/LAB3-Report.md`

### Pokretanje Lab 3

```bash
export DOTNET_ROOT="$PWD/.dotnet"
export PATH="$PWD/.dotnet:$PATH"
DOTNET_CLI_HOME="$PWD/.dotnet-home" NUGET_PACKAGES="$PWD/.nuget/packages" dotnet build CarRent-System.slnx
DOTNET_CLI_HOME="$PWD/.dotnet-home" NUGET_PACKAGES="$PWD/.nuget/packages" dotnet run --project src/CarRent.Web/CarRent.Web.csproj
```

### EF migracije

```bash
dotnet ef database update --project src/CarRent.DAL/CarRent.DAL.csproj --startup-project src/CarRent.Web/CarRent.Web.csproj --context CarRentDbContext
```

Detalji: `src/CarRent.DAL/migrations-readme.md`

### SQL Server (opcionalno)

```bash
docker compose up -d
```

U `appsettings.Development.json` postavi `"DatabaseProvider": "SqlServer"`.

## Lab 4 (CRUD + AJAX + validacija + JS)

Lab 4 dodaje:

- puni CRUD za sve entitete (form ViewModeli + `EntityMappers`)
- AJAX pretragu na listama (`SearchRows` + `site.js`)
- autocomplete dropdown (`LookupApiController`, `_AutocompletePartial`)
- client/server validaciju (jQuery Validate + blur)
- custom datumsko-vremensku kontrolu (`_DateTimePartial`)
- lokalizaciju `hr` / `en-US`

Dokumentacija: `lab-4/LAB4-Report.md`, upute: `lab-4/Lab4.md`.

### Pokretanje Lab 4

Isto kao Lab 3 (`dotnet run --project src/CarRent.Web/CarRent.Web.csproj`).

Korisne rute za test:

- `/BranchOffice`, `/Vehicle`, `/Customer`, `/Reservation`, `/Addon`, `/ServiceRecord`, `/Employee`, `/partneri`
- API lookup: `/api/lookup/customers?q=ana`

## AI log workflow (Lab 1)

Export zadnjeg transkripta:

```bash
bash .github/hooks/export_cursor_transcript.sh
```

Auto watch:

```bash
bash .github/hooks/start_transcript_watch.sh
bash .github/hooks/stop_transcript_watch.sh
```

## AI log workflow (Lab 2)

Lab 2 koristi istu logiku, ali izlaz ide u `lab2/`.

Export:

```bash
bash .github/hooks/export_cursor_transcript_lab2.sh
```

Auto watch:

```bash
bash .github/hooks/start_transcript_watch_lab2.sh
bash .github/hooks/stop_transcript_watch_lab2.sh
```

Hook skripta za agent log:

```bash
bash .github/hooks/log_ai_lab2.sh
```

Ako zelis zasebnu hook konfiguraciju za Lab 2, koristi datoteku:

- `.github/hooks.lab2.json`

## AI log workflow (Lab 3)

Export Lab 3 dijela Cursor transkripta (od upita „pogledaj lab-3” do početka Lab 4):

```bash
bash .github/hooks/export_cursor_transcript_lab3.sh
```

Auto watch:

```bash
bash .github/hooks/start_transcript_watch_lab3.sh
bash .github/hooks/stop_transcript_watch_lab3.sh
```

Hook skripta:

```bash
bash .github/hooks/log_ai_lab3.sh
```

Izlaz:

- `lab-3/ai_conversation.jsonl`
- `lab-3/agent_log.txt`

## AI log workflow (Lab 4)

Export Lab 4 dijela Cursor transkripta (od upita „pogledaj lab-4” do kraja):

```bash
bash .github/hooks/export_cursor_transcript_lab4.sh
```

Auto watch (sinkronizacija u `lab-4/ai_conversation.jsonl`):

```bash
bash .github/hooks/start_transcript_watch_lab4.sh
bash .github/hooks/stop_transcript_watch_lab4.sh
```

Hook skripta:

```bash
bash .github/hooks/log_ai_lab4.sh
```

Hook konfiguracija:

- `.github/hooks.lab4.json`

Izlaz:

- `lab-4/ai_conversation.jsonl`
- `lab-4/agent_log.txt`
