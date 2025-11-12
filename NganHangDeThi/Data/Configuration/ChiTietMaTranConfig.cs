using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entity;

namespace NganHangDeThi.Data.Configuration;

public class ChiTietMaTranConfig : IEntityTypeConfiguration<ChiTietMaTran>
{
    public void Configure(EntityTypeBuilder<ChiTietMaTran> builder)
    {
        builder.HasKey(x => x.Id);

        // chuong da duoc su dung trong ma tran thi khong duoc xoa
        builder.HasOne(x => x.Chuong)
            .WithMany()
            .HasForeignKey(x => x.ChuongId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.MaTran)
            .WithMany(x => x.DsChiTietMaTran)
            .HasForeignKey(x => x.MaTranId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
