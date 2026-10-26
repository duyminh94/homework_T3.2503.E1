namespace homework.Dtos;

public class AiInspectionResultDto
{
    public bool IsImageClear { get; set; }

    public string? UnclearReason { get; set; }

    public int DefectCategoryId { get; set; }

    public string DefectDescription { get; set; } = string.Empty;

    public double ConfidenceScore { get; set; }
}
