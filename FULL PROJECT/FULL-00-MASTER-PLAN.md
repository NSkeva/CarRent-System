# FULL projekt — Master plan (korak po korak)

**Cilj:** Maksimalan broj bodova prema `Kriteriji-projekt-pragovi.pdf` + Lab 5 + operativne nadogradnje.  
**Pravilo rada:** Jedan korak → implementacija → test → **jedan MD report** (teorija + kod + demo + pitanja profesora).

**Ukupno bodova (projekt):** 70 (od toga 40 usmeno, ~20 tehnički u kodu, 12 dojam).

---

## 1. Pregled — što je gotovo vs što radimo


| #   | Zahtjev (PDF / Lab)              | Bodovi       | Kod                                       | MD report                            | Status |
| --- | -------------------------------- | ------------ | ----------------------------------------- | ------------------------------------ | ------ |
| L5  | REST API CRUD + DTO (8 entiteta) | Lab 5        | ✅                                         | `lab-5/LAB5-PUNI-PREGLED.md`         | ✅      |
| L5  | Identity + role Admin/Manager    | Lab 5        | ✅                                         | isto                                 | ✅      |
| L5  | Upload Dropzone (vozilo)         | Lab 5        | ✅                                         | isto §8                              | ✅      |
| L5  | Google OAuth                     | Lab 5        | ⏭️ preskočeno                             | `FULL-11-GOOGLE-OAUTH.md`            | ⏭️     |
| L5  | Integracijski testovi API        | Lab 5        | ✅ 55+                                     | `FULL-08-...md`                      | ✅      |
| F01 | Playwright E2E 10+ koraka        | 2 (+3 bonus) | ✅ 13 koraka                               | `FULL-01-PLAYWRIGHT-E2E.md`          | ✅      |
| F02 | Global search                    | 2            | ✅                                         | `FULL-02-GLOBAL-SEARCH.md`           | ✅      |
| F03 | Logging file + API               | 2            | ✅                                         | `FULL-03-LOGGING.md`                 | ✅      |
| F04 | Responsive mobile                | 2            | ✅                                         | `FULL-04-RESPONSIVE-MOBILE.md`       | ✅      |
| F05 | AI integracija                   | 3            | ✅ dva chata + session + Draft rezervacija | `FULL-05-AI-KLIJENTSKI-CHAT.md`      | ✅      |
| F06 | MCP + agentic IDE                | 2            | ✅                                         | `FULL-06-MCP-SERVER.md`              | ✅      |
| F07 | Deploy cloud / VM                | 3            | ✅ Cloud Run URL                           | `FULL-07-DEPLOY-CLOUD.md`            | ✅      |
| F08 | CRUD bez grešaka + API testovi   | 2            | ✅                                         | `FULL-08-...md`                      | ✅      |
| —   | Email obavijesti (fleet)         | bonus/dojam  | ✅ Gmail SMTP                              | `FULL-09-EMAIL-OBAVESTI.md`          | ✅      |
| —   | Push obavijesti (fleet)          | bonus/dojam  | ✅ Web Push                                | `FULL-10-PUSH-OBAVESTI.md`           | ✅      |
| —   | Interaktivni timeline            | bonus/dojam  | ✅                                         | `FULL-12-TIMELINE-SCHEDULER.md`      | ✅      |
| —   | Usmeno razumijevanje             | 40           | —                                         | `FULL-USMENO-PITANJA.md` + tutorijal | 📖     |


---

## 2. Redoslijed koraka (preporučeno)

```
Korak 1  → Timeline + testovi                    → FULL-12 + FULL-08  ✅
Korak 2  → Email (SMTP + outbox)                 → FULL-09            ✅
Korak 3  → Push (Web Push)                         → FULL-10            ✅
Korak 4  → AI chat                                 → FULL-05            ✅
Korak 5  → Google OAuth                            → ⏭️ preskočeno
Korak 6  → Cloud deploy                            → FULL-07            ✅
Korak 7  → Final QA + dokumentacija                → FULL-REPORT + tutorijal ✅
```

**Zašto ovaj redoslijed:** prvo kod u repou i testovi, zatim obavijesti (ne ovise o cloudu), AI/OAuth (lokalni secrets), deploy na kraju (treba tvoj GCP račun).

---

## 3. Što svaki MD report MORA sadržavati

Svaki `FULL-XX-*.md` (i novi FULL-09–12) po završetku koraka:

1. **Kriterij i bodovi** — citat iz PDF-a / Lab uputa
2. **Teorija** — što je, zašto, kako se veže na predavanje
3. **Gdje u kodu** — tablica putanja + linije / klase
4. **Tok podataka** — dijagram ili numerirani koraci (HTTP → service → DB)
5. **Konfiguracija** — `appsettings`, env, user-secrets, Docker
6. **Kako pokazati uživo** — demo scenarij (2–5 min)
7. **Testiranje** — naredbe + očekivani output
8. **Pitanja profesora** — 5–10 Q&A s točnim putanjama u kodu
9. **Status** — ✅ / ⚠️ / ❌ + datum završetka koraka

---

## 4. Detalji po koraku

### Korak 1 — Stabilizacija + interaktivni timeline

**Implementacija:**

