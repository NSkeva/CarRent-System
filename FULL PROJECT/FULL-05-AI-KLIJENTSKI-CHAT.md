# FULL-05 — AI integracija (klijentski + operativni chat)

**Kriterij PDF:** AI integracija — unos podataka putem AI upita i sl. — **3 boda**  
**Status:** ✅ Implementirano (višekorakni chat, rule-based + opcionalno OpenAI, upis rezervacije u bazu)  
**Datum završetka koraka:** 2026-07-10

---

## 1. Kriterij i bodovi

Profesor traži da korisnik **prirodnim jezikom** komunicira s aplikacijom, a sustav **odgovara na temelju podataka** ili **pokreće poslovnu akciju** (npr. provjera dostupnosti, rezervacija).


| Što PDF traži          | Naša implementacija                                     |
| ---------------------- | ------------------------------------------------------- |
| AI upit u aplikaciji   | 2 web chata (klijent + operativa)                       |
| Odgovor / akcija       | Dostupnost iz baze, višekorakna rezervacija, email timu |
| Integracija s podacima | EF Core — vozila, rezervacije, kupci                    |


**Napomena:** SMS/WhatsApp nije implementiran; email obavijest timu pokriva „obavijest” nakon rezervacije.

---

## 2. Teorija

### 2.1 Što je „AI integracija” u ovom projektu?

Nije magični black box — sustav ima **tri sloja**:

1. **Razumijevanje upita** — parser namjere, datuma, trajanja, reference na prethodni korak („taj golf”, „7 dana”)
2. **Poslovna logika** — upiti prema SQLite bazi (slobodna vozila, konflikti termina)
3. **Generiranje odgovora** — rule-based tekst (uvijek) + opcionalno **OpenAI GPT** (prirodniji jezik)

To je hibridni pristup koji se u industriji često koristi: **deterministički backend** + **LLM za formulaciju** (ako je API ključ postavljen).

### 2.2 Zašto dva chata?


| Chat                                      | Tko                | Svrha                                                        |
| ----------------------------------------- | ------------------ | ------------------------------------------------------------ |
| **Klijentski** (`/asistent`)              | Javno, bez prijave | Simulacija landing stranice — najam, dostupnost, rezervacija |
| **Operativni** (`/operativa/ai-asistent`) | Admin / Manager    | Dnevni plan, povrati, nacrti, navigacija po sustavu          |


Razdvajanje je sigurnosno i UX: klijent ne vidi interne operacije; tim ima drugačiji kontekst i pitanja.

### 2.3 Session (pamćenje razgovora)

HTTP je stateless. Chat koristi **ASP.NET Session** (cookie `CarRent.Session`) za objekt `FleetClientChatSession`:

- datumi, ponuđena vozila, odabrani model
- kontakt (ime, email, mobitel, lokacija)
- povijest poruka (zadnjih ~24)
- ID kreirane rezervacije (sprječava dupli upis)

Bez sessiona svaka poruka bi bila izolirana — korisnik ne bi mogao reći „ok, taj model 7 dana” nakon prve ponude.

### 2.4 Rule-based vs OpenAI


| Način          | Kada                        | Prednost                                                      |
| -------------- | --------------------------- | ------------------------------------------------------------- |
| **Rule-based** | Uvijek (fallback)           | Besplatno, predvidljivo, radi offline                         |
| **OpenAI GPT** | Ako postoji `OpenAI:ApiKey` | Prirodniji ton; i dalje koristi činjenice iz rule-based sloja |


OpenAI **ne smije izmišljati** vozila — u system prompt šaljemo stanje sessiona i snapshot flote iz baze.

---

## 3. Gdje u kodu — pregled datoteka

