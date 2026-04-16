# Usmeno - kratke biljeske (Lab 2)

## Zasto MVC

- Jasna podjela odgovornosti: Controller obrada, View prikaz, Model podaci.
- Lakse odrzavanje i testiranje odvojenih slojeva.
- Direktno prati predavanje i zadane Lab 2 kriterije.

## Kako radi routing

- U `Program.cs` je route: `{controller=Home}/{action=Index}/{id?}`.
- URL `Vehicle/Details/3` mapira na `VehicleController.Details(int id)`.

## Kako radi mock repository + DI

- Podaci se pune iz `SeedData.Create()` (Lab 1).
- Repozitoriji implementiraju `GetAll` i `GetById`.
- Repozitoriji se registriraju kao singletoni i injektiraju kroz konstruktor kontrolera.

## Kako radi HTML binding

- Viewovi su strongly typed preko `@model`.
- Prikaz podataka ide kroz `@Model.Property`.
- Lista i detalji koriste iste modele/repozitorije bez dupliranja logike.

## Sto je unique u UX-u

- Glassmorphism dizajn (blur paneli, gradijenti, custom kartice).
- Timeline resource schedule prikaz za mjesec.
- Dnevni plan 2 stupca i print A4 fallback.
- Vozni park kartice i partneri tablica kao preduvjet za buduci operativni UI.

## Sub-agent objasnjenje

- Definiran instruction file za UX sub-agenta.
- Vodjen je zaseban log poziva i smjernica.
- Time je dokazano da je UI smjer vođen specijaliziranim agentom.