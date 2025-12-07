using Microsoft.EntityFrameworkCore.Storage;
using NganHangDeThi.Repository.KhoaRepos;
using NganHangDeThi.Repository.LopHocRepos;
using NganHangDeThi.Repository.MonHocRepos;

namespace NganHangDeThi.Repository.UnitOfWorks;

public interface IUnitOfWork : IDisposable
{
    IMonHocRepo MonHocRepo { get; }
    ILopHocRepo LopHocRepo { get; }
    IKhoaRepo KhoaRepo { get; }

    IDbContextTransaction BeginTransaction();
    int ExecuteSqlRaw(string query, params object[] parameters);

    int Complete();
}
