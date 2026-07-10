using CarRent.Model.Enums;

namespace CarRent.Web.Services;

public enum FleetClientChatPhase
{
    Idle,
    Browsing,
    SelectingVehicle,
    CollectingContact,
    ReadyToConfirm,
    Completed
}

public sealed class FleetChatTurn
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}

public sealed class FleetOfferedVehicle
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public VehicleType Type { get; set; }
    public decimal DailyPrice { get; set; }

    public string Label => $"{Brand} {Model} ({RegistrationNumber})";

    public static FleetOfferedVehicle From(FleetVehicleSnapshot v) => new()
    {
        Id = v.Id,
        Brand = v.Brand,
        Model = v.Model,
        RegistrationNumber = v.RegistrationNumber,
        Type = v.Type,
        DailyPrice = v.DailyPrice
    };
}

public sealed class FleetClientChatSession
{
    public FleetClientChatPhase Phase { get; set; } = FleetClientChatPhase.Idle;
    public List<FleetChatTurn> History { get; set; } = [];
    public DateOnly? RequestedFrom { get; set; }
    public DateOnly? RequestedTo { get; set; }
    public int? SelectedVehicleId { get; set; }
    public string? SelectedVehicleLabel { get; set; }
    public decimal? SelectedDailyPrice { get; set; }
    public List<FleetOfferedVehicle> LastOffered { get; set; } = [];
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public LocationType? PickupLocation { get; set; }
    public int? CreatedReservationId { get; set; }

    public void AddTurn(string role, string content)
    {
        History.Add(new FleetChatTurn { Role = role, Content = content });
        if (History.Count > 24)
            History.RemoveRange(0, History.Count - 24);
    }
}
