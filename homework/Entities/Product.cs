using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace homework.Entities;

public class Product
{
    [Key]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public int TotalStock { get; set; }

    public int PassedStock { get; set; }

    public int FailedStock { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<InspectionReport> InspectionReports { get; set; } = new();
}
