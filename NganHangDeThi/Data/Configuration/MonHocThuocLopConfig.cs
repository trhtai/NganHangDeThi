using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entity;

namespace NganHangDeThi.Data.Configuration;

public class MonHocThuocLopConfig : IEntityTypeConfiguration<MonHocThuocLop>
{
    public void Configure(EntityTypeBuilder<MonHocThuocLop> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.MonHoc)
            .WithMany()
            .HasForeignKey(x => x.MonHocId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasOne(x => x.LopHoc)
            .WithMany(x => x.DsMonHoc)
            .HasForeignKey(x => x.LopHocId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
