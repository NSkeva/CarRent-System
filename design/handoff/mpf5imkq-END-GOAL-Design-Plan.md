# END GOAL — Plan dizajna platforme CarRent Control Surface

**Verzija:** 1.0  
**Datum:** 2026-05-21  
**Namjena:** Detaljna specifikacija za Open Design → mockupi → implementacija u `src/CarRent.Web`  
**Oslonac na labove:** Lab 1 (domena), Lab 2 (MVC + glass UI + custom stranice), Lab 3 (EF + routing), Lab 4 (CRUD + AJAX + validacija + JS kontrole)

---

## 1. Produktna vizija (END GOAL)

### 1.1 Što platforma jest

Jedinstveno **operativno sučelje** za rent-a-car tvrtku koje omogućuje:

- pregled stanja flote i rezervacija u realnom vremenu (dashboard),
- planiranje rada po danu i mjesecu (dnevni plan, timeline),
- upravljanje master podacima (vozila, kupci, poslovnice, zaposlenici, partneri, dodaci),
- vođenje rezervacija i servisnih zapisa,
- brzu pretragu i uređivanje bez punog reloada stranice gdje je to moguće.

### 1.2 Što platforma NIJE (u ovoj fazi)

- Javni B2C booking portal za krajnje korisnike.
- Mobilna native aplikacija (responsive web je dovoljan).
- Potpuni ERP (računovodstvo, plaće) — fokus je operativa renta.

### 1.3 Korisničke uloge (cilj)

| Uloga | Opis | Faza |
| --- | --- | --- |
| Operater | Rezervacije, dnevni plan, kupci | Lab 1–4 + EG |
| Fleet manager | Vozila, servisi, vozni park | Lab 1–4 + EG |
| Admin | Poslovnice, zaposlenici, partneri, postavke | Lab 1–4 + EG |
| Gost / neprijavljen | Nema pristupa | Faza 2 (login) |

### 1.4 Ključni principi UX-a

1. **Operativna brzina** — najčešći zadaci (današnji odlasci/povrati, nova rezervacija) ≤ 3 klika s dashboarda.
2. **Kontekst** — svaki detalj entiteta pokazuje povezane zapise (vozilo → servisi + rezervacije).
3. **Sigurnost unosa** — validacija na blur i submit; destruktivne akcije s potvrdom.
4. **Konzistentnost** — isti obrasci za sve CRUD entitete (toolbar, tablica, forma, detalji).
5. **Pristupačnost** — kontrast na tamnoj temi, fokus stanja, keyboard za autocomplete i datum.

---

## 2. Informacijska arhitektura

### 2.1 Primarna navigacija (END GOAL layout)

Header je podijeljen u **zone**:

```
[Logo: CarRent Control Surface]  |  Operativa ▾  |  Podaci ▾  |  [🔍 Global search?]  |  [HR/EN]  |  [Korisnik]
```

**Operativa** (Lab 2 custom stranice — prioritet dizajna):

- Početna (Dashboard)
- Timeline (Mjesečni raspored)
- Dnevni plan
- Vozni park

**Podaci** (Lab 3/4 CRUD):

- Rezervacije
- Vozila
- Kupci
- Servisi
- Poslovnice
- Dodaci
- Zaposlenici
- Partneri

Na mobilnom: hamburger → full-screen drawer s istim grupama.

### 2.2 Breadcrumbs

Format: `Početna › Sekcija › [Entitet ›] Akcija`

Primjeri:

- `Početna › Rezervacije › #1042`
- `Početna › Vozila › Uredi › ZG-123-AB`

Breadcrumbs su ispod headera, boja `--muted`, 14px.

### 2.3 URL strategija (zadržati iz Lab 3)

Kratki HR aliasi za operativu, RESTful controller rute za CRUD. Design ne smije zahtijevati drukčije URL-ove osim ako se eksplicitno dogovori refaktor.

---

## 3. Globalni shell i komponente

### 3.1 App shell (wireframe)

