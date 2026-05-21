---
name: mvc-list-page
description: Koristi za izradu novih MVC list (Index) stranica u CarRent.Web.
---

# MVC List Page skill

## Koraci

1. Dodaj EF entitet (ako ne postoji) u `CarRent.Model`.
2. Dodaj repository metode `GetAllAsync` / `GetByIdAsync` u `EfRepositories.cs`.
3. Kreiraj controller s `Index` akcijom.
4. Kreiraj `Views/{Controller}/Index.cshtml` s tablicom i linkom na Details.
5. Dodaj navigacijski link u `_Layout.cshtml`.
6. Azuriraj `lab-3/sitemap.md`.

## Primjer strukture view-a

- naslov panel
- `table.data-table`
- stupci entiteta
- link `asp-action="Details" asp-route-id`
