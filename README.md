# CarRent-System

Projekt za kolegij Programiranje web aplikacija u ASP.NET.

## Struktura repozitorija

- `src/CarRent.Console/` - Lab 1 model, seed podaci i LINQ upiti
- `src/CarRent.Web/` - Lab 2 ASP.NET Core MVC aplikacija (HTML binding)
- `lab-1/` - Lab 1 dokumentacija i logovi
- `lab2/` - Lab 2 upute, report i log artefakti
- `.github/hooks/` - skripte za transcript/agent logging workflow

## Lab 2 (MVC + HTML Binding)

Web aplikacija je u projektu `src/CarRent.Web/` i koristi:

- MVC routing: `{controller=Home}/{action=Index}/{id?}`
- mock repository sloj nad statickim podacima iz Lab 1 (`SeedData`)
- obavezne `Index` i `Details` stranice po entitetima
- custom stranice: `Timeline`, `Dnevni plan`, `Vozni park`, `Partneri`
- unique glassmorphism UI

## Pokretanje

Projekti ciljaju `net10.0`.

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
