using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class CauHoiTuLuanYConfiguration : IEntityTypeConfiguration<CauHoiTuLuanY>
{
    public void Configure(EntityTypeBuilder<CauHoiTuLuanY> builder)
    {
        builder.ToTable("CauHoiTuLuanY");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenY)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NoiDung)
            .IsRequired();

        builder.Property(x => x.ThangDiem)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        // Quan hệ CauHoi -> CauHoiTuLuanY (Cascade) đã cấu hình ở CauHoiConfiguration.
        builder.HasOne(x => x.CauHoi)
            .WithMany(x => x.DanhSachY)
            .HasForeignKey(x => x.CauHoiId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CauHoiId);
    }
}