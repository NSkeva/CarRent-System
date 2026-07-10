# FULL-12 — Interaktivni timeline (raspored rezervacija)

**Kriterij:** Bonus / operativni dojam (nije zaseban PDF bod, ali pokazuje naprednu funkcionalnost)  
**Status:** ✅ Implementirano  
**Datum:** 2026-07-10

---

## 1. Što je timeline?

**Timeline** je **mjesečni raspored** koji prikazuje koja su vozila zauzeta kojim rezervacijama — operativni alat za dispečera. Korisnik vidi „ko ima što kad” bez otvaranja svake rezervacije posebno.

**Ruta:** `/raspored` ili `/Timeline`  
**Alias u Program.cs:** `timeline_short` → `raspored`

---

## 2. Teorija


| Pojam            | U CarRent                 |
| ---------------- | ------------------------- |
| **Vremenska os** | Dani u mjesecu            |
| **Resurs**       | Vozilo (red)              |
| **Blok**         | Rezervacija (od–do)       |
| **API**          | JSON za JS render / modal |


Za razliku od **Dnevnog plana** (`/dnevni-plan`) koji je lista događaja za **jedan dan**, timeline je **grid** za **cijeli mjesec**.

---

## 3. Gdje u kodu


| Komponenta             | Putanja                                                            |
| ---------------------- | ------------------------------------------------------------------ |
| MVC stranica           | `Controllers/TimelineController.cs`, `Views/Timeline/Index.cshtml` |
| API podaci             | `Api/Controllers/TimelineApiController.cs`                         |
| JS modal / interakcija | `wwwroot/js/timeline-modal.js`                                     |
| Layout slotova         | `Services/TimelineLayoutHelper.cs`, `TimelineSlotHelper.cs`        |
| Testovi                | `tests/.../TimelineApiTests.cs`, `TimelineSlotHelperTests.cs`      |


---

## 4. Tok podataka

```
Korisnik otvori /raspored
  → TimelineController.Index()
  → View učitava JS
  → fetch GET /api/timeline/... (mjesec, godina)
  → TimelineApiController
  → EF: rezervacije + vozila za raspon
  → JSON slotovi
  → JS crta grid / otvara modal s detaljima rezervacije
```

---

## 5. Demo za profesora (1 min)

1. Prijava Admin
2. **Operativa → Raspored** ili `/raspored`
3. Pokaži mjesec s blokovima rezervacija
4. Klik na blok → detalji (modal)
5. Spomeni: „Podaci dolaze iz iste SQLite baze kao rezervacije — API odvojen od MVC viewa.”

---

## 6. Pitanja profesora

**P: Zašto API za timeline, ne samo Razor?**  
O: JSON omogućuje **dinamičko** osvježavanje i modal bez full page reload — isti obrazac kao Lab 4 AJAX.

**P: Kako znaš da se rezervacije ne preklapaju?**  
O: `ReservationSchedulingValidator` pri kreiranju/izmjeni — testovi u `ReservationAvailabilityHelperTests`.

**P: Gdje je u navigaciji?**  
O: Sidebar **Operativa** → Raspored.

---

## 7. Status


| Stavka                    | Status                      |
| ------------------------- | --------------------------- |
| MVC view                  | ✅                           |
| Timeline API              | ✅                           |
| Integracijski test        | ✅                           |
| Drag/resize (ako postoji) | ⚠️ provjeri verziju u viewu |


---

*FULL-12 — Timeline raspored.*