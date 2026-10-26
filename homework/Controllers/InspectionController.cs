using homework.Data;
using homework.Dtos;
using homework.Entities;
using homework.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework.Controllers;

[ApiController]
[Route("api")]
public class InspectionController : ControllerBase
{
    private const int MaxProductNameLength = 200;
    private const int NewProductTotalStock = 100;
    private const string NewProductCategory = "Chưa phân loại";

    private readonly AppDbContext _db;
    private readonly ImageStorageService _imageStorage;
    private readonly GeminiVisionService _geminiVision;
    private readonly InspectionService _inspectionService;
    private readonly ILogger<InspectionController> _logger;

    public InspectionController(
        AppDbContext db,
        ImageStorageService imageStorage,
        GeminiVisionService geminiVision,
        InspectionService inspectionService,
        ILogger<InspectionController> logger)
    {
        _db = db;
        _imageStorage = imageStorage;
        _geminiVision = geminiVision;
        _inspectionService = inspectionService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromForm] AnalyzeRequestDto request)
    {
        string? savedImageUrl = null;
        var analysisSucceeded = false;

        try
        {
            var productName = request.ProductName?.Trim() ?? string.Empty;
            if (productName.Length == 0)
                return BadRequest(new { message = "Nhập tên sản phẩm cần kiểm tra" });

            if (productName.Length > MaxProductNameLength)
                return BadRequest(new { message = $"Tên sản phẩm tối đa {MaxProductNameLength} ký tự" });

            var product = await _db.Products.FirstOrDefaultAsync(item => item.ProductName == productName)
                ?? new Product { ProductName = productName, Category = NewProductCategory, TotalStock = NewProductTotalStock };

            var inspectorExists = await _db.Inspectors.AnyAsync(inspector => inspector.InspectorId == request.InspectorId);
            if (!inspectorExists)
                return BadRequest(new { message = "Không tìm thấy nhân viên kiểm tra" });

            savedImageUrl = await _imageStorage.SaveImageAsync(request.Image);

            var categories = await _db.DefectCategories.AsNoTracking().ToListAsync();
            var (imageBytes, mimeType) = await _imageStorage.ReadImageAsync(savedImageUrl);
            var result = await _geminiVision.AnalyzeImageAsync(imageBytes, mimeType, categories);
            var category = categories.First(item => item.DefectCategoryId == result.DefectCategoryId);

            var draftReport = CreateDraftReport(request, product, savedImageUrl, category, result);
            _db.InspectionReports.Add(draftReport);
            await _db.SaveChangesAsync();

            analysisSucceeded = true;
            return Ok(new
            {
                reportId = draftReport.ReportId,
                productName = product.ProductName,
                imageUrl = draftReport.ImageUrl,
                defectCategoryId = category.DefectCategoryId,
                defectCategoryName = category.CategoryName,
                severityLevel = category.SeverityLevel,
                defectDescription = draftReport.DefectDescription,
                qualityStatus = draftReport.QualityStatus,
                confidenceScore = draftReport.ConfidenceScore,
                isUserConfirmed = draftReport.IsUserConfirmed
            });
        }
        catch (InvalidImageException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnclearImageException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (AiValidationException ex)
        {
            _logger.LogWarning(ex, "AI trả kết quả không hợp lệ cho sản phẩm {ProductName}", request.ProductName);
            return UnprocessableEntity(new { message = "AI trả kết quả không hợp lệ, vui lòng thử lại" });
        }
        catch (AiRateLimitException ex)
        {
            _logger.LogWarning(ex, "Gemini quá tải");
            return StatusCode(429, new { message = "AI đang quá tải, vui lòng thử lại sau ít phút" });
        }
        catch (AiServiceException ex)
        {
            _logger.LogError(ex, "Không gọi được dịch vụ AI");
            return StatusCode(502, new { message = "Không kết nối được dịch vụ AI, vui lòng thử lại sau" });
        }
        catch (AiConfigurationException ex)
        {
            _logger.LogError(ex, "Cấu hình Gemini có vấn đề");
            return StatusCode(500, new { message = "Hệ thống AI chưa được cấu hình đúng" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Phân tích ảnh thất bại cho sản phẩm {ProductName}", request.ProductName);
            return StatusCode(500, new { message = "Lỗi hệ thống, vui lòng thử lại sau" });
        }
        finally
        {
            if (!analysisSucceeded && savedImageUrl != null)
                _imageStorage.DeleteImage(savedImageUrl);
        }
    }

    [HttpPost("confirm-report")]
    public async Task<IActionResult> ConfirmReport([FromBody] ConfirmReportRequestDto request)
    {
        try
        {
            var result = await _inspectionService.ConfirmReportAsync(request.ReportId);
            return Ok(result);
        }
        catch (ReportNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ReportAlreadyConfirmedException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (StockFullException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xác nhận phiếu kiểm {ReportId} thất bại", request.ReportId);
            return StatusCode(500, new { message = "Lỗi hệ thống, vui lòng thử lại sau" });
        }
    }

    private static InspectionReport CreateDraftReport(
        AnalyzeRequestDto request,
        Product product,
        string imageUrl,
        DefectCategory category,
        AiInspectionResultDto result)
    {
        return new InspectionReport
        {
            Product = product,
            InspectorId = request.InspectorId,
            DefectCategoryId = category.DefectCategoryId,
            ImageUrl = imageUrl,
            DefectDescription = result.DefectDescription,
            QualityStatus = QualityStatusRule.FromSeverity(category.SeverityLevel),
            ConfidenceScore = result.ConfidenceScore,
            IsUserConfirmed = false
        };
    }
}
