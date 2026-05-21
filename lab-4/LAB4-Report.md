# Lab 4 Report - CRUD, AJAX, validacija, JS kontrole

## 1. Sto je novo u Lab 4

Lab 4 nadogradjuje Lab 3 web aplikaciju s:

- punim CRUD-om za sve entitete (Create / Edit / Delete + Index / Details)
- AJAX pretragom na svim listama (`SearchRows` partial)
- custom autocomplete dropdownom (AJAX lookup API)
- client + server validacijom (DataAnnotations + jQuery Validate + blur u `site.js`)
- custom datumsko-vremenskom kontrolom (`_DateTimePartial` + `site.js`, hr/en format)
- JS animacijama (`rise-in`, `fade-in` na tablicama i karticama)
- lokalizacijom `hr` / `en-US` u `Program.cs`

## 2. Form modeli (odvojeno od entiteta)

`src/CarRent.Web/ViewModels/FormViewModels.cs`:

- `BranchOfficeFormVm`, `VehicleFormVm`, `CustomerFormVm`, `ReservationFormVm`
- `AddonFormVm`, `ServiceRecordFormVm`, `EmployeeFormVm`, `PartnerFormVm`
- pomocni: `AutocompleteFieldVm`, `DateTimeFieldVm`, `LookupItemVm`

Mapiranje entitet <-> forma: `src/CarRent.Web/Services/EntityMappers.cs`.

## 3. CRUD kontroleri

`src/CarRent.Web/Controllers/EntityCrudControllers.cs` - CRUD za:

- BranchOffice, Vehicle, Customer, Reservation, Addon, ServiceRecord, Employee

`Controllers.cs` - Partners CRUD prebacen na `PartnerFormVm`.

Svaki controller ima:

- `Index`, `SearchRows?q=`, `Create` (GET/POST), `Edit` (GET + `[ActionName("Edit")]` POST), `Delete` (POST), `Details`

Poslovna pravila:

- brisanje poslovnice blokirano ako ima vozila (`TempData["Error"]`)

## 4. AJAX pretraga listi

Svaki `Index.cshtml` koristi `_CrudIndexHeaderPartial` s:

- `data-ajax-search` -> `/{Controller}/SearchRows`
- `data-target` -> `#...-rows` tbody

`wwwroot/js/site.js` - debounced `fetch` i zamjena HTML redaka.

## 5. Autocomplete dropdown

- API: `LookupApiController` -> `/api/lookup/customers|vehicles|branches`
- Partial: `Views/Shared/_AutocompletePartial.cshtml`
- Koristenje: Vehicle (poslovnica), Reservation (kupac, vozilo), ServiceRecord (vozilo), Employee (poslovnica)

## 6. Validacija

- Server: `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Phone]` na form VM-ovima + `ModelState.IsValid`
- Client: `_ValidationScriptsPartial` (jQuery Validate unobtrusive) + blur handler u `site.js`
- Poruke: `<span asp-validation-for="..." class="field-error">`

## 7. Datumska kontrola

- Partial: `Views/Shared/_DateTimePartial.cshtml`
- JS picker u `site.js` (bez browser default `type="date"`)
- Format ovisno o `navigator.language` (hr: `dd.mm.yyyy hh:mm`, en: `MM/dd/yyyy hh:mm`)
- Primjena: Customer `DateOfBirth`, Reservation `StartDate`/`EndDate`, Employee `HiredAt`, ServiceRecord datumi

## 8. JavaScript animacije

- `rise-in` na `.glass-card`, `.fleet-card`, `.panel`
- `fade-in` nakon AJAX refresha tablice

## 9. Test plan (rucno)

1. Pokreni app (`dotnet run --project src/CarRent.Web/CarRent.Web.csproj`)
2. Za svaki entitet: otvori Index, upisi tekst u pretragu, provjeri AJAX refresh
3. Create novi zapis s praznim obaveznim poljima -> validacijske poruke
4. Create valjan zapis -> redirect na Index
5. Edit postojeci zapis -> spremi promjene
6. Delete zapis -> nestaje s liste
7. Reservation forma: autocomplete kupca/vozila + custom datumi
8. BranchOffice s vozilima: delete -> poruka greske
9. Promijeni jezik preglednika na en -> provjeri format datuma u pickeru

## 10. Logovi agenta

- `lab-4/agent_log.txt` — hook `log_ai_lab4.sh` (`.github/hooks.lab4.json`)
- `lab-4/ai_conversation.jsonl` — export Cursor transkripta

```bash
bash .github/hooks/export_cursor_transcript_lab4.sh
```

Skripta izvozi samo Lab 4 razgovor iz glavnog Cursor transkripta (marker: „pogledaj lab-4”).
