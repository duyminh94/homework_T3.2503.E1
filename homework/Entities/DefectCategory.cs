using System.ComponentModel.DataAnnotations;

namespace homework.Entities;

public class DefectCategory
{
    [Key]
    public int DefectCategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string SeverityLevel { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public List<InspectionReport> InspectionReports { get; set; } = new();
}