```
src/CarRent.Web/
├── Controllers/
│   ├── PublicAssistantController.cs      ← GET /asistent, POST /asistent/ask
│   ├── OperatorAiController.cs           ← GET /operativa/ai-asistent (auth)
│   └── ClientChatController.cs           ← legacy /ClientChat/Ask (MCP)
├── Services/
│   ├── AiClientChatService.cs            ← orkestracija klijentskog chata
│   ├── AiOperatorChatService.cs          ← operativni chat
│   ├── FleetClientChatConversation.cs    ← višekorakni tok rezervacije
│   ├── FleetClientReservationSubmissionService.cs  ← upis u DB + email
│   ├── FleetClientChatSessionStore.cs    ← session load/save
│   ├── FleetClientChatModels.cs          ← session, faze, ponuđena vozila
│   ├── FleetAiAvailabilityService.cs     ← slobodna vozila po terminu
│   ├── FleetAiDateParser.cs              ← vikend, 15.6., trajanje
│   ├── FleetAiIntentParser.cs            ← namjera, kontakt, „da”
│   ├── FleetAiVehicleMatcher.cs          ← „golf”, „prvi”, „taj model”
│   └── FleetAiOpenAiHelper.cs            ← HTTP prema OpenAI API
├── Views/
│   ├── PublicAssistant/Index.cshtml
│   ├── OperatorAi/Index.cshtml
│   └── Shared/_FleetChatPanel.cshtml
├── wwwroot/js/fleet-ai-chat.js
└── Program.cs                            ← AddSession, registracija servisa

tests/CarRent.Web.IntegrationTests/
├── AiChatIntegrationTests.cs
├── FleetAiIntentParserTests.cs
├── FleetAiDateParserTests.cs
├── FleetAiAvailabilityServiceTests.cs
└── FleetClientReservationSubmissionTests.cs

src/CarRent.McpServer/CarRentTools.cs     ← AskClientAssistant → /asistent/ask
scripts/setup-openai-secrets.sh
```

---

## 4. Tok podataka

### 4.1 Klijentski chat — jedna poruka

```
Browser (fleet-ai-chat.js)
  POST /asistent/ask  { "message": "..." }
  Cookie: session id
        ↓
PublicAssistantController.Ask()
        ↓
FleetClientChatSessionStore.Load()
        ↓
AiClientChatService.GetReplyAsync(message, session)
        ├─ FleetClientChatConversation.ProcessAsync()   ← ažurira fazu + logika
        ├─ (opcionalno) FleetAiOpenAiHelper.TryConversationAsync()
        └─ session.AddTurn(user/assistant)
        ↓
FleetClientChatSessionStore.Save()
        ↓
JSON { "reply": "..." }
```

### 4.2 Višekorakna rezervacija (faze)

```
Idle / Browsing
  → korisnik: „Trebam auto ovaj vikend”
  → FleetAiDateParser (vikend) + FleetAiAvailabilityService
  → faza: SelectingVehicle, LastOffered = lista slobodnih

SelectingVehicle
  → korisnik: „Ok Golf 7 dana”
  → FleetAiVehicleMatcher + TryParseRentalDays
  → provjera konflikta u bazi
  → faza: CollectingContact

CollectingContact
  → korisnik: ime, email, mob, lokacija
  → FleetAiIntentParser.TryExtractContact
  → faza: ReadyToConfirm

ReadyToConfirm
  → korisnik: „da”
  → FleetClientReservationSubmissionService.SubmitAsync()
        ├─ FindOrCreateCustomer
        ├─ Reservation (Draft) u SQLite
        ├─ FleetNotificationOutbox (ClientAssistantBooking)
        └─ FleetNotificationDispatchService (email timu)
  → faza: Completed
```

### 4.3 Operativni chat

```
POST /operativa/ai-asistent/ask  [Authorize Admin,Manager]
  → AiOperatorChatService
  → kontekst: odlasci/povrati danas, nacrti, neaktivna vozila
  → rule-based ili OpenAI
```

---

## 5. Konfiguracija

### 5.1 OpenAI (opcionalno)

`appsettings.Development.json.example`:

```json
"OpenAI": {
  "ApiKey": "",
  "Model": "gpt-4o-mini"
}
```

```bash
./scripts/setup-openai-secrets.sh
# ili:
dotnet user-secrets set "OpenAI:ApiKey" "sk-..." --project src/CarRent.Web
dotnet user-secrets set "OpenAI:Model" "gpt-4o-mini" --project src/CarRent.Web
```

Bez ključa — **sve radi** rule-based.

### 5.2 Session

`Program.cs`:

- `AddDistributedMemoryCache()` + `AddSession()`
- `app.UseSession()` prije autentikacije

### 5.3 Rute


| URL                      | Auth                |
| ------------------------ | ------------------- |
| `/asistent`              | Javno               |
| `/asistent/ask`          | Javno               |
| `/operativa/ai-asistent` | Admin, Manager      |
| `/ClientChat/Ask`        | Javno (legacy, MCP) |


`PendingRoleMiddleware` dopušta `/asistent` korisnicima bez uloge.

---

## 6. Kako pokazati uživo (demo ~5 min)

### A) Klijentski asistent (javno)

