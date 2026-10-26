using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace homework.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DefectCategories",
                columns: table => new
                {
                    DefectCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SeverityLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefectCategories", x => x.DefectCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Inspectors",
                columns: table => new
                {
                    InspectorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspectors", x => x.InspectorId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalStock = table.Column<int>(type: "int", nullable: false),
                    PassedStock = table.Column<int>(type: "int", nullable: false),
                    FailedStock = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "InspectionReports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    InspectorId = table.Column<int>(type: "int", nullable: false),
                    DefectCategoryId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DefectDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QualityStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    IsUserConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionReports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_InspectionReports_DefectCategories_DefectCategoryId",
                        column: x => x.DefectCategoryId,
                        principalTable: "DefectCategories",
                        principalColumn: "DefectCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionReports_Inspectors_InspectorId",
                        column: x => x.InspectorId,
                        principalTable: "Inspectors",
                        principalColumn: "InspectorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionReports_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DefectCategories",
                columns: new[] { "DefectCategoryId", "CategoryName", "Description", "SeverityLevel" },
                values: new object[,]
                {
                    { 1, "Không lỗi", "Sản phẩm đạt chất lượng, không phát hiện lỗi", "None" },
                    { 2, "Trầy xước bề mặt", "Vết xước nhỏ trên bề mặt, không ảnh hưởng chức năng", "Minor" },
                    { 3, "Vết bẩn / ố màu", "Vết bẩn hoặc ố màu nhẹ, có thể làm sạch", "Minor" },
                    { 4, "Móp méo biến dạng", "Sản phẩm bị móp, méo hoặc biến dạng hình dạng", "Major" },
                    { 5, "Nứt vỡ", "Sản phẩm bị nứt, vỡ hoặc hư hỏng nghiêm trọng", "Major" }
                });

            migrationBuilder.InsertData(
                table: "Inspectors",
                columns: new[] { "InspectorId", "Department", "Email", "InspectorName" },
                values: new object[,]
                {
                    { 1, "Kiểm soát chất lượng", "an.nguyen@example.com", "Nguyễn Văn An" },
                    { 2, "Kho vận", "binh.tran@example.com", "Trần Thị Bình" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Category", "CreatedAt", "FailedStock", "PassedStock", "ProductName", "TotalStock" },
                values: new object[,]
                {
                    { 1, "Gốm sứ", new DateTime(2026, 10, 23, 8, 0, 0, 0, DateTimeKind.Utc), 0, 0, "Cốc sứ trắng", 100 },
                    { 2, "Bao bì", new DateTime(2026, 10, 23, 8, 0, 0, 0, DateTimeKind.Utc), 0, 0, "Chai thủy tinh 500ml", 100 },
                    { 3, "Bao bì", new DateTime(2026, 10, 23, 8, 0, 0, 0, DateTimeKind.Utc), 0, 0, "Hộp carton đóng gói", 100 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_DefectCategoryId",
                table: "InspectionReports",
                column: "DefectCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_InspectorId",
                table: "InspectionReports",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_ProductId",
                table: "InspectionReports",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionReports");

            migrationBuilder.DropTable(
                name: "DefectCategories");

            migrationBuilder.DropTable(
                name: "Inspectors");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
