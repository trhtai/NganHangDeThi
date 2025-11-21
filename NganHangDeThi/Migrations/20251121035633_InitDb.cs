using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NganHangDeThi.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LopHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLop = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NamHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GVCN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaTran",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianCapNhatGanNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaTran", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenMon = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chuong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViTri = table.Column<int>(type: "int", nullable: false),
                    TenChuong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false)
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
                name: "DeThi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDe = table.Column<int>(type: "int", nullable: false),
                    KyThi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianLamBai = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    MaTranId = table.Column<int>(type: "int", nullable: false),
                    LopHocId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeThi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeThi_LopHoc_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHoc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeThi_MaTran_MaTranId",
                        column: x => x.MaTranId,
                        principalTable: "MaTran",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeThi_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonHocThuocLop",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    LopHocId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHocThuocLop", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonHocThuocLop_LopHoc_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonHocThuocLop_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CauHoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MucDo = table.Column<int>(type: "int", nullable: false),
                    Loai = table.Column<int>(type: "int", nullable: false),
                    DaRaDe = table.Column<bool>(type: "bit", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChuongId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauHoi_CauHoi_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CauHoi",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CauHoi_Chuong_ChuongId",
                        column: x => x.ChuongId,
                        principalTable: "Chuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietMaTran",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoCau = table.Column<int>(type: "int", nullable: false),
                    MucDoCauHoi = table.Column<int>(type: "int", nullable: false),
                    LoaiCauHoi = table.Column<int>(type: "int", nullable: false),
                    ChuongId = table.Column<int>(type: "int", nullable: false),
                    MaTranId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietMaTran", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietMaTran_Chuong_ChuongId",
                        column: x => x.ChuongId,
                        principalTable: "Chuong",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChiTietMaTran_MaTran_MaTranId",
                        column: x => x.MaTranId,
                        principalTable: "MaTran",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CauTraLoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LaDapAnDung = table.Column<bool>(type: "bit", nullable: false),
                    ViTriGoc = table.Column<byte>(type: "tinyint", nullable: false),
                    DaoViTri = table.Column<bool>(type: "bit", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CauHoiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauTraLoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauTraLoi_CauHoi_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDeThi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CauHoiId = table.Column<int>(type: "int", nullable: false),
                    DeThiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDeThi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietDeThi_CauHoi_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CauHoi",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChiTietDeThi_DeThi_DeThiId",
                        column: x => x.DeThiId,
                        principalTable: "DeThi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietCauTraLoiTrongDeThi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LaDapAnDung = table.Column<bool>(type: "bit", nullable: false),
                    ViTri = table.Column<byte>(type: "tinyint", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChiTietDeThiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietCauTraLoiTrongDeThi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietCauTraLoiTrongDeThi_ChiTietDeThi_ChiTietDeThiId",
                        column: x => x.ChiTietDeThiId,
                        principalTable: "ChiTietDeThi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_ChuongId",
                table: "CauHoi",
                column: "ChuongId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_ParentId",
                table: "CauHoi",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CauTraLoi_CauHoiId",
                table: "CauTraLoi",
                column: "CauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietCauTraLoiTrongDeThi_ChiTietDeThiId",
                table: "ChiTietCauTraLoiTrongDeThi",
                column: "ChiTietDeThiId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDeThi_CauHoiId_DeThiId",
                table: "ChiTietDeThi",
                columns: new[] { "CauHoiId", "DeThiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDeThi_DeThiId",
                table: "ChiTietDeThi",
                column: "DeThiId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietMaTran_ChuongId",
                table: "ChiTietMaTran",
                column: "ChuongId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietMaTran_MaTranId",
                table: "ChiTietMaTran",
                column: "MaTranId");

            migrationBuilder.CreateIndex(
                name: "IX_Chuong_MonHocId",
                table: "Chuong",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_DeThi_LopHocId",
                table: "DeThi",
                column: "LopHocId");

            migrationBuilder.CreateIndex(
                name: "IX_DeThi_MaTranId",
                table: "DeThi",
                column: "MaTranId");

            migrationBuilder.CreateIndex(
                name: "IX_DeThi_MonHocId",
                table: "DeThi",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocThuocLop_LopHocId",
                table: "MonHocThuocLop",
                column: "LopHocId");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocThuocLop_MonHocId",
                table: "MonHocThuocLop",
                column: "MonHocId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauTraLoi");

            migrationBuilder.DropTable(
                name: "ChiTietCauTraLoiTrongDeThi");

            migrationBuilder.DropTable(
                name: "ChiTietMaTran");

            migrationBuilder.DropTable(
                name: "MonHocThuocLop");

            migrationBuilder.DropTable(
                name: "ChiTietDeThi");

            migrationBuilder.DropTable(
                name: "CauHoi");

            migrationBuilder.DropTable(
                name: "DeThi");

            migrationBuilder.DropTable(
                name: "Chuong");

            migrationBuilder.DropTable(
                name: "LopHoc");

            migrationBuilder.DropTable(
                name: "MaTran");

            migrationBuilder.DropTable(
                name: "MonHoc");
        }
    }
}