```
┌─────────────────────────────────────────────────────────────┐
│ STICKY HEADER (glass, blur)                                  │
├─────────────────────────────────────────────────────────────┤
│ Breadcrumbs (opcionalno)                                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  PAGE CONTENT (max 1360px, centrirano, grid gap 16–24px)    │
│                                                              │
│  ┌─ Panel (glass) ─────────────────────────────────────┐   │
│  │ Page title + actions                                 │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌─ Panel ─────────────────────────────────────────────┐   │
│  │ Main content (table / form / cards)                  │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 3.2 Komponente design sustava

| Komponenta | Opis | Lab status | END GOAL |
| --- | --- | --- | --- |
| **Glass Panel** | Zaobljeni container, blur, border | L2 | Dotjerati padding i shadow |
| **KPI Card** | Broj + label + opcionalna ikona | L2 | Trend strelica, klik → filter |
| **Quick Link Card** | Naslov + podnaslov + hover glow | L2 | Ikona + badge “Novo” |
| **CRUD Toolbar** | Naslov, Primary CTA, search | L4 | Filter chips, export CSV (F2) |
| **Data Table** | Sortabilni stupci, akcije u retku | L4 | Row hover, zebra, sticky header |
| **Fleet Card** | Slika, meta, action bar | L2 | Funkcionalni linkovi, status badge |
| **Autocomplete** | Text + hidden ID + dropdown lista | L4 | Loading spinner, “Nema rezultata” |
| **DateTime Picker** | Custom kalendar + sat/min | L4 | Bolji UX mjesec navigacije |
| **Status Chip** | Boja po enumu | L2 djelomično | Legenda boja na Timelineu |
| **Form Field** | Label, input, error span | L4 | Help text, required asterisk |
| **Btn Primary** | Gradijent akcent | L4 | Disabled, loading |
| **Btn Danger** | Delete / destructive | L4 | Samo u modalu za delete |
| **Toast** | Uspjeh / greška | TempData | Fixed top-right, auto-dismiss |
| **Empty State** | Ilustracija + tekst + CTA | Nedostaje | Obavezno na svim listama |
| **Confirm Modal** | Delete potvrda | Nedostaje | Obavezno EG |
| **Skeleton** | Loading placeholder | Nedostaje | AJAX search i stranica |

### 3.3 Interakcije i animacije (zadržati + proširiti)

- **rise-in** pri učitavanju kartica/panela (stagger 40ms) — Lab 4.
- **fade-in** pri AJAX refreshu tablice — Lab 4.
- Hover na karticama: blagi lift + glow.
- Modal: fade + scale 0.96 → 1.
- Ne koristiti agresivne animacije na Timeline tablici (performanse).

### 3.4 Obavijesti

| Tip | Trigger | Izgled |
| --- | --- | --- |
| Uspjeh | CRUD save, delete OK | Zeleni toast, 4s |
| Greška | Validacija, business rule | Crveni toast ili inline banner |
| Upozorenje | Npr. brisanje poslovnice blokirano | Žuti banner iznad tablice (već `TempData`) |

---

## 4. Domena podataka (što UI mora podržati)

Iz `semantic-model.md` — dizajn mora predvidjeti prikaz ovih polja:

### 4.1 Enumeracije (boje chipova)

**ReservationStatus:** Draft (siva), Confirmed (cyan), Active (ljubičasta), Completed (zelena), Cancelled (crvena).

**ServiceStatus:** Planned, InProgress, Completed, Cancelled — analogno.

**VehicleType:** Car, Van, Scooter, Motorcycle, Bicycle — ikona + tekst na Fleet i filterima.

**LocationType:** MainOffice, Airport, Downtown, HotelPartner, RemoteDropoff — na rezervaciji pickup/dropoff.

### 4.2 Relacije koje utječu na UI

- Poslovnica → lista vozila na Details (brisanje blokirano ako ima vozila).
- Vozilo → servisi + rezervacije na Details.
- Kupac → povijest rezervacija na Details.
- Rezervacija → kupac, vozilo, dodaci (ReservationAddon: naziv, cijena, količina).

---

## 5. Specifikacija ekrana (detaljno)

---

### S01 — Home / Dashboard

**URL:** `/`, `/pocetna`  
**Svrha:** Ulazna točka; odgovor na “što danas zahtijeva pažnju?”.

#### Layout (END GOAL)

1. **Hero sekcija** (glass panel)
   - Naslov: “Fleet Pulse” ili “Operativni pregled”
   - Podnaslov: jedna rečenica + datum/dan (dinamički)
   - Desno: quick action gumb **+ Nova rezervacija** (primary)

2. **KPI red** (4 kartice, responsive 2×2 na tabletu)
   - Ukupno vozila (klik → Vozni park)
   - Aktivne rezervacije (klik → Rezervacije filtrirane Active+Confirmed)
   - Broj poslovnica
   - Servisi u sljedećih 30 dana (klik → Servisi)

   Svaka kartica: ikona, broj (veliki), label, opcionalno “+2 od jučer” (F2).

3. **Quick links** (grid 2–4 kolone)
   - Timeline, Dnevni plan, Vozni park, Partneri
   - Kartica: naslov, 1 linija opisa, strelica ili ikona

4. **Opcionalno F2:** “Današnji događaji” — mini lista 3 odlaska + 3 povrata s linkom na Dnevni plan.

#### Stanja

- Učitavanje: skeleton KPI.
- Prazno (teoretski): onboarding CTA “Dodaj prvo vozilo”.

#### Lab mapiranje

- Već postoji logika u `DashboardRepository` — dizajn samo vizualno podiže razinu.

---

### S02 — Timeline (Mjesečni raspored)

**URL:** `/raspored`, `/raspored/mjesecni`  
**Svrha:** Vidjeti zauzetost vozila po danima u mjesecu (dispečerski pogled).

#### Layout (END GOAL)

1. **Toolbar panel**
   - Search: vozilo / registracija (text)
   - Filter: tip vozila (select)
   - Filter: status rezervacije (select)
   - Mjesec: **custom month picker** (ne browser default) — usklađeno s Lab 4 datetime filozofijom
   - Gumb “Primijeni” + “Reset”
   - Desno: legenda chip boja po statusu

2. **Glavna matrica** (horizontal scroll na mobilnom)
   - Sticky prvi stupac: vozilo (marka model + registracija manjim fontom)
   - Zaglavlje: dani 1–N (vikend vizualno blago tamniji)
   - Ćelija: 0–n **status chipova**; klik chip → otvori Rezervacija Details
   - Hover ćelije: tooltip s kupcem, od–do datumom

3. **Poboljšanja EG**
   - Vizualna traka “span” preko više dana za istu rezervaciju (Gantt traka) umjesto samo chip po ćeliji — **prioritetni vizualni upgrade**.
   - Indikator konflikta ako dvije rezervacije overlap (crveni rub).

#### Interakcije

- GET forma (kao sada) — dizajn predviđa jasne kontrole.
- Print: landscape A3 opcija (F2).

#### Lab mapiranje

- Logika `TimelineVm` ostaje; dizajn mijenja prezentaciju tablice.

---

### S03 — Dnevni plan

**URL:** `/dnevni-plan`, `/operativa/dnevni-plan`  
**Svrha:** Operativa za jedan dan — tko vraća vozilo, tko odlazi.

#### Layout (END GOAL)

1. **Header**
   - Naslov “Dnevni plan”
   - Date picker (dan) — custom ili styled native (print friendly)
   - Gumb “Učitaj”
   - Gumb “Ispis A4” (print CSS: sakrij nav, bijela pozadina, crni tekst)

2. **Dvije kolone** (1 kolona na mobilnom)
   - **Lijevo: Povrati danas** (ikona strelice dolje)
   - **Desno: Odlasci danas** (ikona strelice gore)

3. **Kartica događaja** (umjesto plain liste)
   - Vozilo (bold), registracija
   - Kupac, vrijeme (iz rezervacije EndDate/StartDate)
   - Badge statusa
   - Akcije: Detalji rezervacije | Kontakt kupca (mailto/tel link)

4. **Prazno stanje**
   - Ilustracija + “Nema povrata/odlazaka za {datum}”

#### Lab mapiranje

- `DailyPlanVm` ostaje; mijenja se HTML/CSS kartica.

---

### S04 — Vozni park (Fleet)

**URL:** `/vozni-park`  
**Svrha:** Vizualni pregled flote; brze akcije na vozilu.

#### Layout (END GOAL)

1. **Header**
   - Naslov + podnaslov
   - Filter traka: tip vozila, poslovnica, samo aktivna (checkbox), search
   - Toggle prikaz: **Kartice** | **Tablica** (tablica može reuse Vehicle Index stila)

2. **Grid kartica** (min 280px širina)
   - Slika vozila (16:9, placeholder ako nema)
   - Badge: Active/Neaktivno
   - Naslov: Brand Model
   - Meta: Reg, Poslovnica, Cijena/dan, Tip
   - **Action bar** (ikonice + label na hover):
     - Detalji → Vehicle/Details
     - Uredi → Vehicle/Edit
     - Servisi → ServiceRecord/Index?s=vehicleId (F2 filter) ili Details sekcija
     - Nova rezervacija → Reservation/Create?s=vehicleId
   - Delete samo u detaljima ili s modalom (izbjegavati slučajni klik na kartici)

3. **Hover** — glow, blagi scale 1.02

#### Lab mapiranje

- `FleetCardVm` + slike već postoje; gumbi trenutno nefunkcionalni — EG dizajn veže prave URL-ove.

---

### S05–S07 — Partneri (lista, create, edit)

**URL:** `/partneri`, `/partneri/novi`, `/partneri/uredi/{id}`

#### S05 Lista

- CRUD toolbar: “Partneri”, “+ Novi partner”, AJAX search
- Tablica: Tvrtka, Kontakt osoba, Telefon, Email, Akcije (Uredi, Obriši)
- Delete → **confirm modal** (EG)
- Row klik na tvrtku → Edit (opcionalno)

#### S06/S07 Forma

Polja: CompanyName*, ContactPerson*, Phone*, Email*  
Layout: jednostavna forma max 640px u glass panelu  
Validacija: inline error ispod polja, blur highlight (Lab 4)

---

### S08–S10 — Poslovnice

#### S08 Lista

Stupci: Naziv, Tip lokacije, Adresa, Telefon, Akcije  
Search AJAX po nazivu/adresi/telefonu

#### S09 Create/Edit

Polja: Name*, LocationType* (select enum), Address*, Phone*  
Bez autocomplete

#### S10 Details

- Kartica s osnovnim podacima
- Sekcija **Vozila na lokaciji** (mini tablica ili lista linkova na Vehicle/Details)
- Akcije: Uredi, Natrag, Obriši (s porukom ako ima vozila)

---

### S11–S13 — Vozila

#### S11 Lista

Stupci: Registracija, Marka/Model, Tip, Cijena/dan, Aktivno (badge), Akcije  
Search: brand, model, reg  
Primary CTA: Novo vozilo

#### S12 Create/Edit

Polja:

- RegistrationNumber*, Brand*, Model*, Year, Type, MileageKm, DailyPrice, IsActive
- **Autocomplete:** Poslovnica (BranchOfficeId) — `/api/lookup/branches`

Layout: dvije kolone na desktopu (osnovno lijevo, poslovnica/desno)

#### S13 Details (END GOAL — jedan od najvažnijih ekrana)

**Struktura “master-detail”:**

1. **Hero strip**
   - Registracija velikim fontom
   - Brand Model, godina, tip
   - Badge Active/Inactive
   - Cijena/dan, kilometraža
   - CTA: Uredi | Nova rezervacija | Novi servis

2. **Tabovi ili sidra sekcije**
   - **Pregled** — poslovnica, link na Branch Details
   - **Rezervacije** — tablica zadnjih N, link na sve
   - **Servisi** — tablica servisa, status chip, sljedeći preporučeni datum

3. **Sidebar (desktop)** — slika vozila

---

### S14–S16 — Kupci

#### S14 Lista

Stupci: Ime i prezime, Email, Telefon, Broj rezervacija (count), Akcije

#### S15 Create/Edit

- FirstName*, LastName*, Email*, Phone*
- **DateTime partial:** DateOfBirth
- DriverLicenseNumber*

#### S16 Details

- Osnovni podaci + kontakt linkovi
- Tablica rezervacija (link na Details)
- CTA Nova rezervacija (prefill customerId u query)

---

### S17–S19 — Rezervacije (jezgra poslovanja)

#### S17 Lista

Stupci: ID, Kupac, Vozilo, Početak, Završetak, Status (chip), Cijena, Akcije  
Filteri EG (F2 ili kasnije): status, datum raspon  
Search: ID ili status string (kao sada)

#### S18 Create/Edit (END GOAL — kompleksna forma)

**Sekcije forme:**

1. **Sudionici**
   - Autocomplete Kupac (customers API)
   - Autocomplete Vozilo (vehicles API)

2. **Period i lokacije**
   - DateTime: StartDate*, EndDate*
   - PickupLocation*, DropoffLocation* (select LocationType)
   - Validacija: End > Start (server + poruka)

3. **Status i cijena**
   - Status* (select)
   - BasePrice* (decimal)

4. **Dodaci (N-N) — END GOAL proširenje**
   - Lista dostupnih Addona s checkbox ili količina +
   - Prikaz izračuna: suma dodataka (informativno)
   - Na Edit: učitati postojeće ReservationAddon zapise

5. **Footer**
   - Spremi (primary), Odustani (ghost)

**UX napomene:**

- Kad se odabere vozilo, predložiti BasePrice = DailyPrice × broj dana (F2 automatska kalkulacija).
- Draft status omogućuje spremanje nepotpune rezervacije (poslovno pravilo — opcionalno).

#### S19 Details

- Header: Rezervacija #ID + status chip
- Kartice: Kupac (link), Vozilo (link), datumi, lokacije
- Tablica **Dodaci na rezervaciji** (AddonName, Quantity, PriceAtReservation)
- Ukupna cijena (Base + addons) — EG
- Akcije: Uredi, Obriši, Promijeni status (quick actions F2)

---

### S20 — Dodaci (Addon)

Lista + CRUD: Name*, PricePerDay*  
Jednostavniji entitet — referentni dizajn za “lightweight CRUD”.  
Na Details (ako postoji): rezervacije koje koriste addon (F2).

---

### S21 — Servisi (ServiceRecord)

#### Lista

Stupci: ID, Vozilo (label), Datum servisa, Status chip, Opis (skraćeno), Trošak, Akcije

#### Create/Edit

- Autocomplete vozilo
- DateTime ServiceDate*
- Status*, Description*, MileageAtService, Cost
- DateTime NextRecommendedServiceDate (opcionalno)

#### Details

- Poveznica na vozilo, povijest servisa na istom vozilu

---

### S22 — Zaposlenici

Lista + CRUD: FirstName*, LastName*, JobTitle*, Autocomplete poslovnica, DateTime HiredAt*  
Details: poslovnica link, opcionalno lista rezervacija koje je kreirao (F2 — nema u modelu još).

---

## 6. Globalni UX flowovi

### 6.1 Nova rezervacija (happy path)

```
Dashboard [+ Nova rezervacija]
    → Reservation Create
    → odabir kupca (autocomplete) + vozila (autocomplete)
    → odabir datuma (custom picker)
    → odabir dodataka
    → Spremi
    → Toast uspjeh
    → Reservation Details ili Index
