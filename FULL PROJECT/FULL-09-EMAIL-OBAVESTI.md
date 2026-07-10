# FULL-09 — Email obavijesti (Gmail SMTP)

**Korak 2** master plana · **Status:** ✅ **GOTOVO** (testirano na `nikola.skeva1@gmail.com`, 10. 07. 2026.)

---

## 1. Kriterij i bodovi

Fleet lifecycle generira obavijesti (registracija, servis, povrat vozila, nacrt rezervacije…). Poruke se ne šalju odmah iz business logike, nego kroz **outbox** i zatim **Gmail SMTP** — pouzdan obrazac za produkciju i ocjenu projekta.

---

## 2. Gdje se u aplikaciji šalju poruke (za korisnika)


| Gdje u UI                                     | Što vidiš                                                           |
| --------------------------------------------- | ------------------------------------------------------------------- |
| **Operativa → Obavijesti** (`/Notifications`) | Tablica svih poruka u outboxu; status **Čeka slanje** / **Poslano** |
| Gumb **Pošalji pending (N)**                  | Ručno šalje sve Email stavke koje čekaju                            |
| Pozadina (automatski)                         | Svakih ~30 s worker šalje pending emailove bez klika                |
| **Gmail inbox**                               | Stvarni primatelj — tvoj mail ili email kupca iz rezervacije        |


**Kada se poruke uopće kreiraju?**

1. **Pri startu aplikacije** — `FleetLifecycleService.SyncAsync()` (registracija vozila, servis sutra, povrati danas…)
2. **Pri promjeni rezervacije** — nacrt ističe, no-show, kilometraža… (`FleetLifecycleRules` + `FleetNotificationService`)

Poruke se **ne šalju** s Početne, Timelinea ili Dnevnog plana direktno — sve ide kroz outbox + dispatcher.

---

## 3. Teorija

1. **Outbox** — `PreparedFleetNotificationSender` zapisuje u `FleetNotificationOutbox` (kanali: `Prepared`, `Email`, opcionalno `Push`).
2. **Dispatcher** — `FleetNotificationOutboxDispatcher` (BackgroundService) poziva `FleetNotificationDispatchService`.
3. **SMTP** — `SmtpEmailTransport` (MailKit) → `smtp.gmail.com:587` + STARTTLS.
4. **Dedup** — `DedupKey` (unique) sprječava duplikate.

---

## 4. Gdje u kodu


| Komponenta             | Putanja                                                                         |
| ---------------------- | ------------------------------------------------------------------------------- |
| Generiranje obavijesti | `Services/FleetNotificationService.cs`                                          |
| Lifecycle trigger      | `Services/FleetLifecycleService.cs`, `Middleware/FleetLifecycleMiddleware.cs`   |
| Outbox zapis           | `Services/Notifications/PreparedFleetNotificationSender.cs`                     |
| SMTP slanje            | `Services/Notifications/SmtpEmailTransport.cs`                                  |
| Dispatch logika        | `Services/Notifications/FleetNotificationDispatchService.cs`                    |
| Background worker      | `Services/Notifications/FleetNotificationOutboxDispatcher.cs`                   |
| Konfiguracija          | `Services/FleetLifecycleOptions.cs` → `FleetNotificationOptions`, `SmtpOptions` |
| UI                     | `Views/Notifications/Index.cshtml`, `Controllers/NotificationsController.cs`    |
| Gmail setup skripta    | `scripts/setup-gmail-secrets.sh`                                                |
| DI registracija        | `Program.cs`                                                                    |
| Tablica                | `FleetNotificationOutbox` u SQLite (`Data/carrent.dev.db`)                      |


---

## 5. Tok podataka

```
[Start app / HTTP zahtjev]
  → FleetLifecycleService.SyncAsync()
    → FleetNotificationService.PrepareDailyNotificationsAsync()
      → PreparedFleetNotificationSender.SendAsync()
        → INSERT FleetNotificationOutbox (Channel = Prepared + Email ako EmailEnabled)

[FleetNotificationOutboxDispatcher, svakih 30s]
  → FleetNotificationDispatchService.ProcessPendingAsync()
    → SELECT * FROM FleetNotificationOutbox WHERE Channel='Email' AND SentAt IS NULL
    → Recipient = red.Recipient ILI FleetNotifications:DefaultRecipient
    → SmtpEmailTransport → Gmail SMTP
    → UPDATE SentAt = UTC now

[Ručno: POST /Notifications/DispatchNow]
  → isti ProcessPendingAsync()
```

**Primatelj:**

- Ako rezervacija ima kupca s emailom → šalje **kupcu** (npr. seed `marko@example.com`).
- Ako nema `Recipient` (registracija, servis…) → šalje na `**DefaultRecipient`** (tvoj Gmail iz user-secrets).

---

## 6. Konfiguracija

### Produkcija / lokalno (Gmail)

```bash
./scripts/setup-gmail-secrets.sh nikola.skeva1@gmail.com "APP_PASSWORD"
```

Tajne u `dotnet user-secrets` (`UserSecretsId`: `carrent-web-local-secrets`) — **ne u gitu**.


| Ključ                                 | Opis                                   |
| ------------------------------------- | -------------------------------------- |
| `FleetNotifications:EmailEnabled`     | `true` — zapisuje Email kanal u outbox |
| `FleetNotifications:DispatchEnabled`  | `true` — pokreće background worker     |
| `FleetNotifications:DefaultRecipient` | Gmail za interne obavijesti            |
| `FleetNotifications:Smtp:`*           | Host, port, user, password, From       |


---

## 7. Demo (2–3 min)

1. `./scripts/setup-gmail-secrets.sh` (jednom)
2. `./scripts/run-local.sh`
3. Prijava `admin@carrent.local` / `Admin123!`
4. **Operativa → Obavijesti** — badge **Gmail SMTP aktivan**
5. **Pošalji pending** ili pričekaj 30 s
6. Gmail inbox — poruka stigla ✅

**Potvrđeni test (07/2026):** `CarRent — test Gmail SMTP` → `nikola.skeva1@gmail.com`

---

## 8. Testiranje

```bash
PATH="$(pwd)/.dotnet:$PATH"
dotnet test tests/CarRent.Web.IntegrationTests --filter FleetNotificationDispatch
```

---

## 9. Pitanja profesora (Q&A)

**Q: Zašto outbox?**  
A: Odvajanje poslovne logike od SMTP-a; retry; audit u bazi.

**Q: Gdje korisnik vidi status?**  
A: Operativa → Obavijesti — stupac Status, SentAt u bazi.

**Q: Tko prima mail?**  
A: `Recipient` na zapisu ili `DefaultRecipient` iz konfiguracije.

**Q: Je li lozinka u repou?**  
A: Ne — user-secrets lokalno; na cloudu env var.

---

## 10. Status


| Stavka                   | Status |
| ------------------------ | ------ |
| Outbox + Email kanal     | ✅      |
| Gmail SMTP (MailKit)     | ✅      |
| Background dispatcher    | ✅      |
| Ručno slanje u UI        | ✅      |
| `setup-gmail-secrets.sh` | ✅      |
| Test na pravom Gmailu    | ✅      |
| FULL-09 dokumentacija    | ✅      |


**Sljedeći korak:** Korak 3 — Push obavijesti → `FULL-10-PUSH-OBAVESTI.md`

---

*Završeno: 10. 07. 2026.*