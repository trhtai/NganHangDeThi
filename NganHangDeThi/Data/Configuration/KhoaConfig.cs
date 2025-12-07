using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entity;

namespace NganHangDeThi.Data.Configuration;

public class KhoaConfig : IEntityTypeConfiguration<Khoa>
{
    public void Configure(EntityTypeBuilder<Khoa> builder)
    {
        builder.HasKey(x => x.Id);

        // Cấu hình quan hệ 1-N với Lớp học
        builder.HasMany(k => k.DsLopHoc)
               .WithOne(l => l.Khoa)
               .HasForeignKey(l => l.KhoaId)
               .OnDelete(DeleteBehavior.NoAction); // Xóa khoa thì lớp không mất, chỉ set KhoaId = null

        // Cấu hình quan hệ 1-N với Môn học
        builder.HasMany(k => k.DsMonHoc)
               .WithOne(m => m.Khoa)
               .HasForeignKey(m => m.KhoaId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}