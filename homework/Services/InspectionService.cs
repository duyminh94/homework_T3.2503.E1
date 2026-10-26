using homework.Data;
using homework.Dtos;
using Microsoft.EntityFrameworkCore;

namespace homework.Services;

public class InspectionService
{
    private readonly AppDbContext _db;

    public InspectionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ConfirmReportResultDto> ConfirmReportAsync(int reportId)
    {
        var report = await _db.InspectionReports
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.ReportId == reportId);
        if (report == null)
            throw new ReportNotFoundException($"Không tìm thấy phiếu kiểm {reportId}");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var confirmedRows = await _db.InspectionReports
                .Where(item => item.ReportId == reportId && !item.IsUserConfirmed)
                .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.IsUserConfirmed, true));
            if (confirmedRows == 0)
                throw new ReportAlreadyConfirmedException("Phiếu kiểm này đã được xác nhận trước đó");

            var updatedProductRows = await AddToStockAsync(report.ProductId, report.QualityStatus);
            if (updatedProductRows == 0)
                throw new StockFullException("Sản phẩm đã kiểm đủ số lượng tồn kho, không thể xác nhận thêm");

            var product = await _db.Products
                .AsNoTracking()
                .FirstAsync(item => item.ProductId == report.ProductId);

            await transaction.CommitAsync();

            return new ConfirmReportResultDto
            {
                ReportId = report.ReportId,
                QualityStatus = report.QualityStatus,
                IsUserConfirmed = true,
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                TotalStock = product.TotalStock,
                PassedStock = product.PassedStock,
                FailedStock = product.FailedStock
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private Task<int> AddToStockAsync(int productId, string qualityStatus)
    {
        var productsWithRoom = _db.Products
            .Where(item => item.ProductId == productId && item.PassedStock + item.FailedStock < item.TotalStock);

        return qualityStatus == QualityStatusRule.Pass
            ? productsWithRoom.ExecuteUpdateAsync(setters => setters.SetProperty(item => item.PassedStock, item => item.PassedStock + 1))
            : productsWithRoom.ExecuteUpdateAsync(setters => setters.SetProperty(item => item.FailedStock, item => item.FailedStock + 1));
    }
}
