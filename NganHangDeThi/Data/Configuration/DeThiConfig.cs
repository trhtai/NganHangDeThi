using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entity;

namespace NganHangDeThi.Data.Configuration;

public class DeThiConfig : IEntityTypeConfiguration<DeThi>
{
    public void Configure(EntityTypeBuilder<DeThi> builder)
    {
        builder.HasKey(x => x.Id);

        // ma tran da ra de thi khong duoc xoa
        builder.HasOne(x => x.MaTran)
            .WithMany(x => x.DsDeThi)
            .HasForeignKey(x => x.MaTranId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.LopHoc)
            .WithMany(x => x.DsDeThi)
            .HasForeignKey(x => x.LopHocId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
