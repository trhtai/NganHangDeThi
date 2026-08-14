using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class HinhAnhCauHoiConfiguration : IEntityTypeConfiguration<HinhAnhCauHoi>
{
    public void Configure(EntityTypeBuilder<HinhAnhCauHoi> builder)
    {
        builder.ToTable("HinhAnhCauHoi");

        // ĐÃ BỎ CHECK CONSTRAINT "đúng 1 trong 3 khoá ngoại" ở tầng DB vì provider SQLite hỗ trợ
        // CHECK constraint không ổn định khi migrate (SQLite không có ALTER TABLE ADD CONSTRAINT,
        // EF Core phải rebuild lại toàn bộ bảng và bộ sinh SQL cho SQLite dễ lỗi cú pháp với CHECK
        // phức tạp kèm nhiều FK). Rule "đúng 1 trong 3 khoá ngoại phải có giá trị" chuyển sang
        // validate ở tầng Service/Application (VD trong HinhAnhCauHoiService trước khi SaveChanges):
        //
        //   int soLuong = new[] { CauHoiId, PhuongAnTraLoiId, NhomCauHoiId }.Count(x => x != null);
        //   if (soLuong != 1) throw new ValidationException("Ảnh phải gắn với đúng 1 vị trí.");
        //
        // Nếu sau này đổi sang SQL Server/PostgreSQL cho bản production, có thể thêm lại
        // HasCheckConstraint như cũ vì 2 provider đó hỗ trợ ALTER TABLE ADD CONSTRAINT đầy đủ.

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DuongDanFile)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ViTri)
            .IsRequired()
            .HasConversion<int>();

        // QUAN TRỌNG: cả 3 quan hệ dưới đây đặt DeleteBehavior.Restrict thay vì Cascade.
        // Lý do: CauHoi --(cascade)--> PhuongAnTraLoi --(nếu cascade)--> HinhAnhCauHoi và
        // CauHoi --(nếu cascade)--> HinhAnhCauHoi (qua CauHoiId) là 2 đường cascade khác nhau
        // cùng trỏ tới bảng HinhAnhCauHoi → SQL Server sẽ từ chối tạo constraint (multiple
        // cascade paths / có thể gây cycle). Vì vậy: xoá ảnh phải được xử lý tường minh trong
        // tầng Service (xoá ảnh trước, hoặc xoá kèm trong cùng transaction) trước khi xoá
        // CauHoi/PhuongAnTraLoi/NhomCauHoi cha.
        builder.HasOne(x => x.CauHoi)
            .WithMany(x => x.DanhSachHinhAnh)
            .HasForeignKey(x => x.CauHoiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PhuongAnTraLoi)
            .WithMany(x => x.DanhSachHinhAnh)
            .HasForeignKey(x => x.PhuongAnTraLoiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.NhomCauHoi)
            .WithMany(x => x.DanhSachHinhAnh)
            .HasForeignKey(x => x.NhomCauHoiId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}