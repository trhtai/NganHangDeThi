using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class NhomCauHoiConfiguration : IEntityTypeConfiguration<NhomCauHoi>
{
    public void Configure(EntityTypeBuilder<NhomCauHoi> builder)
    {
        builder.ToTable("NhomCauHoi");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoaiNhom)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.NoiDungDuLieuChung)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Chuong -> NhomCauHoi: Restrict — không cho xoá Chương nếu vẫn còn nhóm câu hỏi thuộc chương đó,
        // tránh xoá lan quá sâu ngoài ý muốn từ danh mục xuống ngân hàng câu hỏi.
        builder.HasOne(x => x.Chuong)
            .WithMany()
            .HasForeignKey(x => x.ChuongId)
            .OnDelete(DeleteBehavior.Restrict);

        // FileImport -> NhomCauHoi: SetNull — xoá lịch sử 1 lượt import không được xoá dữ liệu câu hỏi
        // thật sự đã lưu vào ngân hàng, chỉ mất liên kết truy vết nguồn gốc.
        builder.HasOne(x => x.FileImport)
            .WithMany(x => x.DanhSachNhomDaLuu)
            .HasForeignKey(x => x.FileImportId)
            .OnDelete(DeleteBehavior.SetNull);

        // Xoá 1 nhóm thì xoá luôn toàn bộ câu hỏi con thuộc nhóm đó (chúng không có ý nghĩa đứng riêng lẻ).
        builder.HasMany(x => x.DanhSachCauHoiCon)
            .WithOne(x => x.NhomCauHoi)
            .HasForeignKey(x => x.NhomCauHoiId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ChuongId);
    }
}