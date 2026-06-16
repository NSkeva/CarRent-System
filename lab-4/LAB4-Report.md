# Lab 4 Report - CRUD, AJAX, validacija, JS kontroles

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

## 11. Usmeno — pitanja profesora i kako objasniti

Kratki vodič za usmenu: što bi profesor mogao pitati, gdje je to u kodu i kako to objasniti jednostavno.

---

### CRUD općenito

**P: Gdje je implementiran CRUD i za koje entitete?**

**O:** U `src/CarRent.Web/Controllers/EntityCrudControllers.cs` — zaseban controller po entitetu (BranchOffice, Vehicle, Customer, Reservation, Addon, ServiceRecord, Employee). Partneri su u `Controllers.cs` jer su bili već u Lab 3. Svaki controller ima `Index`, `Details`, `Create` (GET/POST), `Edit` (GET/POST), `Delete` (POST) i `SearchRows` za AJAX pretragu.

**P: Zašto Create i Edit ne koriste direktno EF entitet na formi?**

**O:** Forme koriste ViewModele u `FormViewModels.cs` (npr. `VehicleFormVm`), a mapiranje entitet ↔ forma ide kroz `EntityMappers.cs`. Razlog: forma ima drugačija polja od entiteta (npr. autocomplete ID-evi, format datuma), validacijske atribute i ne želimo izložiti cijeli entitet u viewu.

**P: Kako Create radi od klika na „Spremi” do baze?**

**O:** Korisnik pošalje POST na npr. `/Vehicle/Create` → controller provjeri `ModelState.IsValid` → `EntityMappers.ToEntity(formVm)` napravi entitet → `repository.AddAsync(entity)` → EF `SaveChanges` u repozitoriju → redirect na `Index`. Ako validacija padne, vraća se ista forma s porukama.

**P: Kako Edit razlikuje od Create?**

**O:** GET `Edit/{id}` učita zapis iz baze, mapper ga pretvori u `*FormVm` i popuni formu. POST `Edit` prima isti VM, ponovno validira, mapira na entitet, postavi `Id` i pozove `UpdateAsync`. U kodu je `[ActionName("Edit")]` na POST metodi da URL ostane `/Vehicle/Edit/5`.

**P: Kako Delete radi i zašto neki delete ne prolazi?**

**O:** Delete je POST (npr. gumb u listi s anti-forgery tokenom). Controller dohvati entitet po ID-u i pozove `DeleteAsync`. Za poslovnicu (`BranchOffice`) prije brisanja provjeravamo ima li vozila — ako ima, ne brišemo nego stavimo poruku u `TempData["Error"]` i vratimo korisnika na listu. To je poslovno pravilo umjesto cascade delete.

**P: Što je `SearchRows` i zašto nije običan submit forme?**

**O:** `SearchRows(string? q)` vraća samo partial view s redovima tablice (`_IndexRows`), ne cijelu stranicu. JavaScript na klijentu šalje `fetch` na tu akciju dok korisnik tipka u polje pretrage — to je AJAX pretraga bez reloada stranice.

---

### AJAX pretraga na listama

**P: Gdje se AJAX pretraga „palí” u browseru?**

**O:** U `wwwroot/js/site.js` — sluša `input` na elementima s `data-ajax-search` i `data-target`. Debounce (~300 ms) čeka da korisnik prestane tipkati, zatim `fetch(url + ?q=...)` i zamijeni HTML unutar ciljnog elementa (npr. `#vehicle-rows` tbody).

**P: Gdje je to povezano u Razor viewu?**

**O:** `Views/Shared/_CrudIndexHeaderPartial.cshtml` — polje za pretragu ima atribute `data-ajax-search="/Vehicle/SearchRows"` i `data-target="#vehicle-rows"`. Index stranica uključuje taj partial i ima prazan tbody s tim ID-om koji se puni iz servera.

**P: Što server vraća AJAX pretrazi?**

**O:** Samo HTML fragment — partial `Views/{Entity}/_IndexRows.cshtml` renderiran s filtriranom listom iz repozitorija (`GetAllAsync(q)` ili slično). Nema JSON-a; zamjena je „server-side HTML swap”.

---

### Autocomplete dropdown

**P: Gdje je autocomplete i kako koristi AJAX?**

**O:** Tri dijela:

1. **API** — `LookupApiController.cs`, rute `/api/lookup/customers`, `vehicles`, `branches` — vraćaju JSON listu (`id`, `label`) filtriranu po `q`.
2. **Partial** — `Views/Shared/_AutocompletePartial.cshtml` — skriveno polje za ID + vidljivo polje za tekst + dropdown lista.
3. **JS** — `site.js` na `input` šalje `fetch` na API, prikazuje rezultate, na odabir upisuje label i ID u skriveno polje.

**P: Zašto autocomplete nije običan `<select>`?**

**O:** Jer ima puno zapisa (kupci, vozila) — učitavanje svega odjednom bi bilo sporo. AJAX dohvaća samo prvih N rezultata koji odgovaraju upitu dok korisnik tipka.

**P: Gdje se autocomplete koristi u formama?**

**O:** Npr. rezervacija — kupac i vozilo; vozilo — poslovnica; servis — vozilo; zaposlenik — poslovnica. U `FormViewModels` imamo `AutocompleteFieldVm` ili ID + display polja koja partial renderira.

---

### Validacija (client + server)

**P: Gdje je server-side validacija?**

