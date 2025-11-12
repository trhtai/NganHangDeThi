using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entity;

namespace NganHangDeThi.Data.Configuration;

public class CauHoiConfig : IEntityTypeConfiguration<CauHoi>
{
    public void Configure(EntityTypeBuilder<CauHoi> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasOne(x => x.Chuong)
            .WithMany(x => x.DsCauHoi)
            .HasForeignKey(x => x.ChuongId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
