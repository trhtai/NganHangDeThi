using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Configurations;

public class LoiImportCauHoiConfiguration : IEntityTypeConfiguration<LoiImportCauHoi>
{
    public void Configure(EntityTypeBuilder<LoiImportCauHoi> builder)
    {
        builder.ToTable("LoiImportCauHoi");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ViTriCauTrongFile)
            .IsRequired();

        builder.Property(x => x.NoiDungLoi)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.DoanVanBanGoc)
            .IsRequired();

        // Quan hệ FileImport -> LoiImportCauHoi (Cascade) đã cấu hình ở FileImportConfiguration.
        builder.HasOne(x => x.FileImport)
            .WithMany(x => x.DanhSachLoi)
            .HasForeignKey(x => x.FileImportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.FileImportId);
    }
}