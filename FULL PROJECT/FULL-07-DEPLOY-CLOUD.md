# FULL-07 — Deploy na cloud (Docker + Google Cloud Run)

**Kriterij PDF:** Deploy na cloud provider (Google, Azure) ili Virtual Machine — **3 boda**  
**Status:** ✅ **Deployano na Google Cloud Run**  
**Datum:** 2026-07-10 (Korak 6 — završeno)

---

## 1. Teorija

**Deploy** = aplikacija dostupna na **javnom URL-u**, ne samo `localhost`.


| Pojam                   | U našem projektu                                      |
| ----------------------- | ----------------------------------------------------- |
| **Docker image**        | Paket aplikacije + .NET runtime                       |
| **Container**           | Pokrenuta instanca imagea                             |
| **Cloud Run**           | Google serverless — pokreće container, daje HTTPS URL |
| **SQLite u containeru** | OK za demo; podaci se resetiraju pri redeployu        |


**Zašto ne Firebase Hosting?** Naš je **ASP.NET MVC** (server-side). Treba Kestrel u containeru, ne statički hosting.

---

## 2. Gdje u projektu

```
Dockerfile                      ← multi-stage build (.NET 10)
scripts/run-docker-local.sh     ← lokalni test imagea
scripts/setup-gcp-deploy.sh     ← jednokratno enable API-ja
scripts/deploy-gcp.sh           ← build + push + Cloud Run
```

### Dockerfile (sažetak)

- **Build stage:** `dotnet publish` Release
- **Runtime:** `mcr.microsoft.com/dotnet/aspnet:10.0`
- **Port:** 8080 (`ASPNETCORE_URLS=http://+:8080`)
- **Baza:** `/app/Data/carrent.db` (SQLite, migracije pri startu)

---

## 3. Tok deploya

```
Izvorni kod
    → docker build
    → image gcr.io/PROJECT/carrent-web:latest
    → docker push (Google Container Registry)
    → gcloud run deploy
    → javni HTTPS URL (npr. https://carrent-web-xxx.a.run.app)
```

Pri prvom requestu container se podigne, pokrene migracije + seed (`Program.cs`).

---

## 4. Korak po korak — što TI radiš

### A) Lokalni test Docker imagea (preporučeno)

```bash
./scripts/run-docker-local.sh
# ili u pozadini:
docker build -t carrent-local:latest .
docker run -p 8080:8080 carrent-local:latest
```

