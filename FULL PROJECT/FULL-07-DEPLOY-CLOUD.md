# FULL-07 — Deploy na cloud (Docker + Google Cloud Run)

**Kriterij PDF:** Deploy na cloud provider (Google, Azure) ili Virtual Machine — **3 boda**  
**Status:** ⚠️ Kod i skripte spremni — **ti moraš pokrenuti deploy**

---

## 1. Teorija — što je deploy?

**Deploy** = postavljanje aplikacije na **javni server** dostupan preko interneta (ne samo `localhost`).

**Zašto cloud:**
- Javni URL za demo profesoru
- Skaliranje, monitoring
- Kriterij projekta

**Naš stack:**
- **Docker** — pakira app + runtime u image
- **Google Cloud Run** — pokreće container bez upravljanja VM-om (serverless)

**Alternativa:** bilo koji VM (Hetzner, DigitalOcean) s `docker run`.

---

## 2. Gdje je u projektu

```
Dockerfile                    ← multi-stage build ASP.NET
scripts/deploy-gcp.sh         ← build, push, gcloud run deploy
src/CarRent.Web/              ← aplikacija koja se deploya
```

### Dockerfile (sažetak)

- Build: `dotnet publish` na .NET 10
- Runtime: `mcr.microsoft.com/dotnet/aspnet:10.0`
- Port: **8080**
- Env: `DatabaseProvider=Sqlite`, mapa `/app/Data`, `/app/logs`

---

## 3. Gdje u kodu — važne postavke

| Postavka | Vrijednost u containeru |
|----------|-------------------------|
| `ASPNETCORE_URLS` | `http://+:8080` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| Baza | SQLite u `/app/Data/carrent.db` |

**Upozorenje:** Na Cloud Run bez persistent volumea SQLite se **resetira** pri redeployu. Za trajnu bazu treba Cloud SQL ili mounted disk — za **demo predaju** često dovoljno.

---

## 4. Kako deployati (Google Cloud)

### Preduvjeti (TI moraš imati)

1. Google Cloud račun (studentski kredit OK)
2. Instaliran [gcloud CLI](https://cloud.google.com/sdk/docs/install)
3. Instaliran Docker
4. Kreiran GCP projekt

### Naredbe

```bash
# Jednokratno
gcloud auth login
gcloud config set project TVOJ_PROJECT_ID

# Deploy
export GCP_PROJECT_ID=TVOJ_PROJECT_ID
export GCP_REGION=europe-west1
./scripts/deploy-gcp.sh
```

Skripta:
1. `docker build` → image
2. `docker push` → `gcr.io/PROJECT/carrent-web`
3. `gcloud run deploy` → javni URL

---

## 5. Moguća pitanja profesora

**P: Gdje je aplikacija deployana?**  
O: _(Ti ispunjavaš)_ — URL s Cloud Run npr. `https://carrent-web-xxxxx-ew.a.run.app`

**P: Zašto Docker?**  
O: Reproducibilan build; isti image radi lokalno i u cloudu.

**P: Zašto Cloud Run, a ne Firebase Hosting?**  
O: Firebase Hosting je za statiku; naš **ASP.NET MVC** treba server-side runtime (Kestrel u containeru).

**P: Koja baza u produkciji?**  
O: Trenutno SQLite u containeru; za ozbiljnu produkciju SQL Server / PostgreSQL / Cloud SQL.

---

## 6. Što reći na usmenom

> „Aplikacija je u Docker imageu, deployana na Google Cloud Run; ASP.NET sluša na portu 8080, baza je SQLite unutar containera za demo.”

---

## 7. Daljnji koraci — implementacija (opcionalno)

- Cloud SQL PostgreSQL + connection string
- Persistent volume za SQLite
- GitHub Actions CI/CD
- Custom domena + HTTPS cert
- Azure App Service (ako profesor preferira Azure)

---

## 8. Koraci koje TI moraš poduzeti

| Korak | Obavezno za 3 boda? | Akcija |
|-------|---------------------|--------|
| GCP račun | **Da** | cloud.google.com |
| `gcloud` login | **Da** | `gcloud auth login` |
| `GCP_PROJECT_ID` | **Da** | export prije skripte |
| Docker radi | **Da** | `docker build -t test .` lokalno probaj |
| Pokreni `deploy-gcp.sh` | **Da** | Jednom prije predaje |
| Spremi URL u report | **Da** | Zalijepi u FULL-REPORT ili LAB report |
| Seed korisnici | Auto | Migracije + seed pri prvom startu containera |

**Lokalni test Docker imagea (prije clouda):**

```bash
docker build -t carrent-local .
docker run -p 8080:8080 carrent-local
# otvori http://localhost:8080
```

**Nemaš GCP?** Alternativa za kriterij „VM”: najjeftiniji VPS + `docker run` — isti Dockerfile.
