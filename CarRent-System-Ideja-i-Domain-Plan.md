# CarRent-System - ideja i domain plan

Dokument je blueprint za implementaciju kroz semestar.

## 1. Vizija projekta

`CarRent-System` je web aplikacija za interno poslovanje rent-a-car firme.

Primarne potrebe:
- pregled raspolozivosti vozila kroz vrijeme
- rad s rezervacijama (kreiranje, izmjene, statusi)
- upravljanje voznim parkom i servisima
- dnevni operativni pregled ulaza/izlaza vozila

---

## 2. MVP podstranice

## Timeline

Svrha:
- prikaz vozila po redovima
- dani u mjesecu po stupcima
- rezervacije kao blokovi koji se protezu preko vise dana

MVP funkcije:
- pregled svih rezervacija po vozilu
- dodavanje rezervacije
- izmjena statusa rezervacije

## Vozni park

Svrha:
- CRUD nad vozilima i drugim sredstvima najma (romobil, skuter, motor)

MVP funkcije:
- grid kartice vozila (slika, osnovni podaci, status)
- akcije: `Preview`, `Edit`, `Delete`
- filtriranje po tipu i statusu

## Dnevni plan (IN/OUT)

Svrha:
- operativni prikaz sto danas krece i sto danas zavrsava

MVP funkcije:
- dvije kolone: `IN` i `OUT`
- kartice rezervacija za danasnji datum

---

## 3. Objektni model za Lab 1

Predlozeno minimalno 8 klasa:

1. `Vehicle` (kompleksna)
2. `Reservation` (kompleksna)
3. `Customer` (kompleksna)
4. `ServiceRecord` (kompleksna)
5. `Addon`
6. `ReservationAddon` (vezna N-N klasa)
7. `BranchOffice`
8. `Employee`

## Predlozeni enumi

- `VehicleType` (`Car`, `Van`, `Scooter`, `Motorcycle`, `Bicycle`)
- `LocationType` (`MainOffice`, `Airport`, `Downtown`, `HotelPartner`, `RemoteDropoff`)
- `ReservationStatus` (`Draft`, `Confirmed`, `Active`, `Completed`, `Cancelled`)
- `ServiceStatus` (`Planned`, `InProgress`, `Completed`, `Cancelled`)

---

## 4. Relacije (zahtjev 1-N i N-N)

## 1-N

- `Customer` -> `Reservations`
- `Vehicle` -> `ServiceRecords`
- `BranchOffice` -> `Vehicles`

## N-N

- `Reservation` <-> `Addon` preko `ReservationAddon`

---

## 5. Seed scenarij za Lab 1

Minimalni seed:
- 3 poslovnice (`BranchOffice`)
- 6+ vozila (`Vehicle`)
- 4 kupca (`Customer`)
- 6+ rezervacija (`Reservation`)
- 5+ servisa (`ServiceRecord`)
- 3 dodatne usluge (`Addon`)

Time su pokriveni:
- barem 3 glavna objekta
- razgranati odnosi i podaci za LINQ upite

---

## 6. Kandidati za LINQ upite

- sva vozila koja su slobodna u odabranom periodu
- broj rezervacija po lokaciji preuzimanja
- vozila koja trebaju servis unutar 30 dana
- dnevni plan IN/OUT (danasnji datum)
- top kupci po broju rezervacija
- ukupna zarada po tipu vozila

---

## 7. Besplatne ASP.NET alternative za Timeline scheduler

Tvoj referentni primjer je Syncfusion Timeline Resource View (odlicno UX rjesenje, ali licencno ograniceno za komercijalnu upotrebu izvan njihovih uvjeta) [Syncfusion Resources in React Scheduler](https://ej2.syncfusion.com/react/documentation/schedule/resources#timeline-resource-view).

## Opcija A - FullCalendar (open source jezgra)

Prednosti:
- stabilan i popularan scheduler
- dobra dokumentacija i community

Nedostatak:
- pravi resource timeline view je premium feature
- za potpuno free varijantu trebao bi fallback prikaz bez resource timeline plugin-a

## Opcija B - Blazor Scheduler open-source komponente (bez enterprise license)

Prednosti:
- cisti .NET/Blazor stack
- nema vendor lock-in premium nivoa ako ostanes na OSS rjesenjima

Nedostatak:
- timeline/resource UX je cesto skromniji ili zahtijeva custom doradu

## Opcija C - Vlastiti timeline prikaz (preporuka za semestar)

Pristup:
- ASP.NET Core + Blazor (Server ili WebAssembly)
- custom grid/tablica: redovi vozila, stupci dani
- rezervacija kao span blok preko dana

Prednosti:
- 100% besplatno
- potpuna kontrola nad funkcionalnostima koje profesor trazi
- mozes implementirati tocno ono sto treba za labose, bez viska

Nedostatak:
- vise pocetnog rada u UI logici

## Preporuka

Za semestralni projekt i kriterije kolegija: **Opcija C (vlastiti timeline u Blazoru)** je najrealniji i najodrziviji smjer.

Razlog:
- izbjegavas premium ovisnost,
- direktno pokazujes znanje modeliranja i poslovne logike,
- funkcionalnost mozes graditi inkrementalno kroz labose.

---

## 8. Tehnicki smjer razvoja po fazama

1. Lab 1: domenski model + seed + LINQ + async demonstracija (konzola)
