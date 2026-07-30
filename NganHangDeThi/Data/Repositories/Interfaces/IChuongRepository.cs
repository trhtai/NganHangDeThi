using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Models;

namespace NganHangDeThi.Data.Repositories.Interfaces;

public interface IChuongRepository
{
    Task<PagedResult<Chuong>> GetPagedAsync(
        int monHocId,
        string? searchText,
        ChuongSortColumn sortColumn,
        SortDirection sortDirection,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    Task<Chuong?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Tránh trùng tên trong cùng 1 môn học.
    /// </summary>
    Task<bool> TenChuongExistsAsync(int monHocId, string ten, int? excludeId, CancellationToken ct = default);

    /// <summary>
    /// Trả về (ThuTu lớn nhất trong môn học + 1), dùng làm giá trị mặc định khi thêm chương mới.
    /// </summary>
    Task<int> GetNextThuTuAsync(int monHocId, CancellationToken ct = default);

    Task AddAsync(Chuong item, CancellationToken ct = default);
    Task UpdateAsync(Chuong item, CancellationToken ct = default);
    Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
}