```

### 6.2 Dnevna operativa

```
Dashboard → Dnevni plan (danas)
    → kartica odlaska → Reservation Details
    → markiranje statusa Active na početku dana (Edit)
```

### 6.3 Fleet → servis

```
Vozni park → kartica → Detalji vozila → tab Servisi → Novi servis
    → ServiceRecord Create (vehicle prefilled)
```

### 6.4 Brisanje s poslovnim pravilom

```
BranchOffice Index → Obriši
    → Modal “Jeste li sigurni?”
    → Server odbija (ima vozila)
    → Toast/warning banner s porukom (već implementirano)
```

---

## 7. Responsive i print

### 7.1 Breakpoint ponašanje

| Komponenta | Mobile | Desktop |
| --- | --- | --- |
| Nav | Drawer | Horizontal |
| KPI grid | 1–2 col | 4 col |
| CRUD tablica | Kartice po retku | Tablica |
| Timeline | Scroll X + sticky col | Puna širina |
| Forma | 1 kolona | 2 kolone gdje smisleno |

### 7.2 Print (Dnevni plan)

- `@media print`: bez headera, bijela pozadina, crni tekst, page-break između kolona ako treba
- Datum i naslov na vrhu stranice

---

## 8. Pristupačnost i i18n

- Kontrast teksta na `--bg`: minimalno WCAG AA za body tekst.
- Fokus ring na inputima i gumbima (cyan 2px).
- `lang="hr"` na HTML; `UseRequestLocalization` hr / en-US.
- Datumski format: hr `dd.MM.yyyy. HH:mm`, en `MM/dd/yyyy HH:mm` — usklađeno s Lab 4 JS.
- Label uvijek povezan s inputom; greške čitljive screen readeru (`aria-invalid`).

---

## 9. Mapiranje END GOAL → postojeći kod (implementacija nakon designa)

| Design prioritet | Datoteke za izmjenu |
| --- | --- |
| Layout / Nav | `Views/Shared/_Layout.cshtml`, `site.css` |
| Dashboard | `Views/Home/Index.cshtml` |
| Timeline | `Views/Timeline/Index.cshtml`, `site.css` |
| Dnevni plan | `Views/DailyPlan/Index.cshtml`, print CSS |
| Fleet | `Views/Fleet/Index.cshtml` |
| CRUD liste | `Views/*/Index.cshtml`, `_CrudIndexHeaderPartial` |
| CRUD forme | `Views/*/ _FormPartial.cshtml`, Shared partiali |
| Detalji | `Views/*/Details.cshtml` |
| Rezervacija dodaci | Controller + forma + `ReservationAddon` (novi feature) |
| Modal delete | `site.js` + partial `_ConfirmDeleteModal` |
| Tokeni | `site.css` `:root` iz `design-tokens.md` |

**Ne mijenjati:** EF migracije osim ako novi feature zahtijeva; plan datoteke labova.

---

## 10. Faza 2 (izvan trenutnog END GOAL mockupa — samo zabilježiti)

Za potpunu platformu u budućim labovima / projektu:

- **Login** i role-based menu (Operater vs Admin)
- **Global search** (vozilo, kupac, rezervacija ID)
- **Izvještaji** (prihod po mjesecu, iskorištenost flote)
- **Audit log** promjena na rezervaciji
- **Soft delete** (`DeletedAt`) vizualno “Arhivirano”
- **Notifikacije** (servis dolazi, rezervacija ističe sutra)
- **Upload slike vozila** umjesto Picsum placeholdera
- **API** za mobilnu aplikaciju

Dizajn F2 ekrana nije obavezan u prvoj Open Design iteraciji.

---

## 11. Checklist za Open Design (prije handoffa natrag)

- [ ] Svi ekrani iz `screen-inventory.md` prioritet P1 imaju desktop mockup
- [ ] Min. 2 CRUD forme (Rezervacija + Vozilo) s autocomplete i datetime
- [ ] Komponent library exportiran (gumbi, inputi, chip, panel, tablica)
- [ ] Tokeni usklađeni s `design-tokens.md`
- [ ] Mobile verzija: Dashboard, Rezervacije lista, Rezervacija forma
- [ ] Timeline s Gantt trakama ili jasnim chip legendom
- [ ] Delete modal i toast stil
- [ ] Empty state primjer
- [ ] Specifikacija spacinga za developer handoff (8px grid)

---

## 12. Sažetak za tebe (korisnika)

1. Stavi `open-design-brief.md` + ovaj dokument u Open Design.  
2. Dizajniraj **operativne** ekrane prvo (Dashboard, Timeline, Dnevni plan, Fleet, Rezervacije).  
3. Zatim **CRUD pattern** koji se replicira na ostale entitete.  
4. Vrati mockupe → implementiramo u Razor/CSS uz zadržavanje Lab 4 funkcionalnosti (AJAX, validacija, API lookup).  
5. Rezervacija s **dodacima** na formi je glavna funkcionalna razlika EG vs trenutni kod — planirati u dizajnu i kodu kad stigne handoff.

---

*Kraj dokumenta — END GOAL Design Plan v1.0*
