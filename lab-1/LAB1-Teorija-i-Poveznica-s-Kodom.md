# Lab 1 - Teorija i poveznica s kodom

Ovaj dokument povezuje teoriju iz `lab-1/Lab-1.md` s kodom koji se generira za projekt `CarRent-System`.

## 1. Checklist zahtjeva iz Lab 1

## Repozitorij i predaja

- [x] Javni repozitorij postoji.
- [x] U rootu postoji `lab-1/`.
- [x] Log AI rada postoji (`lab-1/ai_conversation.jsonl`).
- [ ] Kod i dokumentacija pushani su na `main`.

## Objektni model

- [ ] Barem 7 klasa.
- [ ] Barem 4 kompleksne klase (svaka ima vise od 5 svojstava).
- [ ] Barem 1 vlastiti enum.
- [ ] Barem 1 `DateTime` svojstvo.
- [ ] Ispravne veze 1-N i N-N.

## Podaci i upiti

- [ ] U programu postoje barem 3 glavna objekta s razgranatim podacima.
- [ ] Postoje smisleni LINQ upiti.
- [ ] Student zna objasniti i mijenjati LINQ upite.

## Async/Await

- [ ] Razumijevanje `Task`, `async`, `await`.
- [ ] Prakticni primjer async poziva.

---

## 2. C# osnove i mapiranje na CarRent kod

## Klase i svojstva

U C# klasa predstavlja domenski pojam. U ovom projektu to su npr. `Vehicle`, `Reservation`, `Customer`.

- Koristimo javna svojstva (`get; set;`), ne javna polja.
- Svako svojstvo ima smislen tip (`DateTime`, `decimal`, enum, lista).

Primjer:
- `Reservation.StartDate` i `Reservation.EndDate` su `DateTime`.
- `Vehicle.Type` je enum `VehicleType`.

## Konstruktori

Konstruktori inicijaliziraju liste (`new List<...>()`) kako bismo izbjegli `null` reference.

Primjer:
- `Vehicle.ServiceRecords` je odmah inicijaliziran.
- `Reservation.Addons` je odmah inicijaliziran.

## Metode

Metode modeliraju poslovnu logiku.

Primjeri:
- `Reservation.GetDurationDays()`
- `Reservation.TotalCost()`
- helper metode u `DemoQueries` za izvjestaje

---

## 3. Relacije 1-N i N-N

## 1-N

- `Customer` -> vise `Reservation`
- `Vehicle` -> vise `ServiceRecord`

U kodu:
- parent ima kolekciju djece (`List<T>`)
- child drzi strani kljuc (`CustomerId`, `VehicleId`)

## N-N

- `Reservation` <-> `Addon` preko `ReservationAddon`

Zasto vezna klasa:
- lakse je dodati dodatne atribute veze (npr. kolicina, cijena u trenutku rezervacije)
- relacija je jasnija u modelu

---

## 4. Enum i DateTime u praksi

Enumi:
- `VehicleType`
- `LocationType`
- `ReservationStatus`
- `ServiceStatus`

`DateTime`:
- `Reservation.StartDate`, `Reservation.EndDate`
- `ServiceRecord.ServiceDate`

Time se direktno pokrivaju obavezni uvjeti iz Lab 1.

---

## 5. LINQ teorija i konkretni upiti

## Gdje koristimo LINQ

Nad listama rezervacija, vozila i servisa.

## Primjeri koji imaju poslovni smisao

- filtriranje aktivnih ili buducih rezervacija (`Where`)
- sortiranje vozila po kilometrazi (`OrderByDescending`)
- dohvat prve sljedece rezervacije (`FirstOrDefault`)
- broj vozila po tipu (`GroupBy` + `Count`)
- dnevni plan IN/OUT (`Where` nad datumima)

## Zasto je ovo bitno za usmeno

Profesor moze traziti:
- da promijenis kriterij filtriranja
- da dodas sortiranje
- da umjesto `First` koristis `FirstOrDefault` i objasnis razliku

---

## 6. Async/Await teorija i primjena

`async/await` sluzi da ne blokiras glavnu dretvu dok cekas sporiju operaciju.

U konzolnom primjeru:
- `Task.Delay(...)` simulira IO poziv (API ili baza)
- `await` ceka zavrsetak bez blokiranja cijelog programa

U web aplikaciji:
- isto koristis za upite prema bazi i vanjskim servisima
- rezultat je bolja skalabilnost i odziv

---

## 7. Poveznica teorije i koda (sto je gdje)

Planned kod je u `src/CarRent.Console/`:

- `Models/Entities` -> klase i relacije
- `Models/Enums` -> enum tipovi
- `Data/SeedData.cs` -> punjenje modela podacima
- `Services/DemoQueries.cs` -> LINQ upiti
- `Program.cs` -> pokretanje i async demonstracija

---

## 8. Sto mozes reci na usmenom (kratko)

- Zasto koristim enum umjesto stringa.
- Razlika izmedu 1-N i N-N na mom modelu.
- Zasto koristim `FirstOrDefault` umjesto `First`.
- Kako bih prebacio isti model iz konzole u ASP.NET slojeve (Model/Service/DAL).
- Gdje bi u pravoj aplikaciji koristio `async/await`.
