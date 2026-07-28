using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Models;

namespace NganHangDeThi.Data.Repositories.Interfaces;

public interface ILopRepository
{
    Task<PagedResult<Lop>> GetPagedAsync(
        string? searchText,
        LopSortColumn sortColumn,
        SortDirection sortDirection,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    Task<Lop?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> MaLopExistsAsync(string ten, int? excludeId, CancellationToken ct = default);

    Task AddAsync(Lop item, CancellationToken ct = default);

    Task UpdateAsync(Lop item, CancellationToken ct = default);

    Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
}
