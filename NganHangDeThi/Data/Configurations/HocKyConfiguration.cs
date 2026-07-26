using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class HocKyConfiguration : IEntityTypeConfiguration<HocKy>
{
    public void Configure(EntityTypeBuilder<HocKy> builder)
    {
        builder.ToTable("HocKy");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TenHocKy)
            .IsRequired()
            .HasMaxLength(50);

        // NienKhoa: 1-n
        builder.HasOne(e => e.NienKhoa)
            .WithMany(e => e.DsHocKy)
            .HasForeignKey(e => e.NienKhoaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
