using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Repository.GenericRepos;

namespace NganHangDeThi.Repository.MonHocRepos;

public class MonHocRepo(AppDbContext context) : GenericRepo<MonHoc, int>(context), IMonHocRepo
{
}
