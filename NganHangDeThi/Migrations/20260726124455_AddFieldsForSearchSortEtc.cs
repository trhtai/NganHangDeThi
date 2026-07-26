using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NganHangDeThi.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsForSearchSortEtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaLopUnSign",
                table: "Lop",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenLopUnSign",
                table: "Lop",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenKhoaUnSign",
                table: "Khoa",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaLopUnSign",
                table: "Lop");

            migrationBuilder.DropColumn(
                name: "TenLopUnSign",
                table: "Lop");

            migrationBuilder.DropColumn(
                name: "TenKhoaUnSign",
                table: "Khoa");
        }
    }
}
