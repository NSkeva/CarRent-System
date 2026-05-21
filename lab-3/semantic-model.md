# Semanticki DB model - CarRent

## Entiteti (tablice)

| Entitet | Tablica | Glavna svojstva |
| --- | --- | --- |
| BranchOffice | BranchOffices | Id, Name, LocationType, Address, Phone |
| Vehicle | Vehicles | Id, RegistrationNumber, Brand, Model, Year, Type, MileageKm, IsActive, DailyPrice, BranchOfficeId |
| Customer | Customers | Id, FirstName, LastName, Email, Phone, DateOfBirth, DriverLicenseNumber, CreatedAt |
| Reservation | Reservations | Id, CustomerId, VehicleId, StartDate, EndDate, PickupLocation, DropoffLocation, Status, BasePrice, CreatedAt |
| Addon | Addons | Id, Name, PricePerDay |
| ReservationAddon | ReservationAddons | ReservationId, AddonId, AddonName, PriceAtReservation, Quantity |
| ServiceRecord | ServiceRecords | Id, VehicleId, ServiceDate, Status, Description, MileageAtService, Cost, NextRecommendedServiceDate |
| Employee | Employees | Id, FirstName, LastName, JobTitle, BranchOfficeId, HiredAt |
| Partner | Partners | Id, CompanyName, ContactPerson, Phone, Email |

## Veze

- BranchOffice 1-N Vehicle
- BranchOffice 1-N Employee
- Vehicle 1-N ServiceRecord
- Vehicle 1-N Reservation
- Customer 1-N Reservation
- Reservation N-N Addon (preko ReservationAddon, kompozitni kljuc)
- Reservation 1-N ReservationAddon

## Napomene za EF

- Svi entiteti imaju `[Key]` na `Id` (osim ReservationAddon gdje je kompozitni kljuc).
- Kolekcije su `virtual ICollection<T>`.
- Strani kljucevi imaju `[ForeignKey]` anotacije.
