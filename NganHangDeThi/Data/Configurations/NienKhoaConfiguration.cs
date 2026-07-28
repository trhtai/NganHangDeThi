using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class NienKhoaConfiguration : IEntityTypeConfiguration<NienKhoa>
{
    public void Configure(EntityTypeBuilder<NienKhoa> builder)
    {
        builder.ToTable("NienKhoa");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.HasIndex(e => e.TenNienKhoa)
            .IsUnique();

        builder.Property(e => e.TenNienKhoa)
            .IsRequired()
            .HasMaxLength(50);
    }
}
