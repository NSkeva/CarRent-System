# FULL-10 — Push obavijesti (Web Push)

**Korak 3** master plana · **Status:** ✅ implementirano (čeka uključivanje pusha u browseru)

---

## 1. Kriterij

Browser **push obavijesti** za Admin/Manager — uz email outbox, fleet događaji stižu kao native notifikacija dok je app otvoren ili u pozadini (ovisno o browseru).

---

## 2. Gdje u aplikaciji


| Gdje                       | Što                                               |
| -------------------------- | ------------------------------------------------- |
| **Operativa → Obavijesti** | Gumb **Uključi push** — pretplata browsera        |
| **Pošalji pending**        | Šalje i Email i Push kanale iz outboxa            |
| **Browser**                | Native notifikacija (klik → vraća na Obavijesti)  |
| **Pozadina**               | Dispatcher svakih ~30 s šalje pending Push stavke |


---

## 3. Teorija

1. **Outbox** — `PreparedFleetNotificationSender` zapisuje `Channel=Push` kad je `PushEnabled=true`.
2. **VAPID** — javni/privatni ključ par za Web Push protokol.
3. **Service Worker** — `wwwroot/sw.js` prima push i prikazuje notifikaciju.
4. **Pretplata** — spremljena u `FleetPushSubscriptions` po korisniku i endpointu.

---

## 4. Kod


| Komponenta              | Putanja                                                      |
| ----------------------- | ------------------------------------------------------------ |
| Web Push transport      | `Services/Notifications/WebPushTransport.cs`                 |
| Dispatch (email + push) | `Services/Notifications/FleetNotificationDispatchService.cs` |
| API pretplata           | `Api/Controllers/PushSubscriptionController.cs`              |
| Service worker          | `wwwroot/sw.js`                                              |
| Klijent JS              | `wwwroot/js/push-notifications.js`                           |
| UI                      | `Views/Notifications/Index.cshtml`                           |
| Entitet                 | `FleetPushSubscription`                                      |
| Migracija               | `Migrations/20260710120000_AddPushSubscriptions.cs`          |


---

## 5. Tok podataka

```
Lifecycle → FleetNotificationService → outbox (Channel=Push)

Korisnik: Obavijesti → Uključi push
  → Notification.requestPermission()
  → serviceWorker + pushManager.subscribe(VAPID public)
  → POST /api/push/subscribe → FleetPushSubscriptions

Dispatcher → ProcessPushAsync()
  → za svaku Push stavku bez SentAt
  → WebPushTransport → svi registrirani uređaji
  → SentAt = now
```

---

## 6. Konfiguracija (TI)

```bash
./scripts/setup-push-secrets.sh
# ili ručno user-secrets za WebPush:PrivateKey
```


| Ključ                                   | Opis                             |
| --------------------------------------- | -------------------------------- |
| `FleetNotifications:PushEnabled`        | `true`                           |
| `FleetNotifications:WebPush:PublicKey`  | u `appsettings.Development.json` |
| `FleetNotifications:WebPush:PrivateKey` | **samo user-secrets**            |


---

## 7. Demo

1. Restart app (`./scripts/run-local.sh`)
2. **Operativa → Obavijesti**
3. **Uključi push** → dopusti notifikacije u browseru
4. **Pošalji pending** (ili pričekaj dispatcher)
5. Native notifikacija u OS-u / browseru

**Napomena:** Push radi na `localhost` u Chrome/Firefox. Bez pretplate push stavke ostaju u outboxu dok ne klikneš Uključi push.

---

## 8. Testiranje

```bash
PATH="$(pwd)/.dotnet:$PATH"
dotnet test tests/CarRent.Web.IntegrationTests --filter FleetNotification
```

---

## 9. Status


| Stavka                       | Status   |
| ---------------------------- | -------- |
| Web Push + VAPID             | ✅        |
| Service worker               | ✅        |
| Pretplata API                | ✅        |
| Dispatch Push kanala         | ✅        |
| UI Uključi push              | ✅        |
| `setup-push-secrets.sh`      | ✅        |
| Klik Uključi push u browseru | ⏳ **TI** |


**Sljedeći korak:** Korak 4 — AI chat → `FULL-05-AI-KLIJENTSKI-CHAT.md`

---

*Završeno: 10. 07. 2026.*