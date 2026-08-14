using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class FileImportConfiguration : IEntityTypeConfiguration<FileImport>
{
    public void Configure(EntityTypeBuilder<FileImport> builder)
    {
        builder.ToTable("FileImport");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenFileGoc)
            .IsRequired()
            .HasMaxLength(260); // giới hạn độ dài path Windows

        builder.Property(x => x.DuongDanLuuTru)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.NguoiImport)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TrangThai)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Môn/Chương do người dùng chọn tay trước khi import (quyết định A1) — Restrict để
        // không cho xoá Môn/Chương khi đã có lịch sử import gắn vào.
        builder.HasOne(x => x.MonHoc)
            .WithMany()
            .HasForeignKey(x => x.MonHocId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Chuong)
            .WithMany()
            .HasForeignKey(x => x.ChuongId)
            .OnDelete(DeleteBehavior.Restrict);

        // Xoá 1 lượt import thì xoá luôn danh sách lỗi ghi nhận của lượt đó (chỉ là log, không phải
        // dữ liệu nghiệp vụ thật).
        builder.HasMany(x => x.DanhSachLoi)
            .WithOne(x => x.FileImport)
            .HasForeignKey(x => x.FileImportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CreatedAt);
    }
}