using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.Entities;
using System.Reflection;

namespace NganHangDeThi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CauHoi> CauHoi => Set<CauHoi>();
    public DbSet<CauHoiTuLuanY> CauHoiTuLuanY => Set<CauHoiTuLuanY>();
    public DbSet<NhomCauHoi> NhomCauHoi => Set<NhomCauHoi>();
    public DbSet<PhuongAnTraLoi> PhuongAnTraLoi => Set<PhuongAnTraLoi>();
    public DbSet<MucDoCauHoi> MucDoCauHoi => Set<MucDoCauHoi>();
    public DbSet<DeThi> DeThi => Set<DeThi>();
    public DbSet<DeThiCauHoi> DeThiCauHoi => Set<DeThiCauHoi>();
    public DbSet<HinhAnhCauHoi> HinhAnhCauHoi => Set<HinhAnhCauHoi>();
    public DbSet<FileImport> FileImport => Set<FileImport>();
    public DbSet<LoiImportCauHoi> LoiImportCauHoi => Set<LoiImportCauHoi>();
    public DbSet<MaTranDeThi> MaTranDeThi => Set<MaTranDeThi>();
    public DbSet<MaTranChiTiet> MaTranChiTiet => Set<MaTranChiTiet>();

    public DbSet<Khoa> Khoa => Set<Khoa>();
    public DbSet<NienKhoa> NienKhoa => Set<NienKhoa>();
    public DbSet<HocKy> HocKy => Set<HocKy>();
    public DbSet<Lop> Lop => Set<Lop>();
    public DbSet<ChuongTrinhHoc> ChuongTrinhHoc => Set<ChuongTrinhHoc>();
    public DbSet<MonHoc> MonHoc => Set<MonHoc>();
    public DbSet<Chuong> Chuong => Set<Chuong>();
    public DbSet<CaiDat> CaiDat => Set<CaiDat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
