using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Models;

namespace NganHangDeThi.Data.Repositories.Interfaces;

public interface IMonHocRepository
{
    Task<PagedResult<MonHoc>> GetPagedAsync(
        string? searchText,
        MonHocSortColumn sortColumn,
        SortDirection sortDirection,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    Task<MonHoc?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> TenMonHocExistsAsync(string tenMonHoc, int? excludeId, CancellationToken ct = default);

    Task AddAsync(MonHoc monHoc, CancellationToken ct = default);

    Task UpdateAsync(MonHoc monHoc, CancellationToken ct = default);

    Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
}
