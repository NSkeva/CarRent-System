# Brief za Open Design — CarRent Control Surface

Kopiraj ovaj tekst u Open Design kao početni prompt, uz prilog `END-GOAL-Design-Plan.md`.

---

## Projekt

**CarRent Control Surface** — interna web aplikacija za upravljanje rent-a-car operacijama (flota, rezervacije, servisi, partneri, poslovnice). Korisnici su **zaposlenici rent agencije** (operateri, dispečeri, admin), ne krajnji klijenti koji rezerviraju online.

## Vizualni smjer

- **Dark glassmorphism** — tamna pozadina, poluprozirni paneli, blur, tanki svijetli rubovi.
- Akcenti: ljubičasta `#8A7DFF` + cyan `#3FD0FF`.
- Font: **Inter**. Profesionalno, moderno, “control center” / fleet operations, ne consumer booking app.
- Izbjegavati generički Bootstrap look; custom komponente.

## Obavezni ekrani (prva iteracija mockupa)

1. **Dashboard (Home)** — 4 KPI kartice + quick links + opcionalno mini graf aktivnosti.
2. **Timeline** — mjesečni raspored: vozila × dani, rezervacije kao obojeni chipovi po statusu.
3. **Dnevni plan** — dvije kolone: povrati danas / odlasci danas, date picker, print-friendly.
4. **Vozni park** — grid kartica vozila (slika, reg, poslovnica, cijena, akcije).
5. **Rezervacije — lista** — tablica + AJAX search + status badge.
6. **Rezervacije — forma (Create/Edit)** — autocomplete kupac/vozilo, custom datetime, dodaci (checkbox lista), validacija.
7. **Vozila — detalji** — hero + tabovi/sekcije: podaci, servisi, rezervacije.
8. **Global layout** — sticky header, grupirana navigacija, breadcrumbs, footer minimal.

## Komponente koje moraju biti u design systemu

- Glass panel, KPI card, data table, CRUD toolbar (search + primary button)
- Autocomplete dropdown (AJAX)
- Custom date-time picker (ne native browser)
- Status chips (rezervacija, servis)
- Primary / secondary / danger button, link-style delete
- Field validation states (error/success border)
- Empty state ilustracija + CTA
- Toast notification
- Delete confirmation modal

## Tehnički kontekst (za realističan dizajn)

Implementacija: **ASP.NET Core MVC**, Razor views, postojeći URL-ovi iz sitemap-a. Dizajn mora biti izvediv u HTML/CSS + malo JS (bez Reacta). Tablice se djelomično osvježavaju AJAX-om (zamjena tbody).

## Output koji očekujemo iz Open Designa

- Desktop 1440px + mobile 390px za ključne ekrane
- Komponent library (buttons, inputs, table, cards)
- Export tokena (boje, font, radius) usklađen s `design/design-tokens.md`
- PNG ili Figma link za handoff

Detalji po ekranu: vidi `design/END-GOAL-Design-Plan.md`.
