using homework.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext db, ILogger<ProductsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var products = await _db.Products
                .AsNoTracking()
                .OrderBy(product => product.ProductName)
                .Select(product => new
                {
                    productId = product.ProductId,
                    productName = product.ProductName,
                    category = product.Category,
                    totalStock = product.TotalStock,
                    passedStock = product.PassedStock,
                    failedStock = product.FailedStock,
                    remainingStock = product.TotalStock - product.PassedStock - product.FailedStock
                })
                .ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách sản phẩm thất bại");
            return StatusCode(500, new { message = "Lỗi hệ thống, vui lòng thử lại sau" });
        }
    }
}
