using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entity;

namespace NganHangDeThi.Data.Configuration;

public class ChuongConfig : IEntityTypeConfiguration<Chuong>
{
    public void Configure(EntityTypeBuilder<Chuong> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.MonHoc)
            .WithMany(x => x.DsChuong)
            .HasForeignKey(x => x.MonHocId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
