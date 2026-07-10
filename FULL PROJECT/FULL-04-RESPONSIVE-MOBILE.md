# FULL-04 — Responsive / mobile UI

**Kriterij PDF:** Responsive — mobile/web UI — **2 boda**  
**Status:** ✅ Implementirano (hamburger + prilagođeni layouti)

---

## 1. Teorija — što je responsive design?

**Responsive web design** = UI se prilagođava **širini ekrana** (desktop, tablet, mobitel) bez zasebne mobilne aplikacije.

**Tehnike:**

- **CSS media queries** (`@media (max-width: …)`)
- **Flexbox / grid** koji prelama sadržaj
- **Mobilni izbornik** umjesto horizontalne navigacije

**Zašto:** CarRent se može koristiti u terenu (manager na mobitelu provjerava flotu).

---

## 2. Gdje je u projektu

```
src/CarRent.Web/
├── Views/Shared/_Layout.cshtml     ← hamburger gumb, nav#siteNav
├── wwwroot/js/site.js              ← initMobileNav()
├── wwwroot/css/site.css            ← mobile nav, global search, chat
└── wwwroot/css/glass.css           ← KPI grid, breakpoint 980px / 640px
```

---

## 3. Gdje u kodu

### Hamburger meni

**HTML** (`_Layout.cshtml`):

```html
<button type="button" class="mobile-nav-toggle" data-mobile-nav-toggle>☰</button>
<nav class="nav-primary" id="siteNav">...</nav>
```

**JS** (`site.js` → `initMobileNav()`):

- Klik na ☰ → `nav` dobije klasu `mobile-open`
- Klik izvan → zatvara meni

**CSS** (`site.css`):

```css
@media (max-width: 980px) {
  .mobile-nav-toggle { display: inline-flex; }
  .nav-primary { display: none; position: fixed; ... }
  .nav-primary.mobile-open { display: flex; }
}
```

### Ostali responsive dijelovi (već iz Lab 2–4)


| Breakpoint | Što se mijenja               |
| ---------- | ---------------------------- |
| ≤980px     | KPI 2 kolone; mobilni nav    |
| ≤640px     | KPI 1 kolona; toolbar stupci |
| ≤900px     | Vehicle edit grid 1 kolona   |


---

## 4. Kako pokazati

1. Otvori app u browseru
2. **F12** → Toggle device toolbar (Ctrl+Shift+M)
3. Postavi širinu **390px** (iPhone)
4. Klikni **☰** → padajući izbornik Operativa / Podaci
5. Playwright E2E korak 13 automatski provjerava vidljivost hamburgera na `/Fleet`

---

## 5. Moguća pitanja profesora

**P: Gdje je responsive kod?**  
O: Primarno `site.css` (mobile nav) i `glass.css` (gridovi). Logika izbornika u `site.js`.

**P: Imate li zasebnu mobilnu app?**  
O: Ne — responsive **web** (jedan codebase).

**P: Što se događa ispod 980px?**  
O: Horizontalni nav se skriva, pojavljuje hamburger koji otvara full-screen overlay navigaciju.

**P: Je li viewport meta tag postavljen?**  
O: Da — `_Layout.cshtml`: `<meta name="viewport" content="width=device-width, initial-scale=1.0" />`

---

## 6. Što reći na usmenom

> „Koristimo CSS media queries i mobilni hamburger izbornik; ispod 980px navigacija prelazi u overlay koji kontrolira JavaScript u site.js.”

---

## 7. Daljnji koraci — implementacija (opcionalno)

- Touch-friendly veći gumbi na formama
- Horizontal scroll za široke tablice s sticky headerom
- PWA manifest (instalacija na home screen)
- Test na pravom mobitelu (ne samo DevTools)

---

## 8. Koraci koje TI moraš poduzeti


| Korak             | Obavezno? | Akcija                   |
| ----------------- | --------- | ------------------------ |
| Pokreni app       | Da        | `./scripts/run-local.sh` |
| Demo na usmenom   | Preporuka | F12 → mobilni viewport   |
| Ništa instalirati | —         | Čisto CSS/JS             |


**Nema** dodatne konfiguracije.