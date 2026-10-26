using homework.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InspectorsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<InspectorsController> _logger;

    public InspectorsController(AppDbContext db, ILogger<InspectorsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var inspectors = await _db.Inspectors
                .AsNoTracking()
                .OrderBy(inspector => inspector.InspectorName)
                .Select(inspector => new
                {
                    inspectorId = inspector.InspectorId,
                    inspectorName = inspector.InspectorName,
                    department = inspector.Department
                })
                .ToListAsync();

            return Ok(inspectors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách nhân viên kiểm tra thất bại");
            return StatusCode(500, new { message = "Lỗi hệ thống, vui lòng thử lại sau" });
        }
    }
}
