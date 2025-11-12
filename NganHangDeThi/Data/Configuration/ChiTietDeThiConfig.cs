using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entity;

namespace NganHangDeThi.Data.Configuration;

public class ChiTietDeThiConfig : IEntityTypeConfiguration<ChiTietDeThi>
{
    public void Configure(EntityTypeBuilder<ChiTietDeThi> builder)
    {
        builder.HasKey(x => x.Id);

        // khong ton tai 2 cau hoi trong cung mot de thi
        builder.HasIndex(x => new { x.CauHoiId, x.DeThiId })
            .IsUnique();

        // cau hoi da ra de thi khong duoc xoa
        builder.HasOne(x => x.CauHoi)
            .WithMany()
            .HasForeignKey(x => x.CauHoiId)
            .OnDelete(DeleteBehavior.NoAction);

        // xoa de thi thixoa toan bo chi tiet de thi, cau hoi van ton tai
        builder.HasOne(x => x.DeThi)
            .WithMany(x => x.DsChiTietDeThi)
            .HasForeignKey(x => x.DeThiId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
