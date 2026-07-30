using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Models;

namespace NganHangDeThi.Data.Repositories.Interfaces;

public interface IHocKyRepository
{
    Task<PagedResult<HocKy>> GetPagedAsync(
        int nienKhoaId,
        string? searchText,
        HocKySortColumn sortColumn,
        SortDirection sortDirection,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    Task<HocKy?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Trùng tên chỉ tính khi trùng trong CÙNG 1 niên khóa (2 niên khóa khác nhau
    /// đều có thể có "Học kỳ 1", "Học kỳ 2"...).
    /// </summary>
    Task<bool> TenHocKyExistsAsync(int nienKhoaId, string ten, int? excludeId, CancellationToken ct = default);

    Task AddAsync(HocKy item, CancellationToken ct = default);
    Task UpdateAsync(HocKy item, CancellationToken ct = default);
    Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
}