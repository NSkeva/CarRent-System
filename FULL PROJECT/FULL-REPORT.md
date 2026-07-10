# FULL projekt — FINALNI REPORT (predaja + bodovi)

**Student:** Nikola Skeva · **Datum:** 2026-07-10  
**Repozitorij:** CarRent-System · **Branch:** main

---

## 1. Javni deploy URL


|                            |                                                                                                              |
| -------------------------- | ------------------------------------------------------------------------------------------------------------ |
| **Produkcija (Cloud Run)** | [https://carrent-web-qf75dpugxq-ew.a.run.app](https://carrent-web-qf75dpugxq-ew.a.run.app) |
| **AI asistent (javno)**    | [https://carrent-web-qf75dpugxq-ew.a.run.app/asistent](https://carrent-web-qf75dpugxq-ew.a.run.app/asistent) |
| **GCP projekt**            | `carrent-dev` (CarRent-Dev) |
| **Regija**                 | europe-west1                                                                                                 |
| **Lokalno**                | [http://localhost:5000](http://localhost:5000) (`./scripts/run-local.sh`)                                    |


**Login (seed):** `admin@carrent.local` / `Admin123!`

---

## 2. Tablica bodova prema PDF kriterijima (70 bodova)


| Kriterij                             | Bodovi | Status      | Dokaz / report                                        |
| ------------------------------------ | ------ | ----------- | ----------------------------------------------------- |
| Deploy cloud / VM                    | 3      | ✅           | Cloud Run URL gore, `FULL-07`                         |
| Playwright E2E 10+ koraka (+3 bonus) | 2 (+3) | ✅           | 13 koraka, `FULL-01`, `run-e2e.sh`                    |
| AI integracija                       | 3      | ✅           | `/asistent` + operativa, Draft rezervacija, `FULL-05` |
| Global search                        | 2      | ✅           | Ctrl+K, `FULL-02`                                     |
| Logging file/API                     | 2      | ✅           | Serilog + `/api/logs/recent`, `FULL-03`               |
| Responsive mobile                    | 2      | ✅           | Hamburger 390px, `FULL-04`                            |
| CRUD bez grešaka + API testovi       | 2      | ✅           | MVC + xUnit, `FULL-08`                                |
| MCP + agentic IDE                    | 2      | ✅           | `CarRent.McpServer`, `FULL-06`                        |
| Okvirni dojam / stabilnost           | 12     | ✅           | Demo + cloud + email + testovi                        |
| **Usmeno razumijevanje**             | **40** | 📖 priprema | `FULL-USMENO-PITANJA.md`, `LAB5-PUNI-PREGLED`         |


**Tehnički implementirano u kodu:** ~20/20 (+ do 3 Playwright bonus)  
**Deploy:** +3  
**Dojam:** ovisi o demo izvedbi  
**Usmeno:** ključ za ocjenu 5 (≥63/70)

---

## 3. Lab vježbe (1–5)


| Lab   | Tema                           | Report                       |
| ----- | ------------------------------ | ---------------------------- |
| Lab 1 | Model, LINQ                    | `lab-1/`                     |
| Lab 2 | MVC, HTML binding              | `lab2/LAB2-Report.md`        |
| Lab 3 | EF Core, migracije             | `lab-3/LAB3-Report.md`       |
| Lab 4 | CRUD, AJAX, validacija         | `lab-4/LAB4-Report.md`       |
| Lab 5 | API, Identity, upload, testovi | `lab-5/LAB5-PUNI-PREGLED.md` |


---

## 4. Dodatne nadogradnje (izvan PDF tablice)


| Značajka                      | Status        | Report                         |
| ----------------------------- | ------------- | ------------------------------ |
| Email obavijesti (Gmail SMTP) | ✅             | `FULL-09`                      |
| Web Push                      | ✅             | `FULL-10`                      |
| Timeline raspored             | ✅             | `FULL-12`                      |
| Google OAuth                  | ⏭️ preskočeno | `FULL-11`                      |
| Fleet lifecycle + outbox      | ✅             | `FULL-09`, `LAB5-PUNI-PREGLED` |


---

## 5. Demo scenarij 15 min (checklist)

Prije ulaska kod profesora — **odčekiraj:**

- `./scripts/run-local.sh` radi (ili otvori cloud URL)
- Prijava Admin uspješna
- Ctrl+K global search — rezultat
- `/asistent` — razgovor → Draft rezervacija
- `/Reservation` — vidiš nacrt
- `/Notifications` — email status
- `/operativa/ai-asistent` — operativno pitanje
- F12 mobile 390px — hamburger
- `dotnet test tests/CarRent.Web.IntegrationTests/` — prolazi
- (Opcionalno) `./scripts/run-e2e.sh`
- Cloud URL otvoren u drugoj kartici

**Detaljni tekst:** `FULL-TUTORIAL-KOMPLET.md` §7

---

## 6. Testiranje — naredbe

```bash
# Lokalno
./scripts/run-local.sh

# Integracijski
dotnet test tests/CarRent.Web.IntegrationTests/

# E2E Playwright
./scripts/run-e2e.sh

# Docker
./scripts/run-docker-local.sh

# Cloud (već deployano)
curl -I https://carrent-web-hfcdfitrgq-ew.a.run.app/asistent
```

---

## 7. Struktura dokumentacije (što učiti)


| Prioritet | Datoteka                        | Zašto                            |
| --------- | ------------------------------- | -------------------------------- |
| **1**     | `FULL-TUTORIAL-KOMPLET.md`      | Cijeli projekt jednim dokumentom |
| **2**     | `FULL-USMENO-PITANJA.md`        | 80+ pitanja profesora            |
| **3**     | `lab-5/LAB5-PUNI-PREGLED.md`    | API, Identity, DTO               |
| **4**     | `FULL-05-AI-KLIJENTSKI-CHAT.md` | AI 3 boda                        |
| **5**     | `FULL-07-DEPLOY-CLOUD.md`       | Deploy 3 boda                    |
| **6**     | FULL-01 … FULL-12               | Pojedinačni kriteriji            |


---

## 8. Procjena bodova (realistično)


| Scenarij                                  | Bodovi         |
| ----------------------------------------- | -------------- |
| Demo + osnovno usmeno                     | **35–45** / 70 |
| Demo + dobro usmeno (API, EF, AI)         | **55–62** / 70 |
| Demo + odlično usmeno + svi testovi uživo | **63–70** / 70 |


**Za prag 35:** dovoljno pokazati app, AI chat, cloud URL, spomenuti API i testove.  
**Za ocjenu 5:** moraš fluentno objasniti REST, DTO, Identity, EF migracije, AI tok, outbox, Docker.

---

## 9. Što TI još trebaš (5 min prije predaje)

1. Pročitaj **§S** u `FULL-USMENO-PITANJA.md` (minimalni set)
2. Jednom prođi demo checklist §5
3. Imaj otvoren cloud URL + lokalni localhost
4. Imaj otvoren `Program.cs` i `AiClientChatService.cs` u IDE-u

---

## 10. Indeks svih FULL datoteka

```
FULL PROJECT/
├── FULL-00-MASTER-PLAN.md
├── FULL-TUTORIAL-KOMPLET.md      ← GLAVNI TUTORIJAL
├── FULL-USMENO-PITANJA.md        ← PITANJA PROFESORA
├── FULL-REPORT.md                ← ovaj dokument
├── FULL-01-PLAYWRIGHT-E2E.md
├── FULL-02-GLOBAL-SEARCH.md
├── FULL-03-LOGGING.md
├── FULL-04-RESPONSIVE-MOBILE.md
├── FULL-05-AI-KLIJENTSKI-CHAT.md
├── FULL-06-MCP-SERVER.md
├── FULL-07-DEPLOY-CLOUD.md
├── FULL-08-API-INTEGRACIJSKI-TESTOVI.md
├── FULL-09-EMAIL-OBAVESTI.md
├── FULL-10-PUSH-OBAVESTI.md
├── FULL-11-GOOGLE-OAUTH.md
├── FULL-12-TIMELINE-SCHEDULER.md
└── Kriteriji-projekt-pragovi.pdf
```

---

*Finalni report — CarRent FULL projekt, 2026-07-10.*