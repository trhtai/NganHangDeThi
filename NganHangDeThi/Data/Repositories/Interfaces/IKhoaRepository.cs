using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.Data.Repositories.Interfaces;

public interface IKhoaRepository
{
    Task<List<Khoa>> GetAllAsync(CancellationToken ct = default);
}
