# FULL-02 — Global search (pretraga stranica i podataka)

**Kriterij PDF:** Global search — mogućnost pretrage izbornika, stranica i podataka — **2 boda**  
**Status:** ✅ Implementirano

---

## 1. Teorija — što je global search?

**Global search** je jedan unos (npr. paleta pretrage) koji pretražuje:

- **Navigaciju** (stranice aplikacije)
- **Podatke u bazi** (vozila, kupci, rezervacije…)

Inspiracija: VS Code Command Palette, Spotlight, Ctrl+K u modernim appovima.

**Zašto nije ista stvar kao AJAX pretraga na listi (Lab 4):**

- AJAX na listi filtrira **jednu tablicu** na jednoj stranici
- Global search radi **iz bilo koje stranice** i vraća **mješovite rezultate** (stranica + entitet)

---

## 2. Gdje je u projektu

```
src/CarRent.Web/
├── Services/GlobalSearchService.cs           ← logika pretrage
├── Api/Controllers/GlobalSearchApiController.cs ← REST endpoint
├── Views/Shared/_GlobalSearchPartial.cshtml  ← HTML modal
├── Views/Shared/_Layout.cshtml               ← gumb + partial
└── wwwroot/js/site.js                        ← initGlobalSearch(), Ctrl+K
```

---

## 3. Gdje u kodu — tok zahtjeva

```
Korisnik Ctrl+K ili klik "Pretraga"
    → site.js fetch GET /api/search?q=...
    → GlobalSearchApiController.Get()
    → GlobalSearchService.SearchAsync()
    → DbContext upiti (Vehicle, Customer, Reservation, Addon…)
    → JSON lista rezultata
    → site.js render linkova → klik → navigacija
```

### API


| Metoda | Ruta                  | Auth                                  |
| ------ | --------------------- | ------------------------------------- |
| GET    | `/api/search?q=tekst` | `[Authorize]` — moraš biti prijavljen |


### Što se pretražuje


| Tip rezultata | Primjer                                        |
| ------------- | ---------------------------------------------- |
| `page`        | Početna, Vozila, Rezervacije, Klijentski chat… |
| `vehicle`     | Brand, model, registracija                     |
| `customer`    | Ime, email                                     |
| `reservation` | ID, kupac, vozilo                              |
| `addon`       | Naziv (samo Admin)                             |


**Admin** vidi i admin stranice + dodatke; **Manager** vidi operativu + podatke bez admin-only stranica.

---

## 4. Kako pokazati (demo)

1. Prijavi se kao Admin
2. Pritisni **Ctrl+K** ili klikni **Pretraga** u headeru
3. Upiši npr. `bmw`, `rezerv`, `dodatak`
4. Enter ili klik na rezultat → otvara stranicu

Terminal:

```bash
# Nakon prijave u browseru — ili s MCP ključem:
curl -H "X-Mcp-Key: carrent-mcp-dev-key" "http://localhost:5000/api/search?q=vozilo"
```

---

## 5. Moguća pitanja profesora

**P: Gdje je global search?**  
O: Backend `GlobalSearchService.cs`, API `GlobalSearchApiController.cs`, frontend `site.js` + `_GlobalSearchPartial.cshtml`.

**P: Zašto API, a ne MVC action?**  
O: JavaScript `fetch` očekuje JSON; isti endpoint mogu koristiti MCP alat i testovi.

**P: Zašto zahtijeva prijavu?**  
O: `[Authorize]` na controlleru — Manager ne smije vidjeti admin-only rezultate; servis filtrira po `isAdmin`.

**P: Razlika od `/api/lookup/vehicles`?**  
O: Lookup je za autocomplete na formi (Lab 4); global search pokriva **više entiteta i stranica** odjednom.

---

## 6. Što reći na usmenom

> „Global search je Command Palette: Ctrl+K šalje upit na `/api/search`, servis pretražuje statičke rute i EF entitete, frontend prikaže rezultate i navigira na URL.”

---

## 7. Daljnji koraci — implementacija (opcionalno)

- Fuzzy search (tolerancija na tipfelere)
- Highlight matchanog teksta
- Prečaci tipa „Nova rezervacija” kao akcije
- Indeks u bazi za velike količine podataka

---

## 8. Koraci koje TI moraš poduzeti


| Korak         | Obavezno? | Akcija                                        |
| ------------- | --------- | --------------------------------------------- |
| Pokreni app   | Da        | `./scripts/run-local.sh`                      |
| Prijavi se    | Da        | Search radi samo za autentificirane korisnike |
| Ništa dodatno | —         | Nema API ključeva ili configa                 |


**Napomena:** Gumb „Pretraga” vidljiv je samo kad si prijavljen (`_Layout.cshtml`).