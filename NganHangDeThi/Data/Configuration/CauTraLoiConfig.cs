using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entity;

namespace NganHangDeThi.Data.Configuration;

public class CauTraLoiConfig : IEntityTypeConfiguration<CauTraLoi>
{
    public void Configure(EntityTypeBuilder<CauTraLoi> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.CauHoi)
            .WithMany(x => x.DsCauTraLoi)
            .HasForeignKey(x => x.CauHoiId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
