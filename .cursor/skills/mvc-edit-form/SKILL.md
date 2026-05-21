---
name: mvc-edit-form
description: Koristi za izradu MVC Create/Edit formi s tag helperima i EF spremanjem.
---

# MVC Edit Form skill

## Koraci

1. Controller akcije: `Create` (GET/POST), `Edit` (GET/POST), opcionalno `Delete` (POST).
2. Koristi `[ValidateAntiForgeryToken]` na POST akcijama.
3. Kreiraj partial `_XyzFormPartial.cshtml` s:
   - `asp-for` inputima
   - `form asp-action` tag helperom
4. Create/Edit view pozivaju partial preko `<partial name="_XyzFormPartial" model="Model" />`.
5. Repository metode: `AddAsync`, `UpdateAsync`, `DeleteAsync`.

## Referenca u projektu

- `PartnersController` + `Views/Shared/_PartnerFormPartial.cshtml`
