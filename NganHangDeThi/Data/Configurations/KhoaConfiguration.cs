using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class KhoaConfiguration : IEntityTypeConfiguration<Khoa>
{
    public void Configure(EntityTypeBuilder<Khoa> builder)
    {
        builder.ToTable("Khoa");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.HasIndex(e => e.MaKhoa)
            .IsUnique();

        builder.Property(e => e.MaKhoa)
            .HasMaxLength(20);

        builder.Property(e => e.TenKhoa)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.MoTa)
            .HasMaxLength(500);
    }
}
