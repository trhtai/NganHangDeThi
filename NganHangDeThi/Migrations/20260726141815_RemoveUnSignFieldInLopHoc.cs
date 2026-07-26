using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NganHangDeThi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnSignFieldInLopHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaLopUnSign",
                table: "Lop");

            migrationBuilder.DropColumn(
                name: "TenLopUnSign",
                table: "Lop");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
