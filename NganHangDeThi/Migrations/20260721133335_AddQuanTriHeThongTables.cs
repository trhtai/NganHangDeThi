using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NganHangDeThi.Migrations
{
    /// <inheritdoc />
    public partial class AddQuanTriHeThongTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Khoa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaKhoa = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TenKhoa = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NienKhoa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenNienKhoa = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NamNhapHoc = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NienKhoa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenMon = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    KhoaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonHoc_Khoa_KhoaId",
                        column: x => x.KhoaId,
                        principalTable: "Khoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lop",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaLop = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TenLop = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    KhoaId = table.Column<int>(type: "INTEGER", nullable: false),
                    NienKhoaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lop", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lop_Khoa_KhoaId",
                        column: x => x.KhoaId,
                        principalTable: "Khoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lop_NienKhoa_NienKhoaId",
                        column: x => x.NienKhoaId,
                        principalTable: "NienKhoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Chuong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenChuong = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ThuTu = table.Column<int>(type: "INTEGER", nullable: false),
                    MonHocId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chuong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chuong_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChuongTrinhHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NamHoc = table.Column<int>(type: "INTEGER", nullable: false),
                    LopId = table.Column<int>(type: "INTEGER", nullable: false),
                    MonHocId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuongTrinhHoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChuongTrinhHoc_Lop_LopId",
                        column: x => x.LopId,
                        principalTable: "Lop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChuongTrinhHoc_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chuong_MonHocId_ThuTu",
                table: "Chuong",
                columns: new[] { "MonHocId", "ThuTu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhHoc_LopId_MonHocId",
                table: "ChuongTrinhHoc",
                columns: new[] { "LopId", "MonHocId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhHoc_MonHocId",
                table: "ChuongTrinhHoc",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_Khoa_MaKhoa",
                table: "Khoa",
                column: "MaKhoa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lop_KhoaId",
                table: "Lop",
                column: "KhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Lop_MaLop",
                table: "Lop",
                column: "MaLop",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lop_NienKhoaId",
                table: "Lop",
                column: "NienKhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_KhoaId",
                table: "MonHoc",
                column: "KhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_NienKhoa_TenNienKhoa",
                table: "NienKhoa",
                column: "TenNienKhoa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chuong");

            migrationBuilder.DropTable(
                name: "ChuongTrinhHoc");

            migrationBuilder.DropTable(
                name: "Lop");

            migrationBuilder.DropTable(
                name: "MonHoc");

            migrationBuilder.DropTable(
                name: "NienKhoa");

            migrationBuilder.DropTable(
                name: "Khoa");
        }
    }
}