Otvori: [http://localhost:8080](http://localhost:8080)  
Login: `admin@carrent.local` / `Admin123!`  
AI asistent: [http://localhost:8080/asistent](http://localhost:8080/asistent)

**Provjereno:** image se builda i vraća HTTP 200 na `/asistent` i Login.

### B) Google Cloud — s gcloud CLI

**Nemaš `gcloud` na Archu?** Nije u pacmanu — instaliraj jednom:

```bash
./scripts/install-gcloud.sh
export PATH="$HOME/google-cloud-sdk/bin:$PATH"
gcloud auth login
```

1. Račun na [cloud.google.com](https://cloud.google.com) (studentski kredit ~300$ OK)
2. Kreiraj projekt (npr. `carrent-demo-2026`)

```bash
export PATH="$HOME/google-cloud-sdk/bin:$PATH"   # ako nisi dodao u .zshrc
./scripts/setup-gcp-deploy.sh TVOJ_PROJECT_ID

export GCP_PROJECT_ID=TVOJ_PROJECT_ID
export GCP_REGION=europe-west1
./scripts/deploy-gcp.sh
```

1. Skripta ispiše **javni URL** — zalijepi ga u §5 ispod.

### B-alt) Google Cloud — BEZ gcloud (samo browser + Docker)

Ako ne želiš instalirati CLI:

```bash
# 1. Besplatan Docker Hub račun: hub.docker.com
export DOCKERHUB_USER=tvoj_username
./scripts/push-dockerhub.sh
```

Zatim u browseru:

1. [console.cloud.google.com](https://console.cloud.google.com) → novi projekt
2. **APIs & Services → Enable APIs** → uključi **Cloud Run API**
3. **Cloud Run → Create service**
4. **Container image URL:** `docker.io/TVOJ_USER/carrent-web:latest`
5. **Container port:** `8080`
6. **Authentication:** Allow unauthenticated invocations
7. **Create** → kopiraj URL (npr. `https://carrent-web-xxxxx.a.run.app`)

Ovo zadovoljava kriterij „deploy na cloud” bez lokalnog `gcloud` naredbe.

### C) Env varijable na cloudu (opcionalno)

Za email obavijesti na Cloud Runu (nakon deploya):

```bash
gcloud run services update carrent-web --region europe-west1 \
  --set-env-vars "FleetNotifications__EmailEnabled=true,FleetNotifications__DefaultRecipient=tvoj@gmail.com,..."
```

SMTP lozinku **nikad** u git — samo kroz `gcloud run services update` ili Secret Manager.

---

## 5. Deploy URL

```
JAVNI URL:     https://carrent-web-qf75dpugxq-ew.a.run.app
AI asistent:   https://carrent-web-qf75dpugxq-ew.a.run.app/asistent
Datum deploya: 2026-07-10
GCP projekt:   carrent-dev (CarRent-Dev)
Regija:        europe-west1
Servis:        carrent-web
Image:         gcr.io/carrent-dev/carrent-web:latest
```

Login (seed): `admin@carrent.local` / `Admin123!`

---

## 6. Demo scenarij za profesora (2 min)

1. Otvori **javni URL** u browseru
2. Prijava Admin → Početna / Timeline
3. `**/asistent`** — kratki AI chat
4. Spomeni: „Isti Dockerfile radi lokalno i na Cloud Run”

---

## 7. Testiranje


| Test            | Naredba                                      | Očekivano         |
| --------------- | -------------------------------------------- | ----------------- |
| Docker build    | `docker build -t carrent-local:test .`       | exit 0            |
| Container start | `docker run -p 8080:8080 carrent-local:test` | Listening on 8080 |
| HTTP            | `curl -I http://localhost:8080/asistent`     | 200               |
| Cloud deploy    | `./scripts/deploy-gcp.sh`                    | URL u outputu     |


---

## 8. Pitanja profesora

**P: Gdje je deployano?**  
O: Google Cloud Run, regija `europe-west1`, javni HTTPS URL (§5).

**P: Zašto Docker?**  
O: Reproducibilan build — isti image lokalno i u cloudu.

**P: Koja baza?**  
O: SQLite u containeru za demo; produkcija bi koristila Cloud SQL / PostgreSQL.

**P: Što se dogodi pri redeployu?**  
O: SQLite se resetira (nova instanca) — za predaju seed ponovo napuni podatke.

**P: Google OAuth na cloudu?**  
O: Namjerno preskočeno u projektu — koristimo email/lozinka + seed računi.

---

## 9. Što reći na usmenom

> „Aplikaciju pakiramo u Docker image, testiramo lokalno na portu 8080, zatim deployamo na Google Cloud Run koji daje javni HTTPS URL. ASP.NET Core sluša na 8080, baza je SQLite unutar containera za demo.”

---

## 10. Status


| Stavka                     | Status                                                                                       |
| -------------------------- | -------------------------------------------------------------------------------------------- |
| Dockerfile                 | ✅                                                                                            |
| Lokalni docker build + run | ✅ testirano                                                                                  |
| `deploy-gcp.sh`            | ✅                                                                                            |
| `setup-gcp-deploy.sh`      | ✅                                                                                            |
| Javni Cloud Run URL        | ✅ [https://carrent-web-hfcdfitrgq-ew.a.run.app](https://carrent-web-hfcdfitrgq-ew.a.run.app) |
| Google OAuth               | ⏭️ preskočeno                                                                                |


---

## 11. Sljedeći korak

**Korak 7 — Final QA** → `FULL-REPORT.md` (E2E, integracija, demo checklist)

---

*Korak 6 FULL projekta — Cloud deploy.*