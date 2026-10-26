namespace homework.Dtos;

public class ReportItemDto
{
    public int ReportId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string InspectorName { get; set; } = string.Empty;

    public string DefectCategoryName { get; set; } = string.Empty;

    public string SeverityLevel { get; set; } = string.Empty;

    public string DefectDescription { get; set; } = string.Empty;

    public string QualityStatus { get; set; } = string.Empty;

    public double ConfidenceScore { get; set; }

    public bool IsUserConfirmed { get; set; }

    public DateTime CreatedAt { get; set; }
}
