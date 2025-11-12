using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NganHangDeThi.Migrations
{
    /// <inheritdoc />
    public partial class AddHinhAnhToChitietdapantrongdethi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "ChiTietCauTraLoiTrongDeThi",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HinhAnh",
                table: "ChiTietCauTraLoiTrongDeThi");
        }
    }
}
