using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace homework.Entities;

public class InspectionReport
{
    [Key]
    public int ReportId { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int InspectorId { get; set; }

    public Inspector? Inspector { get; set; }

    public int DefectCategoryId { get; set; }

    public DefectCategory? DefectCategory { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    public string DefectDescription { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string QualityStatus { get; set; } = string.Empty;

    public double ConfidenceScore { get; set; }

    public bool IsUserConfirmed { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
