namespace CarRent.Web.Services;

public static class FleetAiVehicleMatcher
{
    public static FleetOfferedVehicle? Match(string message, FleetClientChatSession session)
    {
        if (session.LastOffered.Count == 0)
            return null;

        var lower = message.ToLowerInvariant();

        if (session.SelectedVehicleId is not null && FleetAiIntentParser.ReferencesPreviousChoice(message))
        {
            return session.LastOffered.FirstOrDefault(v => v.Id == session.SelectedVehicleId)
                   ?? session.LastOffered.FirstOrDefault();
        }

        if (lower.Contains("prv", StringComparison.Ordinal) || lower.Trim() is "1" or "1.")
            return session.LastOffered[0];

        foreach (var vehicle in session.LastOffered)
        {
            if (lower.Contains(vehicle.Brand.ToLowerInvariant(), StringComparison.Ordinal) ||
                lower.Contains(vehicle.Model.ToLowerInvariant(), StringComparison.Ordinal) ||
                lower.Contains(vehicle.RegistrationNumber.ToLowerInvariant(), StringComparison.Ordinal))
                return vehicle;
        }

        if (FleetAiIntentParser.ReferencesPreviousChoice(message) && session.LastOffered.Count == 1)
            return session.LastOffered[0];

        return null;
    }
}
