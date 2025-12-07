using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Repository.GenericRepos;

namespace NganHangDeThi.Repository.KhoaRepos;

public class KhoaRepo(AppDbContext context) : GenericRepo<Khoa, int>(context), IKhoaRepo
{
}