using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class MonHocConfiguration : IEntityTypeConfiguration<MonHoc>
{
    public void Configure(EntityTypeBuilder<MonHoc> builder)
    {
        builder.ToTable("MonHoc");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TenMon)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(e => e.Khoa)
            .WithMany(e => e.DanhSachMonHoc)
            .HasForeignKey(e => e.KhoaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
