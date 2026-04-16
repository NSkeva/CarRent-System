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
