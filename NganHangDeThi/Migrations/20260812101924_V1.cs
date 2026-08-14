using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NganHangDeThi.Migrations
{
    /// <inheritdoc />
    public partial class V1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaiDat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HienThiXacNhanThoat = table.Column<bool>(type: "INTEGER", nullable: false),
                    DinhDangNgayGio = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaiDat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Khoa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenKhoa = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenKhoaUnSign = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MucDoCauHoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaSo = table.Column<int>(type: "INTEGER", nullable: false),
                    TenMucDo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ThuTu = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MucDoCauHoi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NienKhoa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenNienKhoa = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NienKhoa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lop",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaLop = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    KhoaId = table.Column<int>(type: "INTEGER", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "MonHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenMon = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenMonUnSign = table.Column<string>(type: "TEXT", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Chuong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenChuong = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ThuTu = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenChuongUnsign = table.Column<string>(type: "TEXT", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "MaTranDeThi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenMaTran = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MonHocId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaTranDeThi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaTranDeThi_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileImport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenFileGoc = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    DuongDanLuuTru = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    TongSoCauNhanDien = table.Column<int>(type: "INTEGER", nullable: false),
                    SoCauThanhCong = table.Column<int>(type: "INTEGER", nullable: false),
                    SoCauLoi = table.Column<int>(type: "INTEGER", nullable: false),
                    NguoiImport = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MonHocId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChuongId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileImport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileImport_Chuong_ChuongId",
                        column: x => x.ChuongId,
                        principalTable: "Chuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileImport_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeThi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaDe = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MaTranDeThiId = table.Column<int>(type: "INTEGER", nullable: false),
                    LopId = table.Column<int>(type: "INTEGER", nullable: true),
                    HocKyId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeThi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeThi_HocKy_HocKyId",
                        column: x => x.HocKyId,
                        principalTable: "HocKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeThi_Lop_LopId",
                        column: x => x.LopId,
                        principalTable: "Lop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeThi_MaTranDeThi_MaTranDeThiId",
                        column: x => x.MaTranDeThiId,
                        principalTable: "MaTranDeThi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaTranChiTiet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoLuongCau = table.Column<int>(type: "INTEGER", nullable: false),
                    LoaiCauHoi = table.Column<int>(type: "INTEGER", nullable: true),
                    MaTranDeThiId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChuongId = table.Column<int>(type: "INTEGER", nullable: false),
                    MucDoCauHoiId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaTranChiTiet", x => x.Id);
                    table.CheckConstraint("CK_MaTranChiTiet_SoLuongCauDuong", "SoLuongCau > 0");
                    table.ForeignKey(
                        name: "FK_MaTranChiTiet_Chuong_ChuongId",
                        column: x => x.ChuongId,
                        principalTable: "Chuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaTranChiTiet_MaTranDeThi_MaTranDeThiId",
                        column: x => x.MaTranDeThiId,
                        principalTable: "MaTranDeThi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaTranChiTiet_MucDoCauHoi_MucDoCauHoiId",
                        column: x => x.MucDoCauHoiId,
                        principalTable: "MucDoCauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoiImportCauHoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ViTriCauTrongFile = table.Column<int>(type: "INTEGER", nullable: false),
                    NoiDungLoi = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DoanVanBanGoc = table.Column<string>(type: "TEXT", nullable: false),
                    FileImportId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoiImportCauHoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoiImportCauHoi_FileImport_FileImportId",
                        column: x => x.FileImportId,
                        principalTable: "FileImport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhomCauHoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoaiNhom = table.Column<int>(type: "INTEGER", nullable: false),
                    NoiDungDuLieuChung = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ChuongId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileImportId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhomCauHoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NhomCauHoi_Chuong_ChuongId",
                        column: x => x.ChuongId,
                        principalTable: "Chuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NhomCauHoi_FileImport_FileImportId",
                        column: x => x.FileImportId,
                        principalTable: "FileImport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CauHoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoaiCauHoi = table.Column<int>(type: "INTEGER", nullable: false),
                    NoiDung = table.Column<string>(type: "TEXT", nullable: false),
                    DiemToiDa = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    MucDoCauHoiId = table.Column<int>(type: "INTEGER", nullable: true),
                    NhomCauHoiId = table.Column<int>(type: "INTEGER", nullable: true),
                    ThuTuTrongNhom = table.Column<int>(type: "INTEGER", nullable: true),
                    DaXoa = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NoiDungUnsign = table.Column<string>(type: "TEXT", nullable: false),
                    ChuongId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileImportId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauHoi_Chuong_ChuongId",
                        column: x => x.ChuongId,
                        principalTable: "Chuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CauHoi_FileImport_FileImportId",
                        column: x => x.FileImportId,
                        principalTable: "FileImport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CauHoi_MucDoCauHoi_MucDoCauHoiId",
                        column: x => x.MucDoCauHoiId,
                        principalTable: "MucDoCauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CauHoi_NhomCauHoi_NhomCauHoiId",
                        column: x => x.NhomCauHoiId,
                        principalTable: "NhomCauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CauHoiTuLuanY",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenY = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "TEXT", nullable: false),
                    ThangDiem = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ThuTu = table.Column<int>(type: "INTEGER", nullable: false),
                    CauHoiId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoiTuLuanY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauHoiTuLuanY_CauHoi_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeThiCauHoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ThuTuTrongDe = table.Column<int>(type: "INTEGER", nullable: false),
                    ThuTuPhuongAnDaTron = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DeThiId = table.Column<int>(type: "INTEGER", nullable: false),
                    CauHoiId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeThiCauHoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeThiCauHoi_CauHoi_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeThiCauHoi_DeThi_DeThiId",
                        column: x => x.DeThiId,
                        principalTable: "DeThi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhuongAnTraLoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KyTuNhan = table.Column<char>(type: "char(1)", nullable: false),
                    NoiDung = table.Column<string>(type: "TEXT", nullable: false),
                    LaDapAnDung = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    KhongHoanVi = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ThuTu = table.Column<int>(type: "INTEGER", nullable: false),
                    CauHoiId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuongAnTraLoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhuongAnTraLoi_CauHoi_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhCauHoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DuongDanFile = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ViTri = table.Column<int>(type: "INTEGER", nullable: false),
                    ThuTu = table.Column<int>(type: "INTEGER", nullable: false),
                    CauHoiId = table.Column<int>(type: "INTEGER", nullable: true),
                    PhuongAnTraLoiId = table.Column<int>(type: "INTEGER", nullable: true),
                    NhomCauHoiId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhCauHoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HinhAnhCauHoi_CauHoi_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HinhAnhCauHoi_NhomCauHoi_NhomCauHoiId",
                        column: x => x.NhomCauHoiId,
                        principalTable: "NhomCauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HinhAnhCauHoi_PhuongAnTraLoi_PhuongAnTraLoiId",
                        column: x => x.PhuongAnTraLoiId,
                        principalTable: "PhuongAnTraLoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CaiDat",
                columns: new[] { "Id", "DinhDangNgayGio", "HienThiXacNhanThoat" },
                values: new object[] { 1, "dd/MM/yyyy HH:mm", true });

            migrationBuilder.InsertData(
                table: "MucDoCauHoi",
                columns: new[] { "Id", "MaSo", "TenMucDo", "ThuTu" },
                values: new object[,]
                {
                    { 1, 2, "Dễ", 1 },
                    { 2, 3, "Trung bình", 2 },
                    { 3, 4, "Khó", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_ChuongId",
                table: "CauHoi",
                column: "ChuongId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_DaXoa",
                table: "CauHoi",
                column: "DaXoa");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_FileImportId",
                table: "CauHoi",
                column: "FileImportId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_LoaiCauHoi",
                table: "CauHoi",
                column: "LoaiCauHoi");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_MucDoCauHoiId",
                table: "CauHoi",
                column: "MucDoCauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_NhomCauHoiId",
                table: "CauHoi",
                column: "NhomCauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoiTuLuanY_CauHoiId",
                table: "CauHoiTuLuanY",
                column: "CauHoiId");

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
                name: "IX_DeThi_HocKyId",
                table: "DeThi",
                column: "HocKyId");

            migrationBuilder.CreateIndex(
                name: "IX_DeThi_LopId",
                table: "DeThi",
                column: "LopId");

            migrationBuilder.CreateIndex(
                name: "IX_DeThi_MaTranDeThiId_MaDe",
                table: "DeThi",
                columns: new[] { "MaTranDeThiId", "MaDe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeThiCauHoi_CauHoiId",
                table: "DeThiCauHoi",
                column: "CauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_DeThiCauHoi_DeThiId_CauHoiId",
                table: "DeThiCauHoi",
                columns: new[] { "DeThiId", "CauHoiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeThiCauHoi_DeThiId_ThuTuTrongDe",
                table: "DeThiCauHoi",
                columns: new[] { "DeThiId", "ThuTuTrongDe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileImport_ChuongId",
                table: "FileImport",
                column: "ChuongId");

            migrationBuilder.CreateIndex(
                name: "IX_FileImport_CreatedAt",
                table: "FileImport",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FileImport_MonHocId",
                table: "FileImport",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhCauHoi_CauHoiId",
                table: "HinhAnhCauHoi",
                column: "CauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhCauHoi_NhomCauHoiId",
                table: "HinhAnhCauHoi",
                column: "NhomCauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhCauHoi_PhuongAnTraLoiId",
                table: "HinhAnhCauHoi",
                column: "PhuongAnTraLoiId");

            migrationBuilder.CreateIndex(
                name: "IX_HocKy_NienKhoaId",
                table: "HocKy",
                column: "NienKhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_LoiImportCauHoi_FileImportId",
                table: "LoiImportCauHoi",
                column: "FileImportId");

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
                name: "IX_MaTranChiTiet_ChuongId",
                table: "MaTranChiTiet",
                column: "ChuongId");

            migrationBuilder.CreateIndex(
                name: "IX_MaTranChiTiet_MaTranDeThiId_ChuongId_MucDoCauHoiId_LoaiCauHoi",
                table: "MaTranChiTiet",
                columns: new[] { "MaTranDeThiId", "ChuongId", "MucDoCauHoiId", "LoaiCauHoi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaTranChiTiet_MucDoCauHoiId",
                table: "MaTranChiTiet",
                column: "MucDoCauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_MaTranDeThi_MonHocId",
                table: "MaTranDeThi",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_KhoaId",
                table: "MonHoc",
                column: "KhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_MucDoCauHoi_MaSo",
                table: "MucDoCauHoi",
                column: "MaSo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhomCauHoi_ChuongId",
                table: "NhomCauHoi",
                column: "ChuongId");

            migrationBuilder.CreateIndex(
                name: "IX_NhomCauHoi_FileImportId",
                table: "NhomCauHoi",
                column: "FileImportId");

            migrationBuilder.CreateIndex(
                name: "IX_NienKhoa_TenNienKhoa",
                table: "NienKhoa",
                column: "TenNienKhoa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhuongAnTraLoi_CauHoiId_KyTuNhan",
                table: "PhuongAnTraLoi",
                columns: new[] { "CauHoiId", "KyTuNhan" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaiDat");

            migrationBuilder.DropTable(
                name: "CauHoiTuLuanY");

            migrationBuilder.DropTable(
                name: "ChuongTrinhHoc");

            migrationBuilder.DropTable(
                name: "DeThiCauHoi");

            migrationBuilder.DropTable(
                name: "HinhAnhCauHoi");

            migrationBuilder.DropTable(
                name: "LoiImportCauHoi");

            migrationBuilder.DropTable(
                name: "MaTranChiTiet");

            migrationBuilder.DropTable(
                name: "DeThi");

            migrationBuilder.DropTable(
                name: "PhuongAnTraLoi");

            migrationBuilder.DropTable(
                name: "HocKy");

            migrationBuilder.DropTable(
                name: "Lop");

            migrationBuilder.DropTable(
                name: "MaTranDeThi");

            migrationBuilder.DropTable(
                name: "CauHoi");

            migrationBuilder.DropTable(
                name: "NienKhoa");

            migrationBuilder.DropTable(
                name: "MucDoCauHoi");

            migrationBuilder.DropTable(
                name: "NhomCauHoi");

            migrationBuilder.DropTable(
                name: "FileImport");

            migrationBuilder.DropTable(
                name: "Chuong");

            migrationBuilder.DropTable(
                name: "MonHoc");

            migrationBuilder.DropTable(
                name: "Khoa");
        }
    }
}
