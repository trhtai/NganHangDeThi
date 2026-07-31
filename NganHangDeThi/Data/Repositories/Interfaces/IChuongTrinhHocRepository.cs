using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Models;

namespace NganHangDeThi.Data.Repositories.Interfaces;

public interface IChuongTrinhHocRepository
{
    Task<PagedResult<ChuongTrinhHoc>> GetPagedAsync(
        int lopId,
        string? searchText,
        CurriculumSortColumn sortColumn,
        SortDirection sortDirection,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    Task<ChuongTrinhHoc?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra 1 lớp đã được gán môn học này trong năm học này chưa (chống trùng).
    /// </summary>
    Task<bool> ExistsAsync(int lopId, int monHocId, int namHoc, int? excludeId, CancellationToken ct = default);

    /// <summary>
    /// Lấy toàn bộ danh mục môn học để đổ vào ComboBox chọn môn khi thêm/sửa.
    /// </summary>
    Task<List<MonHoc>> GetMonHocOptionsAsync(CancellationToken ct = default);

    Task AddAsync(ChuongTrinhHoc item, CancellationToken ct = default);
    Task UpdateAsync(ChuongTrinhHoc item, CancellationToken ct = default);
    Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
}