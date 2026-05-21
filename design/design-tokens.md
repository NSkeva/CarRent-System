# Design tokens — CarRent Control Surface

Referenca za Open Design i kasniji `site.css`. Trenutna implementacija već koristi slične vrijednosti (Lab 2 glassmorphism).

## Boje

| Token | Hex / vrijednost | Upotreba |
| --- | --- | --- |
| `--bg` | `#0B1020` | Pozadina stranice |
| `--bg-soft` | `#151D33` | Podloga panela / dropdowna |
| `--glass` | `rgba(255,255,255,0.12)` | Stakleni paneli |
| `--line` | `rgba(255,255,255,0.22)` | Rubovi, divideri |
| `--txt` | `#EDF2FF` | Primarni tekst |
| `--muted` | `#A8B2D9` | Sekundarni tekst, breadcrumbs |
| `--acc` | `#8A7DFF` | Primarni akcent (CTA, link hover) |
| `--acc-2` | `#3FD0FF` | Sekundarni akcent (gradijenti) |
| `--success` | `#5DFFB0` | Uspjeh, validno polje |
| `--warning` | `#FFD166` | Upozorenje, servis uskoro |
| `--danger` | `#FF6B7F` | Greška, delete, invalid |
| `--chip-confirmed` | `#3FD0FF33` | Timeline chip — Confirmed |
| `--chip-active` | `#8A7DFF44` | Timeline chip — Active |
| `--chip-completed` | `#5DFFB033` | Timeline chip — Completed |
| `--chip-cancelled` | `#FF6B7F33` | Timeline chip — Cancelled |

### Gradijent pozadine

- Radial: `#1F3164` (20% 0%) → `--bg` (45%)
- Kartice: linear 130deg `rgba(138,125,255,0.24)` → `rgba(63,208,255,0.13)`

## Tipografija

| Uloga | Font | Veličina | Težina |
| --- | --- | --- | --- |
| Brand | Inter | 18–20px | 800 |
| H1 stranice | Inter | 28–32px | 700 |
| H2 sekcije | Inter | 20–22px | 600 |
| Body | Inter / Segoe UI fallback | 15–16px | 400 |
| Small / meta | Inter | 12–13px | 400, `--muted` |
| Tablica header | Inter | 13px | 600, uppercase opcionalno |

## Spacing i radius

| Token | Vrijednost |
| --- | --- |
| `--radius-sm` | 6px |
| `--radius-md` | 10px |
| `--radius-lg` | 16px (paneli) |
| `--space-xs` | 4px |
| `--space-sm` | 8px |
| `--space-md` | 16px |
| `--space-lg` | 24px |
| `--page-max` | 1360px |

## Efekti

- **Glass panel:** `backdrop-filter: blur(16px)`, border 1px `--line`
- **Sticky header:** `backdrop-filter: blur(14px)`, bg `rgba(10,16,34,0.76)`
- **Shadow hover kartice:** blagi glow `0 8px 24px rgba(63,208,255,0.15)`
- **Animacije:** `rise-in` 450ms, `fade-in` 350ms (već u Lab 4 JS)

## Ikone (preporuka za design)

- Line icons (Lucide / Phosphor stil), 20–24px, boja `--muted` ili `--acc-2`
- Akcije u tablici: olovka (edit), kanta (delete), oko (details)

## Breakpointi

| Naziv | Širina | Ponašanje |
| --- | --- | --- |
| Mobile | &lt; 640px | Nav hamburger, tablica → kartice |
| Tablet | 640–980px | 2-col grid, skriveni sidebar |
| Desktop | &gt; 980px | Puna navigacija, max-width shell |
