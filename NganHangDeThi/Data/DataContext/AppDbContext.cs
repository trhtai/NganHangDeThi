using System.Reflection;
using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.Entity;

namespace NganHangDeThi.Data.DataContext;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public required DbSet<LopHoc> LopHoc { get; set; }
    public required DbSet<MonHoc> MonHoc { get; set; }
    public required DbSet<MonHocThuocLop> MonHocThuocLop { get; set; }
    public required DbSet<Chuong> Chuong { get; set; }
    public required DbSet<CauHoi> CauHoi { get; set; }
    public required DbSet<CauTraLoi> CauTraLoi { get; set; }
    public required DbSet<DeThi> DeThi { get; set; }
    public required DbSet<ChiTietDeThi> ChiTietDeThi { get; set; }
    public required DbSet<MaTran> MaTran { get; set; }
    public required DbSet<ChiTietMaTran> ChiTietMaTran { get; set; }
    public required DbSet<ChiTietCauTraLoiTrongDeThi> ChiTietCauTraLoiTrongDeThi { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
