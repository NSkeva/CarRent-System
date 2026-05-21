---
name: entity-framework
description: Koristi za EF Core promjene u CarRent projektu (entiteti, DbContext, migracije, seed).
---

# Entity Framework skill (CarRent)

## Kada koristiti

- dodavanje/izmjena EF entiteta u `src/CarRent.Model`
- promjene `CarRentDbContext` u `src/CarRent.DAL`
- generiranje migracija i seed podataka

## Pravila

1. Svaki entitet mora imati `[Key]` na `Id`.
2. 1-N veze: `virtual ICollection<T>` + `[ForeignKey]` na FK polju.
3. N-N veze: junction entitet + kompozitni kljuc u `OnModelCreating`.
4. Seed podaci idu u `CarRent.DAL/SeedData.cs` preko `HasData`.
5. Ne mijenjaj mock repozitorije - koristi `EfRepositories.cs`.

## Migracije

```bash
dotnet ef migrations add Naziv \
  --project src/CarRent.DAL/CarRent.DAL.csproj \
  --startup-project src/CarRent.Web/CarRent.Web.csproj \
  --context CarRentDbContext
```
