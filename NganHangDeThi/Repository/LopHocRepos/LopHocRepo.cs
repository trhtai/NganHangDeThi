using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Repository.GenericRepos;

namespace NganHangDeThi.Repository.LopHocRepos;

public class LopHocRepo(AppDbContext context) : GenericRepo<LopHoc, int>(context), ILopHocRepo
{
}
