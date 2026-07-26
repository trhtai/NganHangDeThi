using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class ChuongTrinhHocConfiguration : IEntityTypeConfiguration<ChuongTrinhHoc>
{
    public void Configure(EntityTypeBuilder<ChuongTrinhHoc> builder)
    {
        builder.ToTable("ChuongTrinhHoc");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.HasIndex(e => new { e.LopId, e.MonHocId })
            .IsUnique();

        builder.HasOne(e => e.Lop)
            .WithMany(e => e.DanhSachMonHoc)
            .HasForeignKey(e => e.LopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.MonHoc)
            .WithMany()
            .HasForeignKey(e => e.MonHocId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
