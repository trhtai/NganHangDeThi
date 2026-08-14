using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class PhuongAnTraLoiConfiguration : IEntityTypeConfiguration<PhuongAnTraLoi>
{
    public void Configure(EntityTypeBuilder<PhuongAnTraLoi> builder)
    {
        builder.ToTable("PhuongAnTraLoi");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.KyTuNhan)
            .IsRequired()
            .HasColumnType("char(1)");

        builder.Property(x => x.NoiDung)
            .IsRequired();

        builder.Property(x => x.LaDapAnDung)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.KhongHoanVi)
            .IsRequired()
            .HasDefaultValue(false);

        // Quan hệ CauHoi -> PhuongAnTraLoi (Cascade) đã cấu hình ở CauHoiConfiguration.
        builder.HasOne(x => x.CauHoi)
            .WithMany(x => x.DanhSachPhuongAn)
            .HasForeignKey(x => x.CauHoiId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ảnh gắn với phương án: không cascade từ đây — xem HinhAnhCauHoiConfiguration.
        builder.HasMany(x => x.DanhSachHinhAnh)
            .WithOne(x => x.PhuongAnTraLoi)
            .HasForeignKey(x => x.PhuongAnTraLoiId)
            .OnDelete(DeleteBehavior.Restrict);

        // Mỗi câu hỏi không được trùng ký tự nhãn (2 đáp án cùng là "A").
        builder.HasIndex(x => new { x.CauHoiId, x.KyTuNhan })
            .IsUnique();
    }
}