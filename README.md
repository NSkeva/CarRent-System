# CarRent-System

Projekt za kolegij Programiranje web aplikacija u ASP.NET.

## Status

Repozitorij je inicijalno postavljen.
Opis projekta, arhitektura i upute za pokretanje bit ce dodani naknadno.

Lab 1 dokumentacija i inicijalni kod nalaze se u `lab-1/` i `src/CarRent.Console/`.

## Struktura

- `lab-1/` - materijali i logovi za Lab 1
- `.github/hooks/` - skripte za lokalne hookove
- `.github/hooks.json` - konfiguracija hookova
- `src/CarRent.Console/` - inicijalni C# projekt (model, seed, LINQ, async demo)

## AI log za predaju

Ako hook eventovi ne upisuju sve poruke, koristi export cijelog Cursor transkripta:

```bash
bash .github/hooks/export_cursor_transcript.sh
```

To ce kopirati zadnji kompletni transcript u `lab-1/ai_conversation.jsonl`.

Za automatsko osvjezavanje tijekom rada:

```bash
bash .github/hooks/start_transcript_watch.sh
```

Za zaustavljanje:

```bash
bash .github/hooks/stop_transcript_watch.sh
```

## Pokretanje C# koda

Projekt je postavljen na `net10.0`.

Za lokalni run/build:

```bash
dotnet --version
dotnet build CarRent-System.slnx
dotnet run --project src/CarRent.Console/CarRent.Console.csproj
dotnet test CarRent-System.slnx
```

Ako imas problem s pravima na `~/.dotnet` ili NuGet cache, koristi:

```bash
DOTNET_CLI_HOME="$PWD/.dotnet-home" NUGET_PACKAGES="$PWD/.nuget/packages" dotnet run --project src/CarRent.Console/CarRent.Console.csproj
```
