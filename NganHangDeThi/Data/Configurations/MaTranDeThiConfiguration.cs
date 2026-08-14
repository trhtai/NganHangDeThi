using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class MaTranDeThiConfiguration : IEntityTypeConfiguration<MaTranDeThi>
{
    public void Configure(EntityTypeBuilder<MaTranDeThi> builder)
    {
        builder.ToTable("MaTranDeThi");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenMaTran)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.MonHoc)
            .WithMany()
            .HasForeignKey(x => x.MonHocId)
            .OnDelete(DeleteBehavior.Restrict);

        // Xoá 1 ma trận thì xoá luôn các dòng chi tiết cấu hình của nó (không có ý nghĩa đứng riêng).
        builder.HasMany(x => x.DanhSachChiTiet)
            .WithOne(x => x.MaTranDeThi)
            .HasForeignKey(x => x.MaTranDeThiId)
            .OnDelete(DeleteBehavior.Cascade);

        // Không cho xoá ma trận nếu đã có đề thi sinh ra từ nó — tránh mất dữ liệu đề thi đã phát hành.
        builder.HasMany(x => x.DanhSachDeThi)
            .WithOne(x => x.MaTranDeThi)
            .HasForeignKey(x => x.MaTranDeThiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.MonHocId);
    }
}