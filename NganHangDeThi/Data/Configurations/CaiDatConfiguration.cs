using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class CaiDatConfiguration : IEntityTypeConfiguration<CaiDat>
{
    public void Configure(EntityTypeBuilder<CaiDat> builder)
    {
        builder.ToTable("CaiDat");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        // Seed Data.
        builder.HasData(
            new CaiDat
            {
                Id = 1,
                HienThiXacNhanThoat = true,
                DinhDangNgayGio = "dd/MM/yyyy HH:mm"
            }
        );
    }
}
