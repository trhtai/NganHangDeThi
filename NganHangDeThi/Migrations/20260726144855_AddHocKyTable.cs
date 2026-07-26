using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NganHangDeThi.Migrations
{
    /// <inheritdoc />
    public partial class AddHocKyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lop_NienKhoa_NienKhoaId",
                table: "Lop");

            migrationBuilder.DropIndex(
                name: "IX_Lop_NienKhoaId",
                table: "Lop");

            migrationBuilder.DropColumn(
                name: "NamNhapHoc",
                table: "NienKhoa");

            migrationBuilder.DropColumn(
                name: "NienKhoaId",
                table: "Lop");

            migrationBuilder.CreateTable(
                name: "HocKy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenHocKy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NienKhoaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocKy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HocKy_NienKhoa_NienKhoaId",
                        column: x => x.NienKhoaId,
                        principalTable: "NienKhoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HocKy_NienKhoaId",
                table: "HocKy",
                column: "NienKhoaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HocKy");

            migrationBuilder.AddColumn<int>(
                name: "NamNhapHoc",
                table: "NienKhoa",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NienKhoaId",
                table: "Lop",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Lop_NienKhoaId",
                table: "Lop",
                column: "NienKhoaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lop_NienKhoa_NienKhoaId",
                table: "Lop",
                column: "NienKhoaId",
                principalTable: "NienKhoa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