1. `./scripts/run-local.sh` → [http://localhost:5000/asistent](http://localhost:5000/asistent)
2. Poruka: **„Trebam auto ovaj vikend”** → lista slobodnih vozila
3. **„Ok, Golf 7 dana”** → procjena cijene + pitanje za kontakt
4. **„Marko Horvat, [marko@test.com](mailto:marko@test.com), 0912345678, aerodrom”**
5. **„Da”** → poruka s brojem rezervacije

Provjera u sustavu:

- **Podaci → Rezervacije** — novi **Nacrt**
- **Operativa → Obavijesti** — email „AI asistent — nova rezervacija #…”
- Inbox `nikola.skeva1@gmail.com` (ako je SMTP postavljen)

### B) Operativni asistent

1. Prijava `manager@carrent.local` / `Manager123!`
2. Operativa → **AI asistent**
3. **„Što je danas na rasporedu?”** / gumbi **Danas**, **Povrati**

### C) MCP (opcionalno)

Cursor MCP tool `AskClientAssistant` → `POST /asistent/ask`

---

## 7. Testiranje

```bash
# Integracija
dotnet test tests/CarRent.Web.IntegrationTests/ --filter "AiChat|FleetAi|FleetClient"

# E2E (korak 9 — javni asistent)
./scripts/run-e2e.sh
```


| Test                                    | Što provjerava                              |
| --------------------------------------- | ------------------------------------------- |
| `AiChatIntegrationTests`                | anonimni ask, auth na operativi, multi-turn |
| `FleetClientReservationSubmissionTests` | cijeli flow do Draft rezervacije + outbox   |
| `FleetAiIntentParserTests`              | „trebam auto”, „7 dana”, match Golf         |
| `FleetAiAvailabilityServiceTests`       | EF upit bez crasha                          |


---

## 8. Pitanja profesora (Q&A)

**P: Gdje je AI integracija?**  
O: Dvije stranice — `/asistent` (klijent), `/operativa/ai-asistent` (tim). Servisi u `Services/Ai*.cs` i `FleetClient*.cs`.

**P: Koristite li ChatGPT?**  
O: Opcionalno preko OpenAI API (`FleetAiOpenAiHelper`). Bez ključa radi rule-based s podacima iz baze.

**P: Odakle zna što je slobodno?**  
O: `FleetAiAvailabilityService` — aktivna vozila minus rezervacije u statusima Draft/Confirmed/Active za zadani period (`ReservationAvailabilityHelper` logika).

**P: Može li AI kreirati rezervaciju?**  
O: Da. Na „da” u zadnjoj fazi `FleetClientReservationSubmissionService` kreira `Reservation` (Draft) i `Customer`, te šalje email timu (`ClientAssistantBooking`).

**P: Zašto nije stigao SMS na mobitel?**  
O: Nema SMS providera (Twilio). Obavijest ide **emailom timu**; klijent dobije poruku u chatu da će ga tim kontaktirati.

**P: Kako pamti „taj model”?**  
O: `FleetClientChatSession.LastOffered` + `FleetAiVehicleMatcher` — match po brand/model/registraciji ili „prvi” / „taj”.

**P: Je li chat siguran za javni internet?**  
O: Javni endpoint samo čita flotu i kreira Draft rezervaciju; nema pristupa CRUD-u bez uloge. Rate limiting nije implementiran (moguća nadogradnja).

**P: Razlika klijent vs operativa?**  
O: Različit system prompt i rule set — klijent: rezervacija; operativa: dnevni plan, linkovi na Timeline/Rezervacije.

**P: Gdje je MCP?**  
O: `CarRent.McpServer` → `AskClientAssistant` → `/asistent/ask`.

---

## 9. Što reći na usmenom (30 sekundi)

> „Implementirali smo dva AI chata: javni klijentski asistent na `/asistent` i interni za tim. Chat pamti kontekst u sessionu, parsira datume i modele iz prirodnog jezika, dohvaća dostupnost iz EF baze, a na potvrdu kreira Draft rezervaciju i šalje email obavijest. Rule-based radi bez API ključa; opcionalno dodajemo OpenAI za prirodniji odgovor.”

---

## 10. Ograničenja i poznati nedostatci


| Tema                   | Stanje                               |
| ---------------------- | ------------------------------------ |
| SMS klijentu           | ❌ Nije implementirano                |
| Email potvrda klijentu | ❌ Samo tim (DefaultRecipient)        |
| WhatsApp               | ❌ Planirano kao proširenje           |
| GPT bez ključa         | Rule-based — manje „fleksibilan” ton |
| Potvrda rezervacije    | Draft — tim ručno potvrđuje u CRUD-u |


---

## 11. Sljedeći korak projekta

Prema `FULL-00-MASTER-PLAN.md`: **Korak 5 — Google OAuth** → `FULL-11-GOOGLE-OAUTH.md`

---

*Ažurirano: Korak 4 FULL projekta — AI chat.*