**O:** Na ViewModel klasama u `FormViewModels.cs` — atributi `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, custom `[Phone]`. U controlleru prije spremanja: `if (!ModelState.IsValid) return View(model);`.

**P: Gdje je client-side validacija i zašto na blur?**

**O:** Dva sloja:

1. **jQuery Validate unobtrusive** — `_ValidationScriptsPartial.cshtml` u layoutu/formi, čita `data-val-`* atribute koje generira ASP.NET iz DataAnnotations.
2. **Custom blur** u `site.js` — kad polje izgubi fokus, ručno označi grešku / poruku (zahtjev laba: validacija se okida na blur).

**P: Gdje se prikazuju poruke korisniku?**

**O:** U form viewovima: `<span asp-validation-for="Email" class="field-error">` — server renderira greške nakon POST-a; jQuery Validate dodaje iste klase na klijentu prije slanja.

**P: Zašto mora postojati i server validacija ako imamo JS?**

**O:** Klijent se može zaobići (DevTools, curl). Server je jedini pouzdan — uvijek provjeravamo `ModelState` prije pisanja u bazu.

---

### Datumska kontrola (partial view)

**P: Gdje je custom datum+vrijeme i zašto nije browser `type="date"`?**

**O:** Partial `Views/Shared/_DateTimePartial.cshtml` + logika u `site.js`. Lab eksplicitno traži da ne koristimo default browser datepicker — kontrola je naša (kalendar/vrijeme u JS-u ili tekstualni unos s parserom).

**P: Kako radi hr vs en format?**

**O:** U `site.js` čitamo `navigator.language` (ili postavke lokalizacije app-a). Za `hr` očekujemo npr. `dd.mm.yyyy hh:mm`, za `en-US` npr. `MM/dd/yyyy hh:mm`. Pri slanju forme vrijednost se normalizira u format koji server/EF razumije.

**P: Gdje je lokalizacija aplikacije postavljena?**

**O:** U `Program.cs` — `AddLocalization`, `UseRequestLocalization` s kulturama `hr` i `en-US`. To utječe na format brojeva/datuma na serveru; datumski picker na klijentu prati jezik preglednika prema zahtjevu laba.

**P: Na kojim formama se koristi datumska kontrola?**

**O:** Customer (`DateOfBirth`), Reservation (`StartDate`, `EndDate`), Employee (`HiredAt`), ServiceRecord (datumi servisa) — svugdje gdje entitet ima `DateTime` polje, umjesto običnog `<input type="datetime-local">`.

---

### JavaScript animacije

**P: Gdje su animacije i čemu služe?**

**O:** U `wwwroot/css/site.css` (ili `glass.css`) — klase `rise-in`, `fade-in` s `@keyframes`. U `site.js` nakon AJAX zamjene redova tablice dodajemo `fade-in` da prijelaz bude vidljiv. Na karticama/panelima (`glass-card`, `fleet-card`) `rise-in` pri učitavanju stranice — ilustrira „napredno” korištenje JS/CSS, ne samo statičan HTML.

**P: Jesu li animacije samo ukras?**

**O:** Da, ali u sklopu laba služe kao dokaz kontrole DOM-a nakon dinamičkog sadržaja (npr. nakon `fetch` i `innerHTML` zamjene redova).

---

### Entity Framework i CRUD u pozadini

**P: Gdje se zapisi stvarno spremaju u bazu?**

**O:** U EF repozitorijima (`EfRepositories.cs` ili slično u Web projektu) — `AddAsync` → `context.Set<T>().Add`, `UpdateAsync` → attach/update, `DeleteAsync` → `Remove` ili provjera relacija. `SaveChangesAsync` šalje SQL prema SQLite/SQL Server bazi iz Lab 3.

**P: Razlika između soft delete i našeg brisanja poslovnice?**

**O:** Soft delete (iz predavanja) = polje `DeletedAt`, zapis ostaje u bazi. Mi za poslovnicu radimo **zabranu brisanja** ako postoje vozila — to je poslovno pravilo, ne cascade. Ostali entiteti mogu ići na fizički `Remove` ako EF i relacije dopuste.

**P: Što ako profesor pita cascade delete?**

**O:** U EF konfiguraciji (`CarRentDbContext` / fluent API) može se postaviti `OnDelete(DeleteBehavior.Cascade)` ili `Restrict`. U Lab 4 smo za kritične slučajeve eksplicitno provjerili u controlleru prije `Delete` — sigurnije za demonstraciju i jasnije korisniku (`TempData` poruka).

---

### Brzi „mapa datoteka” za usmeno


| Tema               | Gdje pogledati                                                         |
| ------------------ | ---------------------------------------------------------------------- |
| CRUD akcije        | `Controllers/EntityCrudControllers.cs`, `Controllers.cs` (Partneri)    |
| Forme / validacija | `ViewModels/FormViewModels.cs`, `Views/*/Create.cshtml`, `Edit.cshtml` |
| Mapiranje          | `Services/EntityMappers.cs`                                            |
| AJAX lista         | `site.js`, `_CrudIndexHeaderPartial.cshtml`, `*/_IndexRows.cshtml`     |
| Autocomplete API   | `Controllers/LookupApiController.cs`, `_AutocompletePartial.cshtml`    |
| Datum              | `_DateTimePartial.cshtml`, `site.js`                                   |
| Baza               | `Repositories/EfRepositories.cs`, `CarRent.DAL/CarRentDbContext.cs`    |
| Lokalizacija       | `Program.cs`                                                           |


---

### Jedna rečenica za cijeli Lab 4 (ako traže sažetak)

Lab 4 je MVC aplikacija s punim CRUD-om preko form ViewModela i EF repozitorija, AJAX pretragom listi i autocomplete dropdownom koji dohvaćaju podatke s API-ja, dvostrukom validacijom (blur + server), custom datumskom kontrolom preko partial viewa i JS animacijama — sve povezano kroz postojeću Lab 3 arhitekturu bez mijenjanja plan datoteka labova.