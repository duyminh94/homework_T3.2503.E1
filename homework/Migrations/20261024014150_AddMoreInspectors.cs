using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace homework.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreInspectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Inspectors",
                columns: new[] { "InspectorId", "Department", "Email", "InspectorName" },
                values: new object[,]
                {
                    { 3, "Sản xuất", "chau.le@example.com", "Lê Minh Châu" },
                    { 4, "Đóng gói", "dung.pham@example.com", "Phạm Quốc Dũng" },
                    { 5, "Kiểm soát chất lượng", "lan.hoang@example.com", "Hoàng Thị Lan" },
                    { 6, "Kho vận", "huy.vu@example.com", "Vũ Đức Huy" },
                    { 7, "Sản xuất", "ha.dang@example.com", "Đặng Thu Hà" },
                    { 8, "Đóng gói", "khai.bui@example.com", "Bùi Quang Khải" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 8);
        }
    }
}
