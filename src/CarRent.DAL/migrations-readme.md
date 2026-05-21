# EF migracije - CarRent

## Preduvjeti

```bash
export DOTNET_ROOT="$PWD/.dotnet"
export PATH="$PWD/.dotnet:$PATH"
export DOTNET_CLI_HOME="$PWD/.dotnet-home"
export NUGET_PACKAGES="$PWD/.nuget/packages"
```

## Kreiranje migracije

```bash
dotnet ef migrations add NazivMigracije \
  --project src/CarRent.DAL/CarRent.DAL.csproj \
  --startup-project src/CarRent.Web/CarRent.Web.csproj \
  --context CarRentDbContext
```

## Primjena migracije

```bash
dotnet ef database update \
  --project src/CarRent.DAL/CarRent.DAL.csproj \
  --startup-project src/CarRent.Web/CarRent.Web.csproj \
  --context CarRentDbContext
```

Napomena: pri pokretanju web app-a migracije se takoder automatski primjenjuju (`db.Database.Migrate()` u `Program.cs`).
