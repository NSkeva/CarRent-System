using CarRent.Model.Entities;

namespace CarRent.Web.Services;

public static class VehicleImageHelper
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    public const long MaxImageSize = 5 * 1024 * 1024;

    public static string GetDisplayUrl(Vehicle vehicle)
        => HasCustomImage(vehicle)
            ? vehicle.MainImagePath!
            : $"https://picsum.photos/seed/carrent-{vehicle.Id}/420/240";

    public static bool HasCustomImage(Vehicle vehicle)
        => !string.IsNullOrWhiteSpace(vehicle.MainImagePath);

    public static bool IsAllowedImageExtension(string extension)
        => AllowedImageExtensions.Contains(extension.ToLowerInvariant());

    public static string MainImageFileName(string extension)
        => $"main{extension.ToLowerInvariant()}";
}
