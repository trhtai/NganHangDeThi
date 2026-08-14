using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class MaTranChiTietConfiguration : IEntityTypeConfiguration<MaTranChiTiet>
{
    public void Configure(EntityTypeBuilder<MaTranChiTiet> builder)
    {
        builder.ToTable("MaTranChiTiet", tb =>
            tb.HasCheckConstraint("CK_MaTranChiTiet_SoLuongCauDuong", "SoLuongCau > 0"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SoLuongCau)
            .IsRequired();

        builder.Property(x => x.LoaiCauHoi)
            .HasConversion<int>();

        // Quan hệ MaTranDeThi -> MaTranChiTiet (Cascade) đã cấu hình ở MaTranDeThiConfiguration.
        builder.HasOne(x => x.MaTranDeThi)
            .WithMany(x => x.DanhSachChiTiet)
            .HasForeignKey(x => x.MaTranDeThiId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Chuong)
            .WithMany()
            .HasForeignKey(x => x.ChuongId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MucDoCauHoi)
            .WithMany()
            .HasForeignKey(x => x.MucDoCauHoiId)
            .OnDelete(DeleteBehavior.Restrict);

        // Không cho cấu hình trùng (Chương + Mức độ + Loại câu hỏi) trong cùng 1 ma trận.
        builder.HasIndex(x => new { x.MaTranDeThiId, x.ChuongId, x.MucDoCauHoiId, x.LoaiCauHoi })
            .IsUnique();
    }
}