# FULL-05 — AI klijentski chat (integracija)

**Kriterij PDF:** AI integracija — unos podataka putem AI upita i sl. — **3 boda**  
**Status:** ✅ Implementirano (web chat + rule-based AI; opcionalno OpenAI)

---

## 1. Teorija — što je AI integracija u ovom kontekstu?

Profesor traži da korisnik može **prirodnim jezikom** postaviti upit aplikaciji, a sustav **odgovori ili pokrene akciju** (npr. provjera slobodnih vozila).

**Naš pristup:**
- **Web chat** na `/ClientChat` — simulira klijenta koji pita rent-a-car
- **Backend** analizira poruku i odgovara:
  - **Rule-based** (default, bez API ključa)
  - **OpenAI GPT** (ako postaviš `OpenAI:ApiKey`)

**WhatsApp:** Nije implementiran — web chat pokriva isti poslovni scenarij; WhatsApp bi bio dodatni kanal (Twilio).

---

## 2. Gdje je u projektu

```
src/CarRent.Web/
├── Controllers/ClientChatController.cs   ← stranica + POST Ask
├── Services/AiClientChatService.cs     ← logika odgovora
├── Views/ClientChat/Index.cshtml       ← chat UI
├── appsettings.json                    ← OpenAI:ApiKey, OpenAI:Model
└── Middleware/PendingRoleMiddleware.cs ← dozvoljen /ClientChat

Views/Shared/_Layout.cshtml             ← link u Operativa izborniku
```

---

## 3. Gdje u kodu — tok

```
GET /ClientChat
    → Index.cshtml (chat panel)
    → JS fetch POST /ClientChat/Ask { message }
    → ClientChatController.Ask()
    → AiClientChatService.GetReplyAsync()
        → (ako ima OpenAI key) TryOpenAiAsync + kontekst flote iz baze
        → inače GetRuleBasedReplyAsync (slobodna vozila, cijene, rezervacije)
    → JSON { reply: "..." }
    → prikaz u chat bubble
```

### Autentikacija

- `[AllowAnonymous]` na `ClientChatController` — **javno** (klijent ne mora imati račun)
- `PendingRoleMiddleware` dopušta put `/ClientChat` bez uloge

### Rule-based primjeri

| Upit sadrži | Odgovor |
|-------------|---------|
| slobod / dostup / ima li | Broj slobodnih vozila iz baze |
| rezerv / najam | Upute za rezervaciju |
| cijen / eur | Prosječna dnevna cijena |
| pozdrav / bok | Pozdravna poruka |

---

## 4. Kako pokazati

1. Pokreni app
2. Idi na **Operativa → Klijentski chat** ili `/ClientChat`
3. Upiši: „Ima li slobodnih vozila ovaj vikend?”
4. Odgovor se pojavi u chatu (bez prijave)

**S OpenAI (opcionalno):**

```bash
cd src/CarRent.Web
dotnet user-secrets set "OpenAI:ApiKey" "sk-tvoj-kljuc"
dotnet user-secrets set "OpenAI:Model" "gpt-4o-mini"
# restart app
```

---

## 5. Moguća pitanja profesora

**P: Gdje je AI integracija?**  
O: `AiClientChatService.cs` + `ClientChatController.cs`, stranica `/ClientChat`.

**P: Koristite li ChatGPT / OpenAI?**  
O: Podržano opcionalno; bez ključa radi rule-based asistent s podacima iz EF baze.

**P: Može li AI kreirati rezervaciju?**  
O: Trenutno daje informacije i upute; puni create preko chata moguć je kao nadogradnja (function calling).

**P: Zašto nije WhatsApp?**  
O: Web chat pokriva kriterij „AI upit”; WhatsApp zahtijeva Twilio/Meta setup — planirano kao proširenje.

**P: Odakle AI zna stanje flote?**  
O: `BuildFleetContextAsync()` čita aktivna vozila i zauzete rezervacije iz `CarRentDbContext`.

---

## 6. Što reći na usmenom

> „Klijentski chat prima prirodni jezik, servis dohvaća stanje flote iz baze i odgovara; opcionalno šaljemo kontekst u OpenAI API za pametnije odgovore.”

---

## 7. Daljnji koraci — implementacija (opcionalno)

- **Function calling** — AI direktno zove `POST /api/reservation`
- **Twilio WhatsApp** sandbox — isti `AiClientChatService`, drugi controller
- Povijest razgovora u bazi
- Rate limiting na javni endpoint

---

## 8. Koraci koje TI moraš poduzeti

| Korak | Obavezno? | Akcija |
|-------|-----------|--------|
| Pokreni app | Da | `./scripts/run-local.sh` |
| Demo bez OpenAI | Da | Dovoljno za predaju — rule-based radi odmah |
| OpenAI API key | **Ne** | Samo ako želiš GPT odgovore uživo |
| Kreditna kartica OpenAI | Ne za demo | Rule-based ne troši ništa |

**Za GPT:** kreiraj račun na platform.openai.com, generiraj API key, stavi u user-secrets (ne u git!).
