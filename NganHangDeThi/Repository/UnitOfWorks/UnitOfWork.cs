using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Repository.KhoaRepos;
using NganHangDeThi.Repository.LopHocRepos;
using NganHangDeThi.Repository.MonHocRepos;

namespace NganHangDeThi.Repository.UnitOfWorks;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;

    private IMonHocRepo? _monHocRepo;
    private ILopHocRepo? _lopHocRepo;
    private IKhoaRepo? _khoaRepo;

    public IMonHocRepo MonHocRepo { get => _monHocRepo ??= new MonHocRepo(_context); }
    public ILopHocRepo LopHocRepo { get => _lopHocRepo ??= new LopHocRepo(_context); }
    public IKhoaRepo KhoaRepo { get => _khoaRepo ??= new KhoaRepo(_context); }

    public IDbContextTransaction BeginTransaction()
    {
        return _context.Database.BeginTransaction();
    }

    public int ExecuteSqlRaw(string query, params object[] parameters)
    {
        return _context.Database.ExecuteSqlRaw(query, parameters);
    }

    public int Complete()
    {
        return _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
