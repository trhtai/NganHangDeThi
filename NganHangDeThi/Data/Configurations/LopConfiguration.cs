using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class LopConfiguration : IEntityTypeConfiguration<Lop>
{
    public void Configure(EntityTypeBuilder<Lop> builder)
    {
        builder.ToTable("Lop");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.HasIndex(e => e.MaLop)
            .IsUnique();

        builder.Property(e => e.MaLop)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.TenLop)
            .HasMaxLength(200);

        // Khoa: 1-n.
        builder.HasOne(e => e.Khoa)
            .WithMany(e => e.DanhSachLop)
            .HasForeignKey(e => e.KhoaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
