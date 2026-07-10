# CarRent — DEMO CHEAT SHEET (1 stranica za ispit)

**Printaj ili drži otvoreno na drugom monitoru.**

---

## Pokretanje

```bash
./scripts/run-local.sh          # → http://localhost:5000
```

**Cloud:** https://carrent-web-qf75dpugxq-ew.a.run.app

---

## Login

| Uloga | Email | Lozinka |
|-------|-------|---------|
| Admin | admin@carrent.local | Admin123! |
| Manager | manager@carrent.local | Manager123! |

---

## 10-min demo redoslijed

1. `/` → login Admin
2. **Ctrl+K** → „golf” ili „rezervacija”
3. `/Vehicle` → Details
4. **Nova kartica:** `/asistent` → „vikend” → „golf 7 dana” → kontakt → **„da”**
5. `/Reservation` → filter **Nacrt**
6. `/Notifications` → Poslano
7. `/operativa/ai-asistent` → „što je danas na rasporedu?”
8. F12 → **390px** → hamburger
9. Cloud URL u drugoj kartici
10. Terminal: `dotnet test tests/CarRent.Web.IntegrationTests/`

---

## 5 rečenica koje moraš znati

1. **Arhitektura:** MVC + REST API + EF Core SQLite + Identity Admin/Manager.
2. **API:** DTO odvojen od entiteta; GET/POST/PUT/DELETE na `/api/...`.
3. **AI:** Parser + baza (dostupnost) + session; opcionalno OpenAI; Draft rezervacija + email.
4. **Obavijesti:** Outbox → BackgroundService → Gmail SMTP.
5. **Deploy:** Docker image → GCR → Cloud Run HTTPS; SQLite u containeru za demo.

---

## Brze rute

| Ruta | Što |
|------|-----|
| `/asistent` | Javni AI chat |
| `/operativa/ai-asistent` | Interni AI (auth) |
| `/raspored` | Timeline |
| `/dnevni-plan` | Dnevni plan |
| `/Notifications` | Email outbox |
| `/api/vehicle` | JSON lista vozila |

---

## Ako profesor pita…

| Pitanje | Odgovor u 1 rečenici |
|---------|----------------------|
| Gdje je baza? | SQLite `Data/carrent.dev.db`, migrate pri startu |
| Zašto DTO? | Ne cure navigacije u JSON, stabilan API |
| Zašto Session u chatu? | HTTP stateless — pamti korake rezervacije |
| Gdje deploy? | Cloud Run, projekt wehr-c55cd |
| Google OAuth? | Preskočeno, seed login |

**Puni Q&A:** `FULL-USMENO-PITANJA.md`  
**Puni tutorijal:** `FULL-TUTORIAL-KOMPLET.md`
