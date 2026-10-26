using System.Linq.Expressions;
using homework.Data;
using homework.Dtos;
using homework.Entities;
using homework.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private const int MaxPageSize = 50;

    private static readonly Expression<Func<InspectionReport, ReportItemDto>> ReportItemProjection = report => new ReportItemDto
    {
        ReportId = report.ReportId,
        ImageUrl = report.ImageUrl,
        ProductId = report.ProductId,
        ProductName = report.Product!.ProductName,
        InspectorName = report.Inspector!.InspectorName,
        DefectCategoryName = report.DefectCategory!.CategoryName,
        SeverityLevel = report.DefectCategory!.SeverityLevel,
        DefectDescription = report.DefectDescription,
        QualityStatus = report.QualityStatus,
        ConfidenceScore = report.ConfidenceScore,
        IsUserConfirmed = report.IsUserConfirmed,
        CreatedAt = report.CreatedAt
    };

    private readonly AppDbContext _db;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(AppDbContext db, ILogger<ReportsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetReports(
        [FromQuery] int? productId,
        [FromQuery] string? qualityStatus,
        [FromQuery] bool? isConfirmed,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var isQualityStatusValid = string.IsNullOrWhiteSpace(qualityStatus)
            || qualityStatus is QualityStatusRule.Pass or QualityStatusRule.Fail;
        if (!isQualityStatusValid)
            return BadRequest(new { message = "qualityStatus chỉ nhận Pass hoặc Fail" });

        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        try
        {
            var query = _db.InspectionReports.AsNoTracking().AsQueryable();

            if (productId.HasValue)
                query = query.Where(report => report.ProductId == productId.Value);

            if (!string.IsNullOrWhiteSpace(qualityStatus))
                query = query.Where(report => report.QualityStatus == qualityStatus);

            if (isConfirmed.HasValue)
                query = query.Where(report => report.IsUserConfirmed == isConfirmed.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(report => report.CreatedAt)
                .ThenByDescending(report => report.ReportId)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(ReportItemProjection)
                .ToListAsync();

            return Ok(new { items, totalCount, page = safePage, pageSize = safePageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy lịch sử kiểm tra thất bại");
            return StatusCode(500, new { message = "Lỗi hệ thống, vui lòng thử lại sau" });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var report = await _db.InspectionReports
                .AsNoTracking()
                .Where(item => item.ReportId == id)
                .Select(ReportItemProjection)
                .FirstOrDefaultAsync();

            if (report == null)
                return NotFound(new { message = $"Không tìm thấy phiếu kiểm {id}" });

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy phiếu kiểm {ReportId} thất bại", id);
            return StatusCode(500, new { message = "Lỗi hệ thống, vui lòng thử lại sau" });
        }
    }
}
