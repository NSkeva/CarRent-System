namespace CarRent.Console.Models.Enums;

public enum VehicleType
{
    Car,
    Van,
    Scooter,
    Motorcycle,
    Bicycle
}

public enum LocationType
{
    MainOffice,
    Airport,
    Downtown,
    HotelPartner,
    RemoteDropoff
}

public enum ReservationStatus
{
    Draft,
    Confirmed,
    Active,
    Completed,
    Cancelled
}

public enum ServiceStatus
{
    Planned,
    InProgress,
    Completed,
    Cancelled
}
