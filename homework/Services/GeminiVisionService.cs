using System.Text.Json;
using System.Text.RegularExpressions;
using homework.Dtos;
using homework.Entities;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;

namespace homework.Services;

public class GeminiVisionService
{
    private const int MaxRetryCount = 2;
    private const int RetryDelayStepInSeconds = 2;
    private const string DefaultModelName = "gemini-2.5-flash";
    private const string DefaultUnclearMessage = "Ảnh mờ hoặc không thấy rõ sản phẩm, vui lòng chụp lại";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiVisionService> _logger;

    public GeminiVisionService(IConfiguration configuration, ILogger<GeminiVisionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AiInspectionResultDto> AnalyzeImageAsync(byte[] imageBytes, string mimeType, List<DefectCategory> categories)
    {
        var model = CreateModel();

        var imageBase64 = Convert.ToBase64String(imageBytes);

        var request = new GenerateContentRequest(BuildPrompt(categories));
        request.AddPart(new InlineData { MimeType = mimeType, Data = imageBase64 });
        request.GenerationConfig = new GenerationConfig
        {
            Temperature = 0.2f,
            ResponseMimeType = "application/json",
            ResponseSchema = Schema.FromType<AiInspectionResultDto>()
        };

        var rawText = await GenerateWithRetryAsync(model, request);
        _logger.LogInformation("Gemini trả về: {RawText}", rawText);

        return ParseAndValidate(rawText, categories);
    }

    private GenerativeModel CreateModel()
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new AiConfigurationException("Chưa cấu hình Gemini:ApiKey");

        var modelName = _configuration["Gemini:Model"] ?? DefaultModelName;

        try
        {
            return new GoogleAI(apiKey).GenerativeModel(modelName);
        }
        catch (ArgumentException)
        {
            throw new AiConfigurationException("Gemini:ApiKey không đúng định dạng");
        }
    }

    private async Task<string> GenerateWithRetryAsync(GenerativeModel model, GenerateContentRequest request)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                var response = await model.GenerateContent(request);
                return response.Text ?? string.Empty;
            }
            catch (Exception ex) when (ex is GeminiApiException or HttpRequestException)
            {
                var statusCode = ReadStatusCode(ex.Message);
                var isRateLimited = statusCode == 429;
                var hasRetryLeft = attempt < MaxRetryCount;

                if (isRateLimited && hasRetryLeft)
                {
                    attempt++;
                    var delayInSeconds = RetryDelayStepInSeconds * attempt;
                    _logger.LogWarning("Gemini trả 429, thử lại lần {Attempt} sau {Delay} giây", attempt, delayInSeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
                    continue;
                }

                _logger.LogError(ex, "Gọi Gemini thất bại (HTTP {StatusCode})", statusCode);

                if (isRateLimited)
                    throw new AiRateLimitException("Gemini quá tải sau khi đã thử lại");

                throw new AiServiceException("Không gọi được Gemini");
            }
        }
    }

    private static int ReadStatusCode(string errorMessage)
    {
        var match = Regex.Match(errorMessage, "\"code\"\\s*:\\s*(\\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private static AiInspectionResultDto ParseAndValidate(string rawText, List<DefectCategory> categories)
    {
        AiInspectionResultDto? result;
        try
        {
            result = JsonSerializer.Deserialize<AiInspectionResultDto>(rawText, JsonOptions);
        }
        catch (JsonException)
        {
            throw new AiValidationException("AI trả về JSON sai cấu trúc");
        }

        if (result == null)
            throw new AiValidationException("AI trả về dữ liệu rỗng");

        if (!result.IsImageClear)
        {
            var unclearMessage = string.IsNullOrWhiteSpace(result.UnclearReason) ? DefaultUnclearMessage : result.UnclearReason;
            throw new UnclearImageException(unclearMessage);
        }

        var categoryExists = categories.Any(category => category.DefectCategoryId == result.DefectCategoryId);
        if (!categoryExists)
            throw new AiValidationException($"AI chọn loại lỗi không có trong hệ thống: {result.DefectCategoryId}");

        if (string.IsNullOrWhiteSpace(result.DefectDescription))
            throw new AiValidationException("AI không mô tả tình trạng sản phẩm");

        if (result.ConfidenceScore < 0 || result.ConfidenceScore > 1)
            throw new AiValidationException($"Độ tin cậy nằm ngoài khoảng 0-1: {result.ConfidenceScore}");

        return result;
    }

    private static string BuildPrompt(List<DefectCategory> categories)
    {
        var categoryLines = categories.Select(category =>
            $"- ID {category.DefectCategoryId}: {category.CategoryName} (mức độ {category.SeverityLevel}) - {category.Description}");

        return $@"Bạn là chuyên gia kiểm soát chất lượng (QC) sản phẩm. Hãy quan sát kỹ ảnh sản phẩm được gửi kèm và đánh giá theo đúng các quy tắc sau:
1. Chỉ dựa vào những gì nhìn thấy trong ảnh, tuyệt đối không bịa đặt chi tiết.
2. Nếu ảnh bị mờ, quá tối, quá sáng, bị che khuất, hoặc không thấy rõ sản phẩm để đánh giá: đặt isImageClear = false và ghi lý do bằng tiếng Việt vào unclearReason. Khi đó đặt defectCategoryId = 0, defectDescription = chuỗi rỗng, confidenceScore = 0.
3. Nếu ảnh đủ rõ: đặt isImageClear = true, unclearReason = null, rồi chọn ĐÚNG MỘT loại lỗi trong danh sách bên dưới. Nếu sản phẩm có nhiều lỗi, chọn lỗi nghiêm trọng nhất.
4. defectDescription: mô tả bằng tiếng Việt, ngắn gọn 1-3 câu, nêu vị trí và đặc điểm lỗi. Nếu không có lỗi, mô tả sản phẩm đạt chất lượng.
5. confidenceScore: số thực từ 0 đến 1 thể hiện độ chắc chắn của đánh giá.

Danh sách loại lỗi:
{string.Join("\n", categoryLines)}

Chỉ trả về JSON đúng schema, không thêm văn bản nào khác.";
    }
}
