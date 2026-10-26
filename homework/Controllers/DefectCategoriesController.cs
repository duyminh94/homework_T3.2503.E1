using homework.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework.Controllers;

[ApiController]
[Route("api/defect-categories")]
public class DefectCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<DefectCategoriesController> _logger;

    public DefectCategoriesController(AppDbContext db, ILogger<DefectCategoriesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var categories = await _db.DefectCategories
                .AsNoTracking()
                .OrderBy(category => category.DefectCategoryId)
                .Select(category => new
                {
                    defectCategoryId = category.DefectCategoryId,
                    categoryName = category.CategoryName,
                    severityLevel = category.SeverityLevel,
                    description = category.Description
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách loại lỗi thất bại");
            return StatusCode(500, new { message = "Lỗi hệ thống, vui lòng thử lại sau" });
        }
    }
}
