using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRent.Model.Entities;

public class VehicleAttachment
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Vehicle))]
    public int VehicleId { get; set; }

    [MaxLength(260)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(120)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
