# Inventar ekrana — CarRent

Legenda statusa:

- **L2** Lab 2 — postoji osnovni UI
- **L3** Lab 3 — EF + routing
- **L4** Lab 4 — CRUD + AJAX + validacija
- **EG** END GOAL — ciljani dizajn (mockup + implementacija nakon handoffa)
- **F2** Faza 2 — izvan trenutnih labova, samo u viziji

| ID | Ekran | URL (cilj) | Status koda | END GOAL prioritet |
| --- | --- | --- | --- | --- |
| S01 | Home / Dashboard | `/`, `/pocetna` | L2+L3 | P1 — redesign KPI + quick actions |
| S02 | Timeline | `/raspored`, `/raspored/mjesecni` | L2 | P1 — Gantt-lite vizual |
| S03 | Dnevni plan | `/dnevni-plan` | L2 | P1 — print + kartice događaja |
| S04 | Vozni park | `/vozni-park` | L2 | P1 — funkcionalne akcije na kartici |
| S05 | Partneri — lista | `/partneri` | L3+L4 | P2 |
| S06 | Partneri — nova | `/partneri/novi` | L4 | P2 |
| S07 | Partneri — uredi | `/partneri/uredi/{id}` | L4 | P2 |
| S08 | Poslovnice — lista | `/BranchOffice` | L4 | P2 |
| S09 | Poslovnice — CRUD | Create/Edit | L4 | P2 |
| S10 | Poslovnice — detalji | Details/{id} | L3 | P2 — povezana vozila |
| S11 | Vozila — lista | `/Vehicle` | L4 | P1 |
| S12 | Vozila — CRUD | Create/Edit | L4 | P2 |
| S13 | Vozila — detalji | Details/{id} | L3 | P1 — servisi + rezervacije |
| S14 | Kupci — lista | `/Customer` | L4 | P2 |
| S15 | Kupci — CRUD | Create/Edit | L4 | P2 |
| S16 | Kupci — detalji | Details/{id} | L3 | P2 |
| S17 | Rezervacije — lista | `/Reservation` | L4 | P1 |
| S18 | Rezervacije — CRUD | Create/Edit | L4 | P1 — dodaci N-N |
| S19 | Rezervacije — detalji | `/rezervacije/pregled/{id}` | L3 | P1 |
| S20 | Dodaci — lista/CRUD | `/Addon` | L4 | P3 |
| S21 | Servisi — lista/CRUD | `/ServiceRecord` | L4 | P2 |
| S22 | Zaposlenici — lista/CRUD | `/Employee` | L4 | P3 |
| G01 | Global shell | Layout | L2+L4 | P1 — nav grupe, user zona |
| G02 | Prazno stanje | Inline | djelomično | P1 |
| G03 | Toast / flash | TempData | osnovno | P1 |
| G04 | Modal potvrde brisanja | — | nema | P1 EG |
| G05 | 404 / Error | — | F2 | P3 |
| F2-01 | Login | — | F2 | — |
| F2-02 | Izvještaji | — | F2 | — |

**Ukupno za Open Design (prva iteracija):** S01–S04, S11–S13, S17–S19, G01–G04 + jedan CRUD primjer (S12 ili S18).
