using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class CauHoiConfiguration : IEntityTypeConfiguration<CauHoi>
{
    public void Configure(EntityTypeBuilder<CauHoi> builder)
    {
        builder.ToTable("CauHoi");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoaiCauHoi)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.NoiDung)
            .IsRequired();

        builder.Property(x => x.NoiDungUnsign)
            .IsRequired();

        builder.Property(x => x.DiemToiDa)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.DaXoa)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Chuong -> CauHoi: Restrict — không cho xoá Chương khi vẫn còn câu hỏi thuộc chương đó.
        builder.HasOne(x => x.Chuong)
            .WithMany()
            .HasForeignKey(x => x.ChuongId)
            .OnDelete(DeleteBehavior.Restrict);

        // MucDoCauHoi -> CauHoi: Restrict — không cho xoá 1 mức độ nếu đang có câu hỏi dùng mức đó.
        // Nullable vì câu hỏi thuộc NhomCauHoi (câu con) không có mức độ (quyết định C4).
        builder.HasOne(x => x.MucDoCauHoi)
            .WithMany(x => x.DanhSachCauHoi)
            .HasForeignKey(x => x.MucDoCauHoiId)
            .OnDelete(DeleteBehavior.Restrict);

        // NhomCauHoi -> CauHoi: cấu hình cascade đã đặt ở NhomCauHoiConfiguration (phía "1"),
        // ở đây chỉ cần khai báo lại field khoá ngoại để tránh EF tự suy convention sai.
        builder.HasOne(x => x.NhomCauHoi)
            .WithMany(x => x.DanhSachCauHoiCon)
            .HasForeignKey(x => x.NhomCauHoiId)
            .OnDelete(DeleteBehavior.Cascade);

        // FileImport -> CauHoi: SetNull — xoá lịch sử import không được xoá câu hỏi thật đã lưu.
        builder.HasOne(x => x.FileImport)
            .WithMany(x => x.DanhSachCauHoiDaLuu)
            .HasForeignKey(x => x.FileImportId)
            .OnDelete(DeleteBehavior.SetNull);

        // Xoá 1 câu hỏi thì xoá luôn phương án trả lời / các Ý tự luận của nó.
        builder.HasMany(x => x.DanhSachPhuongAn)
            .WithOne(x => x.CauHoi)
            .HasForeignKey(x => x.CauHoiId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.DanhSachY)
            .WithOne(x => x.CauHoi)
            .HasForeignKey(x => x.CauHoiId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ảnh gắn với CauHoi: KHÔNG cascade từ đây — xem ghi chú trong HinhAnhCauHoiConfiguration
        // (SQL Server không cho phép nhiều đường cascade cùng trỏ tới 1 bảng con).
        builder.HasMany(x => x.DanhSachHinhAnh)
            .WithOne(x => x.CauHoi)
            .HasForeignKey(x => x.CauHoiId)
            .OnDelete(DeleteBehavior.Restrict);

        // Phục vụ tìm kiếm / phát hiện trùng lặp gần đúng (quyết định G2) và lọc theo chương/loại.
        builder.HasIndex(x => x.ChuongId);
        builder.HasIndex(x => x.LoaiCauHoi);
        builder.HasIndex(x => x.DaXoa);
    }
}