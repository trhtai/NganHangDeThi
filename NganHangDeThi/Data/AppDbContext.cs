using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.Entities;
using System.Reflection;

namespace NganHangDeThi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Khoa> Khoa => Set<Khoa>();
    public DbSet<NienKhoa> NienKhoa => Set<NienKhoa>();
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
