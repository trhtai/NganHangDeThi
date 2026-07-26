using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NganHangDeThi.Migrations
{
    /// <inheritdoc />
    public partial class AddDinhDangNgayGio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DinhDangNgayGio",
                table: "CaiDat",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "CaiDat",
                keyColumn: "Id",
                keyValue: 1,
                column: "DinhDangNgayGio",
                value: "dd/MM/yyyy HH:mm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DinhDangNgayGio",
                table: "CaiDat");
        }
    }
}
