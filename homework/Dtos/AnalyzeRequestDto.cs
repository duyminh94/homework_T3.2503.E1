namespace homework.Dtos;

public class AnalyzeRequestDto
{
    public string? ProductName { get; set; }

    public int InspectorId { get; set; }

    public IFormFile? Image { get; set; }
}
