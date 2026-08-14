using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class DeThiConfiguration : IEntityTypeConfiguration<DeThi>
{
    public void Configure(EntityTypeBuilder<DeThi> builder)
    {
        builder.ToTable("DeThi");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MaDe)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TrangThai)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Quan hệ MaTranDeThi -> DeThi (Restrict) đã cấu hình ở MaTranDeThiConfiguration.
        builder.HasOne(x => x.MaTranDeThi)
            .WithMany(x => x.DanhSachDeThi)
            .HasForeignKey(x => x.MaTranDeThiId)
            .OnDelete(DeleteBehavior.Restrict);

        // Xoá Lớp/Học kỳ không được xoá đề thi đã sinh, chỉ mất liên kết.
        builder.HasOne(x => x.Lop)
            .WithMany()
            .HasForeignKey(x => x.LopId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.HocKy)
            .WithMany()
            .HasForeignKey(x => x.HocKyId)
            .OnDelete(DeleteBehavior.SetNull);

        // Xoá 1 đề thi thì xoá luôn danh sách câu hỏi thuộc đề đó (bảng liên kết, không phải câu hỏi gốc).
        builder.HasMany(x => x.DanhSachCauHoi)
            .WithOne(x => x.DeThi)
            .HasForeignKey(x => x.DeThiId)
            .OnDelete(DeleteBehavior.Cascade);

        // Mã đề không trùng trong cùng 1 ma trận.
        builder.HasIndex(x => new { x.MaTranDeThiId, x.MaDe })
            .IsUnique();
    }
}