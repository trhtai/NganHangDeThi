using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Models;

namespace NganHangDeThi.Data.Repositories.Interfaces;

public interface IKhoaRepository
{
    Task<List<Khoa>> GetAllAsync(CancellationToken ct = default);

    Task<PagedResult<Khoa>> GetPagedAsync(
        string? searchText,
        KhoaSortColumn sortColumn,
        SortDirection sortDirection,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    Task<Khoa?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> TenKhoaExistsAsync(string ten, int? excludeId, CancellationToken ct = default);

    Task AddAsync(Khoa item, CancellationToken ct = default);

    Task UpdateAsync(Khoa item, CancellationToken ct = default);

    Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
}
