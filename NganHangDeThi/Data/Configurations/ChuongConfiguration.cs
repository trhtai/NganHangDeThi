using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class ChuongConfiguration : IEntityTypeConfiguration<Chuong>
{
    public void Configure(EntityTypeBuilder<Chuong> builder)
    {
        builder.ToTable("Chuong");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.HasIndex(e => new { e.MonHocId, e.ThuTu })
            .IsUnique();

        builder.Property(e => e.TenChuong)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(e => e.MonHoc)
            .WithMany(e => e.DanhSachChuong)
            .HasForeignKey(e => e.MonHocId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