- Commit: `TimelineApiController`, `timeline-scheduler.js`, CSS, view
- Fix: `TimelineApiTests` (seed rezervacija u InMemory ili fleksibilniji test)
- `dotnet test` → 58/58
- Ručno: drag/resize na `/raspored/mjesecni`

**MD izlaz:** `FULL-12-TIMELINE-SCHEDULER.md` + ažuriran `FULL-08-API-INTEGRACIJSKI-TESTOVI.md`

---

### Korak 2 — Email obavijesti

**Implementacija:**

- `SmtpFleetNotificationSender` ili proširenje postojećeg sendera
- Konfiguracija: `FleetNotifications:Smtp` (host, port, user, password, From)
- Background job / hosted service: procesira `FleetNotificationOutbox` gdje `Channel=Email` i `SentAt=null`
- Dev fallback: **Mailpit / Papercut** ili log-only ako SMTP nije postavljen
- UI: `/Notifications` prikazuje status „Poslano” + `SentAt`

**MD izlaz:** `FULL-09-EMAIL-OBAVESTI.md`

---

### Korak 3 — Push obavijesti

**Implementacija (realistično za projekt):**

- **Web Push** (VAPID ključevi) + Service Worker u `wwwroot`
- Pretplata u browseru (Admin/Manager)
- Sender šalje push kad lifecycle kreira obavijest
- Dev fallback: in-app toast + outbox zapis ako nema pretplate

**MD izlaz:** `FULL-10-PUSH-OBAVESTI.md`

---

### Korak 4 — AI chat (puni bodovi + demo)

**Implementacija:**

- `appsettings.Development.json.example` s `OpenAI:ApiKey` placeholderom
- Dokumentirati rule-based + GPT put
- Opcionalno: chat akcija „preusmjeri na Create rezervaciju” s query parametrima
- E2E već pokriva chat — provjeri prolazi

**MD izlaz:** prošireni `FULL-05-AI-KLIJENTSKI-CHAT.md`

---

### Korak 5 — Google OAuth

**Implementacija (konfiguracija, ne nužno novi kod):**

- Upute za Google Cloud Console → OAuth client
- `dotnet user-secrets` naredbe
- Callback `https://localhost:7001/signin-google`
- Test: login s Google računom (Admin mora prvo kreirati korisnika ili flow PendingAccess)

**MD izlaz:** `FULL-11-GOOGLE-OAUTH.md`

---

### Korak 6 — Cloud deploy

**Implementacija (TI + asistent):**

- `gcloud auth login`, `GCP_PROJECT_ID`
- `./scripts/deploy-gcp.sh`
- Spremiti **javni URL** u report
- Env na Cloud Run: `Mcp__ApiKey`, opcionalno `OpenAI__ApiKey`
- Napomena: SQLite ephemeral — za demo OK; opcija Cloud SQL kasnije

**MD izlaz:** ažuriran `FULL-07-DEPLOY-CLOUD.md` s **stvarnim URL-om**

---

### Korak 7 — Final QA i indeks

**Checklist:**

- `./scripts/run-local.sh`
- `dotnet test tests/CarRent.Web.IntegrationTests/`
- `./scripts/run-e2e.sh`
- Demo 10 min (FULL-REPORT §4)
- Svi MD-ovi imaju §9 Status ✅

**MD izlaz:** ažuriran `FULL-REPORT.md` s tablicom bodova i linkovima

---

## 5. Datoteke koje ćemo kreirati / ažurirati

```
FULL PROJECT/
├── FULL-00-MASTER-PLAN.md
├── FULL-TUTORIAL-KOMPLET.md      ← cijeli projekt, demo 15 min
├── FULL-USMENO-PITANJA.md        ← 80+ pitanja profesora
├── FULL-DEMO-CHEAT-SHEET.md      ← 1 stranica za ispit
├── FULL-REPORT.md                ← finalni report + bodovi
├── FULL-01 … FULL-12
└── Kriteriji-projekt-pragovi.pdf
```

---

## 6. Što treba od tebe po koracima


| Korak    | Ti dostaviš                                                      |
| -------- | ---------------------------------------------------------------- |
| 2 Email  | SMTP podatke (Gmail app password, SendGrid, ili Mailpit lokalno) |
| 3 Push   | Pristanak na Web Push u browseru pri demo                        |
| 4 AI     | OpenAI API ključ (opcionalno, za GPT demo)                       |
| 5 Google | ~~OAuth~~                                                        |
| 6 Deploy | GCP projekt ID, `gcloud` login                                   |


---

## 7. Trenutni status repozitorija

- **Branch:** `main` (remote `0591014`)
- **Necommitano:** timeline scheduler (9 datoteka)
- **Testovi:** 57/58 prolazi (1 timeline test)

---

## 8. Sljedeći korak

**Priprema za predaju / usmeni:**

1. Pročitaj `FULL-TUTORIAL-KOMPLET.md` (2–3h)
2. Uči `FULL-USMENO-PITANJA.md` — sekcije A, D, E, I, M
3. Na dan ispita: `FULL-DEMO-CHEAT-SHEET.md`
4. Jednom prođi demo + `dotnet test`

**Javni URL:** [https://carrent-web-hfcdfitrgq-ew.a.run.app](https://carrent-web-hfcdfitrgq-ew.a.run.app)

---

*Kreirano: master plan za predaju projekta CarRent FULL.*