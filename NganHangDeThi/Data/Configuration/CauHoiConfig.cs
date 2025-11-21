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

        // --- BỔ SUNG MỚI ---
        // Cấu hình quan hệ tự tham chiếu (Self-referencing)
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.DsCauHoiCon)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.NoAction); // Xóa cha không tự động xóa con để tránh lỗi Cycle, ta sẽ xử lý xóa bằng code nếu cần.
        // -------------------
    }
}
