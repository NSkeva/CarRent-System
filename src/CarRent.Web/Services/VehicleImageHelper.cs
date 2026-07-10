using CarRent.Model.Entities;
using CarRent.Model.Enums;

namespace CarRent.Web.Services;

public static class VehicleImageHelper
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    public const long MaxImageSize = 5 * 1024 * 1024;

    private static readonly Dictionary<int, string> SeedVehicleImages = new()
    {
        [1] = "/images/vehicles/skoda-octavia.jpg",
        [2] = "/images/vehicles/vw-golf.jpg",
        [3] = "/images/vehicles/renault-trafic.jpg",
        [4] = "/images/vehicles/piaggio-scooter.jpg",
        [5] = "/images/vehicles/yamaha-mt07.jpg",
        [6] = "/images/vehicles/cube-bicycle.jpg"
    };

    private static readonly Dictionary<VehicleType, string> TypeFallbackImages = new()
    {
        [VehicleType.Car] = "/images/vehicles/vw-golf.jpg",
        [VehicleType.Van] = "/images/vehicles/renault-trafic.jpg",
        [VehicleType.Scooter] = "/images/vehicles/piaggio-scooter.jpg",
        [VehicleType.Motorcycle] = "/images/vehicles/yamaha-mt07.jpg",
        [VehicleType.Bicycle] = "/images/vehicles/cube-bicycle.jpg"
    };

    public static string GetDisplayUrl(Vehicle vehicle)
    {
        if (HasCustomImage(vehicle))
            return vehicle.MainImagePath!;

        if (SeedVehicleImages.TryGetValue(vehicle.Id, out var seedPath))
            return seedPath;

        if (TypeFallbackImages.TryGetValue(vehicle.Type, out var typePath))
            return typePath;

        return "/images/vehicles/vw-golf.jpg";
    }

    public static string? GetSeedDefaultPath(int vehicleId)
        => SeedVehicleImages.TryGetValue(vehicleId, out var path) ? path : null;

    public static bool HasCustomImage(Vehicle vehicle)
        => !string.IsNullOrWhiteSpace(vehicle.MainImagePath);

    public static bool IsAllowedImageExtension(string extension)
        => AllowedImageExtensions.Contains(extension.ToLowerInvariant());

    public static string MainImageFileName(string extension)
        => $"main{extension.ToLowerInvariant()}";
}
