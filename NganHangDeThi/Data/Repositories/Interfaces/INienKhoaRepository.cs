using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Models;

namespace NganHangDeThi.Data.Repositories.Interfaces;

public interface INienKhoaRepository
{
    Task<PagedResult<NienKhoa>> GetPagedAsync(
        string? searchText,
        NienKhoaSortColumn sortColumn,
        SortDirection sortDirection,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    Task<NienKhoa?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> TenNienKhoaExistsAsync(string ten, int? excludeId, CancellationToken ct = default);

    Task AddAsync(NienKhoa item, CancellationToken ct = default);

    Task UpdateAsync(NienKhoa item, CancellationToken ct = default);

    Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
}
