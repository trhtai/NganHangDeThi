using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class DeThiCauHoiConfiguration : IEntityTypeConfiguration<DeThiCauHoi>
{
    public void Configure(EntityTypeBuilder<DeThiCauHoi> builder)
    {
        builder.ToTable("DeThiCauHoi");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ThuTuTrongDe)
            .IsRequired();

        builder.Property(x => x.ThuTuPhuongAnDaTron)
            .HasMaxLength(200);

        // Quan hệ DeThi -> DeThiCauHoi (Cascade) đã cấu hình ở DeThiConfiguration.
        builder.HasOne(x => x.DeThi)
            .WithMany(x => x.DanhSachCauHoi)
            .HasForeignKey(x => x.DeThiId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict — không cho xoá 1 CauHoi trong ngân hàng nếu nó đã được dùng trong 1 đề thi
        // bất kỳ, để bảo toàn dữ liệu đề thi đã phát hành (khớp ghi chú "vòng đời độc lập" ở DeThi).
        builder.HasOne(x => x.CauHoi)
            .WithMany()
            .HasForeignKey(x => x.CauHoiId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1 câu hỏi chỉ xuất hiện đúng 1 lần trong cùng 1 đề thi.
        builder.HasIndex(x => new { x.DeThiId, x.CauHoiId })
            .IsUnique();

        // 1 đề thi không được trùng số thứ tự câu.
        builder.HasIndex(x => new { x.DeThiId, x.ThuTuTrongDe })
            .IsUnique();
    }
}