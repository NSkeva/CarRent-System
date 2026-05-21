# CarRent — Design folder (END GOAL)

Ovaj folder služi kao **jedini izvor istine** za vizualni i UX dizajn platforme prije implementacije u MVC aplikaciji (`src/CarRent.Web`).

## Svrha

1. Unijeti sadržaj u **Open Design** (ili drugi alat) i izraditi high-fidelity mockupe.
2. Vratiti design fileove (Figma export, PNG specifikacije, tokeni) u repo ili uz projekt.
3. Implementirati dizajn u kodu **bez gubitka funkcionalnosti** iz Lab 1–4.

## Dokumenti (redoslijed čitanja)

| Datoteka | Sadržaj |
| --- | --- |
| [END-GOAL-Design-Plan.md](./END-GOAL-Design-Plan.md) | **Glavni dokument** — vizija, sve stranice, komponente, flowovi, stanja |
| [design-tokens.md](./design-tokens.md) | Boje, tipografija, spacing — za Open Design / CSS varijable |
| [screen-inventory.md](./screen-inventory.md) | Tablica svih ekrana + prioritet + status (lab / end goal) |
| [open-design-brief.md](./open-design-brief.md) | Kratki brief za paste u Open Design prompt |

## Što je već u kodu (Lab 1–4)

- Domena i EF model (9 entiteta)
- Glassmorphism tamna tema (početak design sustava)
- Stranice: Home, Timeline, Dnevni plan, Vozni park, Partneri
- CRUD + AJAX pretraga + autocomplete + custom datetime + validacija za sve entitete
- Routing (alias + attribute)

## Što END GOAL dodaje na dizajnu

- Profesionalan **operativni dashboard** (ne samo CRUD tablice)
- Konzistentan **design system** (komponente, stanja, pristupačnost)
- Poboljšane **operativne stranice** (Timeline kao Gantt-lite, Fleet kartice s akcijama)
- **Rezervacija s dodacima** (N-N Addon) na jednoj formi
- Potvrde brisanja, toast obavijesti, prazna stanja, skeleton loading
- Responsive + print (Dnevni plan A4)
- Priprema za buduće labove (auth, izvještaji) — označeno kao Faza 2

## Handoff natrag u kod

Kad design fileovi stignu, implementacija ide:

1. Ažurirati `wwwroot/css/site.css` prema tokenima.
2. Refaktorirati `_Layout.cshtml` i Shared partialove.
3. Po ekranu uskladiti Razor viewove (zadržati postojeće akcije kontrolera).
4. Ne dirati plan datoteke labova (`lab-*/Lab*.md`).
