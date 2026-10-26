using System.ComponentModel.DataAnnotations;

namespace homework.Entities;

public class Inspector
{
    [Key]
    public int InspectorId { get; set; }

    [Required]
    [MaxLength(100)]
    public string InspectorName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    public List<InspectionReport> InspectionReports { get; set; } = new();
}
