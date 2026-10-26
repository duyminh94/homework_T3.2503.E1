using homework.Entities;
using Microsoft.EntityFrameworkCore;

namespace homework.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Inspector> Inspectors => Set<Inspector>();

    public DbSet<DefectCategory> DefectCategories => Set<DefectCategory>();

    public DbSet<InspectionReport> InspectionReports => Set<InspectionReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InspectionReport>()
            .HasOne(report => report.Product)
            .WithMany(product => product.InspectionReports)
            .HasForeignKey(report => report.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InspectionReport>()
            .HasOne(report => report.Inspector)
            .WithMany(inspector => inspector.InspectionReports)
            .HasForeignKey(report => report.InspectorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InspectionReport>()
            .HasOne(report => report.DefectCategory)
            .WithMany(category => category.InspectionReports)
            .HasForeignKey(report => report.DefectCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DefectCategory>().HasData(
            new DefectCategory { DefectCategoryId = 1, CategoryName = "Không lỗi", SeverityLevel = "None", Description = "Sản phẩm đạt chất lượng, không phát hiện lỗi" },
            new DefectCategory { DefectCategoryId = 2, CategoryName = "Trầy xước bề mặt", SeverityLevel = "Minor", Description = "Vết xước nhỏ trên bề mặt, không ảnh hưởng chức năng" },
            new DefectCategory { DefectCategoryId = 3, CategoryName = "Vết bẩn / ố màu", SeverityLevel = "Minor", Description = "Vết bẩn hoặc ố màu nhẹ, có thể làm sạch" },
            new DefectCategory { DefectCategoryId = 4, CategoryName = "Móp méo biến dạng", SeverityLevel = "Major", Description = "Sản phẩm bị móp, méo hoặc biến dạng hình dạng" },
            new DefectCategory { DefectCategoryId = 5, CategoryName = "Nứt vỡ", SeverityLevel = "Major", Description = "Sản phẩm bị nứt, vỡ hoặc hư hỏng nghiêm trọng" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { ProductId = 1, ProductName = "Cốc sứ trắng", Category = "Gốm sứ", TotalStock = 100, PassedStock = 0, FailedStock = 0, CreatedAt = new DateTime(2026, 10, 23, 8, 0, 0, DateTimeKind.Utc) },
            new Product { ProductId = 2, ProductName = "Chai thủy tinh 500ml", Category = "Bao bì", TotalStock = 100, PassedStock = 0, FailedStock = 0, CreatedAt = new DateTime(2026, 10, 23, 8, 0, 0, DateTimeKind.Utc) },
            new Product { ProductId = 3, ProductName = "Hộp carton đóng gói", Category = "Bao bì", TotalStock = 100, PassedStock = 0, FailedStock = 0, CreatedAt = new DateTime(2026, 10, 23, 8, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Inspector>().HasData(
            new Inspector { InspectorId = 1, InspectorName = "Nguyễn Văn An", Department = "Kiểm soát chất lượng", Email = "an.nguyen@example.com" },
            new Inspector { InspectorId = 2, InspectorName = "Trần Thị Bình", Department = "Kho vận", Email = "binh.tran@example.com" },
            new Inspector { InspectorId = 3, InspectorName = "Lê Minh Châu", Department = "Sản xuất", Email = "chau.le@example.com" },
            new Inspector { InspectorId = 4, InspectorName = "Phạm Quốc Dũng", Department = "Đóng gói", Email = "dung.pham@example.com" },
            new Inspector { InspectorId = 5, InspectorName = "Hoàng Thị Lan", Department = "Kiểm soát chất lượng", Email = "lan.hoang@example.com" },
            new Inspector { InspectorId = 6, InspectorName = "Vũ Đức Huy", Department = "Kho vận", Email = "huy.vu@example.com" },
            new Inspector { InspectorId = 7, InspectorName = "Đặng Thu Hà", Department = "Sản xuất", Email = "ha.dang@example.com" },
            new Inspector { InspectorId = 8, InspectorName = "Bùi Quang Khải", Department = "Đóng gói", Email = "khai.bui@example.com" }
        );
    }
}
