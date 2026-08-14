using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class MucDoCauHoiConfiguration : IEntityTypeConfiguration<MucDoCauHoi>
{
    public void Configure(EntityTypeBuilder<MucDoCauHoi> builder)
    {
        builder.ToTable("MucDoCauHoi");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenMucDo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.MaSo)
            .IsRequired();

        // Không cho trùng MaSo (VD 2 dòng cùng ghi MaSo = 2).
        builder.HasIndex(x => x.MaSo)
            .IsUnique();

        // Seed sẵn 3 mức theo tài liệu gốc: 2-Dễ, 3-Trung bình, 4-Khó.
        builder.HasData(
            new MucDoCauHoi { Id = 1, MaSo = 2, TenMucDo = "Dễ", ThuTu = 1 },
            new MucDoCauHoi { Id = 2, MaSo = 3, TenMucDo = "Trung bình", ThuTu = 2 },
            new MucDoCauHoi { Id = 3, MaSo = 4, TenMucDo = "Khó", ThuTu = 3 }
        );
    }
}