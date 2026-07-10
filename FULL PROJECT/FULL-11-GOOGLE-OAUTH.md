# FULL-11 — Google OAuth (3rd party autentikacija)

> **⏭️ PRESKOČENO u ovom projektu** — koristimo email/lozinka + seed računi (`admin@carrent.local`). Kod u repou ostaje ako ikad zatreba.

**Kriterij Lab 5:** Omogućiti 3rd party autentikaciju — **1 bod** (lab)  
**Status:** ⏭️ Namjerno nije u demo scopeu

---

## 1. Teorija

### Što je OAuth 2.0 / OpenID Connect?

Korisnik se **ne prijavljuje lozinkom u tvojoj bazi**, nego kod **Googlea**. Google potvrdi identitet i vrati token tvojoj aplikaciji. ASP.NET Identity **povezuje** vanjski login s lokalnim `AppUser` zapisom.

**Zašto u projektu:** Lab 5 traži vanjski login provider; Google je najčešći izbor.

### Tok (pojednostavljeno)

```
1. Korisnik → Login → „Prijava putem Google”
2. Challenge → redirect na accounts.google.com
3. Korisnik se prijavi kod Googlea
4. Google → callback /signin-google?code=...
5. ASP.NET zamijeni code za token, pročita email
6. Ako korisnik postoji → SignIn
   Ako ne → CreateAsync (bez uloge) → PendingAccess
7. Admin dodijeli ulogu u Manage Users
```

### Zašto PendingAccess?

Poslovni model: ne želimo da bilo tko s Google računom odmah vidi flotu. Novi korisnik **nema ulogu** dok Admin ne dodijeli Admin/Manager (`PendingRoleMiddleware`).

---

## 2. Gdje u kodu

| Datoteka | Uloga |
|----------|--------|
| `Program.cs` (≈ L73–83) | `AddGoogle()` samo ako su ClientId + Secret postavljeni |
| `Areas/Identity/Pages/Account/Login.cshtml` | Gumb „Prijava putem Google” |
| `Areas/Identity/Pages/Account/ExternalLogin.cshtml.cs` | Challenge + Callback |
| `Middleware/PendingRoleMiddleware.cs` | Dopušta `/Identity/Account/ExternalLogin` |
| `appsettings.Development.json` | Prazni placeholderi `Authentication:Google` |

---

## 3. Konfiguracija — što TI moraš napraviti

### Korak A: Google Cloud Console

1. Idi na [Google Cloud Console](https://console.cloud.google.com/)
2. Kreiraj projekt (npr. `CarRent-Demo`)
3. **APIs & Services → OAuth consent screen** — External, test users (tvoj Gmail)
4. **Credentials → Create Credentials → OAuth client ID**
5. Tip: **Web application**
6. **Authorized redirect URIs** (dodaj oba ako koristiš HTTP i HTTPS):

```
http://localhost:5000/signin-google
https://localhost:7001/signin-google
```

7. Kopiraj **Client ID** i **Client Secret**

### Korak B: User secrets (ne u git!)

```bash
./scripts/setup-google-secrets.sh "TVOJ_CLIENT_ID.apps.googleusercontent.com" "TVOJ_CLIENT_SECRET"
```

ili ručno:

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "..." --project src/CarRent.Web
dotnet user-secrets set "Authentication:Google:ClientSecret" "..." --project src/CarRent.Web
```

### Korak C: Restart

```bash
./scripts/run-local.sh
```

Na `/Identity/Account/Login` pojavi se gumb **Prijava putem Google** (samo ako su tajne postavljene).

---

## 4. Demo scenarij

### Varijanta 1 — postojeći korisnik

1. Admin u **Manage Users** kreira korisnika s emailom = tvoj Google email  
2. Dodijeli ulogu **Manager**  
3. Odjava → Login → **Google**  
4. Ulaz na Početnu / Operativu  

### Varijanta 2 — novi Google korisnik

1. Login → Google (email koji nije u bazi)  
2. Redirect na **Pending Access** — čeka ulogu  
3. Admin → Manage Users → dodijeli ulogu  
4. Ponovna prijava → pristup aplikaciji  

---

## 5. Testiranje

| Provjera | Očekivano |
|----------|-----------|
| Bez secrets | Samo email/lozinka forma, nema Google gumba |
| S secrets | Google gumb vidljiv |
| Callback | Nema `redirect_uri_mismatch` u browseru |
| Novi user | `PendingAccess` stranica |
| Postojeći user s ulogom | Redirect na `/` |

Integracijski testovi koriste lažne vrijednosti u `CarRentWebApplicationFactory` — ne testiraju stvarni Google redirect.

---

## 6. Pitanja profesora

**P: Gdje je Google login?**  
O: `Program.cs` → `AddAuthentication().AddGoogle(...)`; UI na `Login.cshtml`; callback u `ExternalLogin.cshtml.cs`.

**P: Gdje se čuvaju tajne?**  
O: `dotnet user-secrets` (Development), na cloudu env var `Authentication__Google__ClientId` itd.

**P: Što ako netko dođe preko Googlea bez uloge?**  
O: `PendingRoleMiddleware` + `PendingAccess` — ne može u CRUD/operativu.

**P: Zašto HTTPS u uputama?**  
O: Google u produkciji zahtijeva HTTPS callback; lokalno često radi i `http://localhost:5000/signin-google`.

---

## 7. Status

| Stavka | Status |
|--------|--------|
| ASP.NET Google handler | ✅ |
| External login flow | ✅ |
| Pending access za nove | ✅ |
| Google Cloud credentials | ⚠️ **TI** |
| User-secrets postavljeni | ⚠️ **TI** |
| Demo uživo s Google računom | ⚠️ nakon koraka B |

---

## 8. Sljedeći korak

**Korak 6 — Cloud deploy (GCP)** → `FULL-07-DEPLOY-CLOUD.md`

Na Cloud Run postavi env:

```
Authentication__Google__ClientId=...
Authentication__Google__ClientSecret=...
```

Authorized redirect URI: `https://TVOJ-SERVIS.run.app/signin-google`

---

*Korak 5 FULL projekta — Google OAuth.*
