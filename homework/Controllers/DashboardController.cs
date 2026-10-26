using homework.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(AppDbContext db, ILogger<DashboardController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var totalProducts = await _db.Products.CountAsync();
            var totalStock = await _db.Products.SumAsync(product => product.TotalStock);
            var passedStock = await _db.Products.SumAsync(product => product.PassedStock);
            var failedStock = await _db.Products.SumAsync(product => product.FailedStock);
            var pendingReports = await _db.InspectionReports.CountAsync(report => !report.IsUserConfirmed);

            var defectBreakdown = await _db.InspectionReports
                .AsNoTracking()
                .Where(report => report.IsUserConfirmed)
                .GroupBy(report => new { report.DefectCategory!.CategoryName, report.DefectCategory.SeverityLevel })
                .Select(group => new
                {
                    categoryName = group.Key.CategoryName,
                    severityLevel = group.Key.SeverityLevel,
                    count = group.Count()
                })
                .OrderByDescending(item => item.count)
                .ToListAsync();

            return Ok(new
            {
                totalProducts,
                totalStock,
                passedStock,
                failedStock,
                pendingReports,
                defectBreakdown
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy số liệu dashboard thất bại");
            return StatusCode(500, new { message = "Lỗi hệ thống, vui lòng thử lại sau" });
        }
    }
}
