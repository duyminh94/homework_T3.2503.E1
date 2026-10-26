namespace homework.Dtos;

public class ConfirmReportResultDto
{
    public int ReportId { get; set; }

    public string QualityStatus { get; set; } = string.Empty;

    public bool IsUserConfirmed { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int TotalStock { get; set; }

    public int PassedStock { get; set; }

    public int FailedStock { get; set; }
}
